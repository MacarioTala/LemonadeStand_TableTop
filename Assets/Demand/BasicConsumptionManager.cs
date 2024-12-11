using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("Tests")]
public class BasicConsumptionManager : iConsumptionManager
{
    public void AdjustDemand(ActionContext context)
    {
        throw new System.NotImplementedException();
    }

    public void FulfillDemand(Market market)
    {
        var demand = market.GetMarketDemand();
        var ordersSentToMarket = market.GetOrdersSentToMarket()
                                       .Where(order=>order.Buyer is Market
                                       &&
                                        order.Buyer.Equals(market)
                                            ).ToList();

        foreach(var good in demand.Keys)
        {
            var remainingDemand = demand[good].CurrentDemand;
            
            var processedOrders = FillOrderBasedOnPrice(ordersSentToMarket,good,remainingDemand);
            
            foreach(var order in processedOrders)
            {
                order.Buyer = market;
                var context = new ActionContext
                {
                    TradeToSubmit = order,
                    MarketToSubmitTo = market,
                    Period = market.CurrentPeriod
                };
                market.ProcessOrder(context);
                remainingDemand -= order.Quantity;
            }
        }
    }

    internal List<Trade> FillOrderBasedOnPrice(List<Trade> trades, Good good, int remainingDemand)
    {
        var filledOrders = trades
                                        .Where(trade=>trade.Good.Equals(good))
                                        .OrderBy(trade=>trade.Price)
                                        .ToList();
        foreach(var order in filledOrders)
        {
            if (remainingDemand <= 0) break;
            
            switch (order.Quantity)
            {
                case var quantity when quantity > remainingDemand:
                    order.FilledQuantity = remainingDemand;
                    remainingDemand = 0;
                    break;
                case var quantity when quantity <= remainingDemand:                
                    order.FilledQuantity = order.Quantity;
                    remainingDemand -= order.Quantity;
                    break;
                default:
                    order.FilledQuantity += remainingDemand;
                    remainingDemand = 0;
                    break;
            }
        }
        return filledOrders.Where(trade=>trade.FilledQuantity > 0).ToList();
    } 
}
