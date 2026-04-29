using System;
using System.Collections.Generic;

public class BasicFixedCostStrategy : iFixedCostStrategy
{
    EconAgent econAgent;
    public decimal CalculateFixedCosts(List<FixedCostInstance> fixedCosts, int period)
    {
       decimal total = 0;
        if(fixedCosts == null|| fixedCosts.Count == 0)
        {
            return total;
        }
        foreach (var cost in fixedCosts)
        {
             if(cost.Template.Frequency <1)
            {
                throw new ArgumentException("Frequency of fixed cost must be greater than 0");
            }
            var periodsCostHasBeenActive = period - cost.PeriodAcquired;
            if ((periodsCostHasBeenActive%cost.Template.Frequency == 0) && period > cost.PeriodAcquired)
            {
                total += cost.Template.Amount;
                econAgent.LogFixedCostPayment(cost,period);
            }
        }
        return total;
    }

    public void SetEconAgent(EconAgent agent)
    {
        econAgent = agent;
    }
}