using System;
using System.Collections.Generic;
using System.Linq;

public class LemonadeSellerStrategy : iStrategy
{
    EconAgent Actor;
    private decimal _aggressionLevel = 0.8m;
    public decimal GetAggressionLevel() => _aggressionLevel;
    
    public LemonadeStandResultObject SetAggressionLevel(decimal aggressionLevel)
    {
        if (aggressionLevel < 0 || aggressionLevel > 1)
            return LemonadeStandResultObject.Failure("Aggression level must be between 0 and 1.");
        _aggressionLevel = aggressionLevel;
        return LemonadeStandResultObject.Success();
    }
    
    public void GenerateGoals(iEconAgent company)
    {
        // Simple goal: maximize lemonade sales
        var salesGoal = new Goal("Maximize Lemonade Sales", 
                                "Sell as much lemonade as possible",
                                null,
                                (c, g) => g.SetOriginalValue("LemonadeSold", 0))
        {
            IsGoalMet = c => true // Ongoing goal, never "met" but always pursued
        };
        company.Goals.Add(salesGoal);
    }
    
    public void PerformStrategy(iEconAgent company)
    {
        // Cast to EconAgent to access concrete methods
        var econAgent = company as EconAgent;
        if (econAgent == null) return;
        
        // This is the main strategy execution
        var market = econAgent.GetMarket();
        if (market == null) return;
        
        var _lemonade = GetLemonadeGood();
        if (_lemonade == null)
        {
            // If no lemonade, try to produce it
            TryProduceLemonade(market);
        }
        
        // Check if we have lemonade to sell
        var lemonadeInventory = econAgent.GetInventory().GetInventoryEntries()
            .FirstOrDefault(entry => entry.good == _lemonade);
        
        if (lemonadeInventory != null && lemonadeInventory.quantity > 0)
        {
            // Sell lemonade to the market
            SellLemonade(_lemonade, lemonadeInventory.quantity);
        }
        else
        {
            // No lemonade available, try to produce
            TryProduceLemonade(market);
        }

        //Buy Ingredients
        BuySupplies();
    }
    
    private void TryProduceLemonade(Market market)
    {
        // Check if company has lemonade recipe
        var lemonadeRecipe = GetLemonadeRecipe();
        if (lemonadeRecipe == null) return;
        
        // Check if we can produce lemonade
        var inventoryEntries = Actor.GetInventory().GetInventoryEntries();
        var maxQuantity = lemonadeRecipe.GetMaxQuantityFromInventory(inventoryEntries);
        
        if (maxQuantity > 0)
        {
            // Produce as much as possible
            var context = new ActionContext 
            { 
                Recipe = lemonadeRecipe, 
                QuantityToMake = maxQuantity, 
                RecipeMaker = Actor,
                Period = market.CurrentPeriod
            };
            Actor.MakeRecipe(context);
        }
    }
    #region Buy Supplies
    internal void BuySupplies()//TODO: add asmdef
    {
        var cash = Actor.GetCash();
        var market = Actor.GetMarket();
        var lemonadeRecipe = GetLemonadeRecipe();

        var currentPrices = market.GetGoodsAvailableInPeriod(Actor)
                    .ToDictionary(x=>x.Good,x=>x.Price);
        var ingredientsToBuy = lemonadeRecipe.GetIngredientMaximumsByBudget(currentPrices,cash);
        
        foreach(var ingredient in ingredientsToBuy)
        {
            var order = new Order(Actor,null,ingredient.Good,ingredient.QuantityNeeded,currentPrices[ingredient.Good]){SubmittingCompany=Actor};
            var context = new ActionContext
                    {
                        TradeToSubmit = order,
                        MarketToSubmitTo = market,
                        Period = market.CurrentPeriod,
                        SubmittingCompany = Actor,
                        Action = ActionEnum.QueueTradeBuy
                    };
                    
            Actor.QueueOrder(context);
        }
    }
    #endregion

    private Dictionary<Good, int> GetDemandedQuantities(Market market, List<AvailableGood> currentPrices, IReadOnlyDictionary<Good, DemandData> demand)
    {
        var newDemand = new Dictionary<Good, int>();
        //Figure out last period's prices
        var previousPeriodPrices = new List<AvailableGood>();
        if (market.CurrentPeriod == 0)
            previousPeriodPrices = market.GetGoodsAvailableInPeriod(Actor);
        else
        {
            previousPeriodPrices = market.GetGoodsAvailableInPeriod(Actor, market.CurrentPeriod - 1);
            foreach (var availableGood in previousPeriodPrices)
            {
                var currentPriceOfGood = currentPrices.FirstOrDefault(x => x.Good == availableGood.Good).Price;
                float priceDelta = (float)(currentPriceOfGood - availableGood.Price) / (float)availableGood.Price;
                var demandRow = demand.FirstOrDefault(x => x.Key == availableGood.Good).Value;

                newDemand.Add(availableGood.Good, demandRow.GetAdjustedDemandFor(ElasticDemandComponentEnum.Price, priceDelta));
            }
        }
        return newDemand;
    }

    private void SellLemonade(Good lemonade, int quantity)
    {
        // Get market price for lemonade
        var market = Actor.GetMarket();
        var marketPrices = market.GetAverageMarketPrices();
        decimal sellPrice;
        
        var lemonadePrice = marketPrices.Where(x=>x.Good==lemonade).Average(x=>x.Price);
        if (lemonadePrice > 0)
        {
            // Sell at market price or slightly below to be competitive
            sellPrice = lemonadePrice * 0.95m;
        }
        else
        {
            // Default price if no market data
            sellPrice = lemonade.GetPrice();
        }
        
        // Don't sell everything at once, keep some inventory
        var quantityToSell = Math.Min(quantity, Math.Max(1, quantity / 2));

        // Create sell order to market
        var order = new Order(Actor, market, lemonade, quantityToSell, sellPrice)
        {
            SubmittingCompany = Actor
        };

        var context = new ActionContext
        {
            TradeToSubmit = order,
            MarketToSubmitTo = market,
            Period = market.CurrentPeriod,
            SubmittingCompany = Actor,
            Action = ActionEnum.QueueTradeSell
        };
        
        Actor.QueueOrder(context);
    }
    
    public LemonadeStandResultObject PublishBidAskSpreadsToMarket(iEconAgent company)
    {
        // Not implementing bid/ask spreads for this simple strategy
        return LemonadeStandResultObject.Success();
    }

    public void SetEconAgent(EconAgent agent)
    {
        Actor = agent;
    }

    public List<ActionContext> GetQueuedActions()
    {
        throw new NotImplementedException();
    }
    #region Helpers
    private Good GetLemonadeGood()
    // Get lemonade good, assuming it exists 
        => Actor.GetInventory().GetInventoryEntries()
            .FirstOrDefault(entry => entry.good.GoodName.Contains("Lemonade", StringComparison.OrdinalIgnoreCase))?.good;
    private Recipe GetLemonadeRecipe()
        => Actor.Recipes
                .FirstOrDefault(r => r.GetProduct().GoodName.Contains("Lemonade", StringComparison.OrdinalIgnoreCase));
    #endregion
}
