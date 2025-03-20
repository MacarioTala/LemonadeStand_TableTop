using System;
using System.Collections.Generic;

public interface ITradeLogger
{
    void LogTrade(Order trade);
    int GetTradeCount();
    void SaveDailySummary(List<Order> trades);
    Dictionary<Guid, List<MarketTransaction>> GetAllTransactions(List<Market> markets, int period);
}