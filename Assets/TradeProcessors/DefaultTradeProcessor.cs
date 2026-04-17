using System.Collections.Generic;
using System.Linq;
using static HistoricalRecordHelper;

public class DefaultTradeProcessor : iTradeProcessor,iMarketAware
{
    private Market _market;
     private List<iOrderPrioritizer> _orderPrioritizers = new();
     private readonly List<Order> _tradesSentToTheMarket = new();

     private iTransactionManager _transactionManager;


    public DefaultTradeProcessor(iTransactionManager manager=null)
    {   
        _orderPrioritizers.Add(new LessaizfairePrioritizer());
        _transactionManager = manager;
    }
    public List<Order> GetOrders()=>_tradesSentToTheMarket;
    private bool BuyOrdersExistInMarket() => _tradesSentToTheMarket.Any(x => x.Buyer!=null);
    private bool SellOrdersExistInMarket() =>_tradesSentToTheMarket.Any(x => x.Seller!=null);

    public List<Order> GetOrderResults(ActionContext context)
    {
        return _tradesSentToTheMarket
                .Where(x => x.Buyer == context.TradeToSubmit.Buyer || x.Seller == context.TradeToSubmit.Seller)
                .ToList();
    }
    public List<Order> ProcessCompanyOrders()
    {
        List<Order> executedTrades = new();
        var ordersInPeriod = _market.GetOrdersSentToMarket();
        List<Good> goodsToTradeThisPeriod = ordersInPeriod
                                            .Select(x => x.Good).Distinct().ToList();

        foreach (var good in goodsToTradeThisPeriod)
        {
           executedTrades.AddRange(ExecuteBestTradesForGood(good,ordersInPeriod));
        }
        _tradesSentToTheMarket.RemoveAll(x=>x.IsFullyFilled);
        return executedTrades;
    }

    internal List<Order> ExecuteBestTradesForGood(Good good, List<Order> OrdersSentToMarket)
    {
        List<Order> executedOrders = new();
        
        var relevantOrders = OrdersSentToMarket.Where(x => x.Good.Equals(good)
                                               )
                                               .ToList();
        foreach (var prioritizer in _orderPrioritizers)
        {
            var prioritizedOrders = prioritizer.Filter(relevantOrders);
            
            while(prioritizedOrders.Any())
            {
                var order = prioritizedOrders.FirstOrDefault(x=> !x.IsFullyFilled);
                if(order is null)
                    break;

                var record = CreateLocalHistoricalRecord(_market,order);

                if(HandleSingleOrder(prioritizedOrders,order,record))
                    break;
                
                if (!TryGetCounterPartiesForOrder(_market,order, out var counterPartyOrders))
                {
                    HandleNoCounterParty(order,record);
                    prioritizedOrders.Remove(order);
                    continue;
                }

                HandleTransaction(order, counterPartyOrders,executedOrders,prioritizedOrders);
            }
        }
        return executedOrders;
    }
    void HandleTransaction(
        Order order, 
        List<Order> counterPartyOrders, 
        List<Order> executedOrders, 
        List<Order> prioritizedOrders)
        //ref int i)
    {
        var transactionResult = _transactionManager
                    .ProcessTransaction(
                                        new ActionContext
                                        {
                                            PrimaryOrder = order,
                                            CounterPartyOrders = counterPartyOrders,
                                            MarketToSubmitTo = _market,
                                            Period = _market.CurrentPeriod
                                        }
                                        );     
        if(!transactionResult.Result.Equals(LemonadeStandResultObject.Success().Result))
        {
            order.OrderStatus = transactionResult;
            prioritizedOrders.Remove(order);
            return;
        }

        executedOrders.Add(order);
        prioritizedOrders.RemoveAll(x=>x.IsFullyFilled);

    }

    public LemonadeStandResultObject QueueOrder(ActionContext context)
    {
        var record = CreateLocalHistoricalRecord(_market,context.TradeToSubmit);

        if (_tradesSentToTheMarket.Contains(context.TradeToSubmit))
        {
            var message = "Order already exists in the queue";
            record.Message = message;
            record.Result = OrderResultEnum.Rejected;
            _market.LogHistoricalRecord(record);
            return LemonadeStandResultObject.Failure(ResultTypeEnum.DuplicateOrder, message);
        }
        _tradesSentToTheMarket.Add(context.TradeToSubmit);
        record.Result = OrderResultEnum.Submitted;
        _market.LogHistoricalRecord(record);
        return LemonadeStandResultObject.Success();
    }

    public void SetMarket(Market market)
    {
        _market = market;
        if (market.TransactionManager is not null) _transactionManager = market.TransactionManager;
    }

    #region Helpers
   
    internal Order GeneratePrimaryOrder(Good good)
    {
        var buyOrders = _market.GetOrdersSentToMarket()
                            .Where(x => (x.Buyer??default) == x.SubmittingCompany
                            &&
                            !x.IsFullyFilled
                            && x.Good.Equals(good) 
                            )
                            .OrderByDescending(x=>x.Quantity)
                            .ThenByDescending(x=>x.Price)
                            .ToList();
        
        if ( buyOrders.Any() ) return buyOrders.First();

        var sellOrders = _market.GetOrdersSentToMarket()
                            .Where(x => (x.Seller??default) == x.SubmittingCompany
                            &&
                            !x.IsFullyFilled
                            && x.Good.Equals(good)
                            )
                            .OrderByDescending(x=>x.Quantity)
                            .ThenBy(x=>x.Price)
                            .ToList();
        return sellOrders.Any() ? sellOrders.First() : null;
    }
    bool HandleSingleOrder(List<Order> prioritizedOrders,Order order, HistoricalRecord record)
    {
        if(prioritizedOrders.Count != 1)
        {
            return false;
        }
        JournalRejectEntry(_market,order,record,ResultTypeEnum.NoMatchingCounterParties.ToString(),ResultTypeEnum.NoMatchingCounterParties);
        return true;
    }
    void HandleNoCounterParty(Order order,HistoricalRecord record)
    {
        JournalRejectEntry(_market,order,record,ResultTypeEnum.NoMatchingCounterParties.ToString(),ResultTypeEnum.NoMatchingCounterParties);
    }
    
     internal bool IsValidCounterParty(Order order, Order primaryOrder)
    {
        //Goods must match
        if(!order.Good.Equals(primaryOrder.Good)) return false;

        //Order cannot be fully filled
        if(order.IsFullyFilled) return false;
        
        //The submitting company cannot be the counterparty
        if(order.SubmittingCompany == primaryOrder.SubmittingCompany) return false;
        
        //Readability helpers for final condition
        var orderIsASellPrimaryIsBuy=order.Seller is not null
                                    &&
                                    primaryOrder.Buyer is not null;
        var orderIsABuyPrimaryIsSell=order.Buyer is not null
                                    &&
                                    primaryOrder.Seller is not null;
        var orderIsASellOrPrimaryIsSell=order.Seller is not null
                                    || primaryOrder.Seller is not null;
        var orderIsABuyOrPrimaryIsBuy=order.Buyer is not null
                                    || primaryOrder.Buyer is not null;
        var buyOrdersExistInMarket = BuyOrdersExistInMarket();
        var sellOrdersExistInMarket = SellOrdersExistInMarket();
        
        //If only buys or sells exist, there is no counterparty
        if(orderIsASellOrPrimaryIsSell && !buyOrdersExistInMarket) return false;
        if(orderIsABuyOrPrimaryIsBuy && !sellOrdersExistInMarket) return false;

        //Valid counterparty for buy order is a sell order
        //in a market where buy orders exist
        var isCounterPartyForBuy =  orderIsASellPrimaryIsBuy 
                                    && buyOrdersExistInMarket;
                                
        //Valid counterparty for sell order is a buy order
        //in a market where sell orders exist
        var isCounterPartyForSell = orderIsABuyPrimaryIsSell 
                                    && sellOrdersExistInMarket;

        return isCounterPartyForBuy || isCounterPartyForSell;
    }

    internal bool TryGetCounterPartiesForOrder(Market market,Order primaryOrder, out List<Order> counterPartyOrders)
    {
        counterPartyOrders = market.GetOrdersSentToMarket()
                                    .Where (x=>IsValidCounterParty(x, primaryOrder)
                                    && x.Good.Equals(primaryOrder.Good))
                                    .OrderBy(x=>x.Buyer != null? -x.Price:x.Price)
                                    .ToList();

        return counterPartyOrders.Any();
    }
    bool TryRemoveIfFullyFilled(List<Order> prioritizedOrders,Order order, ref int i)
    {
        if(order.IsFullyFilled)
            {
                prioritizedOrders.RemoveAt(i);
                i--;
                return true;
            }
        return false; 
    }
    #endregion
}