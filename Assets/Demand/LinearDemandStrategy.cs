using System;
using UnityEngine;

public class LinearDemandStrategy : iDemandStrategy
{
    private const int MinDemand = 0;
    private const int MaxDemand = 1000000;
    public void AdjustDemand(Market market)
    {
        var marketDemand = market.GetMarketDemand();
        foreach(var good in marketDemand.Keys)
        {
            var demandData = marketDemand[good];
            var elasticity = good.DemandElasticity;
            if (elasticity == 0) continue;
            
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

     private decimal CalculateAskForProducedGood (Market market,Good good)
    {
        if(!good.IsProducedGood) return 0;
        var costPerUnit = market.GetMarketCostForGood(market,good);
        var rng = (double)UnityEngine.Random.Range(.01f,.15f);
        var ask = costPerUnit * 1+(decimal)rng;
        return ask;
    }

    public void InitializeDemandForSpecificGood(Market market,Good good,int initialDemand, int minDemand=MinDemand, int maxDemand=MaxDemand)
    {
        decimal ask;
        bool hasCostForGood = CalculateAskForProducedGood(market,good) > 0;
        var marketDemand = market.GetMarketDemand();
        if(good.IsProducedGood && hasCostForGood)
        {
            ask=CalculateAskForProducedGood(market,good);
        }
        else
        {
            ask = good.GetPrice();
        }

        if(marketDemand.ContainsKey(good))
        {
            marketDemand[good].CurrentDemand = initialDemand;
            marketDemand[good].MinDemand = minDemand;
            marketDemand[good].MaxDemand = maxDemand;
            marketDemand[good].Ask = ask;
        }
        else
        {
            var demandData = new DemandData
                            { 
                                CurrentDemand = initialDemand,
                                FulfilmentRate = 0f,
                                MinDemand = minDemand,
                                MaxDemand = maxDemand,
                                Ask = ask
                            };
            marketDemand.Add(good, demandData);
        }
    }

    public void InitializeDemand(Market market)
    {
        throw new NotImplementedException();
    }

    public float GetDemandElasticityForGood(Good good)
    {
        throw new NotImplementedException();
    }

    public float GetIncomeElasticityForGood(Good good)
    {
        throw new NotImplementedException();
    }

    public float GetPriceElasticityForGood(Good good)
    {
        throw new NotImplementedException();
    }
}