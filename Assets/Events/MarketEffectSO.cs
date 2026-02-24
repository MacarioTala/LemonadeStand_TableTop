using UnityEngine;
/// <summary>
///  Need an abstract wrapper for MarketEffects because Unity doesn't seem to like SerializeReference to hold a list of interfaces
/// </summary>
public abstract class MarketEffectSO : ScriptableObject, iMarketEffect
{
    protected iMarketEvent _parentEvent;
    public abstract void Apply(Market market);
    public iMarketEvent GetParentEvent()
    {
        return _parentEvent;
    }
    public virtual void Reset(){}
    public void SetParentEvent(iMarketEvent marketEvent)
    {
        _parentEvent = marketEvent;
    }

}