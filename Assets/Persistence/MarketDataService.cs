using System;
using System.Collections.Generic;
using System.Linq;

public class MarketDataService : iMarketDataService
{
    private readonly iDataHandler<PopulationHistory> _dataHandler;

    public MarketDataService(iDataHandler<PopulationHistory> populationHistoryDataHandler)
    {
        _dataHandler = populationHistoryDataHandler;
    }

    public List<PopulationHistory> GetPopulationHistory(Guid marketId)
    {
        return _dataHandler.LoadHistorical(history => history.MarketId == marketId).ToList();
    }

}
