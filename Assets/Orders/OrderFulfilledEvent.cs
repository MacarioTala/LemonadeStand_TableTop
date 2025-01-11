using System.Collections.Generic;

public class OrderFulfilledEvent
{
    public Good Good { get; set; }
    public int FulfilledQuantity { get; set; }
    public int OriginalQuantity { get; set; }
    public decimal FillPrice { get; set; }
    public Market OrderMarket { get; set; }
    public int Period { get; set; }
    public Order PrimaryOrder { get; set; }
    public List<Order> CounterPartyOrders { get; set; }

#region Convenience Properties
    public bool IsFullyFilled => FulfilledQuantity == OriginalQuantity;
    public int RemainingQuantity => OriginalQuantity - FulfilledQuantity;
    public bool IsPartiallyFilled => FulfilledQuantity > 0 && FulfilledQuantity < OriginalQuantity;
    public bool IsUnfilled => FulfilledQuantity == 0;
    public float FillPercentage => (float)(FulfilledQuantity / OriginalQuantity)*100;

#endregion
}