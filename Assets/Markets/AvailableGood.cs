public struct AvailableGood
{
    public iEconAgent Agent;
    public Good Good;
    public int Quantity;
    public int Price;

    public AvailableGood (iEconAgent agent, Good good, int quantity, int price)
    {
        Agent = agent;
        Good = good;
        Quantity = quantity;
        Price = price;
    }
}