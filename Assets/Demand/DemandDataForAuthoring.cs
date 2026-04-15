//This is a wrapper for DemandData so that EconAgent.DemandData can be set in the inspector
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DemandDataForAuthoring
{
     [SerializeField] private int askInCents;
    [SerializeField] private int currentDemand;
    [SerializeField] private int initialDemand;
    [SerializeField] private int initialPriceInCents;
    [SerializeField] private float fulfilmentRate;
    [SerializeField] private int minDemand;
    [SerializeField] private int maxDemand;
    [SerializeField] private float consumptionRate = 1f;
    [SerializeField] private List<ElasticDemandComponent> elasticDemandComponents = new();

    public DemandData ToDemandData()
    {
        return new DemandData
        {
            Ask = askInCents / 100m,
            CurrentDemand = currentDemand,
            InitialDemand = initialDemand,
            InitialPrice = initialPriceInCents / 100m,
            FulfilmentRate = fulfilmentRate,
            MinDemand = minDemand,
            MaxDemand = maxDemand,
            ConsumptionRate = consumptionRate,
            ElasticDemandComponents = new List<ElasticDemandComponent>(elasticDemandComponents)
        };
    }
}