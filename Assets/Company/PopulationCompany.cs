using System;
using System.Collections.Generic;

public class PopulationCompany : Company
{
#region Demographics
    private int _population;
    public int Population
    {
        get => _population;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Population cannot be negative.");
            _population = value;
        }
    }
#endregion

#region Demand
private readonly Dictionary<Good, DemandData> _demand = new();
    public Dictionary<Good, DemandData> GetDemand()
    {
        return _demand;
    }

    public int GetDemandForGood(Good good,float factor)
    {
        if (_demand.TryGetValue(good, out var demandData))
        {
            float floatDemand = demandData.MinDemand;       
            foreach (var component in demandData.ElasticDemandComponents)
            {
                floatDemand += component.MinDemand + (Population * component.GetMultiplier(factor));
            }
            return (int) Math.Round(floatDemand);
        }
        return 0;
    }
    public void SetDemand(Good good, DemandData demandData)
    {
        _demand[good] = demandData;
    }
#endregion
    private PopulationCompany() {}

    public static class PopulationCompanyBuilder
    {
        public static CompanyBuilder<PopulationCompany> Create()
                             => CompanyBuilder.For<PopulationCompany>();
    }
}