using UnityEngine;

public class CompanyBuilder<T> where T : Company
{
    private readonly T companyToReturn;

    public CompanyBuilder(T company) => companyToReturn = company;
    public static CompanyBuilder<T> Create() => new(ScriptableObject.CreateInstance<T>());
    public T Build() => companyToReturn;
    
    public CompanyBuilder<T> WithFixedCostStrategy(iFixedCostStrategy fixedCostStrategy)
    {
        companyToReturn.FixedCostStrategy = fixedCostStrategy;
        return this;
    }
    public CompanyBuilder<T> WithBehaviourStrategy(iStrategy behaviourStrategy)
    {
        companyToReturn.SetStrategy(behaviourStrategy);
        return this;
    }
    public CompanyBuilder<T> WithInitialCash( int initialCash)
    {
        companyToReturn.SetCash(initialCash);
        return this;
    }
    public CompanyBuilder<T> WithActionsPerTurn(int actionsPerTurn)
    {
        companyToReturn.SetActionsPerCycle(actionsPerTurn);
        return this;
    }
    public CompanyBuilder<T> Named( string name)
    {
        companyToReturn.Name = name;
        return this;
    }

    public CompanyBuilder<T> AtLevel(CompanyLevelEnum companyLevel)
    {
        companyToReturn.companyLevel = companyLevel;
        return this;
    }

    public CompanyBuilder<T> WithPopulation(int population)
    {
        if (companyToReturn is PopulationCompany populationCompany)
        {
            populationCompany.Population = population;
        }
        else
        {
            Debug.LogError("Company is not a PopulationCompany.");
        }
        return this;
    }
}

public static class CompanyBuilder
{
    public static CompanyBuilder<T> For<T>() where T : Company 
            =>CompanyBuilder<T>.Create();
}