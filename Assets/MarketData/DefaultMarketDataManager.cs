using System.Collections.Generic;
using System.Linq;

public class DefaultMarketDataManager : iMarketDataManager,iMarketAware
{
    Market _market;
    readonly List<(Order Order, int Period)> OrdersSubmittedInPeriod = new(); // Read only used to get Order History. 
    private readonly List<Execution> _executedTradesInPeriod = new();
    private readonly List<(Order Order, int Period)> _ordersExecutedInPeriod = new();

    public List<Execution> GetExecutionsInPeriod(int period)
    {
        var executions = _ordersExecutedInPeriod
                          .Where(x => x.Period == period)
                          .SelectMany(x => x.Order.GetExecutions()).ToList();
        return executions;
    }
    public List<Order> GetOrdersSubmittedInPeriod(int period)
    {
        var ordersToReturn = OrdersSubmittedInPeriod
                           .Where(x => x.Period == period)
                           .Select(x => x.Order)
                           .ToList();
        return ordersToReturn;
    }
    public List<(Order Order, int Period)> GetOrdersExecutedInPeriod(params int[] periods)
        =>_ordersExecutedInPeriod.Where(x => periods.Contains(x.Period)).ToList();
    
    public List<MarketData> PublishMarketData()
    {
        //Modify here to reflect things like poor public information
        return _market.MarketData;
    }

    public void PublishSpreadToMarket(ActionContext context)
    {
        List<MarketData> Spread = context.MarketToSubmitTo.MarketData;
        var MarketToSubmitTo = context.MarketToSubmitTo;
        var good = context.GoodToSubmit;
        var bid = context.BidToSubmit;
        var ask = context.AskToSubmit;
        var submittingCompany = context.SubmittingCompany;

        var isGoodInMarketData = Spread.Any(x=>x.Good==good && x.Company.Equals(submittingCompany));
        if(isGoodInMarketData)
        {
            var marketData = Spread.First(x=>x.Good==good && x.Company.Equals(submittingCompany));
            marketData.Bid = bid;
            marketData.Ask = ask;
        }
        else
        {
            var data = new MarketData
            {
                Company = submittingCompany,
                Good = good,
                Bid = bid,
                Ask = ask
            };
            Spread.Add(data);
        }
    }

    public void SetMarket(Market market) => _market = market;

    public void LogOrder(Order order, int period)
    {
        if (!OrdersSubmittedInPeriod.Contains((order, period)))
        {
            OrdersSubmittedInPeriod.Add((order, period));
        }
    }
    public LemonadeStandResultObject RecordOrderInPeriod(Order order, int period)
    {
         if (!_ordersExecutedInPeriod.Contains((order, period)))
        {
            _ordersExecutedInPeriod.Add((order, period));
            return LemonadeStandResultObject.Success();
        }
        return LemonadeStandResultObject.Failure(ResultTypeEnum.DuplicateOrder, "Order already recorded");
    }

    public void RecordTrade(Execution trade)
    {
        if (!_executedTradesInPeriod.Contains(trade)) _executedTradesInPeriod.Add(trade);
    }
}
