using System.Collections.Generic;

public class TradeLoggerV1 : ITradeLogger
{
    public int GetTradeCount()
    {
        throw new System.NotImplementedException();
    }

    public void LogTrade(Order trade)
    {
        throw new System.NotImplementedException();
    }

    public void SaveDailySummary(List<Order> trade_queue)
    {
        throw new System.NotImplementedException();
    }
}