using System.Collections.Generic;
using System.Linq;

public class BasicTradeProcessor : iTradeProcessor
{
     private List<iOrderPrioritizer> _orderPrioritizers = new();
     private readonly List<Trade> _tradesSentToTheMarket = new();
     public List<Trade> TradesToSendToTheEconomy = new();

     private readonly iTransactionManager _transactionManager;


    public BasicTradeProcessor()
    {
        _orderPrioritizers.Add(new LowPricePrioritizer());
        //_orderPrioritizers.Add(new RandomPrioritizer());
        _transactionManager = new BasicTransactionManager();
    }
    public List<Trade> GetOrders()=>_tradesSentToTheMarket;
    public List<Trade> GetOrderResults(ActionContext context)
    {
        return _tradesSentToTheMarket
                .Where(x => x.Buyer == context.Buyer || x.Seller == context.Seller)
                .ToList();
    }

    public List<Trade> ProcessCompanyOrders(Market market)
    {
        List<Trade> executedTrades = new();
        List<Good> goodsToTradeThisPeriod = market.GetOrdersSentToMarket()
                                                  .Where(x => x.Buyer is not Market)
                                                  .Select(x => x.Good).Distinct().ToList();

        foreach (var good in goodsToTradeThisPeriod)
        {
           executedTrades.AddRange(ExecuteBestTradesForGood(good,market.GetOrdersSentToMarket()));
        }

        return executedTrades;
    }

    internal List<Trade> ExecuteBestTradesForGood(Good good, List<Trade> OrdersSentToMarket)
    {
        List<Trade> executedTrades = new();
        var relevantOrders = OrdersSentToMarket.Where(x => x.Good == good
                                               && x.Buyer is not Market
                                               )
                                               .ToList();
        foreach (var prioritizer in _orderPrioritizers)
        {
            var prioritizedOrders = prioritizer.Filter(relevantOrders);
            
            foreach (var order in prioritizedOrders)
            {
                try
                {
                    _transactionManager.ProcessTransaction(new ActionContext
                    {
                        TradeToSubmit = order
                    });
                    executedTrades.Add(order);
                    relevantOrders.Remove(order);
                }
                catch (Company_InsufficientFundsException)
                {
                    throw;
                }
                catch (Company_InventoryException)
                {
                    throw;
                }

            }
        }
        return executedTrades;
    }

    public void QueueOrder(ActionContext context)
    {
        _tradesSentToTheMarket.Add(context.TradeToSubmit);
    }
}