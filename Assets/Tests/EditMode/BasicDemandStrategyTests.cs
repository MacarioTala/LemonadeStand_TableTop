using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class BasicDemandStrategyTests
{
    TheEconomy TestEconomy;
    Good lemon;
    Good water;
    Good sugar;
    [SetUp]
    public void Setup ()
    {
        var economyObject = new GameObject();
        TestEconomy = economyObject.AddComponent<TheEconomy>();
        TestEconomy.Initialize(new MockLogger());
        lemon = Good.CreateInstance("Lemon", new Price_band(.5m, 1.0m), Rarity_enum.Common);
        water = Good.CreateInstance("Water", new Price_band(.5m, 1.0m), Rarity_enum.Common);
        sugar = Good.CreateInstance("Sugar", new Price_band(.5m, 1.0m), Rarity_enum.Common);
    }
    [Test]
    public void GetTotalBoughtReturnsZeroInAPeriodWithNoMarketOrders()
    {
        // Arrange
        var period = 0;
        var strategy = new LinearDemandStrategy();
        var marketToTest = Market.Factory.CreateStarterMarket("Market To Test", CompanyLevelEnum.Market,strategy);
        var company1 = Company.Factory.Create("Company 1", CompanyLevelEnum.Beginner);
        marketToTest.RegisterCompany(company1);
        var company2 = Company.Factory.Create("Company 2", CompanyLevelEnum.Beginner);
        marketToTest.RegisterCompany(company2);
        company1.GetInventory().AddGood(new InventoryEntry(lemon, 2000,3m,period));

        var lemonOrder = new Order(company2, company1, lemon, 500, 3.0m);
        var lemonContext1 = new ActionContext { TradeToSubmit = lemonOrder, MarketToSubmitTo = marketToTest , Period = period};
        var lemonOrder2 = new Order(company2, company1, lemon, 500, 3.0m);
        var lemonContext2 = new ActionContext { TradeToSubmit = lemonOrder2, MarketToSubmitTo = marketToTest , Period = period};
        var lemonOrder3 = new Order(company2, company1, lemon, 500, 3.0m);
        var lemonContext3 = new ActionContext { TradeToSubmit = lemonOrder3, MarketToSubmitTo = marketToTest , Period = period};
        
        marketToTest.QueueOrder(lemonContext1);
        marketToTest.QueueOrder(lemonContext2);
        marketToTest.QueueOrder(lemonContext3);

        const int expected = 0;
        const int tradingPeriod = 0;
        
        // Act
        marketToTest.ProcessCompanyOrders();
        marketToTest.FulfillDemand();
        var actual = ((iDemandStrategy)strategy).GetTotalBoughtByMarket(marketToTest, lemon, tradingPeriod);
        // Assert
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void GetTotalBoughtByMarketReturnsOnlyMarketBuys()
    {
        // Arrange
        var period = 0;
        var strategy = new LinearDemandStrategy();
        var marketToTest = Market.Factory.CreateStarterMarket("Market To Test", CompanyLevelEnum.Market,strategy);
        marketToTest.InitializeDemandForSpecificGood(lemon, 1000);
        var company1 = Company.Factory.Create("Company 1", CompanyLevelEnum.Beginner);
        marketToTest.RegisterCompany(company1);
        var company2 = Company.Factory.Create("Company 2", CompanyLevelEnum.Beginner);
        marketToTest.RegisterCompany(company2);
        company1.GetInventory().AddGood(new InventoryEntry(lemon, 2000,3m,period));

        var lemonOrder = new Order(company2, company1, lemon, 500, 3.0m);
        var lemonContext = new ActionContext { TradeToSubmit = lemonOrder, MarketToSubmitTo = marketToTest , Period = period};
        
        var marketOrder = new Order(marketToTest, company1, lemon, 500, 3.0m);
        var marketContext = new ActionContext { TradeToSubmit = marketOrder, MarketToSubmitTo = marketToTest , Period = period};
        marketToTest.QueueOrder(lemonContext);
        marketToTest.QueueOrder(marketContext);

        const int expected = 500;
        // Act
        marketToTest.ProcessCompanyOrders();
        marketToTest.FulfillDemand();
        var actual = ((iDemandStrategy)strategy).GetTotalBoughtByMarket(marketToTest, lemon, period);
        // Assert
        Assert.AreEqual(expected, actual);
    }
    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(TestEconomy.gameObject);
    }
#region default implementation tests

    [Test]
    public void CalculateDemandForPeriodReturnsBaseMarketDemandWhenNoOrdersExist()
    {
        // Arrange
        var period = 0;
        var strategy = new LinearDemandStrategy();
        var marketToTest = Market.Factory.CreateStarterMarket("Market To Test", CompanyLevelEnum.Market,strategy);
        marketToTest.InitializeDemandForSpecificGood(lemon, 1000);
        var expected = 1000;
        // Act
        var actual = ((iDemandStrategy)strategy).CalculateDemandForPeriod(marketToTest, period)[lemon].CurrentDemand;
        // Assert
        Assert.AreEqual(expected, actual);
    }
    [Test]
    public void CalculateDemandForPeriodReturnsBaseMarketDemandPlusOrdersWhenOrdersExist()
    {
        // Arrange
        var period = 0;
        var strategy = new LinearDemandStrategy();
        var marketToTest = Market.Factory.CreateStarterMarket("Market To Test", CompanyLevelEnum.Market,strategy);
        marketToTest.InitializeDemandForSpecificGood(lemon, 1000);
        var company1 = Company.Factory.Create("Company 1", CompanyLevelEnum.Beginner);
        marketToTest.RegisterCompany(company1);
        var company2 = Company.Factory.Create("Company 2", CompanyLevelEnum.Beginner);
        marketToTest.RegisterCompany(company2);
        company1.GetInventory().AddGood(new InventoryEntry(lemon, 2000,3m,period));

        var lemonOrder = new Order(company2, company1, lemon, 500, 3.0m);
        var lemonContext = new ActionContext { TradeToSubmit = lemonOrder, MarketToSubmitTo = marketToTest , Period = period};
        marketToTest.QueueOrder(lemonContext);

        var expected = 1500;
        // Act
        var actual = ((iDemandStrategy)strategy).CalculateDemandForPeriod(marketToTest, period)[lemon].CurrentDemand;
        // Assert
        Assert.AreEqual(expected, actual);
    }
    [Test]
    public void CalculateDemandForPeriodCountsOnlyOrdersIfNoMarketDemandExists()
    {
        // Arrange
        var period = 0;
        var strategy = new LinearDemandStrategy();
        var marketToTest = Market.Factory.CreateStarterMarket("Market To Test", CompanyLevelEnum.Market,strategy);
        var company1 = Company.Factory.Create("Company 1", CompanyLevelEnum.Beginner);
        marketToTest.RegisterCompany(company1);
        var company2 = Company.Factory.Create("Company 2", CompanyLevelEnum.Beginner);
        marketToTest.RegisterCompany(company2);
        company1.GetInventory().AddGood(new InventoryEntry(lemon, 2000,3m,period));

        var lemonOrder = new Order(company2, company1, lemon, 500, 3.0m);
        var lemonContext = new ActionContext { TradeToSubmit = lemonOrder, MarketToSubmitTo = marketToTest , Period = period};
        marketToTest.QueueOrder(lemonContext);

        var expected = 500;
        // Act
        var actual = ((iDemandStrategy)strategy).CalculateDemandForPeriod(marketToTest, period)[lemon].CurrentDemand;
        // Assert
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void CalculateSupplyForPeriodReturnsTotalQuantityOfAllFilledTrades()
    {
        //Arrange
        var period = 0;
        var strategy = new LinearDemandStrategy();
        var marketToTest = Market.Factory.CreateStarterMarket("Market To Test", CompanyLevelEnum.Market,strategy);
        var company1 = Company.Factory.Create("Company 1", CompanyLevelEnum.Beginner);
        marketToTest.RegisterCompany(company1);
        var company2 = Company.Factory.Create("Company 2", CompanyLevelEnum.Beginner);
        marketToTest.RegisterCompany(company2);
        company1.GetInventory().AddGood(new InventoryEntry(lemon, 2000,3m,period));
        company2.GetInventory().AddGood(new InventoryEntry(water, 2000,3m,period));

        var lemonOrder = new Order(company2, company1, lemon, 500, 3.0m);
        var lemonContext = new ActionContext { TradeToSubmit = lemonOrder, MarketToSubmitTo = marketToTest , Period = period};
        marketToTest.QueueOrder(lemonContext);

        var waterOrder = new Order(company1, company2, water, 500, 3.0m);
        var waterContext = new ActionContext { TradeToSubmit = waterOrder, MarketToSubmitTo = marketToTest , Period = period};
        marketToTest.QueueOrder(waterContext);

        var expected = new Dictionary<Good, int> {{lemon, 500}, {water, 500}};
        //Act
        marketToTest.ProcessCompanyOrders();
        var actual = ((iDemandStrategy)strategy).CalculateSupplyForPeriod(marketToTest, period);
        //Assert
        Assert.AreEqual(expected, actual);
    }
#endregion
}