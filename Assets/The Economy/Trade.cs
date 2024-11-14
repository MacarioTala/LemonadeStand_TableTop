using System.Collections.Generic;

public class Trade
{
    public iCompany buyer;
    public iCompany seller;
    public Good good;
    public int quantity;
    public float price;

    public Trade(iCompany buyer, iCompany seller, Good good, int quantity, float price)
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