using System.Collections.Generic;
using System.Linq;

public class DefaultMarketEventManager : iMarketEventManager, iMarketAware
{
    private Market _market;
    readonly List<ActiveMarketEvent> _activeEvents = new();
    readonly List<(MarketEventSO Event, int PeriodStart, int periodEnd)> _marketEventHistory = new();
    private List<MarketEventSO> PotentialMarketEvents { get; } = new();

    public void AddPotentialMarketEvent(MarketEventSO potentialEvent)
    {
        if (!PotentialMarketEvents.Contains(potentialEvent))
        {
            PotentialMarketEvents.Add(potentialEvent);
        }
    }

    public List<ActiveMarketEvent> GetActiveMarketEvents() => _activeEvents;

    public List<(MarketEventSO Event, int PeriodStart, int periodEnd)> GetMarketEventHistory()=> _marketEventHistory;

    public void ResolveMarketEvents()
    {
        //Check if any active events have expired
        var expiredEvents = _activeEvents
                            .Where(x => x.IsExpired(_market.CurrentPeriod))
                            .ToList();
        foreach (var marketEvent in expiredEvents)
        {
            _activeEvents.Remove(marketEvent);
            _marketEventHistory.Add((marketEvent.EventDefinition, marketEvent.PeriodStart, marketEvent.PeriodEnd));
        }

        //See if any of the current potential events fire
        RollForEvents();

        //Invoke any active events
        foreach (var marketEvent in _activeEvents)
        {
            _market.FireEvent(marketEvent);
            marketEvent.EventDefinition.Invoke(_market,marketEvent);
        }
    }

    private void RollForEvents()
    {
        foreach (var marketEvent in PotentialMarketEvents)
        {
            if (_activeEvents.Any(x => !x.EventDefinition.IsCompatibleWith(marketEvent))) continue;

            if (_activeEvents.Any(x => ReferenceEquals(x.EventDefinition,marketEvent))) continue;

            if (EventRollSucceeds(marketEvent))
            {
                var activeEvent = new ActiveMarketEvent(marketEvent,_market.CurrentPeriod,marketEvent.GetDuration());
                _activeEvents.Add(activeEvent);
            }
        }
    }
    private static bool EventRollSucceeds(iMarketEvent marketEvent)
    {
        var currentRoll = UnityEngine.Random.Range(0, 100);
        var chanceOfEvent = marketEvent.GetProbabilityOf();

        return chanceOfEvent >= currentRoll; ;
    }

    public void RemovePotentialMarketEvent(MarketEventSO potentialEvent)
    {
        if (PotentialMarketEvents.Contains(potentialEvent))
        {
            PotentialMarketEvents.Remove(potentialEvent);
        }
    }

    public void SetMarket(Market market)
    {
        _market = market;
    }
}
