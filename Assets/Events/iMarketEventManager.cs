using System.Collections.Generic;

public interface iMarketEventManager
{
    void AddPotentialMarketEvent(MarketEventSO potentialEvent);
    void RemovePotentialMarketEvent(MarketEventSO potentialEvent);
    void ResolveMarketEvents();
    void RollForEvents();
    public List<ActiveMarketEvent> GetActiveMarketEvents();
    public List<(MarketEventSO Event, int PeriodStart, int periodEnd)> GetMarketEventHistory();

}