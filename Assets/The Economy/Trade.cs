using System.Collections.Generic;

public class Trade
{
    public Company buyer;
    public Company seller;
    public Good good;
    public int quantity;
    public float price;

    public Trade(Company buyer, Company seller, Good good, int quantity, float price)
    {
        this.buyer = buyer;
        this.seller = seller;
        this.good = good;
        this.quantity = quantity;
        this.price = price;
    }
}

public interface ITradeLogger
{
    void LogTrade(Trade trade);
    void SaveDailySummary(List<Trade> trades);
}