using System.Collections.Generic;

public class BasicTradeProcessor : iTradeProcessor
{
     private readonly List<Trade> TradesSentToTheMarket = new();
     public List<Trade> TradesToSendToTheEconomy = new();

    public List<Trade> GetOrderResults(ActionContext context)
    {
        throw new System.NotImplementedException();
    }

    public void ProcessOrders()
    {
        throw new System.NotImplementedException();
    }

    public void QueueOrder(ActionContext context)
    {
        var trade = new Trade(buyer: context.Buyer,
                              seller: context.Seller,
                              good: context.GoodToBuy,
                              quantity: context.Quantity,
                              price: context.Price
                            );
        TradesSentToTheMarket.Add(trade);
    }
    public void SendTradesToEconomy()
    {
        //Adjust this later to have different demand fulfilment strategies
        //For now, make it random
        
        //Send all other trades to the economy
        foreach(var trade in TradesSentToTheMarket)
        {
            if(!trade.good.IsProducedGood)
            {
                TradesToSendToTheEconomy.Add(trade);
            }
        }
    }
}