using System.Collections.Generic;

public interface iStrategy
{
    List<ActionContext> GenerateActionContexts(iCompany company);
    List<Goal> GenerateGoals(iCompany company);

    public void PerformStrategy(ActionContext context);
}