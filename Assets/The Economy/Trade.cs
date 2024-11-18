using System.Collections.Generic;
using JetBrains.Annotations;

public class Trade
{
    public iCompany buyer;
    public iCompany seller;
    public Good good;
    public int quantity;
    public decimal price;

    public Trade(iCompany buyer, iCompany seller, Good good, int quantity, decimal price)
    {
        this.buyer = buyer;
        this.seller = seller;
        this.good = good;
        this.quantity = quantity;
        this.price = price;
    }

    public override string ToString()
    {
        return "Trade: " + buyer.company_name + " buys " + quantity + " " + good.good_name + " from " + seller.company_name + " at " + price;
    }
}

public interface ITradeLogger
{
    void LogTrade(Trade trade);
    void SaveDailySummary(List<Trade> trades);
}