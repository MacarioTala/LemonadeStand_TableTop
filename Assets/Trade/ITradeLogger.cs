using System;
using System.Collections.Generic;

public interface ITradeLogger
{
    void LogTrade(Order trade);
    int GetTradeCount();
    void SaveDailySummary(List<Order> trades);
    Dictionary<Guid, List<Execution>> GetAllTransactions(List<Market> markets, int period);
}