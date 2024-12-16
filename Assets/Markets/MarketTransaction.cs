using System;
using System.Collections.Generic;

public class MarketTransaction
{
    public Guid TransactionId {get;private set;} = Guid.NewGuid();
    public Order RecordedTrade{get; private set;}
    public List<Order> CounterPartyTrades{get; private set;} = new List<Order>();
    public int Period{get; private set;}

    public MarketTransaction(Order recordedTrade, int period)
    {
        RecordedTrade = recordedTrade;
        Period = period;
    }
    public void AddCounterPartyTrade(Order counterPartyTrade)
    {
        CounterPartyTrades.Add(counterPartyTrade);
    }
    public override bool Equals(object other)
    {
        if (other is MarketTransaction otherTrade)
        {
            return RecordedTrade == otherTrade.RecordedTrade && Period == otherTrade.Period;
        }
        return false;
    }
    public override int GetHashCode()
    {
        return RecordedTrade.GetHashCode() ^ Period.GetHashCode();
    }
    public override string ToString()
    {
        return $"Trade: {RecordedTrade.Buyer} buys {RecordedTrade.FilledQuantity} of {RecordedTrade.Good} at Period: {Period}";
    }
}
public enum TradeType
{
    Buy,
    Sell
}