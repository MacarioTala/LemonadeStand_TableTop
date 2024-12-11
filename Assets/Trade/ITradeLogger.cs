using System.Collections.Generic;

public interface ITradeLogger
{
    void LogTrade(Order trade);
    int GetTradeCount();
    void SaveDailySummary(List<Order> trades);
}