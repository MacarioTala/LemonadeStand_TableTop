using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("Tests")]
public class ReduceEnnuiStrategy : iStrategy
{
    //Manifestation of preferences
    readonly Dictionary<Good,(decimal bid, decimal ask)> _bidAskSpreads = new();
    float _aggressionLevel = 0.2f;
    public float GetAggressionLevel() => _aggressionLevel;
    public LemonadeStandResultObject SetAggressionLevel(float aggressionLevel)
    {
        if (aggressionLevel < 0 || aggressionLevel > 1)
            return LemonadeStandResultObject.Failure("Aggression level must be between 0 and 1.");
        _aggressionLevel = aggressionLevel;
        return LemonadeStandResultObject.Success();
    }

    public List<ActionContext> GenerateActionContexts(iCompany company)
    {
        throw new NotImplementedException();
    }

    public Dictionary<Good, (decimal bid, decimal ask)> GetBidAskSpreads()
                        =>_bidAskSpreads;

    public void GenerateBidAskSpreads(iCompany company)
    {
        if (company is PopulationCompany populationCompany)
        {
            var demand = populationCompany.GetDemand();
            var goal = populationCompany.Goals.Find(g => g.Name == "Reduce Ennui");

            foreach (var good in demand.Keys)
            {
                if(good.AffectsMetrics().Contains(MetricEnum.Ennui))
                {
                    var bid = CalculateBidPerCapita(good, populationCompany,goal);
                    // var ask = CalculateAsk(demandData, populationCompany);
                    // AddOrReplaceBidAskSpread(good, bid, ask);
                }
            }
        };
    }
    internal decimal CalculateBidPerCapita(Good good, PopulationCompany populationCompany,Goal goal)
    {
        //Ability
        var availableCashPerCapita = populationCompany.GetCash()/100;
        var willingnessToSpend = availableCashPerCapita * (decimal)_aggressionLevel;

        //Desire
        var ennui = populationCompany.Ennui;
        var targetEnnui = goal.MetricTarget;
        
        var impactOnEnnui = Math.Abs(good.ReducesMetric(MetricEnum.Ennui).By());
        var ennuiReductionRate = ennui*impactOnEnnui;
        var turnsToFinal = MathHelper.EstimatePeriodsToFinal(ennui, targetEnnui, ennuiReductionRate);
        var goodQuality = 1f/(1f + turnsToFinal);

        var bid = willingnessToSpend * (decimal)goodQuality;

        return Math.Clamp(bid, 0.01m, availableCashPerCapita);
    }

    public void GenerateGoals(iCompany company)
    {
        var ennuiGoal = new Goal()
                .Named("Reduce Ennui")
                .DescribedAs("Reduce the ennui of the population to 0")
                .WithGoalEvaluator(c => c is PopulationCompany populationCompany && populationCompany.Ennui == 0)
                .WithGoalInitializer((c, g) => g.SetOriginalValue("Ennui", ((PopulationCompany)c).Ennui))
                .Affecting(MetricEnum.Ennui)
                .WithGoalValue(0f);
        
        company.Goals.Add(ennuiGoal);
    }

    public void AddGoal(Goal goal, iCompany company)
    {
        if (company.Goals.Contains(goal))
        {
            company.Goals.Remove(goal);
        }
        company.Goals.Add(goal);
    }
    public void RemoveGoals(string goalName, iCompany company)
    {
        var goal = company.Goals.Find(g => g.Name == goalName);
        if (goal != null)
        {
            company.Goals.Remove(goal);
        }
    }

    private static bool GoalNotMet(Goal goal, iCompany company) =>
    !(goal?.IsGoalMet(company) ?? false);

    public void PerformStrategy(iCompany company)
    {
        var ennuiGoal = company.Goals.Find(g => g.Name == "Reduce Ennui");
        if(GoalNotMet(ennuiGoal, company))
        {
          PerformStrategicActions(company);   
        }
    }

    private void PerformStrategicActions(iCompany company)
    {
        throw new NotImplementedException("Waiting for goods to have effects on ennui");
    }

    #region Interactions With Market

    public void AddOrReplaceBidAskSpread(Good good, decimal bid, decimal ask)
    {
        _bidAskSpreads[good] = (bid, ask);
    }
    public (decimal bid, decimal ask)? GetBidAskSpreadForGood(Good good)
    {
        if (_bidAskSpreads.TryGetValue(good, out var spread))
        {
            return spread;
        }
        return null;
    }

    public LemonadeStandResultObject CreateOrders(Market market)
    {
        var goodsAvailable = market.GetInventory().GetInventoryEntries();

        foreach (var inventoryEntry in goodsAvailable)
        {
            
        }
        return LemonadeStandResultObject.Failure("Not implemented");
    }
#endregion

    public void PerformStrategy(ActionContext context)
    {
        throw new NotImplementedException();
    }
}

public class StrategyBuilder<T> where T : iStrategy, new()
{
    private T _strategyToReturn;

    public StrategyBuilder(T strategy) =>
        _strategyToReturn = strategy;
    
    public StrategyBuilder()
    {
        _strategyToReturn = new T();
    }
    public T Build()=> _strategyToReturn;
    
    public StrategyBuilder<T> WithAggressionLevel(float aggressionLevel)
    {
        _strategyToReturn.SetAggressionLevel(aggressionLevel);
        return this;
    }
}
public static class StrategyBuilder
{
    public static StrategyBuilder<T> For <T>() where T : iStrategy, new()
    {
        return new StrategyBuilder<T>();
    }
}