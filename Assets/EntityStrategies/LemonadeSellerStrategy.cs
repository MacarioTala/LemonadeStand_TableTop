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
    
    public void PerformStrategy(ActionContext context)
    {
        switch(context.Action)
        {
            case ActionEnum.QueueTradeSell:
                context.TradeToSubmit.Seller.QueueOrder(context);
                break;
                
            case ActionEnum.MakeRecipe:
                context.RecipeMaker.MakeRecipe(context);
                break;
                
            default:
                // For other actions, use default behavior
                var basicStrategy = new BasicGrowthStrategy();
                basicStrategy.PerformStrategy(context);
                break;
        }
    }
    
    public void PerformStrategy(iEconAgent company)
    {
        // Cast to EconAgent to access concrete methods
        var econAgent = company as EconAgent;
        if (econAgent == null) return;
        
        // This is the main strategy execution
        var market = econAgent.GetMarket();
        if (market == null) return;
        
        // Get lemonade good (assuming it exists in the test goods)
        var lemonade = econAgent.GetInventory().GetInventoryEntries()
            .FirstOrDefault(entry => entry.good.GoodName.Contains("Lemonade", StringComparison.OrdinalIgnoreCase))?.good;
        
        if (lemonade == null)
        {
            // If no lemonade, try to produce it
            TryProduceLemonade(econAgent, market);
            return;
        }
        
        // Check if we have lemonade to sell
        var lemonadeInventory = econAgent.GetInventory().GetInventoryEntries()
            .FirstOrDefault(entry => entry.good == lemonade);
        
        if (lemonadeInventory != null && lemonadeInventory.quantity > 0)
        {
            // Sell lemonade to the market
            SellLemonade(econAgent, market, lemonade, lemonadeInventory.quantity);
        }
        else
        {
            // No lemonade available, try to produce
            TryProduceLemonade(econAgent, market);
        }
    }
    
    private void TryProduceLemonade(EconAgent company, Market market)
    {
        // Check if company has lemonade recipe
        var lemonadeRecipe = company.Recipes
            .FirstOrDefault(r => r.GetProduct().GoodName.Contains("Lemonade", StringComparison.OrdinalIgnoreCase));
        
        if (lemonadeRecipe == null) return;
        
        // Check if we can produce lemonade
        var inventory = company.GetInventory().GetInventoryEntries();
        var maxQuantity = lemonadeRecipe.Get_max_quantity(inventory);
        
        if (maxQuantity > 0)
        {
            // Produce as much as possible
            var context = new ActionContext 
            { 
                Recipe = lemonadeRecipe, 
                QuantityToMake = maxQuantity, 
                RecipeMaker = company,
                Period = market.CurrentPeriod
            };
            company.MakeRecipe(context);
        }
    }
    
    private void SellLemonade(EconAgent company, Market market, Good lemonade, int quantity)
    {
        // Get market price for lemonade
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
        var order = new Order(company, market, lemonade, quantityToSell, sellPrice);
        order.SubmittingCompany = company;
        
        var context = new ActionContext
        {
            TradeToSubmit = order,
            MarketToSubmitTo = market,
            Period = market.CurrentPeriod,
            SubmittingCompany = company,
            Action = ActionEnum.QueueTradeSell
        };
        
        market.QueueOrder(context);
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
}
