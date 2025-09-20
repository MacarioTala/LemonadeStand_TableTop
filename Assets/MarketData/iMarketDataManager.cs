using System.Collections.Generic;

public interface iMarketDataManager
{
    List<MarketData> PublishMarketData();
    void PublishSpreadToMarket(ActionContext context);
    List<Execution> GetExecutionsInPeriod(int period);
    List<Order> GetOrdersSubmittedInPeriod(int period);
    List<(Order Order, int Period)> GetOrdersExecutedInPeriod(params int[] periods);
    void LogOrder(Order order, int period);
    LemonadeStandResultObject RecordOrderInPeriod(Order order, int period);
    void RecordTrade(Execution trade);
    
}