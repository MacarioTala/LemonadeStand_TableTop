using System.Collections.Generic;

public interface iPriceManager
{
    void CalculateNewBidAskSpreadForMarket();
    decimal GetMarketCostForGood(Good good);
    Dictionary<Good, decimal> GetAverageMarketPrices();
    void UpdatePricesForMarket();
    
}