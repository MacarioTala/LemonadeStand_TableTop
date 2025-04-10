using System.Collections.Generic;
using System.Linq;


/// <summary>
/// This class is a set of helper methods to tease out interactions
/// between supply and demand in the market.
/// It is intentionally static and stateless.
/// For the love of god, don't put any state in here. 
/// </summary>
public static class MarketObserver
{
    public static List<FulfillmentInfo> CalculateFulfillmentRates(
        List<Order> submittedOrders)
    {
        Dictionary<Good,(int totalSupply,
                         int totalDemand, 
                         int filledSupply, 
                         int filledDemand)> fulfillmentInfoDictionary = new();

       var totalSupply = submittedOrders
            .Where(x => x.IsSell())
            .GroupBy(x => x.Good)
            .Select(g => new { Good = g.Key, Supply = g.Sum(x => x.Quantity) })
            .ToDictionary(x => x.Good, x => x.Supply);

        var totalDemand = submittedOrders
            .Where(x => x.IsBuy())
            .GroupBy(x => x.Good)
            .Select(g => new { Good = g.Key, Demand = g.Sum(x => x.Quantity) })
            .ToDictionary(x => x.Good, x => x.Demand);
      
        var filledSupply = submittedOrders
            .Where(x => x.IsSell() && (x.IsFullyFilled || x.IsPartiallyFilled))
            .GroupBy(x => x.Good)
            .Select(g => new { Good = g.Key, Supply = g.Sum(x => x.FilledQuantity) })
            .ToDictionary(x => x.Good, x => x.Supply);

        var filledDemand = submittedOrders
            .Where(x => x.IsBuy() && (x.IsFullyFilled || x.IsPartiallyFilled))
            .GroupBy(x => x.Good)
            .Select(g => new { Good = g.Key, Demand = g.Sum(x => x.FilledQuantity) })
            .ToDictionary(x => x.Good, x => x.Demand);
            
        foreach (var order in submittedOrders)
        {
            var totalSupplyForGood = totalSupply.TryGetValue(order.Good, out var supply) ? supply : 0;
            var filledSupplyForGood = filledSupply.TryGetValue(order.Good, out var filledS) ? filledS : 0;
            var totalDemandForGood = totalDemand.TryGetValue(order.Good, out var demand) ? demand : 0;
            var filledDemandForGood = filledDemand.TryGetValue(order.Good, out var filledD) ? filledD : 0;

            var fulfillmentInfo = new FulfillmentInfo(
                order.Good,
                totalDemandForGood,
                totalSupplyForGood,
                filledSupplyForGood,
                filledDemandForGood);
            if (fulfillmentInfoDictionary.ContainsKey(order.Good))
            {
                fulfillmentInfoDictionary[order.Good] = (totalSupplyForGood,
                    totalDemandForGood,
                    filledSupplyForGood,
                    filledDemandForGood);
            }
            else
            {
                fulfillmentInfoDictionary.Add(order.Good, (totalSupplyForGood,
                    totalDemandForGood,
                    filledSupplyForGood,
                    filledDemandForGood));
            }
        }  
        var fulfillmentInfoList = fulfillmentInfoDictionary
            .Select(x => new FulfillmentInfo(
                x.Key,
                x.Value.totalDemand,
                x.Value.totalSupply,
                x.Value.filledSupply,
                x.Value.filledDemand))
            .ToList();
        return fulfillmentInfoList;
    }
        
}
