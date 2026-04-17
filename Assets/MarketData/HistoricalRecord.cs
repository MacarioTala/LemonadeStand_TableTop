using System;
using System.Collections.Generic;

public record HistoricalRecord
{
    public Guid Id=new();
    public OrderSnapshot OriginalOrderSnapshot;
    public int CreatedInPeriod;
    public OrderResultEnum Result;
    public int FilledQuantity;
    public int RemainingQuantity;
    public string Message;
    public List<Execution> Executions=new();
}

public class OrderSnapshot
{
    public Guid OrderId;
    public string BuyerName;
    public string SellerName;
    public string GoodName;
    public int Quantity;
    public decimal Price;

    public bool IsBuy() => SellerName == null;
}

public enum OrderResultEnum
{
    Submitted,
    Rejected,
    Filled,
    PartiallyFilled,
    Unfilled
}