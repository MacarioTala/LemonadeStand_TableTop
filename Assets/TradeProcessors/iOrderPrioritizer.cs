using System.Collections.Generic;

public interface iOrderPrioritizer
{
    List<Trade> Filter(List<Trade> Orders);
}