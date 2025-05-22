public struct BidAskSpread
{
    public BidAskSpread(decimal bid, decimal ask, float goodQuality, float allocation)
    {
        Bid = bid;
        GoodQuality = goodQuality;
        Ask = ask;
        Allocation = allocation;
    }
    public decimal Bid { get; }
    public decimal Ask { get; set;}
    public float GoodQuality { get; }
    public float Allocation { get; set;}
}