using System.Collections.Generic;

public interface ITradeLogger
{
    void LogTrade(Trade trade);
    void SaveDailySummary(List<Trade> trades);
}