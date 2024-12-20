using System.Collections.Generic;
using System.Linq;

public class LessaizfairePrioritizer : iOrderPrioritizer
{
    public List<Order> Filter(List<Order> Orders)
    {
        return Orders.OrderByDescending(x=>IsBuyer(x))
                     .ThenByDescending(x => IsBuyer(x) ? x.Quantity : int.MinValue) // Buyers: High Quantity
                     .ThenByDescending(x => IsBuyer(x) ? x.Price : decimal.MaxValue) // Buyers: High Price
                     .ThenBy(x => IsSeller(x) ? x.Price : decimal.MinValue) // Sellers: Low Price
                     .ThenByDescending(x => IsSeller(x) ? x.Quantity : int.MinValue) // Sellers: High Quantity
                     .ThenBy(x => x.Id) // Tie-breaker
                     .ToList();
    }

    private bool IsBuyer(Order order)
    {
        return order.Buyer is not null && order.SubmittingCompany.Equals(order.Buyer);
    }
    private bool IsSeller(Order order)
    {
        return order.Seller is not null && order.SubmittingCompany.Equals(order.Seller);
    }
}