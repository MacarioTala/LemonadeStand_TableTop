using System.Linq;

public interface iDemandStrategy
{
    const int MinDemand = 0;
    const int MaxDemand = 10000;
    void AdjustDemand(Market market);
    void InitializeDemandForSpecificGood(Market market, Good good, int initialDemand, int minDemand = MinDemand, int maxDemand = MaxDemand);
    void InitializeDemand (Market market);

     public void CalculateFulfillmentRates(Market market,int tradingPeriod=-1)
    {
        if (tradingPeriod == -1)//-1 is a sentinel value meaning no parameter was passed
        {
            //if no parameter was passed, always look at the previous trading period
            tradingPeriod = TheEconomy.Instance.tradingPeriod-1;
        }
        
        foreach(var good in market.MarketDemand.Keys)
        {
            var demanded_quantity = market.MarketDemand[good].CurrentDemand;
            var supplied_quantity = market.GetTotalBought(tradingPeriod, good);
            var FulfilmentRate = (float)supplied_quantity/demanded_quantity;
            market.MarketDemand[good].FulfilmentRate = FulfilmentRate;
        }
    }

    public int GetTotalBought(Market market,Good good,int tradingPeriod)
    {
         return 
            market.GetMarketTradesInPeriod()
            .Where(x => x.Period == tradingPeriod
                        && x.InventoryEntry.good.Equals(good)
                        && x.TradeType == TradeType.Buy)
            .Sum(x => x.InventoryEntry.quantity);
    }

    public int GetTotalSold(Market market,int tradingPeriod, Good good) //currently public for testing purposes
    {
        return market.GetMarketTradesInPeriod()
            .Where(x => x.Period == tradingPeriod
                        && x.InventoryEntry.good.Equals(good)
                        && x.TradeType == TradeType.Sell)
            .Sum(x => x.InventoryEntry.quantity);
    }
}