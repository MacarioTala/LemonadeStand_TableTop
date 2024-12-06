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