public interface iPriceManager
{
    decimal GetMarketCostForGood(Market market, Good good);
    void CalculateNewBidAskSpreadForMarket(Market market);
}