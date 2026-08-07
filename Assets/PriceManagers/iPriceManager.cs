using System.Collections.Generic;

public interface iPriceManager
{
    void CalculateNewBidAskSpreadForMarket();
    void CyclePrices ();
    int? GetMarketCostForGood(Good good);
    List<KnownPrice> GetAverageMarketPrices();
    void UpdatePricesForMarket();
    
}