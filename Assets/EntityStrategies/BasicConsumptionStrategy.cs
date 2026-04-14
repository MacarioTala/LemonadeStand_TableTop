using System.Collections.Generic;

public class BasicConsumptionStrategy : iStrategy
{
    EconAgent Actor;
    List<ActionContext> actions=new();

    public List<ActionContext> GetQueuedActions()=> actions;
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
        actions.Clear();
        CreateBuys();

        foreach(var action in actions)
        {
            Actor.QueueOrder(action);
        }
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
        var demandDictionary = Actor.GetDemand(); 
        foreach(var kvp in demandDictionary)
        {
            var good = kvp.Key;
            var requisiteDemand = kvp.Value.GetRequisiteDemand();
            //TODO:Implement discretionary demand
            
            if(requisiteDemand>0)
            {
                var marketOrder = new Order(Actor,null,good,requisiteDemand,Actor.GetPerceivedCostOfGood(good));
                var marketOrderContext = new ActionContextBuilder()
                                        .WithAction(ActionEnum.QueueTradeBuy)
                                        .WithTrade(marketOrder)
                                        .Build();
                actions.Add(marketOrderContext);
            }
        }
    }

    #endregion
}