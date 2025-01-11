using System;
using UnityEngine;

public class LinearDemandStrategy : iDemandStrategy
{
    private const int MinDemand = 0;
    private const int MaxDemand = 1000000;

public void AdjustDemandForPopulation(Market market,Good good)
{
    var populationChangeInPeriod = market.GetPopulationPercentageChangeInPeriod();
    if(populationChangeInPeriod == 0) return;
    
    AdjustDemandBasedOnElasticity(market,good,ElasticityTypeEnum.PopulationElasticity,populationChangeInPeriod);
    
}
public LemonadeStandResultObject AdjustDemandBasedOnElasticity(Market market, Good good, ElasticityTypeEnum elasticity,float percentageChangeInMetric)
{
    var marketDemand = market.GetMarketDemand();

    if(!marketDemand.ContainsKey(good))
    {
        market.InitializeDemandForSpecificGood(good, 0);
    }
    var demandData = marketDemand[good];
    var doesElasticityExist = market.GetEffectiveElasticityForGood(good, elasticity, out var elasticityValue);
    
    if(!doesElasticityExist.Equals(LemonadeStandResultObject.Success())) 
    {
        Debug.Log($"Elasticity not found for {good.good_name}");
        return LemonadeStandResultObject.Failure(ResultTypeEnum.ElasticityNotFound, "");
    }
    
    var currentDemand = demandData.CurrentDemand;
    var MinDemand = demandData.MinDemand;
    var MaxDemand = demandData.MaxDemand;

    var adjustmentFactor = Math.Round(currentDemand * elasticityValue * percentageChangeInMetric);

    if (elasticityValue == 0) return LemonadeStandResultObject.Success();

    var newDemand = Math.Clamp(currentDemand + adjustmentFactor, MinDemand, MaxDemand);

    demandData.CurrentDemand = (int)Math.Round(newDemand,0);

    return LemonadeStandResultObject.Success();
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

    public void OnOrderFulfilled(OrderFulfilledEvent orderFulfilledEvent)
    {
        throw new NotImplementedException();
    }
   
}