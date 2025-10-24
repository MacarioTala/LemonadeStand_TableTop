using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MarketBuilder
{
    public static Market WithCash(this Market market, decimal cash)
    {
        market.SetCash(cash);
        return market;
    }

    public static Market WithDemandStrategy(this Market market, iDemandStrategy strategy)
    {
        market.SetDemandStrategy(strategy);
        return market;
    }

    public static Market WithDemandManager(this Market market, iDemandManager manager)
    {
        market.SetDemandManager(manager);
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
    public static Market WithMarketEventManager(this Market market, iMarketEventManager manager = null)
    {
        if (manager is null)
        {
            market.SetMarketEventManager(new DefaultMarketEventManager());
        }
        else
        {
            market.SetMarketEventManager(manager);
        }
        return market;
    }
    public static Market WithMarketInteractionManager(this Market market, iMarketInteractionManager manager = null)
    {
        if (manager is null)
        {
            market.SetMarketInteractionManager(new DefaultMarketInteractionManager());
        }
        else
        {
            market.SetMarketInteractionManager(manager);
        }
        return market;
    }
    public static Market WithPriceManager(this Market market, iPriceManager priceManager)
    {
        market.SetPriceManager(priceManager);
        return market;
    }
    public static Market WithSupplyProvider(this Market market, iSupplyHelper supplyProvider)
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
        if (market.DemandStrategy == null)
        {
            Debug.LogError("Market.DemandStrategy not set, cannot wire orderFulfilled event handler.");
            return market;
        }

        market.OrderFulfilled += market.DemandStrategy.OnOrderFulfilled;
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
        market.SetStrategy(strategy);
        return market;
    }

    public static Market WithLevel(this Market market, AgentLevelEnum level)
    {
        market.company_level = level;
        return market;
    }
    public static Market Named(this Market market, string name)
    {
        market.Name = name;
        return market;
    }

    public static Market PopulatedWith(this Market market, List<PopulationAgent> marketParticipants)
    {

        foreach (var particpant in marketParticipants)
        {
            market.RegisterMarketParticipant(particpant);
        }
        return market;
    }

    public static Market InitializedWith(this Market market, iMarketInitializer initializer)
    {
        if (initializer == null)
        {
            Debug.LogError("MarketBuilder: initializer is null, cannot initialize");
            return market;
        }
        initializer.InitializeMarket(market);
        return market;
    }
    #region defaults
    public static Market EnsureDefaults(this Market market)
    {
        EnsureDefaultMarketEventManager(market);
        EnsureDefaultDemandManager(market);
        EnsureDefaultDemographicManager(market);
        EnsureDefaulMarketDataManager(market);
        EnsureDefaultPriceManager(market);
        EnsureDefaultTransactionManager(market); // Note: Transaction Manager always set before Trade processor. 
        EnsureDefaultTradeProcessor(market);
        EnsureDefaultDemandStrategy(market);
        EnsureAtLeastOnePriceModifier(market);
        EnsureDefaultMarketLevel(market);
        EnsureDefaultMarketInteractionManager(market);
        EnsureDefaultSupplyProvider(market);

        return market;
    }

    private static void EnsureDefaultSupplyProvider(Market market)
    {
        if (market.SupplyProvider is null)
            market.SetSupplyProvider(new DefaultSupplyHelper());
    }

    private static void EnsureDefaultMarketInteractionManager(Market market)
    {
        if(market.MarketInteractionManager is null)
            market.SetMarketInteractionManager(new DefaultMarketInteractionManager());
    }

    private static void EnsureDefaultMarketLevel(Market market)
    {
        market.company_level = AgentLevelEnum.Market;
    }

    private static void EnsureAtLeastOnePriceModifier(Market market)
    {
        if(!market.PriceModifiers.Any(x=>x is SupplyDemandModifier))
            market.AddPriceModifier(new SupplyDemandModifier());
    }

    private static void EnsureDefaultDemandStrategy(Market market)
    {
        if (market.DemandStrategy is null)
            market.SetDemandStrategy(ScriptableObject.CreateInstance<LinearDemandStrategy>());
    }

    private static void EnsureDefaultTransactionManager(Market market)
    {
        if (market.TransactionManager is null)
            market.SetTransactionManager(new DefaultTransactionManager());
    }

    private static void EnsureDefaultTradeProcessor(Market market)
    {
        if (market.TradeProcessor is null)
            market.SetTradeProcessor(new DefaultTradeProcessor());
    }

    private static void EnsureDefaultPriceManager(Market market)
    {
        if (market.PriceManager is null)
            market.SetPriceManager(new DefaultPriceManager());
    }

    private static void EnsureDefaulMarketDataManager(Market market)
    {
        if (market.MarketDataManager is null)
            market.SetMarketDataManager(new DefaultMarketDataManager());
    }

    private static void EnsureDefaultDemographicManager(Market market)
    {
        if (market.DemographicManager is null)
            market.SetDemographicManager(new DefaultDemographicManager());
    }

    private static void EnsureDefaultDemandManager(Market market)
    {
        if (market.DemandManager is null)
            market.SetDemandManager(new DefaultDemandManager());
    }

    private static void EnsureDefaultMarketEventManager(Market market)
    {
        if (market.MarketEventManager is null)
            market.SetMarketEventManager(new DefaultMarketEventManager());
    }
    #endregion
}