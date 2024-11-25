using System.Collections.Generic;

public interface iFixedCostStrategy
{
    decimal CalculateFixedCosts(List<FixedCost> fixedCosts, int period);
}