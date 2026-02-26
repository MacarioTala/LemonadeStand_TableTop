using System.Collections.Generic;
using System.Linq;

public class ModifyMonsterEffect : MarketEffectSO
{
    float _destructionMultiplier;
    const string _populationReduction = "PopulationReduction";
    int newDuration;
    
    List<(iMarketEvent Event, int OriginalDuration)> _originalEvents = new();
    public string TargetTag => "Monster";

    public override void Apply(Market market,ActiveMarketEvent activeMarketEvent)
    {
        foreach (var marketEvent in market.GetActiveMarketEvents())
        {
            if (_parentEvent == marketEvent.EventDefinition) continue;

            _originalEvents.Add(new(marketEvent.EventDefinition, marketEvent.EventDefinition.GetDuration()));

            if (marketEvent.EventDefinition.HasTag(TargetTag))
            {
                marketEvent.Extend(newDuration);        
            }
 
            if (marketEvent.EventDefinition.HasTag(_populationReduction))
            {
                var changepopulationEffect = marketEvent
                        .EventDefinition
                        .GetEffects()
                        .OfType<ChangePopulationEffect>()
                        .FirstOrDefault();
            
                var newMultiplier = changepopulationEffect.PopulationChangePercentage * _destructionMultiplier;
                changepopulationEffect.ChangeEffectMultiplier(newMultiplier);
            }
        }
    }

    public void Initialize( int newDuration, float populationReductionMultiplier)
    {
        _destructionMultiplier = populationReductionMultiplier;
        this.newDuration = newDuration;
    }
}