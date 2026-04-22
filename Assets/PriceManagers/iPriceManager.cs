using System.Collections.Generic;

public interface iPriceManager
{
    void CalculateNewBidAskSpreadForMarket();
    void CyclePrices ();
    decimal GetMarketCostForGood(Good good);
    List<KnownPrice> GetAverageMarketPrices();
    void UpdatePricesForMarket();
    
}