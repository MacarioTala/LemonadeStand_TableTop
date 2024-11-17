using UnityEngine;

public class LinearDemandStrategy : iDemandStrategy
{
    public void AdjustDemand(Market market)
    {
        var demand_data = market.MarketDemand;
        foreach(var good in demand_data.Keys)
        {
            
            var demandData = demand_data[good];
            var elasticity = good.DemandElasticity;
            
            //Calculate adjustment factor
            var adjustment_factor = 1f;

            //increase demand if fulfilment rate is 60% or lower
            if(demandData.FulfilmentRate <= .6f)
            {
                adjustment_factor += (1f- demandData.FulfilmentRate) * elasticity;  
            }
            //make adjustment_factor equal elasticity if fulfilment rate is 60 to 95%
            else if(demandData.FulfilmentRate > .6f && demandData.FulfilmentRate < .95f)
            {
                adjustment_factor = elasticity;
            }
            //decrease demand if fulfilment rate is 95% or higher
            else if(demandData.FulfilmentRate >= .95f)
            {
                adjustment_factor -= .1f * elasticity;
            }

            demandData.CurrentDemand = Mathf.Clamp(
                Mathf.RoundToInt(demandData.CurrentDemand * adjustment_factor)
                                ,demandData.MinDemand
                                ,demandData.MaxDemand
                                );
        }
    }
}