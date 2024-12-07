using System.Collections.Generic;
#region stubs
public class MockLogger : ITradeLogger
{
    int tradesInPeriod;

    public int GetTradeCount()
    {
        return tradesInPeriod;
    }

    public void LogTrade(Trade trade)
    {
        tradesInPeriod++;
    }
    
    public void SaveDailySummary(List<Trade> trade_queue)
    {
    }
}
#endregion