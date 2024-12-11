using System.Collections.Generic;
using System.Linq;

public class RandomPrioritizer: iOrderPrioritizer
{
    public List<Order> Filter(List<Order> orders)
    {
        var relevantGood = orders.FirstOrDefault().Good;
        var random = new System.Random();
        return orders.OrderBy(x => random.Next()).ToList();
        throw new System.NotImplementedException();
    }
}