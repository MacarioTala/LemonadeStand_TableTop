using System.Collections.Generic;

public sealed class ActiveMarketEvent
{
    public MarketEventSO EventDefinition {get;}
    public int PeriodStart {get;}
    public int PeriodEnd {get; private set;}

    public readonly List<MarketEffectSO> ExtraEffects = new();

    public ActiveMarketEvent(MarketEventSO template, int periodStart,int duration)
    {
        EventDefinition = template;
        PeriodStart = periodStart;
        PeriodEnd = periodStart+duration;
    }

    public bool IsExpired(int currentPeriod)
        => currentPeriod >= PeriodEnd;
    
    public void Extend(int extraPeriods)
        => PeriodEnd+= extraPeriods;
    
    public void AddEffect(MarketEffectSO effect)
        => ExtraEffects.Add(effect);

}