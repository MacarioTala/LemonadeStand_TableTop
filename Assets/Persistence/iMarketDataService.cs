using System;
using System.Collections.Generic;

public interface iMarketDataService
{
    List<PopulationHistory> GetPopulationHistory(Guid marketId);
}