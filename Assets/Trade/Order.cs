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

    public bool IsUnfilled => RemainingQuantity == Quantity;
    private bool IsSelfTrade => Buyer == Seller;

    public bool IsBuy() => Buyer == SubmittingCompany;
    public bool IsSell() => Seller == SubmittingCompany;

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
        string action;
        string counterParty;
        string preposition;
        if (Buyer !=null)
        {
            action = Buyer.Equals(SubmittingCompany) ? " buys " : " sells ";
            counterParty = Seller?.Name?? " anyone ";
            preposition = " from ";
        }
        else
        {
            action = " sells ";
            counterParty = Buyer?.Name?? " anyone ";
            preposition=" to ";
        }
        
        return "Order: " + SubmittingCompany + action + RemainingQuantity + " " + Good.good_name + preposition + counterParty + " at " + Price;
    }
}
