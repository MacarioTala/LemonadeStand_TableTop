using System.Collections.Generic;

public interface iFixedCostStrategy
{
    decimal CalculateFixedCosts(List<FixedCostInstance> fixedCosts, int period);
    void SetEconAgent(EconAgent agent);
}