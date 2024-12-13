public class Order
{
    public iCompany Buyer;
    public iCompany Seller;
    public Good Good;
    public int Quantity;
    public int FilledQuantity=0;
    public int RemainingQuantity=>Quantity-FilledQuantity;
    public decimal Price;

    public bool IsFullyFilled => RemainingQuantity == 0;
    public bool IsPartiallyFilled => RemainingQuantity > 0 && FilledQuantity > 0;

    public LemonadeStandResultObject IsOrderValid()
    {
        if (Seller == null) return LemonadeStandResultObject.Failure(ResultTypeEnum.OrderHasNoSeller, "Order has no seller");
        if (Buyer == null) return LemonadeStandResultObject.Failure(ResultTypeEnum.OrderHasNoBuyer, "Order has no buyer");
        if (Good == null) return LemonadeStandResultObject.Failure(ResultTypeEnum.OrderHasNoGood, "Order has no good");
        if (Quantity == 0) return LemonadeStandResultObject.Failure(ResultTypeEnum.OrderHasInvalidQuantity, "Order has an invalid quantity");
        if (Price <= 0) return LemonadeStandResultObject.Failure(ResultTypeEnum.OrderHasInvalidPrice, "Order has an invalid price");
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
        return "Trade: " + Buyer.Name + " buys " + Quantity + " " + Good.good_name + " from " + Seller.Name + " at " + Price;
    }
}
