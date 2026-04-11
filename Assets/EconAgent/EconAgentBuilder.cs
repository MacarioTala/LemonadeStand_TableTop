using System;
using System.Collections.Generic;
using UnityEngine;

public class EconAgentBuilder<T> where T : EconAgent
{
    private readonly T agentToReturn;

    public EconAgentBuilder(T agent) => agentToReturn = agent;
    public static EconAgentBuilder<T> Create() => new(ScriptableObject.CreateInstance<T>());
    public T Build() => agentToReturn;

    public EconAgentBuilder<T> WithFixedCostStrategy(iFixedCostStrategy fixedCostStrategy)
    {
        agentToReturn.FixedCostStrategy = fixedCostStrategy;
        agentToReturn.FixedCostStrategy.SetEconAgent(agentToReturn);
        return this;
    }
    public EconAgentBuilder<T> WithBehaviourStrategy(iStrategy behaviourStrategy)
    {
        agentToReturn.SetStrategy(behaviourStrategy);
        return this;
    }
    public EconAgentBuilder<T> WithInitialCash(decimal initialCash)
    {
        agentToReturn.SetCash(initialCash);
        return this;
    }
    public EconAgentBuilder<T> WithInitialCashFromTemplate()
    {
        agentToReturn.SetCash(agentToReturn.InitialCashInCents/100);
        return this;
    }
    public EconAgentBuilder<T> WithActionsPerTurn(int actionsPerTurn)
    {
        agentToReturn.SetActionsPerCycle(actionsPerTurn);
        return this;
    }
    public EconAgentBuilder<T> Named(string name)
    {
        agentToReturn.Name = name;
        return this;
    }

    public EconAgentBuilder<T> AtLevel(AgentLevelEnum companyLevel)
    {
        agentToReturn.agentLevel = companyLevel;
        return this;
    }

    public EconAgentBuilder<T> WithPopulation(int population)
    {
        if (agentToReturn is PopulationAgent populationCompany)
        {
            populationCompany.Population = population;
        }
        else
        {
            Debug.LogError("Company is not a PopulationCompany.");
        }
        return this;
    }

    public EconAgentBuilder<T> WithAnxiety(float anxiety)
    {
        agentToReturn.SetAnxiety(anxiety);
        return this;
    }
    public EconAgentBuilder<T> WithEnnui(float ennui)
    {
        if (agentToReturn is PopulationAgent populationCompany)
        {
            populationCompany.Ennui = ennui;
        }
        else
        {
            Debug.LogError("Company is not a PopulationCompany.");
        }
        return this;
    }

    public EconAgentBuilder<T> Demanding(Dictionary<Good, DemandData> demands)
    {
        if (agentToReturn is PopulationAgent populationAgent)
        {
            foreach (var demand in demands)
            {
                populationAgent.SetDemand(demand.Key, demand.Value);
            }
        }
        else
        {
            Debug.LogError("Economic Agent is not a PopulationAgent.");
        }
        return this;
    }
    public EconAgentBuilder<T> WithGoal(Goal goal)
    {
        agentToReturn.AddGoal(goal);
        return this;
    }

    public EconAgentBuilder<T> WithInventory(Inventory inventory)
    {
        agentToReturn.SetInventory(inventory);
        return this;
    }

    public EconAgentBuilder<T> AssumingNewGoodsCost(decimal value)
    {
        agentToReturn.SetMarketIgnorantAssumedCOG(value);
        return this;
    }
}

public static class EconAgentBuilder
{
    public static EconAgentBuilder<T> For<T>() where T : EconAgent
        => EconAgentBuilder<T>.Create();

    public static EconAgentBuilder<EconAgent> ForBaseAgent()
        => EconAgentBuilder<EconAgent>.Create();
    
    public static EconAgentBuilder<T> FromTemplate<T> (T existingTemplate) where T: EconAgent
        => new(existingTemplate);

    public static EconAgentBuilder<EconAgent> Wrap(EconAgent existingTemplate)
        => new(existingTemplate);
}