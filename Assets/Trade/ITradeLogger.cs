using System.Collections.Generic;

public interface ITradeLogger
{
    void LogTrade(Trade trade);
    int GetTradeCount();
    void SaveDailySummary(List<Trade> trades);
}