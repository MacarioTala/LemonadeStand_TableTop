using System.Collections.Generic;

public interface iOrderPrioritizer
{
    List<Order> Filter(List<Order> Orders);
}