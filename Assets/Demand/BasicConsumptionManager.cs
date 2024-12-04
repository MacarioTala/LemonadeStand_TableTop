public class BasicConsumptionManager : iConsumptionManager
{
    public void AdjustDemand(ActionContext context)
    {
        throw new System.NotImplementedException();
    }

    public void FulfillDemand()
    {
        throw new System.NotImplementedException();
    }

    //  public void BuyProducedGoods()//Currently public for testing purposes
    // {
    //     foreach(var good in MarketDemand.Keys.Where(good=>good.IsProducedGood))
    //     {
    //         var remainingDemand = MarketDemand[good].CurrentDemand;       
    //         var quantitySuppliedByTrades = 
    //                     TradesSentToTheMarket.Where(trade=>trade.good == good).Sum(trade=>trade.quantity);
    //         if(quantitySuppliedByTrades <= remainingDemand)
    //         {
    //             //Send all trades for this good to the economy, with the market as a buyer
    //             foreach(var trade in TradesSentToTheMarket.Where(trade=>trade.good == good))
    //             {
    //                 trade.buyer = this;
    //                 TradesToSendToTheEconomy.Add(trade);
    //                 remainingDemand -= trade.quantity;
    //             }
    //         }
    //         else
    //         {
    //             //Shuffle the companies in the trades 
    //             //and randomly buy trades until demand is met
    //             var trades = TradesSentToTheMarket.Where(trade=>trade.good == good).ToList();
    //             trades = trades.Shuffle();
    //             while(remainingDemand > 0)
    //             {
    //                 foreach(var trade in trades)
    //                 {
    //                     //Fill order as if market order
    //                     //FillOrder(trade,remainingDemand,fillPercentage); 
    //                     throw new NotImplementedException();
    //                 }
    //             }
    //         }
    //     }
    // }
}
