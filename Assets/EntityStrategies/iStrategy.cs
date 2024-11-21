using System.Collections.Generic;

public interface iStrategy
{
    List<ActionContext> GenerateActionContexts(iCompany company);
    void GenerateGoals(iCompany company);

    public void PerformStrategy(ActionContext context);
}