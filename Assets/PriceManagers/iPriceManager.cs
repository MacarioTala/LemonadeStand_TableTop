public interface iPriceManager
{
    void CalculateNewBidAskSpreadForMarket();
    decimal GetMarketCostForGood(Good good);

    void UpdatePricesForMarket();
    
}