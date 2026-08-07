using System;
using System.Linq;
using UnityEngine;
[CreateAssetMenu(menuName = "Demand/LinearDemandStrategy")]
public class LinearDemandStrategy : ScriptableObject,iDemandStrategy
{
    private const int GlobalMinDemand = 0;
    private const int GlobalMaxDemand = 1000000;

    public LemonadeStandResultObject AdjustDemandInPeriod(Market market)
    {
        foreach (var good in market.GetPopulationDemand().Keys)
        {
            AdjustDemandForSaturation(market, good);
            AdjustDemandForPopulation(market, good);
        }
        return LemonadeStandResultObject.Success();
    }
    internal void AdjustDemandForPopulation(Market market,Good good)
    {
        var populationChangeInPeriod = market.GetPopulationPercentageChangeInPeriod();
        if(populationChangeInPeriod == 0) return;
        
        AdjustDemandBasedOnElasticityAndHistory(market,good,ElasticityTypeEnum.PopulationElasticity,populationChangeInPeriod);    
    }
    internal LemonadeStandResultObject AdjustDemandBasedOnElasticityAndHistory(Market market, Good good, ElasticityTypeEnum elasticity,float percentageChangeInMetric)
    {
        float elasticityValue;

        elasticityValue=GetElasticity(market, good, elasticity);

        var demandData = market.GetPopulationDemand()[good];
        var currentDemand = demandData.CurrentDemand;
        var MinDemand = demandData.MinDemand;
        var MaxDemand = demandData.MaxDemand;

        var adjustmentFactor = Math.Round(currentDemand * elasticityValue * percentageChangeInMetric);

        if (elasticityValue == 0) return LemonadeStandResultObject.Success();

        var newDemand = Math.Clamp(currentDemand + adjustmentFactor, MinDemand, MaxDemand);

        demandData.CurrentDemand = (int)Math.Round(newDemand, 0);

        return LemonadeStandResultObject.Success();
    }
    
    public void OnOrderFulfilled(OrderFulfilledEvent orderFulfilledEvent)
    {
        var good = orderFulfilledEvent.Good;
        var market = orderFulfilledEvent.OrderMarket;

        var result = market.GetEffectiveElasticityForGood(good, ElasticityTypeEnum.SaturationElasticity);
        
        if (!result.Equals(LemonadeStandResultObject.Success())) 
        {
            Debug.Log($"Elasticity not found for {good.GoodName}");
            return;
        }

        MakeDemandMicroAdjustments(orderFulfilledEvent);

        Debug.Log($"Order Fulfilled for {orderFulfilledEvent.Good.GoodName}. Quantity: {orderFulfilledEvent.FulfilledQuantity}");
    }

    internal static void AdjustDemandForSaturation(Market market,Good good)
    {
        var demandForGood = market.GetPopulationDemand()[good];
        var totalSupply = market.GetSupplyInPeriod(market.CurrentPeriod);
        int supplyForGood = 0;
        if (!(totalSupply == null || totalSupply.Count == 0))
        {
        supplyForGood = totalSupply
                            .Where(x => x.Good == good)
                            .Sum(x => x.Quantity);
        }
        var saturation = demandForGood.CurrentDemand==0?0 
                            : (float)supplyForGood/demandForGood.CurrentDemand ;
    
        
        var elasticity = GetElasticity(market, good, ElasticityTypeEnum.SaturationElasticity);

        var demandAdjustment = demandForGood.CurrentDemand * (1f-saturation) * elasticity;

        demandForGood.CurrentDemand = (int)Math.Round(demandForGood.CurrentDemand + demandAdjustment, 0);
    }


     private static float GetElasticity (Market market, Good good, ElasticityTypeEnum elasticity)
    {
        var doesElasticityExist = market.GetEffectiveElasticityForGood(good, elasticity);
        if (!doesElasticityExist.Equals(LemonadeStandResultObject.Success()))
        {
            Debug.Log($"Elasticity not found for {good.GoodName}");
            return 0f;
        }
        else
        {
            float elasticityValue = (float)doesElasticityExist.ExtraData;
            return elasticityValue;
        }
    }

    private void MakeDemandMicroAdjustments(OrderFulfilledEvent orderFulfilledEvent)
    {
        var market = orderFulfilledEvent.OrderMarket;
        var instability = market.GetMarketInstability();
        var order = orderFulfilledEvent.PrimaryOrder;
        var demandData = orderFulfilledEvent.OrderMarket.GetPopulationDemand()[order.Good];
        var currentDemand = demandData.CurrentDemand;
        var minDemand = demandData.MinDemand;
        var maxDemand = demandData.MaxDemand;

        if (instability == 0) return;//If Market is completely stable, demand does not change

        var demandAdjustment = UnityEngine.Random.Range(0, currentDemand) * instability;
        demandData.CurrentDemand = (int) Math.Clamp(currentDemand + demandAdjustment, minDemand, maxDemand);
        
    }
}