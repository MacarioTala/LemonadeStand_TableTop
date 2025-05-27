using System.Collections.Generic;

public class DemandData
{
    public decimal Ask {get; set;}
    public int CurrentDemand{get; set;}
    public float FulfilmentRate{get; set;}
    public int MinDemand{get; set;}
    public int MaxDemand{get; set;}
    public float ConsumptionRate {get; set;} = 1f; // Default consumption rate is 1. 
                                                   // Meaning that each population unit consumes one unit
                                                   // of the good per period.
    public List<ElasticDemandComponent> ElasticDemandComponents {get; set;} = new ();
}

public class ElasticDemandComponent
{
    public ElasticDemandComponentEnum Type {get; set;}
    public float Sensitivity {get; set;}
    public int MinDemand {get; set;}
 
    public float GetMultiplier(float factor)
    {
        return factor*Sensitivity;
    }
}

public enum ElasticDemandComponentEnum
{
    Price,
    Income,
    Population,
    Substitutes,
    Complements
}


