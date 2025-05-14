using System;
using System.Collections.Generic;

public class PopulationCompany : Company
{
#region Demographics
    private int _population;

    private float _ennui;
    public float Ennui
    {
        get => _ennui;
        set
        {
            if (value < 0 || value > 1)
                throw new ArgumentOutOfRangeException(nameof(value), "Ennui must be between 0 and 1.");
            _ennui = value;
        }
    }
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

    private bool IsDemanded(Good good)
    {
        return _demand.ContainsKey(good);
    }

#endregion
    private PopulationCompany() {}

    public static class PopulationCompanyBuilder
    {
        public static CompanyBuilder<PopulationCompany> Create()
                             => CompanyBuilder.For<PopulationCompany>();
    }
#region Interactions With Market
    public LemonadeStandResultObject CreateOrders(Market market)
    {
        var goodsAvailable = market.GetInventory().GetInventoryEntries();

        foreach (var inventoryEntry in goodsAvailable)
        {
            
        }
        return LemonadeStandResultObject.Failure("Not implemented");
    }
#endregion
#region Consumption
    public void Consume()
    {
        var inventory = GetInventory().GetInventoryEntries();
        foreach (var inventoryEntry in inventory)
        {
            inventoryEntry.good.ApplyEffects(this); //you are here. Make good effects scale with amount consumed.
        }
    }
#endregion
}