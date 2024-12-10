using System.Collections.Generic;
using System.Linq;

public class LowPricePrioritizer : iOrderPrioritizer
{
    public List<Trade> Filter(List<Trade> Orders)
    {
        return Orders.OrderBy(x => x.Price).ToList();
    }
}
