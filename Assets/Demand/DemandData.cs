using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[Serializable]
public class DemandData
{
    public decimal Ask { get; set; }
    public int CurrentDemand { get; set; }
    public int InitialDemand { get; set; }
    public decimal InitialPrice { get; set; }
    public float FulfilmentRate { get; set; }
    public int MinDemand { get; set; }
    public int MaxDemand { get; set; }
    public float ConsumptionRate { get; set; } = 1f; // Default consumption rate is 1. 
                                                     // Meaning that each population unit consumes one unit
                                                     // of the good per period.
    [SerializeField]public List<ElasticDemandComponent> ElasticDemandComponents = new();

    public int GetAdjustedDemand(Dictionary<ElasticDemandComponentEnum, float > stateChanges)
    {
        int newDemand = 0;
        foreach (var component in ElasticDemandComponents)
        {
            if (stateChanges.TryGetValue(component.Type, out var stateChange))
            {
                var adjustment = component.GetDemandAdjustment(stateChange, CurrentDemand);
                newDemand += adjustment;
            }
        }
        // Ensure that the total demand is clamped within the min and max demand limits
        if (CurrentDemand + newDemand < MinDemand)
        {
            CurrentDemand = MinDemand;
        }
        else if (CurrentDemand + newDemand > MaxDemand)
        {
            CurrentDemand = MaxDemand;
        }
        else
        { 
            CurrentDemand += newDemand;
        }

        return CurrentDemand;
    }

    public int GetAdjustedDemandFor(ElasticDemandComponentEnum component, float stateChange)
    {
        var adjustment = ElasticDemandComponents.FirstOrDefault(x=>x.Type==component).GetDemandAdjustment(stateChange,CurrentDemand);
         // Ensure that the total demand is clamped within the min and max demand limits
        if (CurrentDemand + adjustment < MinDemand)
        {
            CurrentDemand = MinDemand;
        }
        else if (CurrentDemand + adjustment > MaxDemand)
        {
            CurrentDemand = MaxDemand;
        }
        else
        { 
            CurrentDemand += adjustment;
        }
        return CurrentDemand;
    }
    
    public int GetDemand()
    { 
        return GetRequisiteDemand();
        //add other demand here later
    }
    public int GetRequisiteDemand()
    {
        return MinDemand;
    }
}


