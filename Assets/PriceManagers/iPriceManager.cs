public interface iPriceManager
{
    void CalculateNewBidAskSpreadForMarket(Market market);
    decimal GetMarketCostForGood(Market market, Good good);

    void UpdatePricesForMarket(Market market);
    
}