using System.Collections.Generic;


/// <summary>
/// This class is a set of helper methods to tease out interactions
/// between supply and demand in the market.
/// It is intentionally static and stateless.
/// For the love of god, don't put any state in here. 
/// </summary>
public static class MarketObserver
{
    public static List<FulfillmentInfo> CalculateFulfillmentRates(
        Dictionary<Good,DemandData> marketDemand,
        Dictionary<Good,int> marketSupply)
    {
        var fulfillmentInfoList = new List<FulfillmentInfo>();
        
        foreach(var good in marketDemand.Keys)
        {
            var demandedQuantity = marketDemand.TryGetValue(good,out var demandData) ? demandData.CurrentDemand:0;
            var suppliedQuantity = marketSupply.TryGetValue(good, out var supplyData) ? supplyData : 0;
            var fulfillmentInfo = new FulfillmentInfo(good, demandedQuantity, suppliedQuantity);
            fulfillmentInfoList.Add(fulfillmentInfo);
        }
        return fulfillmentInfoList;
    }
}
