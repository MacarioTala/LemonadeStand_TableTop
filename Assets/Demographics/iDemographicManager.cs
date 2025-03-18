public interface iDemographicManager
{
    float GetMarketInstability();
    LemonadeStandResultObject SetMarketInstability(float newInstability);
    float GetPopulationEnnui();
    
    string GetEnnuiLevel()
    {
        if (GetPopulationEnnui() == 0) return "Inspired";
        if (GetPopulationEnnui() > 0 && GetPopulationEnnui() < .25) return "Thriving";
        if (GetPopulationEnnui() >= .25 && GetPopulationEnnui() < .5) return "Content";
        if (GetPopulationEnnui() >= .5 && GetPopulationEnnui() < .75) return "Dissatisfied";
        if (GetPopulationEnnui() >= .75 && GetPopulationEnnui() < 1) return "Bored";
        if (GetPopulationEnnui() == 1) return "Bored to death";
        return "Unknown";
    }

    float GetPopulationGrowthRate();
    int GetPopulation();
    LemonadeStandResultObject SetPopulation(int newPopulation);
    float GetPopulationHappiness();
    LemonadeStandResultObject SetPopulationHappiness(float newHappiness);
}