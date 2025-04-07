using System.Collections.Generic;
using System.Linq;

public interface iDemandStrategy
{
    public const int MinDemand = 0;
    public const int MaxDemand = 10000;
    LemonadeStandResultObject AdjustDemandInPeriod(Market market);
    void InitializeDemandForSpecificGood(Market market, Good good, int initialDemand, int minDemand = MinDemand, int maxDemand = MaxDemand, float curvature = 1f);
    void OnOrderFulfilled(OrderFulfilledEvent orderFulfilledEvent);
#region Default Implementations

    public void AddParabolicDemanForGood(Market market, Good good, float curvature, float steepness, float shift)
    {
        var demandData = market.GetMarketDemand()[good];
        demandData.Curvature = curvature;
        demandData.Steepness = steepness;
        demandData.Shift = shift;
    }
    
    public Dictionary<Good, DemandData> GetDemandInPeriod(Market market, int tradingPeriod)
    {
        Dictionary<Good, DemandData> calculatedDemand = market.GetMarketDemand();
        var marketOrders = market.GetOrdersSentToMarket().Where(x=>x.Buyer is not Market).ToList();
        foreach (var order in marketOrders)
        {   
            var good = order.Good;
            var demandData = calculatedDemand.TryGetValue(good, out var value) ? value : new DemandData{CurrentDemand = 0};
            demandData.CurrentDemand += order.Quantity;
            calculatedDemand[good] = demandData;
        }
        return calculatedDemand;
    }
    public LemonadeStandResultObject CalculateFulfillmentRates(Market market,int tradingPeriod)
    {
        throw new System.NotImplementedException();
    }
    public Dictionary<Good, int> CalculateSupplyForPeriod(Market market, int tradingPeriod)
    {
        Dictionary<Good, int> calculatedSupply = new();
        var executedTrades = market.GetExecutionsInPeriod(tradingPeriod)
                                   .Where(x => x.RecordedTrade.IsSell());

        foreach (var trade in executedTrades)
        {
            if(calculatedSupply.ContainsKey(trade.RecordedTrade.Good))
            {
                calculatedSupply[trade.RecordedTrade.Good] += trade.RecordedTrade.Quantity;
            }
            else
            {
                calculatedSupply.Add(trade.RecordedTrade.Good, trade.RecordedTrade.Quantity);
            }
        }
        return calculatedSupply;
    }

    public int GetTotalBoughtByMarket(Market market,Good good,int tradingPeriod)
    {
         return market.GetExecutionsInPeriod(tradingPeriod)
                         .Where(x => x.Period == tradingPeriod
                        && x.RecordedTrade.Good.Equals(good)
                        && x.RecordedTrade.IsBuy()
                        && x.RecordedTrade.Buyer is Market
                        )
                        .Sum(x => x.RecordedTrade.Quantity);
    }

    public int GetTotalSoldByMarket(Market market,int tradingPeriod, Good good) //currently public for testing purposes
    {
        return market.GetExecutionsInPeriod(tradingPeriod)
            .Where(x => x.Period == tradingPeriod
                        && x.RecordedTrade.Good.Equals(good)
                        && x.RecordedTrade.Seller is Market
                        )
            .Sum(x => x.RecordedTrade.Quantity);
    }
#endregion
}