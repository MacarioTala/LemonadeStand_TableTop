using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using static TestHelpers;

[TestFixture]
public class BasicDemandStrategyTests
{
    TheEconomy TestEconomy;
    Market TestMarket;
    Company Company1;
    Company Company2;
    const int Period = 0;

    iDemandStrategy strategy;

    Good lemon;
    Good water;
    Good sugar;
    [SetUp]
    public void Setup ()
    {
        var economyObject = new GameObject();
        TestEconomy = economyObject.AddComponent<TheEconomy>();
        TestEconomy.Initialize(new MockLogger());

        TestMarket = Market.Factory.CreateStarterMarket("Test Market", CompanyLevelEnum.Market, new LinearDemandStrategy());
        strategy = new LinearDemandStrategy();
        TestMarket.DemandStrategy = strategy;
        
        Company1 = Company.Factory.Create("Company 1", CompanyLevelEnum.Beginner);
        Company2 = Company.Factory.Create("Company 2", CompanyLevelEnum.Beginner);
        TestMarket.RegisterCompany(Company1);
        TestMarket.RegisterCompany(Company2);

        lemon = Good.CreateInstance("Lemon", new Price_band(.5m, 1.0m), Rarity_enum.Common);
        water = Good.CreateInstance("Water", new Price_band(.5m, 1.0m), Rarity_enum.Common);
        sugar = Good.CreateInstance("Sugar", new Price_band(.5m, 1.0m), Rarity_enum.Common);
    }
    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(TestEconomy.gameObject);
        Company1 = null;
        Company2 = null;
        TestMarket = null;
        TestEconomy = null;
    }

    [Test]
    public void GetTotalBoughtReturnsZeroInAPeriodWithNoMarketOrders()
    {
        // Arrange
        Company1.GetInventory().AddGood(new InventoryEntry(lemon, 2000,3m,Period));

        var Company1SellsLemonsToCompany2 = new Order(Company2, Company1, lemon, 500, 3.0m);
        var lemonOrder2 = new Order(Company2, Company1, lemon, 500, 3.0m);
        
        Company1.QueueOrder(CreateActionContext(Company1SellsLemonsToCompany2, TestMarket, Period));
        Company2.QueueOrder(CreateActionContext(lemonOrder2, TestMarket, Period));
        
        const int expected = 0;
        const int tradingPeriod = 0;
        
        // Act
        TestMarket.ProcessCompanyOrders();
        TestMarket.FulfillDemand();
        var actual = ((iDemandStrategy)strategy).GetTotalBoughtByMarket(TestMarket, lemon, tradingPeriod);
        // Assert
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void GetTotalBoughtByMarketReturnsOnlyMarketBuys()//You are here
    {
        // Arrange
        TestMarket.InitializeDemandForSpecificGood(lemon, 1000);
        Company1.GetInventory().AddGood(new InventoryEntry(lemon, 2000,3m,Period));

        var Company1SellsLemonsToCompany2 = new Order(Company2, Company1, lemon, 500, 3.0m);
        var Company2BuysLemonsFromCompany1 = new Order(Company2,Company1, lemon, 500, 3.0m);
        var Company1SellsLemonsToAnyone = new Order(null, Company1, lemon, 500, 3.0m);
        var MarketBuysLemonsFromCompany1 = new Order(TestMarket, Company1, lemon, 500, 3.0m);
        

        Company1.QueueOrder(CreateActionContext(Company1SellsLemonsToCompany2, TestMarket, Period));
        Company1.QueueOrder(CreateActionContext(Company1SellsLemonsToAnyone, TestMarket, Period));
        Company2.QueueOrder(CreateActionContext(Company2BuysLemonsFromCompany1, TestMarket, Period));
        TestMarket.QueueOrder(CreateActionContext(MarketBuysLemonsFromCompany1, TestMarket, Period));

        const int expected = 500;
        // Act
        TestMarket.ProcessCompanyOrders();
        TestMarket.FulfillDemand();
        var actual = ((iDemandStrategy)strategy).GetTotalBoughtByMarket(TestMarket, lemon, Period);
        // Assert
        Assert.AreEqual(expected, actual);
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
        var strategy = new LinearDemandStrategy();
        Company1.GetInventory().AddGood(new InventoryEntry(lemon, 2000,3m,Period));

        var lemonOrder = new Order(Company2, Company1, lemon, 500, 3.0m);
        var lemonContext = new ActionContext { TradeToSubmit = lemonOrder, MarketToSubmitTo = TestMarket , Period = Period};
        TestMarket.QueueOrder(lemonContext);

        var expected = 500;
        // Act
        var actual = ((iDemandStrategy)strategy).CalculateDemandForPeriod(TestMarket, Period)[lemon].CurrentDemand;
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