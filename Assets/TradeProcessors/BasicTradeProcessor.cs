using System.Collections.Generic;

public class BasicTradeProcessor : iTradeProcessor
{
     private readonly List<Trade> _tradesSentToTheMarket = new();
     public List<Trade> TradesToSendToTheEconomy = new();

    public void AddOrderToSendToEconomy(Trade trade)
    {
        TradesToSendToTheEconomy.Add(trade);
    }
    public List<Trade> GetOrders()=>_tradesSentToTheMarket;
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
        _tradesSentToTheMarket.Add(context.TradeToSubmit);
        AddOrderToSendToEconomy(context.TradeToSubmit);
    }
    public void SendTradesToEconomy()
    {
        foreach (var trade in TradesToSendToTheEconomy)
        {
            TheEconomy.Instance.QueueOrder(trade);
        }
        TradesToSendToTheEconomy.Clear();
    }
}