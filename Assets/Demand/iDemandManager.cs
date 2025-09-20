using System.Collections.Generic;

public interface iDemandManager
{
    DemandData GetDemandFor(Good good);
    int GetMarketDemandForGood(string goodName);
    void SetMarketDemandForGood(Good good, DemandData demandData);
    IEnumerable<(Good good, decimal Bid, decimal Ask)> GetBidAskSpreadsFromMarket();
    decimal GetPerceivedCostOfGood(Good good);
    void InitializeDemandForSpecificGood(Good good, int InitialDemand, int minDemand = iDemandStrategy.MinDemand, int maxDemand = iDemandStrategy.MaxDemand, float curvature = 1f);
    LemonadeStandResultObject UpdateFulfillmentRates(int tradingPeriod = -1);

}