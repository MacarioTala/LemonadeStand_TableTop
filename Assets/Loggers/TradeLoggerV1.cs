using System;
using System.Collections.Generic;
using UnityEngine;

public class TradeLoggerV1 : ITradeLogger
{
    public Dictionary<Guid, List<Execution>> GetAllTransactions(List<Market> markets, int period)
    {
        var returnDict = new Dictionary<Guid, List<Execution>>();
        foreach (var market in markets)
        {
            var trades = market.GetExecutionsInPeriod(period);
            returnDict.Add(market.MarketId, trades);
        }
        return returnDict;
    }
    public int GetTradeCount()
    {
        Debug.Log("TradeLoggerV1.GetTradeCount() called");
        return 0;
    }

    public void LogTrade(Order trade)
    {
        Debug.Log("TradeLoggerV1.LogTrade() called");
    }

    public void SaveDailySummary(List<Order> trade_queue)
    {
        Debug.Log("TradeLoggerV1.SaveDailySummary() called");
    }
}