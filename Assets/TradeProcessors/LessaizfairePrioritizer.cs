using System.Collections.Generic;
using System.Linq;

public class LessaizfairePrioritizer : iOrderPrioritizer
{
    public List<Order> Filter(List<Order> Orders)
    {
        return Orders.OrderByDescending(x=>IsBuyer(x))
                     .ThenByDescending(x=>x.Quantity)
                     .ThenByDescending(x=>x.Price, Comparer<decimal>.Default)
                     .ThenBy(x=>IsSeller(x))
                     .ThenBy(x=>x.Price)
                     .ThenBy(x=>x.Id)
                     .ToList();
    }

    private bool IsBuyer(Order order)
    {
        return order.Buyer is not null;
    }
    private bool IsSeller(Order order)
    {
        return order.Seller is not null;
    }
}