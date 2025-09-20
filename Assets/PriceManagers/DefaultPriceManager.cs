using System;
using System.Linq;

public class DefaultPriceManager : iPriceManager, iPriceSetter,iMarketAware
{
    Market _market;
    public decimal GetMarketCostForGood(Good good)
    {
        var recipeToUse = _market.GetRecipes().Where(recipe => recipe.GetProduct().Equals(good)).FirstOrDefault() ?? throw new Exception("No recipe found for " + good);
        var costPerUnit = recipeToUse.GetCostPerUnit(_market.GetInventory());
        return costPerUnit;
    }
    public void CalculateNewBidAskSpreadForMarket()
    {
        //remember to call CalculateNewBidAskSpreadForMarket 
        //as part of TheEconomy.Instance.ExecuteDailyTrades.
        //eventually
        var temporaryPriceIncrease = .01m;
        foreach(var entry in _market.GetInventory().GetInventoryEntries())
        {
            var data = new MarketData
            {
                Company = _market,
                Bid = entry.good.GetPrice(),
                Ask = entry.good.GetPrice() * (1 + temporaryPriceIncrease),
                Good = entry.good
            };
            _market.MarketData.Add(data);
        }
    }

    public void UpdatePricesForMarket()    
    {
         foreach(var entry in _market.GetInventory().GetInventoryEntries())
            {   
                var new_price = CalculateNewPrice(entry.good); 
                SetPrice(entry.good,new_price); 
            }
    }
    private decimal CalculateNewPrice (Good good)
        {
            decimal price = good.GetPrice();
            foreach(var modifier in _market.GetPriceModifiers())
            {
                price = modifier.Apply(price,good,_market);
            }
            return price;
        }

     public void SetPrice (Good good, decimal new_price)
        {
            good.SetPrice(new_price);
        }

    public void SetMarket(Market market)=>_market = market;
}