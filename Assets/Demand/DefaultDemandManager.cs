using System;
using System.Collections.Generic;
using System.Linq;

public class DefaultDemandManager : iDemandManager,iMarketAware
{
    Market marketBeingManaged;
    
    private IEnumerable<PopulationAgent> demandersInMarket 
            => marketBeingManaged.GetMarketParticipants()
            .OfType<PopulationAgent>()
            .Where(x=>!x.IsPlayer);

    public IEnumerable<(Good good, int Bid, int Ask)> GetBidAskSpreadsFromMarket()
    {
        return marketBeingManaged.MarketData == null? Enumerable.Empty<(Good, int, int)>()
            : marketBeingManaged.MarketData.Select(x => (x.Good, x.Bid, x.Ask));
    }

    public int GetMarketDemandForGood(Good good)
    {
        return demandersInMarket
                .Sum(x=>x.GetQuantityDemandedFor(good));
    }

    public decimal GetPerceivedCostOfGood(Good good)
    {
        var asks = marketBeingManaged.MarketData
                    .GroupBy(x => x.Good)                    
                    .Select(g=> new KnownPrice
                    {
                        Good = g.Key,
                        Price = (int)Math.Round(g.Average(x=>x.Ask))
                    })
                    .ToList();

        var perceivedCost = marketBeingManaged.GetMarketParticipants()
                    .Where(x => x is not PopulationAgent)
                    .SelectMany(x => x.Recipes
                        .Where(r => r.GetProduct().Equals(good))
                           )
                    .Select(r => r.GetPerceivedCostPerUnit(asks))
                    .DefaultIfEmpty(0m) // If no recipes found, default to 0
                    .Average();
        return perceivedCost;
    }

    public void SetMarket(Market market)
    {
        marketBeingManaged = market;
    }

    public LemonadeStandResultObject UpdateFulfillmentRates(int tradingPeriod = -1)
    {
     // You are here: update this to update the supply of the good too
        // since CalculateFulfillmentRates already calculates total supply
        // Maybe there's no need for a supply provider?

        //Get the demand and supply for the period
        var ordersSubmittedInPeriod = marketBeingManaged.GetOrdersSubmittedInPeriod(tradingPeriod);
        var fulfillmentRates = MarketObserver.CalculateFulfillmentRates(ordersSubmittedInPeriod);

        // Note: Currently only supports one PopulationCompany per market.
        // In the future, we may need to merge fulfillment rates 
        // from multiple PopulationCompanies/market segments.
        var populationCompany = marketBeingManaged.GetMarketParticipants()
            .OfType<PopulationAgent>()
            .FirstOrDefault();
        if (populationCompany != null)
        {
            foreach (var fulfillmentRate in fulfillmentRates)
            {
                if (populationCompany.GetDemand().TryGetValue(fulfillmentRate.Good, out var demandEntry))
                {
                    demandEntry.CurrentDemand = fulfillmentRate.TotalDemand;
                    demandEntry.FulfilmentRate = fulfillmentRate.FulfillmentRate;
                }
            }
        }
        return LemonadeStandResultObject.Success();
    }

    public LemonadeStandResultObject GetEffectiveElasticityForGood(Good good, ElasticityTypeEnum elasticity)
    {
        if (!good.Elasticities.TryGetValue(elasticity, out float elasticityValue))
        {
            return LemonadeStandResultObject.Failure(ResultTypeEnum.ElasticityNotFound, "");
        }
        return LemonadeStandResultObject.Success(extraData: elasticityValue);
    }

    public DemandData GetDemandFor(Good good)
    {
        throw new System.NotImplementedException();
    }
}
