using System;
using System.Collections.Generic;

public class MockDemographicManager : iDemographicManager
{
    private float _marketInstability = 0f; // Stable market by default
    private int _population; // Default population
    IEnumerable<PopulationHistory> _populationHistory;
    public float GetMarketInstability()
    {
        return _marketInstability;
    }

    public int GetPopulation()
    {
        return _population;
    }

    public float GetPopulationEnnui()
    {
        throw new NotImplementedException();
    }

    public float GetPopulationGrowthRate()
    {
        throw new NotImplementedException();
    }

    public float GetPopulationHappiness()
    {
        throw new NotImplementedException();
    }

    public List<PopulationHistory> GetPopulationHistory(Guid marketid)
    {
        return _populationHistory != null ? new List<PopulationHistory>(_populationHistory) : new List<PopulationHistory>();
    }

    public LemonadeStandResultObject RecordDemographicSnapshot(Guid marketId, int period)
    {
        throw new NotImplementedException();
    }

    public LemonadeStandResultObject SetMarketInstability(float newInstability)
    {
        _marketInstability = newInstability;
        return LemonadeStandResultObject.Success();
    }

    public LemonadeStandResultObject SetPopulation(int newPopulation)
    {
        _population = newPopulation;
        return LemonadeStandResultObject.Success();
    }

     public void SetPopulationHistory(IEnumerable<PopulationHistory> populationHistory)
    {
        _populationHistory = populationHistory;
    }

    public LemonadeStandResultObject SetPopulationHappiness(float newHappiness)
    {
        throw new NotImplementedException();
    }
}