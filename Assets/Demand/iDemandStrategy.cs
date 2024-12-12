using System.Collections.Generic;
using System.Linq;

public interface iDemandStrategy
{
    const int MinDemand = 0;
    const int MaxDemand = 10000;
    void AdjustDemand(Market market);

    float GetDemandElasticityForGood(Good good);

    float GetIncomeElasticityForGood(Good good);
    float GetPriceElasticityForGood(Good good);

    void InitializeDemand (Market market);
    void InitializeDemandForSpecificGood(Market market, Good good, int initialDemand, int minDemand = MinDemand, int maxDemand = MaxDemand);
    
    public Dictionary<Good, DemandData> CalculateDemandForPeriod(Market market, int tradingPeriod)
    {
        Dictionary<Good, DemandData> calculatedDemand = market.GetMarketDemand();
        var marketOrders = market.GetOrdersSentToMarket();
        foreach (var order in marketOrders)
        {   
            var good = order.Good;
            var demandData = calculatedDemand.TryGetValue(good, out var value) ? value : new DemandData{CurrentDemand = 0};
            demandData.CurrentDemand += order.Quantity;
            calculatedDemand[good] = demandData;
        }
        return calculatedDemand;
    }
    public Dictionary<Good, int> CalculateSupplyForPeriod(Market market, int tradingPeriod)
    {
        Dictionary<Good, int> calculatedSupply = new();
        var executedTrades = market.GetMarketTradesInPeriod(tradingPeriod);
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
     public void CalculateFulfillmentRates(Market market,int tradingPeriod=-1)
    {
        var marketDemand = market.GetMarketDemand();
        if (tradingPeriod == -1)//-1 is a sentinel value meaning no parameter was passed
        {
            //if no parameter was passed, always look at the previous trading period
            tradingPeriod = TheEconomy.Instance.tradingPeriod-1;
        }
        
        foreach(var good in marketDemand.Keys)
        {
            var demandedQuantity = marketDemand[good].CurrentDemand;
            var suppliedQuantity = market.GetTotalBought(tradingPeriod, good);
            var FulfilmentRate = (float)suppliedQuantity/demandedQuantity;
            marketDemand[good].FulfilmentRate = FulfilmentRate;
        }
    }

    public int GetTotalBought(Market market,Good good,int tradingPeriod)
    {
         return 
            market.GetMarketTradesInPeriod(tradingPeriod)
            .Where(x => x.Period == tradingPeriod
                        && x.RecordedTrade.Good.Equals(good))
            .Sum(x => x.RecordedTrade.Quantity);
    }

    public int GetTotalSold(Market market,int tradingPeriod, Good good) //currently public for testing purposes
    {
        return market.GetMarketTradesInPeriod(tradingPeriod)
            .Where(x => x.Period == tradingPeriod
                        && x.RecordedTrade.Good.Equals(good)
                            )
            .Sum(x => x.RecordedTrade.Quantity);
    }
}