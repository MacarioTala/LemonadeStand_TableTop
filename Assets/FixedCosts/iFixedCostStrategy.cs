using System.Collections.Generic;

public interface iFixedCostStrategy
{
    int CalculateFixedCosts(List<FixedCostInstance> fixedCosts, int period);
    void SetEconAgent(EconAgent agent);
}