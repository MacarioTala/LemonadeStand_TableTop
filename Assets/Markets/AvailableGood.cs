public struct AvailableGood
{
    public iEconAgent Agent;
    public Good Good;
    public int Quantity;
    public decimal Price;

    public AvailableGood (iEconAgent agent, Good good, int quantity, decimal price)
    {
        Agent = agent;
        Good = good;
        Quantity = quantity;
        Price = price;
    }
}