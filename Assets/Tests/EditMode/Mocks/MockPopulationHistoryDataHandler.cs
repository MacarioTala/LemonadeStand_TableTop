using System;
using System.Collections.Generic;

public class MockPopulationHistoryDataHandler : iDataHandler<PopulationHistory>
{
    private List<PopulationHistory> _populationHistory = new();
    public void Add(PopulationHistory item)
    {
        _populationHistory.Add(item);
    }

    public void Delete(int id)
    {
        throw new NotImplementedException();
    }

    public PopulationHistory Load(string path)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<PopulationHistory> LoadHistorical(Func<PopulationHistory, bool> predicate)
    {
        return _populationHistory;
    }

    public void SetPopulationHistory(List<PopulationHistory> populationHistory)
    {
        _populationHistory = populationHistory;
    }

    public void Save(PopulationHistory data)
    {
        _populationHistory.Add(data);
    }
}