using System.Collections.Generic;

public interface iPriceManager
{
    void CalculateNewBidAskSpreadForMarket();
    decimal GetMarketCostForGood(Good good);
    List<KnownPrice> GetAverageMarketPrices();
    void UpdatePricesForMarket();
    
}