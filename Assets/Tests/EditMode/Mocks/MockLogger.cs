using System;
using System.Collections.Generic;
#region stubs
public class MockLogger : ITradeLogger
{
    int tradesInPeriod;
    readonly List<Order> tradesLogged = new();

    public int GetTradeCount()
    {
        return tradesLogged.Count;
    }

    public void LogTrade(Order trade)
    {
        tradesInPeriod++;
    }
    
    public void SaveDailySummary(List<Order> trade_queue)
    {
        tradesLogged.AddRange(trade_queue);
    }

    public Dictionary<Guid, List<Execution>> GetAllTransactions(List<Market> markets,int period)
    {
        throw new NotImplementedException();
    }
}
#endregion