using System.Collections.Generic;
using System.Linq;

public class ModifyMonsterEffect : iMarketEffect
{
    readonly float _destructionMultiplier;
    const string _populationReduction = "PopulationReduction";
    readonly int newDuration;
    
    List<(iMarketEvent Event, int OriginalDuration)> _originalEvents = new();
    public string TargetTag => "Monster";
    private iMarketEvent _parentEvent;

    public void Apply(Market market)
    {
        foreach (var (marketEvent,_,_) in market.GetActiveMarketEvents())
        {
            if (_parentEvent == marketEvent) continue;

            _originalEvents.Add(new(marketEvent, marketEvent.GetDuration()));

            if (marketEvent.HasTag(TargetTag))
            {
                marketEvent.SetDuration(newDuration);        
            }
            if (marketEvent.HasTag(_populationReduction))
            {
                var changepopulationEffect = marketEvent
                        .GetEffects()
                        .OfType<ChangePopulationEffect>()
                        .FirstOrDefault();
                
                changepopulationEffect.SaveOriginalState();
                var newMultiplier = changepopulationEffect.PopulationChangePercentage * _destructionMultiplier;
                changepopulationEffect.ChangeEffectMultiplier(newMultiplier);
            }
        }
    }

    public void SetParentEvent(iMarketEvent marketEvent)
    {
        _parentEvent = marketEvent;
    }

    public iMarketEvent GetParentEvent()
    {
        return _parentEvent;
    }

    public void SaveOriginalState()
    {
        // No-op: Original state is saved in Apply method
    }

    public void Reset()
    {
        foreach (var (marketEvent, originalDuration) in _originalEvents)
        {
            marketEvent.SetDuration(originalDuration);
            foreach (var effect in marketEvent.GetEffects())
            {
                if (effect is ChangePopulationEffect changePopulationEffect)
                {
                    changePopulationEffect.Reset();
                }
            }
        }
    }

    public ModifyMonsterEffect( int newDuration, float destructionMultiplier)
    {
        _destructionMultiplier = destructionMultiplier;
        this.newDuration = newDuration;
    }
}