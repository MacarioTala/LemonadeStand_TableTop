using System;

public class Order
{
    public Guid Id {get;private set;} = Guid.NewGuid();
    public iCompany SubmittingCompany;
    public iCompany Buyer;
    public iCompany Seller;
    public Good Good;
    public int Quantity;
    public int FilledQuantity=0;
    public int RemainingQuantity=>Quantity-FilledQuantity;
    public decimal Price;

    public bool IsFullyFilled => RemainingQuantity == 0;
    public bool IsPartiallyFilled => RemainingQuantity > 0 && FilledQuantity > 0;
    private bool IsSelfTrade => Buyer == Seller;

    public LemonadeStandResultObject OrderStatus { get; set; }

    public LemonadeStandResultObject IsOrderValid()
    {
        if ((Seller == null) && (Buyer == null)) return LemonadeStandResultObject.Failure(ResultTypeEnum.OrderHasNoActors, "Order has no actors");
        if (Good == null) return LemonadeStandResultObject.Failure(ResultTypeEnum.OrderHasNoGood, "Order has no good");
        if (Quantity == 0) return LemonadeStandResultObject.Failure(ResultTypeEnum.OrderHasInvalidQuantity, "Order has an invalid quantity");
        if (Price <= 0) return LemonadeStandResultObject.Failure(ResultTypeEnum.OrderHasInvalidPrice, "Order has an invalid price");
        if (IsSelfTrade) return LemonadeStandResultObject.Failure(ResultTypeEnum.SelfTrade, "Order is a self trade");
        return LemonadeStandResultObject.Success();
    }

    public Order(iCompany buyer, iCompany seller, Good good, int quantity, decimal price)
    {
        Buyer = buyer;
        Seller = seller;
        Good = good;
        Quantity = quantity;
        Price = price;
    }

    public override string ToString()
    {
        var action = Buyer.Equals(SubmittingCompany) ? " buys " : " sells ";
        var counterParty = Buyer.Equals(SubmittingCompany) ? Seller : Buyer;
        return "Order: " + SubmittingCompany + action + Quantity + " " + Good.good_name + " from " + counterParty + " at " + Price;
    }
}
