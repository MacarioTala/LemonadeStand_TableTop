using System.Collections.Generic;
using System.Linq;

public class LowPricePrioritizer : iOrderPrioritizer
{
    public List<Order> Filter(List<Order> Orders)
    {
        return Orders.OrderBy(x => x.Price).ToList();
    }
}
