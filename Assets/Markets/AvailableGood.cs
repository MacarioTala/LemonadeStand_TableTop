public struct AvailableGood
{
    public iEconAgent Agent;
    public string GoodName;
    public int Quantity;
    public decimal Price;

    public AvailableGood (iEconAgent agent, string goodName, int quantity, decimal price)
    {
        Agent = agent;
        GoodName = goodName;
        Quantity = quantity;
        Price = price;
    }
}