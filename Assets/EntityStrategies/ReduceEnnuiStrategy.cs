using System.Collections.Generic;

public class ReduceEnnuiStrategy : iStrategy
{
    public List<ActionContext> GenerateActionContexts(iCompany company)
    {
        throw new System.NotImplementedException();
    }

    public void GenerateGoals(iCompany company)
    {
        var ennuiGoal = new Goal()
                .Named("Reduce Ennui")
                .DescribedAs("Reduce the ennui of the population to 0")
                .WithGoalEvaluator(c => c is PopulationCompany populationCompany && populationCompany.Ennui == 0)
                .WithGoalInitializer((c, g) => g.SetOriginalValue("Ennui", ((PopulationCompany)c).Ennui));
        
        company.Goals.Add(ennuiGoal);

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
        throw new System.NotImplementedException("Waiting for goods to have effects on ennui");
    }

    public void PerformStrategy(ActionContext context)
    {
        throw new System.NotImplementedException();
    }

}