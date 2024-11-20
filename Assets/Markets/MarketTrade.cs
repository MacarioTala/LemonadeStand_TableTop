public class MarketTrade
{
    public InventoryEntry InventoryEntry{get; private set;}
    public int Period{get; private set;}

    public TradeType TradeType{get; private set;}

    public MarketTrade(InventoryEntry inventoryEntry, int period,TradeType tradeType)
    {
        InventoryEntry = inventoryEntry;
        Period = period;
        TradeType = tradeType;
    }
}
public enum TradeType
{
    Buy,
    Sell
}