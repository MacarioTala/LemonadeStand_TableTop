using System;
using System.Collections.Generic;
using System.Linq;

public class DefaultDemandManager : iDemandManager,iMarketAware
{
    Market marketBeingManaged;
    private readonly Dictionary<Good, DemandData> _marketDemand = new();

    public IEnumerable<(Good good, decimal Bid, decimal Ask)> GetBidAskSpreadsFromMarket()
    {
         var spreads = new List<(Good good, decimal Bid, decimal Ask)>();
        if (marketBeingManaged.MarketData == null)
        {
            return Enumerable.Empty<(Good, decimal, decimal)>();
        }
        else
        {
            spreads = marketBeingManaged.MarketData
                .Select(x => (x.Good, x.Bid, x.Ask)).ToList();
        }
        return spreads;
    }

    public DemandData GetDemandFor(Good good)
    {
        throw new NotImplementedException("Might need to  implement this as a list of DemandData instead.");
    }
    public int GetMarketDemandForGood(string goodName)
    {
        var good = _marketDemand.Keys.FirstOrDefault(x => x.GoodName == goodName);
        return _marketDemand[good].CurrentDemand;
    }
    public void SetMarketDemandForGood(Good good, DemandData demandData)
        => _marketDemand[good] = demandData;

    public decimal GetPerceivedCostOfGood(Good good)
    {
        var asks = marketBeingManaged.MarketData
                    .GroupBy(x => x.Good)
                    .ToDictionary(g => g.Key, g => g.Average(x => x.Ask));

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

    public void InitializeDemandForSpecificGood(Good good, int InitialDemand, int minDemand = 0, int maxDemand = 10000, float curvature = 1)
    {
        #pragma warning disable CS0618 // Type or member is obsolete
        //TODO: Remove this in a future refactor.
        marketBeingManaged.DemandStrategy.InitializeDemandForSpecificGood(marketBeingManaged, good, InitialDemand, minDemand, maxDemand, curvature);
        #pragma warning restore CS0618 // Type or member is obsolete
    }

    public LemonadeStandResultObject GetEffectiveElasticityForGood(Good good, ElasticityTypeEnum elasticity)
    {
        if (!good.Elasticities.TryGetValue(elasticity, out float elasticityValue))
        {
            return LemonadeStandResultObject.Failure(ResultTypeEnum.ElasticityNotFound, "");
        }
        return LemonadeStandResultObject.Success(extraData: elasticityValue);
    }
}
