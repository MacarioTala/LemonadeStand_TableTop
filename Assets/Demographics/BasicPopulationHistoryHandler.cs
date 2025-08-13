using System;
using System.Collections.Generic;
using System.Linq;

public class BasicPopulationHistoryHandler : iDataHandler<PopulationHistory>
{
    private readonly List<PopulationHistory> _populationHistory = new();

    public void Save(PopulationHistory data)
    {
        _populationHistory.Add(data);
    }

    public IEnumerable<PopulationHistory> LoadHistorical(Func<PopulationHistory, bool> predicate)
    {
        return _populationHistory.Where(predicate);
    }

    // Other required interface methods can throw NotImplementedException
    // since they're not used in BasicDemographicManager
    public PopulationHistory Load(string path) => throw new NotImplementedException();
    public void Delete(int id) => throw new NotImplementedException();
}
