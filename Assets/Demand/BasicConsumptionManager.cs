using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("Tests")]
public class BasicConsumptionManager : iConsumptionManager
{
    public LemonadeStandResultObject FulfillDemand(Market market)
    {
        var demand = market.GetMarketDemand();
        var ordersSentToMarket = market.GetOrdersSentToMarket()
                                       .ToList();

        bool hasOrderFailures = false;

        foreach(var good in demand.Keys)
        {
            var remainingDemand = demand[good].CurrentDemand;
            
            var processedOrders = FillOrderBasedOnPrice(market,ordersSentToMarket,good,remainingDemand);

            if(processedOrders.Result!=LemonadeStandResultObject.Success().Result)
            {
                hasOrderFailures = true;
            }    
        }
        return hasOrderFailures?LemonadeStandResultObject.Failure(ResultTypeEnum.SomeOrdersNotProcessed,"See individual orders for details"):LemonadeStandResultObject.Success();
    }

    internal LemonadeStandResultObject FillOrderBasedOnPrice(
                                        Market market, 
                                        List<Order> trades, 
                                        Good good, 
                                        int remainingDemand)
    {
        var filledOrders = trades
                                        .Where(trade=>trade.Good.Equals(good))
                                        .OrderBy(trade=>trade.Price)
                                        .ToList();
        bool hasOrderFailures = false;
        foreach(var order in filledOrders)
        {
            if (remainingDemand <= 0) break;
            
            var orderStatus = order.IsOrderValid();
            if(orderStatus.Result!=LemonadeStandResultObject.Success().Result)
            {
                order.OrderStatus = orderStatus;
                hasOrderFailures = true;
            }
            remainingDemand = CalculateFilledQuantity(market, order, remainingDemand);
        }
        if (hasOrderFailures)
        {
            return LemonadeStandResultObject.Failure(ResultTypeEnum.SomeOrdersNotProcessed,"See individual orders for details");
        }
        var returnObject = LemonadeStandResultObject.Success(filledOrders.Where(trade=>trade.FilledQuantity > 0).ToList());
        
        return returnObject;
    }

    private int CalculateFilledQuantity(Market market, Order order, int remainingDemand)
    {
        order.Buyer = market;
        int demandToReturn;
        decimal cashToRemoveFromMarket;
        var marketInventory = market.GetInventory();
        int quantityToFill;
        var marketCounterPartyOrder = new Order(buyer: market
                                                     , seller: order.Seller
                                                     , good: order.Good
                                                     , price: order.Price
                                                     , quantity: 0)
                {
                    SubmittingCompany = market
                };
        

        if (order.RemainingQuantity >= remainingDemand)
        {
            cashToRemoveFromMarket = remainingDemand * order.Price;
            quantityToFill = remainingDemand;
            order.FilledQuantity += quantityToFill;
            marketCounterPartyOrder.Quantity = quantityToFill;
            marketCounterPartyOrder.FilledQuantity = quantityToFill;
            demandToReturn = 0;
            marketInventory.AddGood(new InventoryEntry(order.Good, quantityToFill, order.Price, market.CurrentPeriod));
        }
        else
            {
                quantityToFill = order.RemainingQuantity;//need this because order.RemainingQuantity will be updated in the next line

                cashToRemoveFromMarket = quantityToFill * order.Price;
                order.FilledQuantity += quantityToFill;
                marketCounterPartyOrder.Quantity = quantityToFill;
                marketCounterPartyOrder.FilledQuantity = quantityToFill;
                demandToReturn = remainingDemand - quantityToFill;
                marketInventory.AddGood(new InventoryEntry(order.Good, quantityToFill, order.Price, market.CurrentPeriod));
            }

        market.SetCash(market.GetCash() - cashToRemoveFromMarket);
        order.OrderStatus = LemonadeStandResultObject.Success();
        marketCounterPartyOrder.OrderStatus = LemonadeStandResultObject.Success();

        var marketOrderContext = new ActionContext
                {
                    PrimaryOrder = order,
                    TradeToSubmit = order,
                    MarketToSubmitTo = market,
                    Period = market.CurrentPeriod,
                    CounterPartyOrders = new List<Order>{marketCounterPartyOrder},
                };
        
        market.ProcessMarketOrder(marketOrderContext);

        market.RaiseOrderFulfilledEvent(
            new OrderFulfilledEvent
            {
                Good = order.Good,
                FulfilledQuantity = quantityToFill,
                OriginalQuantity = order.Quantity,
                FillPrice = order.Price,
                OrderMarket = market,
                Period = market.CurrentPeriod,
                PrimaryOrder = order,
                CounterPartyOrders = new List<Order>{marketCounterPartyOrder}
            }
        );
    
        return demandToReturn;
    }
}
