using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Tests")]
public class ReduceEnnuiStrategy : iStrategy
{
    //Manifestation of preferences
    readonly Dictionary<Good,BidAskSpread> _bidAskSpreads = new();
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

    public Dictionary<Good, BidAskSpread> GetBidAskSpreads()
                        =>_bidAskSpreads;
    

    public Dictionary<Good,BidAskSpread> GenerateBidAskSpreads(iCompany company)
    {
        if (company is PopulationCompany populationCompany)
        {
            var demand = populationCompany.GetDemand()
                .Where(x=> x.Key.AffectsMetrics().Contains(MetricEnum.Ennui));
            var goal = populationCompany.Goals.Find(g => g.Name == "Reduce Ennui");

            foreach (var row in demand)
            {
                var spread = CalculateBidPerCapita(row.Key, populationCompany,goal);
                spread.Ask = 0m;//for now, the population is not selling anything
                AddOrReplaceBidAskSpread(row.Key,spread);
            }
            AllocateBudget(company);
        };
        return _bidAskSpreads;
    }

    private void AllocateBudget(iCompany company)
    {
        var totalCash = company.GetCash();
        var goodsToAllocateBudgetTo = _bidAskSpreads.ToList();

        var totalScore = goodsToAllocateBudgetTo.Sum(x => x.Value.GoodQuality);
        var allocations = goodsToAllocateBudgetTo
            .Select(x => {
                            var good = x.Key;
                            var proportion = x.Value.GoodQuality/totalScore;
                            var updatedSpread = 
                                    new BidAskSpread(
                                        bid: x.Value.Bid,
                                        goodQuality: x.Value.GoodQuality,
                                        ask: x.Value.Ask,
                                        allocation: (float)proportion);
                            return (Good: good, Spread: updatedSpread);
                         }
                ).ToList(); 

        foreach (var allocation in allocations)
        {
            _bidAskSpreads[allocation.Good] = allocation.Spread;
        }
    }
    internal BidAskSpread CalculateBidPerCapita(Good good, PopulationCompany populationCompany,Goal goal)
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

        var bid = Math.Clamp(willingnessToSpend * (decimal)goodQuality, 0.01m, availableCashPerCapita);

        return new BidAskSpread(bid: bid,goodQuality: goodQuality,ask: 0m,allocation: 0f);
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

    private void CreateBuys(Company company,int period)
    {
        foreach(var spread in _bidAskSpreads)
        {
            var good = spread.Key;
            var bid = spread.Value.Bid;
            var cash = company.GetCash();

            #region prioritize goods to bid for
            var quantity = (int)Math.Floor(cash * (decimal)spread.Value.Allocation);
            #endregion

            var order = new Order(company,null,good,quantity,bid);
        
            var context = new ActionContextBuilder()
                .WithAction(ActionEnum.QueueTradeBuy)
                .ForMarket(company.GetMarket())
                .ForPeriod(period)
                .WithTrade(order)
                .Build();
            company.QueueOrder(context);
        }
    }

    #region Interactions With Market

    public void AddOrReplaceBidAskSpread(Good good, BidAskSpread bidAskSpread)
    {
        _bidAskSpreads[good] = bidAskSpread;
    }
    public BidAskSpread? GetBidAskSpreadForGood(Good good)
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
