using UnityEngine;
/// <summary>
///  Need an abstract wrapper for MarketEffects because Unity doesn't seem to like SerializeReference to hold a list of interfaces
/// </summary>
public abstract class MarketEffectSO : ScriptableObject, iMarketEffect
{
    protected MarketEventSO _parentEvent;
    public abstract void Apply(Market market,ActiveMarketEvent activeMarketEvent);
    public MarketEventSO GetParentEvent()
    {
        return _parentEvent;
    }
    public void SetParentEvent(MarketEventSO marketEvent)
    {
        _parentEvent = marketEvent;
    }

}