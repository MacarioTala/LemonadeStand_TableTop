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
        var lemonadeInventory = GetLemonadeEntry(_lemonade);
        
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
        var remainingCash = GetSpendableCash();//aggressionLevel is currently a 'risk appetite'. Spend this much of all reserves
        var market = Actor.GetMarket();
        var lemonadeRecipe = GetLemonadeRecipe();

        var currentPrices = market.GetGoodsAvailableInPeriod(Actor)
                    .ToDictionary(x=>x.Good,x=>x.Price);
        var ingredientsToBuy = lemonadeRecipe.GetIngredientMaximumsByBudget(currentPrices,remainingCash);
        
        foreach(var ingredient in ingredientsToBuy)
        {
            remainingCash = BuyIngredientAndGiveChange(remainingCash, market, currentPrices, ingredient);
        }
        StockpileIngredientsWithRemainingCash(remainingCash);
    }

    #endregion

    private void StockpileIngredientsWithRemainingCash(decimal remainingCash)
    {
        var market = Actor.GetMarket();
        var lemonadeRecipe = GetLemonadeRecipe();
        var currentPrices = market.GetIngredientBidAskSpreadForPeriod(Actor,market.CurrentPeriod)
                    .ToDictionary(x=>x.Good,x=>x.Ask);
        int quantityToBuy = 0;
        int maxToBuy;
        //will currently randomly pick from ingredients to stockpile based on cash
        var currentDemand = Actor.GetDemand();
        var newDemand = GetNewDemandedQuantities();
        if(currentDemand.Count()>0)
        {
            int randomIndex = UnityEngine.Random.Range(0,currentDemand.Count-1);
            var randomGoodToBuy = currentDemand.Keys.ElementAt(randomIndex);
            var maxQuantityBuyable = (int)(remainingCash/currentPrices[randomGoodToBuy]);
            if(!(newDemand.Count()==0))
                maxToBuy = newDemand[randomGoodToBuy];
            else
                maxToBuy = currentDemand[randomGoodToBuy].CurrentDemand;
            
            quantityToBuy = maxQuantityBuyable>maxToBuy?maxToBuy : maxQuantityBuyable;

            if(quantityToBuy>0)
                BuyIngredientAndGiveChange(remainingCash,market,currentPrices,new Ingredient(randomGoodToBuy,quantityToBuy));
        }
    }
    private Dictionary<Good, int> GetNewDemandedQuantities()
    {   
        var market = Actor.GetMarket();
        var newDemand = new Dictionary<Good, int>();
        var currentDemand = Actor.GetDemand();
        var currentPrices = market.GetIngredientBidAskSpreadForPeriod(Actor,market.CurrentPeriod)
                    .ToDictionary(x=>x.Good,x=>x.Ask);
        //Figure out last period's prices
        var previousPeriodPrices = new Dictionary<Good, int>();
        if (market.CurrentPeriod >0)
        {
            //TODO: refactor later when we need the strategy to have demand for new goods introduced
            previousPeriodPrices = market.GetIngredientBidAskSpreadForPeriod(Actor, market.CurrentPeriod - 1)
                                   .ToDictionary(x=>x.Good,x=>x.Ask);
            foreach (var availableGood in previousPeriodPrices)
            {
                var currentPriceOfGood = currentPrices.GetValueOrDefault(availableGood.Key);
                float priceDelta = (float)(currentPriceOfGood - availableGood.Value) / (float)availableGood.Value;
                if(priceDelta>0)
                {
                    if(currentDemand.TryGetValue(availableGood.Key,out var demandRow))
                    newDemand.Add(availableGood.Key, demandRow.GetAdjustedDemandFor(ElasticDemandComponentEnum.Price, priceDelta));
                }
            }
        }
        return newDemand;
    }

    private void SellLemonade(Good lemonade, int quantity)
    {
        // Get market price for lemonade
        var market = Actor.GetMarket();
        var marketPrices = market.GetAverageMarketPrices();
        int sellPrice;

        var costOfLemonade = GetLemonadeEntry(lemonade).Cost;
        
        var lemonadePrices = marketPrices.Where(x=>x.Good==lemonade); 
        var lemonadePrice = lemonadePrices
                           .Any()
                           ? (int)Math.Round(lemonadePrices.Average(x=>x.Price))
                           : costOfLemonade;
        if (lemonadePrice > 0)
        {
            // Sell at market price or slightly below to be competitive
            sellPrice = (int)lemonadePrice -1;
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
    private decimal BuyIngredientAndGiveChange(decimal remainingCash, Market market, Dictionary<Good, int> currentPrices, Ingredient ingredient)
    {
        var amountOfIngredient = ingredient.QuantityNeeded;
        var costOfIngredient = currentPrices[ingredient.Good];
        var order = new Order(Actor, null, ingredient.Good, amountOfIngredient, costOfIngredient) { SubmittingCompany = Actor };
        var context = new ActionContext
        {
            TradeToSubmit = order,
            MarketToSubmitTo = market,
            Period = market.CurrentPeriod,
            SubmittingCompany = Actor,
            Action = ActionEnum.QueueTradeBuy
        };

        Actor.QueueOrder(context);

        remainingCash -= amountOfIngredient * costOfIngredient;
        return remainingCash;
    }
    private Good GetLemonadeGood()
    // Get lemonade good, assuming it exists 
        => Actor.GetInventory().GetInventoryEntries()
            .FirstOrDefault(entry => entry.good.GoodName.Contains("Lemonade", StringComparison.OrdinalIgnoreCase))?.good;
    private InventoryEntry GetLemonadeEntry(Good _lemonade)
    => Actor.GetInventory().GetInventoryEntries()
            .FirstOrDefault(entry => entry.good == _lemonade);
    private Recipe GetLemonadeRecipe()
        => Actor.Recipes
                .FirstOrDefault(r => r.GetProduct().GoodName.Contains("Lemonade", StringComparison.OrdinalIgnoreCase));
    private decimal GetSpendableCash()
        =>(Actor.GetCash()*_aggressionLevel)-Actor.FixedCosts.Sum(x=>x.Template.Amount);

    #endregion
}
