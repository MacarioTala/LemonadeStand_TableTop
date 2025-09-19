using System.Collections.Generic;
using System.Linq;

public class DefaultMarketEventManager : iMarketEventManager, iMarketAware
{
    private Market _market;
    readonly List<(iMarketEvent Event, int PeriodStart, int duration)> _activeEvents = new();
    readonly List<(iMarketEvent Event, int PeriodStart, int periodEnd)> _marketEventHistory = new();
    private List<iMarketEvent> _potentialMarketEvents { get; } = new();

    public void AddPotentialMarketEvent(iMarketEvent potentialEvent)
    {
          if (!_potentialMarketEvents.Contains(potentialEvent))
        {
            _potentialMarketEvents.Add(potentialEvent);
        }
    }

    public List<(iMarketEvent Event, int PeriodStart, int duration)> GetActiveMarketEvents() => _activeEvents;

    public List<(iMarketEvent Event, int PeriodStart, int periodEnd)> GetMarketEventHistory()=> _marketEventHistory;

    public void ResolveMarketEvents()
    {
        //Check if any active events have expired
        var expiredEvents = _activeEvents
                            .Where(x => x.Event.IsExpiredAt(x.PeriodStart, _market.CurrentPeriod))
                            .ToList();
        foreach (var marketEvent in expiredEvents)
        {
            _activeEvents.Remove(marketEvent);
            _marketEventHistory.Add((marketEvent.Event, marketEvent.PeriodStart, _market.CurrentPeriod));
            marketEvent.Event.Reset();
        }

        //Invoke any active events
        foreach (var marketEvent in _activeEvents)
        {
            marketEvent.Event.Invoke(_market);
        }
    }

    public void RollForEvents()
    {
        foreach (var marketEvent in _potentialMarketEvents)
        {
            if (_activeEvents.Any(x => !x.Event.IsCompatibleWith(marketEvent))) continue;

            if (_activeEvents.Any(x => x.Event.Equals(marketEvent))) continue;

            if (EventRollSucceeds(marketEvent))
            {
                _activeEvents.Add((marketEvent, _market.CurrentPeriod, marketEvent.GetDuration()));
            }
        }
    }
    private static bool EventRollSucceeds(iMarketEvent marketEvent)
    {
        var currentRoll = UnityEngine.Random.Range(0, 100);
        var chanceOfEvent = marketEvent.GetProbabilityOf();

        return chanceOfEvent >= currentRoll; ;
    }

    public void RemovePotentialMarketEvent(iMarketEvent potentialEvent)
    {
        if (_potentialMarketEvents.Contains(potentialEvent))
        {
            _potentialMarketEvents.Remove(potentialEvent);
        }
    }

    public void SetMarket(Market market)
    {
        _market = market;
    }
}