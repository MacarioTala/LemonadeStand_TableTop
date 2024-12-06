using JetBrains.Annotations;

public class Trade
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


    public Trade(iCompany buyer, iCompany seller, Good good, int quantity, decimal price)
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
