using System;
using System.Collections.Generic;
using System.Linq;

public class BasicTradeProcessor : iTradeProcessor
{
     private List<iOrderPrioritizer> _orderPrioritizers = new();
     private readonly List<Order> _tradesSentToTheMarket = new();
     public List<Order> TradesToSendToTheEconomy = new();

     private readonly iTransactionManager _transactionManager;


    public BasicTradeProcessor()
    {
        _orderPrioritizers.Add(new LowPricePrioritizer());
        //_orderPrioritizers.Add(new RandomPrioritizer());
        _transactionManager = new BasicTransactionManager();
    }
    public List<Order> GetOrders()=>_tradesSentToTheMarket;
    public List<Order> GetOrderResults(ActionContext context)
    {
        return _tradesSentToTheMarket
                .Where(x => x.Buyer == context.TradeToSubmit.Buyer || x.Seller == context.TradeToSubmit.Seller)
                .ToList();
    }

    private bool CounterPartyFoundForOrder (Order order, Market market)
    {
        throw new NotImplementedException();
    }

    public List<Order> ProcessCompanyOrders(Market market)
    {
        List<Order> executedTrades = new();
        List<Good> goodsToTradeThisPeriod = market.GetOrdersSentToMarket()
                                                  .Where(x => x.Buyer is not Market)
                                                  .Select(x => x.Good).Distinct().ToList();

        foreach (var good in goodsToTradeThisPeriod)
        {
           executedTrades.AddRange(ExecuteBestTradesForGood(good,market,market.GetOrdersSentToMarket().Where(x => x.Buyer is not Market).ToList()));
        }
        _tradesSentToTheMarket.RemoveAll(x=>x.Buyer is not Market);
        return executedTrades;
    }

    internal List<Order> ExecuteBestTradesForGood(Good good, Market market, List<Order> OrdersSentToMarket)
    {
        List<Order> executedTrades = new();
        var relevantOrders = OrdersSentToMarket.Where(x => x.Good == good
                                               && x.Buyer is not Market
                                               )
                                               .ToList();
        foreach (var prioritizer in _orderPrioritizers)
        {
            var prioritizedOrders = prioritizer.Filter(relevantOrders);
            
            foreach (var order in prioritizedOrders)
            {
                if (!CounterPartyFoundForOrder(order, market))
                {
                    order.OrderStatus = LemonadeStandResultObject.Failure(ResultTypeEnum.NoMatchingCounterParties, "No matching counterparty found");
                }
                else
                {
                    _transactionManager.ProcessTransaction(new ActionContext
                        {
                            TradeToSubmit = order,
                            MarketToSubmitTo = market,
                            Period = market.CurrentPeriod
                        });
                        executedTrades.Add(order);
                        relevantOrders.Remove(order);
                }
            }
        }
        return executedTrades;
    }

    internal Order GeneratePrimaryOrder(Market market)
    {
        var buyOrders = market.GetOrdersSentToMarket()
                            .Where(static x => (x.Buyer??default) == x.SubmittingCompany)
                            .OrderByDescending(x=>x.Quantity)
                            .ThenByDescending(x=>x.Price)
                            .ToList();
        
        if ( buyOrders.Any() ) return buyOrders.First();

        var sellOrders = market.GetOrdersSentToMarket()
                            .Where(static x => (x.Seller??default) == x.SubmittingCompany)
                            .OrderByDescending(x=>x.Quantity)
                            .ThenBy(x=>x.Price)
                            .ToList();
        return sellOrders.Any() ? sellOrders.First() : null;
    }

    private bool SellOrdersExistInMarket()
    {
        var sellOrdersExist = _tradesSentToTheMarket.Any(x => x.Seller!=null);
        return sellOrdersExist;
    }
    private bool BuyOrdersExistInMarket()
    {
        var buyOrdersExist = _tradesSentToTheMarket.Any(x => x.Buyer!=null);
        return buyOrdersExist;
    }

    public LemonadeStandResultObject FindCounterPartiesForOrder(ActionContext context)
    {
        var primaryOrder = GeneratePrimaryOrder(context.MarketToSubmitTo);
        var counterPartiesForOrder = context.MarketToSubmitTo.GetOrdersSentToMarket()
                                    .Where (x=>IsValidCounterParty(x, primaryOrder))
                                    .ToList();

        if (!counterPartiesForOrder.Any()) 
           return LemonadeStandResultObject.Failure(
                            ResultTypeEnum.NoMatchingCounterParties
                            , "No matching counterparties found");
                                    
        return LemonadeStandResultObject.Success(counterPartiesForOrder);
    }

    internal bool IsValidCounterParty(Order order, Order primaryOrder)
    {
        //Goods must match
        if(order.Good != primaryOrder.Good) return false;
        //The submitting company cannot be the counterparty
        if(order.SubmittingCompany == primaryOrder.SubmittingCompany) return false;
        
        //Readability helpers for final condition
        var orderIsASellPrimaryIsBuy=order.Seller is not null
                                    &&
                                    primaryOrder.Buyer is not null;
        var orderIsABuyPrimaryIsSell=order.Buyer is not null
                                    &&
                                    primaryOrder.Seller is not null;
        var orderIsASellPrimaryIsSell=order.Seller is not null
                                    || primaryOrder.Seller is not null;
        var orderIsABuyPrimaryIsBuy=order.Buyer is not null
                                    || primaryOrder.Buyer is not null;
        var buyOrdersExistInMarket = BuyOrdersExistInMarket();
        var sellOrdersExistInMarket = SellOrdersExistInMarket();
        
        //If only buys or sells exist, there is no counterparty
        if(orderIsASellPrimaryIsSell && !buyOrdersExistInMarket) return false;
        if(orderIsABuyPrimaryIsBuy && !sellOrdersExistInMarket) return false;

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
    public LemonadeStandResultObject QueueOrder(ActionContext context)
    {
        if(_tradesSentToTheMarket.Contains(context.TradeToSubmit))
            {
                return LemonadeStandResultObject.Failure(ResultTypeEnum.DuplicateOrder, "Order already exists in the queue");
            }
        _tradesSentToTheMarket.Add(context.TradeToSubmit);
        return LemonadeStandResultObject.Success();
    }
}