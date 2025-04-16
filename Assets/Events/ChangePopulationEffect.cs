using System;
using UnityEngine;

[Tooltip("Population change as a percentage. Use negative values to decrease population.")]
public class ChangePopulationEffect : iMarketEffect
{
    public float PopulationChangePercentage;
    float _conversion => PopulationChangePercentage / 100f;
    
    public void Apply(Market market)
    {
        var population = market.GetPopulation();
        var delta = (int)Math.Round(population * _conversion,0);
        market.SetPopulation(population + delta);
    }

    public ChangePopulationEffect(float populationChangePercentage)
    {
        PopulationChangePercentage = populationChangePercentage;
    }
}
