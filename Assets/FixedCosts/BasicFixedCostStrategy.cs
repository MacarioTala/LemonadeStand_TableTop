using System;
using System.Collections.Generic;

public class BasicFixedCostStrategy : iFixedCostStrategy
{
    public decimal CalculateFixedCosts(List<FixedCost> fixedCosts, int period)
    {
       decimal total = 0;
        if(fixedCosts == null|| fixedCosts.Count == 0)
        {
            return total;
        }
        foreach (var cost in fixedCosts)
        {
            if(cost.Frequency <1)
            {
                throw new ArgumentException("Frequency of fixed cost must be greater than 0");
            }
            if (((period - cost.PeriodAcquired)%cost.Frequency == 0) && period > cost.PeriodAcquired)
            {
                total += cost.Amount;
            }
        }
        return total;
    }
}