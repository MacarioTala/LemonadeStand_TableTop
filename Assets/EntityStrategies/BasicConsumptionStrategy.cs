public class BasicConsumptionStrategy : iStrategy
{
    EconAgent Actor;
    public void GenerateGoals(iEconAgent company)
    {
        throw new System.NotImplementedException();
    }

    public decimal GetAggressionLevel()
    {
        throw new System.NotImplementedException();
    }

    public void PerformStrategy(ActionContext context)
    {
        throw new System.NotImplementedException();
    }

    public void PerformStrategy(iEconAgent company)
    {
        throw new System.NotImplementedException();
    }

    public LemonadeStandResultObject SetAggressionLevel(decimal aggressionLevel)
    {
        throw new System.NotImplementedException();
    }

    public void SetEconAgent(EconAgent agent)
    {
        Actor = agent;
    }
    #region Strategy guts
    private void CreateBuys()
    {
        var demand = Actor.GetDemand();    
    }

    #endregion
}