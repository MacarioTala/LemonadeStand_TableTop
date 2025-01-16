using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

[TestFixture]
public class SupplyAndDemandTests
{
    public Market test_market;
    Good lemon;
    Good water;
    Good sugar;
    Good lemonade;
    readonly Price_band band1 = new(.5m, 1.0m);
    readonly Price_band band2 = new(1.0m, 3.0m);
    readonly Price_band band3 = new(3.0m, 5.0m);
    readonly Price_band band4 = new(5.0m, 10.0m);
    readonly ITradeLogger trade_logger = new MockLogger();

    GameObject TestEconomy;

    [SetUp]
    public void Setup()
    {
        // Set up the Economy
        TestEconomy = new GameObject("TestEconomy");
        var economyComponent=TestEconomy.AddComponent<TheEconomy>();
        if (TheEconomy.Instance == null)
        {
            economyComponent.Initialize(trade_logger);
        }
        // Set up goods
        lemon = Good.CreateInstance("Lemon", band2, Rarity_enum.Common);
        water = Good.CreateInstance("Water", band1, Rarity_enum.Common);
        sugar = Good.CreateInstance("Sugar", band1, Rarity_enum.Common);
        lemonade = Good.CreateInstance("Lemonade", band3, Rarity_enum.Uncommon);

        // set test_market to the Initial Market
        test_market = Market.Factory.CreateStarterMarket("The First Market"
                                                        , CompanyLevelEnum.Market
                                                        , new LinearDemandStrategy());             
        //Make the market demand lemons and lemonade
        test_market.InitializeDemandForSpecificGood(lemonade,1000);
        test_market.InitializeDemandForSpecificGood(lemon,1000);
    }
#region UpdatePrices tests
    [Test]
    public void UpdatePricesIncreasesPriceByPriceIncrementRateWhenDemandThresholdIsReached()
    {
        // Arrange
        var testMarket = Market.Factory.CreateStarterMarket("Market To Test", CompanyLevelEnum.Market, new LinearDemandStrategy());
        var testPeriod = 0;
        var company1 = Company.Factory.Create("Test Company 1", CompanyLevelEnum.Beginner);
        company1.GetInventory().AddGood(new InventoryEntry(lemonade, 2000, 3.0m, 0));
        testMarket.RegisterCompany(company1);

        var buyLemonadeOrder = new Order(testMarket, company1, lemonade, 500, 3.0m);
        var buyLemonadeContext = new ActionContext { TradeToSubmit = buyLemonadeOrder, MarketToSubmitTo = testMarket, Period = testPeriod };
        testMarket.QueueMarketOrder(buyLemonadeContext);
        var currentLemonadePrice = lemonade.GetPrice();
        var priceIncrementRate = lemonade.Get_price_increment_rate();
        var expectedLemonPrice = Math.Round(currentLemonadePrice * (1 + priceIncrementRate), 2);
        // Act
        testMarket.ProcessCompanyOrders();
        testMarket.FulfillDemand();
        testMarket.UpdatePrices();   
        // Only one entry per good in market inventories
        var actualLemonade = testMarket.GetInventory().GetInventoryEntriesByGood(lemonade.GoodName).FirstOrDefault();
        var actualLemonadePrice = Math.Round(actualLemonade.good.GetPrice(),2);
        // Assert
        Assert.AreEqual(expectedLemonPrice, actualLemonadePrice);
    }

    [Test]
    public void IfMarketBuyingInAPeriodExceedsDemandThresholdIncreasePrices()
    {
        // Arrange
        var marketToTest = Market.Factory.CreateStarterMarket("Market To Test", CompanyLevelEnum.Market, new LinearDemandStrategy());
        marketToTest.InitializeDemandForSpecificGood(lemonade, 1000);
        var testPeriod = 0;
        var currentLemonadePrice = lemonade.GetPrice();
        var price_increment_rate = lemonade.Get_price_increment_rate();
        var expectedLemonadePrice = Math.Round(currentLemonadePrice * (1 + price_increment_rate), 2);
        var company = Company.Factory.Create("Test Company", CompanyLevelEnum.Beginner);
        marketToTest.RegisterCompany(company);
        company.GetInventory().AddGood(new InventoryEntry(lemonade, 2000, 3.0m, 0));

        var buyLemonadeOrder = new Order(marketToTest, company, lemonade, 1500, 3.0m); 
        var buyLemonadeContext = new ActionContext { TradeToSubmit = buyLemonadeOrder, MarketToSubmitTo = marketToTest, Period = testPeriod };
        marketToTest.QueueMarketOrder(buyLemonadeContext);

        // Act
        marketToTest.ProcessCompanyOrders();
        marketToTest.FulfillDemand();
        marketToTest.UpdatePrices();
        var actualLemonade = marketToTest.GetInventory().GetInventoryEntriesByGood(lemonade.GoodName).FirstOrDefault();
        var actualLemonadePrice = Math.Round(actualLemonade.good.GetPrice(),2);
        // Assert
        Assert.AreEqual(expectedLemonadePrice, actualLemonadePrice);
    }
    #endregion
    [Test]
    public void GetTotalSoldReturnsTotalAmountOfGoodSoldByAMarketInAPeriod()
    {
        // Arrange
        var testMarket = Market.Factory.CreateMarket("Market To Test", CompanyLevelEnum.Market, new LinearDemandStrategy());
        testMarket.SetCash(1000000);
        var period = 0;
        testMarket.GetInventory().AddGood(new InventoryEntry(lemon, 2000, 3.0m, period));

        var company1 = Company.Factory.Create("Test Company 1", CompanyLevelEnum.Beginner);
        company1.SetCash(10000);
        var company2 = Company.Factory.Create("Test Company 2", CompanyLevelEnum.Beginner);
        company2.SetCash(10000);
        var company3 = Company.Factory.Create("Test Company 3", CompanyLevelEnum.Beginner);
        company3.SetCash(10000);

        var company1SellLemonOrder = new Order(company1, testMarket, lemon, 500, 3.0m); 
        var company2SellLemonOrder = new Order(company2, testMarket, lemon, 500, 3.0m);
        var company3SellLemonOrder = new Order(company3, testMarket, lemon, 500, 3.0m);

        var company1SellLemonContext = new ActionContext { TradeToSubmit = company1SellLemonOrder, MarketToSubmitTo = testMarket, Period = period };
        var company2SellLemonContext = new ActionContext { TradeToSubmit = company2SellLemonOrder, MarketToSubmitTo = testMarket, Period = period };
        var company3SellLemonContext = new ActionContext { TradeToSubmit = company3SellLemonOrder, MarketToSubmitTo = testMarket, Period = period };
        
        testMarket.QueueMarketOrder(company1SellLemonContext);
        testMarket.QueueMarketOrder(company2SellLemonContext);
        testMarket.QueueMarketOrder(company3SellLemonContext);
        
        testMarket.ProcessCompanyOrders();
        
        const int expected = 1500;
        const int trading_period = 0;
        // Act
        var actual = testMarket.GetTotalSoldByMarket(trading_period,lemon);
        // Assert
        Assert.AreEqual(expected, actual);
    }
#region CalculateFulfillmentRate tests
    [Test]
    public void CalculateFulfillmentRateReturns1WhenDemandIsMet()
    {
        // Arrange
        var sellingCompany = Company.Factory.Create("Test Company", CompanyLevelEnum.Beginner);
        // Set up a market with demand for lemons
        Market MarketThatDemandsLemons = Market.Factory.CreateStarterMarket("Market That Demands Lemons", CompanyLevelEnum.Market, new LinearDemandStrategy());
        MarketThatDemandsLemons.InitializeDemandForSpecificGood(lemon, 1000);
        var period = 0;
    
        //give the selling company some lemons
        sellingCompany.GetInventory().AddGood(new InventoryEntry(lemon, 1000, 3.0m, 0));
        MarketThatDemandsLemons.RegisterCompany(sellingCompany);

        //create the ActionContext
        var testContext = new ActionContext
        {
            TradeToSubmit = new Order(MarketThatDemandsLemons, sellingCompany, lemon, 1000, 3.0m),
            MarketToSubmitTo = MarketThatDemandsLemons,
            Period = period
        };

        //have the market buy the lemons
        MarketThatDemandsLemons.QueueMarketOrder(testContext);
        MarketThatDemandsLemons.ProcessCompanyOrders();
        MarketThatDemandsLemons.FulfillDemand();
        MarketThatDemandsLemons.CalculateFulfillmentRates(testContext.Period);
        
        // Act
        var actualFulfillmentRate = MarketThatDemandsLemons.GetMarketDemand()[lemon].FulfilmentRate;
        // Assert
        Assert.AreEqual(1, actualFulfillmentRate);
    }
    [Test]
    public void CalculateFulfillmentRatesReturnsLessThan1WhenDemandIsNotMet()
    {
        // Arrange
        //Make a market that demands lemons
        Market MarketThatDemandsLemons = Market.Factory.CreateStarterMarket("Market That Demands Lemons", CompanyLevelEnum.Market, new LinearDemandStrategy());
        MarketThatDemandsLemons.InitializeDemandForSpecificGood(lemon, 1000);

        //Make a company to sell the lemons
        var sellingCompany = Company.Factory.Create("Test Company", CompanyLevelEnum.Beginner);
        MarketThatDemandsLemons.RegisterCompany(sellingCompany);
        //give the selling company some lemons
        sellingCompany.GetInventory().AddGood(new InventoryEntry(lemon, 1000, 3.0m, 0));
        var period = 0;

        //have the market buy some lemons
        var marketBuysLemons = new Order(MarketThatDemandsLemons, sellingCompany, lemon, 500, 3.0m);
        var lemonBuyingContext = new ActionContext { TradeToSubmit = marketBuysLemons, MarketToSubmitTo = MarketThatDemandsLemons ,Period= period};
        MarketThatDemandsLemons.QueueOrder(lemonBuyingContext);
        MarketThatDemandsLemons.ProcessCompanyOrders();
        MarketThatDemandsLemons.CalculateFulfillmentRates(lemonBuyingContext.Period);
        //Act
        var actualFulfillmentRate = MarketThatDemandsLemons.GetMarketDemand()[lemon].FulfilmentRate;
        // Assert
        Assert.Less(actualFulfillmentRate, 1);
    }
#endregion    
    [Test]
    public void UnleashMarketForcesIncreasesDemandWhenDemandIs60PercentFilled()
    {
        //Assert
        //Make a market that demands lemons
        Market marketThatDemandsLemons = Market.Factory.CreateStarterMarket("Market That Demands Lemons", CompanyLevelEnum.Market, new LinearDemandStrategy());
        marketThatDemandsLemons.InitializeDemandForSpecificGood(lemon, 1000);

        var expectedLemonDemand = 1400;
        var sellingCompany = Company.Factory.Create("Test Company", CompanyLevelEnum.Beginner);
        marketThatDemandsLemons.RegisterCompany(sellingCompany);
        var period = 0;

        //give the selling company some lemons
        sellingCompany.GetInventory().AddGood(new InventoryEntry(lemon, 900, 3.0m, 0));
        var marketBuysLemons = new Order(marketThatDemandsLemons, sellingCompany, lemon, 600, 3.0m);
        var lemonBuyingContext =new ActionContext { TradeToSubmit = marketBuysLemons, MarketToSubmitTo = marketThatDemandsLemons, Period = period };
    
        //Act
        //have the market buy some lemons
        marketThatDemandsLemons.QueueMarketOrder(lemonBuyingContext);
        marketThatDemandsLemons.ProcessCompanyOrders();
        marketThatDemandsLemons.UnleashMarketForces(lemonBuyingContext.Period);
        var actualLemonDemand = marketThatDemandsLemons.GetMarketDemand()[lemon].CurrentDemand;
        //Assert
        Assert.AreEqual(expectedLemonDemand, actualLemonDemand);
    }

    [Test]
    public void UnleashMarketForcesDecreasesDemandWhenDemandIs100PercentFilled()
    {
        //Assert
        var period = 0;
        var marketToTest = Market.Factory.CreateMarket("Market To Test", CompanyLevelEnum.Market, new LinearDemandStrategy());
        marketToTest.SetCash(1000000);
        var initialLemonadeDemand = 1000;
        marketToTest.InitializeDemandForSpecificGood(lemonade, initialLemonadeDemand);
        var currentLinearDemandAdjustment = 0.9;
        var expectedLemonadeDemand = initialLemonadeDemand * currentLinearDemandAdjustment;
        var sellingCompany = Company.Factory.Create("Test Company", CompanyLevelEnum.Beginner);
        marketToTest.RegisterCompany(sellingCompany);

        //give the selling company some lemonade
        sellingCompany.GetInventory().AddGood(new InventoryEntry(lemonade, 1000, 3.0m,0));
        //Act
        //have the market buy some lemonade
        var marketBuysLemonade = new Order(marketToTest, sellingCompany, lemonade, 1000, 3.0m);
        var testContext = new ActionContext
        {
            TradeToSubmit = marketBuysLemonade,
            MarketToSubmitTo = marketToTest,
            Period = period
        };
        marketToTest.QueueMarketOrder(testContext);
        marketToTest.ProcessCompanyOrders();
        marketToTest.UnleashMarketForces(testContext.Period);
        var actualLemonadeDemand = marketToTest.GetMarketDemand()[lemonade].CurrentDemand;
        //Assert
        Assert.AreEqual(expectedLemonadeDemand, actualLemonadeDemand);
    }

    [Test]
    public void GetTotalSupplyReturnsTotalSupplyOfGoodInInventory()
    {
        // Arrange
        var period = 0;
        var testMarket = Market.Factory.CreateMarket("Test Market", CompanyLevelEnum.Market, new LinearDemandStrategy());
        var lemonInventoryEntry = new InventoryEntry(lemon, 500, 3.0m, period);
        var lemonInventoryEntry2 = new InventoryEntry(lemon, 500, 3.0m, period+1);
        var lemonInventoryEntry3 = new InventoryEntry(lemon, 500, 3.0m, period+2);
        testMarket.GetInventory().AddGood(lemonInventoryEntry);
        testMarket.GetInventory().AddGood(lemonInventoryEntry2);
        testMarket.GetInventory().AddGood(lemonInventoryEntry3);
        const int expected = 1500;
        // Act
        var actual = testMarket.GetTotalSupply(lemon);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TearDown]
    public void TearDown()
    {
        ScriptableObject.DestroyImmediate(test_market);
        ScriptableObject.DestroyImmediate(lemon);
        ScriptableObject.DestroyImmediate(water);
        ScriptableObject.DestroyImmediate(sugar);
        GameObject.DestroyImmediate(TestEconomy);
    }
}