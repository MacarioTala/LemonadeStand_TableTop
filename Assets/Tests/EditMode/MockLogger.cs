using System.Collections.Generic;
#region stubs
public class MockLogger : ITradeLogger
{
    int tradesInPeriod;
    readonly List<Trade> tradesLogged = new();

    public int GetTradeCount()
    {
        return tradesLogged.Count;
    }

    public void LogTrade(Trade trade)
    {
        tradesInPeriod++;
    }
    
    public void SaveDailySummary(List<Trade> trade_queue)
    {
        tradesLogged.AddRange(trade_queue);
    }
}
#endregion