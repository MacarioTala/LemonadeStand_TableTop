using System;
using System.Collections.Generic;
using System.Linq;

public class MockMarketDataService : iMarketDataService
{
    IEnumerable<PopulationHistory> PopulationHistory;

    public void SetPopulationHistory(IEnumerable<PopulationHistory> populationHistory)
    {
        PopulationHistory = populationHistory;
    }
    
    public List<PopulationHistory> GetPopulationHistory(Guid marketId)
    {
        return PopulationHistory.Where(x => x.MarketId == marketId).ToList();
    }
}
