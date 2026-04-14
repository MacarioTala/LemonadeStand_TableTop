using System.Collections.Generic;

public interface iDemandManager
{
    DemandData GetDemandFor(Good good);
    int GetMarketDemandForGood(Good good);
    IEnumerable<(Good good, decimal Bid, decimal Ask)> GetBidAskSpreadsFromMarket();
    decimal GetPerceivedCostOfGood(Good good);
    LemonadeStandResultObject GetEffectiveElasticityForGood(Good good, ElasticityTypeEnum elasticity);
    LemonadeStandResultObject UpdateFulfillmentRates(int tradingPeriod = -1);

}