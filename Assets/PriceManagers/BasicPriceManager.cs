using System;
using System.Linq;

public class BasicPriceManager : iPriceManager, iPriceSetter
{
    public decimal GetMarketCostForGood(Market market, Good good)
    {
        var recipeToUse = market.GetRecipes().Where(recipe=>recipe.GetProduct().Equals(good)).FirstOrDefault() ?? throw new Exception("No recipe found for "+good);
        var costPerUnit = recipeToUse.GetCostPerUnit(market.GetInventory());
        return costPerUnit;
    }
    public void CalculateNewBidAskSpreadForMarket(Market market)
    {
        //remember to call CalculateNewBidAskSpreadForMarket 
        //as part of TheEconomy.Instance.ExecuteDailyTrades.
        //eventually
        var temporaryPriceIncrease = .01m;
        foreach(var entry in market.GetInventory().GetInventoryEntries())
        {
            var data = new MarketData
            {
                Company = market,
                Bid = entry.good.GetPrice(),
                Ask = entry.good.GetPrice() * (1 + temporaryPriceIncrease),
                Good = entry.good
            };
            market.MarketData.Add(data);
        }
    }

    public void UpdatePricesForMarket(Market market)    
    {
         foreach(var entry in market.GetInventory().GetInventoryEntries())
            {   
                var new_price = CalculateNewPrice(entry.good,market); 
                SetPrice(entry.good,new_price); 
            }
    }
    private decimal CalculateNewPrice (Good good,Market market)
        {
            decimal price = good.GetPrice();
            foreach(var modifier in market.GetPriceModifiers())
            {
                price = modifier.Apply(price,good,market);
            }
            return price;
        }

     public void SetPrice (Good good, decimal new_price)
        {
            good.Set_price(new_price);
        }
}