using System.Collections.Generic;
using System.Linq;

public class BasicGrowthStrategy : iStrategy
{
    EconAgent Actor;
    //Manifestation of preferences
    decimal _aggressionLevel = 0.2m;
    public decimal GetAggressionLevel() => _aggressionLevel;

    public void GenerateGoals(iEconAgent company)
    {
        var goals = new List<Goal>();
        var doubleCashGoal = new Goal("Double Initial Cash",
                                      "Double the initial cash of the company",
                                      null,
                                      (c,g) => g.SetOriginalValue("InitialCash",c.GetCash())
                                      );
        doubleCashGoal.IsGoalMet = c=>c.GetCash() >= (decimal)doubleCashGoal.GetOriginalValue<decimal>("InitialCash")*2;
        company.Goals.Add(doubleCashGoal);

        var tenLemonadeGoal = new Goal("Have 10 Lemonade",
                                      "Have 10 Lemonade in stock",
                                      null,
                                      (c, g) => g.SetOriginalValue("InitialLemonade", 0)
                                      )
        {
            IsGoalMet = c => c.GetInventory().GetInventoryEntriesByGood("Lemonade").Sum(e => e.quantity) >= 10
        };
        company.Goals.Add(tenLemonadeGoal);
    }

    public void PerformStrategy(iEconAgent company)
    {
        throw new System.NotImplementedException();
    }

    public LemonadeStandResultObject SetAggressionLevel(decimal aggressionLevel)
    {
        _aggressionLevel = aggressionLevel;
        return LemonadeStandResultObject.Success();
    }

    public LemonadeStandResultObject PublishBidAskSpreadsToMarket(iEconAgent company)
    {
        throw new System.NotImplementedException();
    }

    public void SetEconAgent(EconAgent agent)
    {
        Actor = agent;
    }

    public List<ActionContext> GetQueuedActions()
    {
        throw new System.NotImplementedException();
    }

}
