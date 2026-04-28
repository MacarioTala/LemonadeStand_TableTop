using System;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName ="LemonadeStandAssets/EventEffects/ChangePopulationEffect")]
public class ChangePopulationEffect : MarketEffectSO
{
    [Tooltip("Population change as a percentage. Use negative values to decrease population.")]
    public float PopulationChangePercentage;
    float _conversion => PopulationChangePercentage / 100f;
    
    public override void Apply(Market market,ActiveMarketEvent activeMarketEvent)
    {
        var marketPopulations = market.GetMarketParticipants()
            .Where(x => x is PopulationAgent)
            .ToList();

        foreach (PopulationAgent populationCompany in marketPopulations.Cast<PopulationAgent>())
        {
            var population = populationCompany.Population;
            var delta = (int)Math.Round(population * _conversion,0);
            populationCompany.Population += delta;
        }
    }

    public ChangePopulationEffect(float populationChangePercentage)
        =>  PopulationChangePercentage = populationChangePercentage;
    
    public void SetPopulationChangePercentage(float populationChangePercentage)
        => PopulationChangePercentage=populationChangePercentage;

    public void ChangeEffectMultiplier(float multiplier)
    {
        PopulationChangePercentage *= multiplier;
    }
}
