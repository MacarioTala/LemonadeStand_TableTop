public class MarketTrade
{
    public Order RecordedTrade{get; private set;}
    public int Period{get; private set;}

    public MarketTrade(Order recordedTrade, int period)
    {
        RecordedTrade = recordedTrade;
        Period = period;
    }

    public override bool Equals(object other)
    {
        if (other is MarketTrade otherTrade)
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
        return $"Trade: {RecordedTrade} Period: {Period}";
    }
}
public enum TradeType
{
    Buy,
    Sell
}