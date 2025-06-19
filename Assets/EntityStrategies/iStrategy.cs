using System.Collections.Generic;
using System.Linq;

public interface iStrategy
{
    void GenerateGoals(iCompany company);
    float GetAggressionLevel();
    LemonadeStandResultObject SetAggressionLevel(float aggressionLevel);
    public void PerformStrategy(ActionContext context);
    public void PerformStrategy(iCompany company);

    #region Default implementations
    /// <summary>
    /// The intention here is to have these be mostly static methods
    /// that can be used by any strategy.
    /// </summary>

    public static decimal CalculateInitialBid(Good good, Company company, decimal multiplier = 1m)
    {
        var market = company.GetMarket();
        var perceivedCost = market.GetPerceivedCostOfGood(good);
        var initialBid = perceivedCost * multiplier;
        return initialBid;
    }
    public static int GetQuantityDemandedAtState(Good good, Company company, Dictionary<ElasticDemandComponentEnum, float> stateChanges)
    {
        var demand = company.GetDemandFor(good);

        if (demand == null || demand.MaxDemand <= 0)
        {
            return 0; // No demand for this good
        }
        // If no other demand component evaluates
        // to other than 0, then we return the minimum demand
        var demandToReturn = demand.CurrentDemand>0?demand.CurrentDemand:demand.MinDemand;

        // var currentStateChanges = company.GetStateChanges();
        // if (currentStateChanges == null || !currentStateChanges.Any())
        // {
        //     return demandToReturn; // No state changes, return current demand
        // }
        // else
        // { 
        //     // Calculate total demand adjustment based on state changes
        //     var totalAdjustment = demand.GetTotalDemandAdjustment(stateChanges);

        //     // Apply the adjustment to the current demand
        //     demandToReturn += totalAdjustment;

            
        // }

        return demandToReturn;
    }
    #endregion
}
    