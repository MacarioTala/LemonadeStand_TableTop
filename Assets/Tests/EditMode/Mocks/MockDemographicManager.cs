public class MockDemographicManager : iDemographicManager
{
    private float _marketInstability = 0f; // Stable market by default
    private int _population; // Default population
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
        throw new System.NotImplementedException();
    }

    public float GetPopulationGrowthRate()
    {
        throw new System.NotImplementedException();
    }

    public float GetPopulationHappiness()
    {
        throw new System.NotImplementedException();
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

    public LemonadeStandResultObject SetPopulationHappiness(float newHappiness)
    {
        throw new System.NotImplementedException();
    }
}