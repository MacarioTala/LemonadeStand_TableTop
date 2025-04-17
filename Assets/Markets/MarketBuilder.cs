using System;
using UnityEngine;

public static class MarketBuilder
{
    public static Market WithDemandStrategy(this Market market, iDemandStrategy strategy)
    {
        market.SetDemandStrategy(strategy);
        return market;
    }
    public static Market WithConsumptionManager(this Market market, iConsumptionManager consumptionManager)
    {
        market.SetConsumptionManager(consumptionManager);
        return market;
    }
    public static Market WithDataService(this Market market, iMarketDataService dataService)
    {
        market.SetMarketDataService(dataService);
        return market;
    }
    public static Market WithDemographicManager(this Market market, iDemographicManager demographicManager)
    {
        market.SetDemographicManager(demographicManager);
        return market;
    }

    public static Market WithPriceManager(this Market market, iPriceManager priceManager)
    {
        market.SetPriceManager(priceManager);
        return market;
    }
    public static Market WithSupplyProvider(this Market market, iSupplyProvider supplyProvider)
    {
        market.SetSupplyProvider(supplyProvider);
        return market;
    }
    
    public static Market WithTradeProcessor(this Market market, iTradeProcessor tradeProcessor)
    {
        market.SetTradeProcessor(tradeProcessor);
        return market;
    }
    public static Market WithTransactionManager(this Market market, iTransactionManager transactionManager)
    {
        market.SetTransactionManager(transactionManager);
        return market;
    }

    public static Market WithOrderFulfilledEvents(this Market market)
    {
        if(market.DemandStrategy == null)
        {
            Debug.LogError("Market.DemandStrategy not set, cannot wire orderFulfilled event handler.");
            return market;
        }
        
        market.OrderFulfilled += market.DemandStrategy.OnOrderFulfilled;
        return market;
    }
    
    public static Market WithPopulation(this Market market, int population)
    {
        if(market.DemographicManager == null)
        {
            Debug.LogError("Market.DemographicManager not set, cannot set population.");
            return market;
        }
        market.SetInitialPopulation(population);
        return market;
    }

    public static Market WithCash(this Market market, decimal cash)
    {
        market.SetCash(cash);
        return market;
    }
    public static Market WithMarketDataManager(this Market market, iMarketDataManager marketDataManager)
    {
        market.SetMarketDataManager(marketDataManager);
        return market;
    }

    public static Market WithPriceModifier(this Market market, iPriceModifier priceModifier)
    {
        market.AddPriceModifier(priceModifier);
        return market;
    }

    public static Market WithMarketStrategy(this Market market, iStrategy strategy)
    {
        market.SetMarketStrategy(strategy);
        return market;
    }

    public static Market Named(this Market market, string name)
    {
        market.Name = name;
        return market;
    }

    public static Market WithLevel(this Market market, CompanyLevelEnum level)
    {
        market.company_level = level;
        return market;
    }
}