using System.Collections.Generic;

public interface iMarketEventManager
{
    void AddPotentialMarketEvent(iMarketEvent potentialEvent);
    void RemovePotentialMarketEvent(iMarketEvent potentialEvent);
    void ResolveMarketEvents();
    void RollForEvents();
    public List<(iMarketEvent Event, int PeriodStart, int duration)> GetActiveMarketEvents();
    public List<(iMarketEvent Event, int PeriodStart, int periodEnd)> GetMarketEventHistory();

}