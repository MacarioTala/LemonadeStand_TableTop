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
        TheEconomy.SetupForTests(new MockLogger());
        TestEconomy = TheEconomy.Instance;

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

        var Company1SellsLemonsToAnyone = new Order(null, Company1, lemon, 1000, 3.0m);
        var Company2BuysLemonsFromAnyone = new Order(Company2,null, lemon, 500, 3.0m);

        Company1.QueueOrder(CreateActionContext(Company1SellsLemonsToAnyone, TestMarket, Period));
        Company2.QueueOrder(CreateActionContext(Company2BuysLemonsFromAnyone, TestMarket, Period));

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
        var strategy = new LinearDemandStrategy();
        
        TestMarket.InitializeDemandForSpecificGood(lemon, 1000);
        Company1.GetInventory().AddGood(new InventoryEntry(lemon, 2000,3m,Period));

        var Company1BuysLemonsFromAnyone = new Order(Company1, null, lemon, 500, 3.0m);
        var lemonContext = new ActionContext { TradeToSubmit = Company1BuysLemonsFromAnyone, MarketToSubmitTo = TestMarket , Period = Period};
        TestMarket.QueueOrder(lemonContext);

        var expected = 1500;
        // Act
        var actual = ((iDemandStrategy)strategy).CalculateDemandForPeriod(TestMarket, Period)[lemon].CurrentDemand;
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
        Company1.GetInventory().AddGood(new InventoryEntry(lemon, 2000,3m,Period));
        Company2.GetInventory().AddGood(new InventoryEntry(water, 2000,3m,Period));

        var Company2BuysLemonsFromAnyone = new Order(Company2, null, lemon, 500, 3.0m);
        var Company1SellsLemonsToAnyone = new Order(null, Company1, lemon, 500, 3.0m);
        var Company2lemonContext = new ActionContext { TradeToSubmit = Company2BuysLemonsFromAnyone, MarketToSubmitTo = TestMarket , Period = Period};
        var Company1LemonContext = new ActionContext { TradeToSubmit = Company1SellsLemonsToAnyone, MarketToSubmitTo = TestMarket , Period = Period};
        Company2.QueueOrder(Company2lemonContext);
        Company1.QueueOrder(Company1LemonContext);

        var Company1BuysWaterFromAnyone = new Order(Company1, null, water, 500, 3.0m);
        var Company2SellsWaterToAnyone = new Order(null, Company2, water, 500, 3.0m);
        var Company1waterContext = new ActionContext { TradeToSubmit = Company1BuysWaterFromAnyone, MarketToSubmitTo = TestMarket , Period = Period};
        var Company2WaterContext = new ActionContext { TradeToSubmit = Company2SellsWaterToAnyone, MarketToSubmitTo = TestMarket , Period = Period};
        Company1.QueueOrder(Company1waterContext);
        Company2.QueueOrder(Company2WaterContext);

        var expected = new Dictionary<Good, int> {{lemon, 500}, {water, 500}};
        //Act
        TestMarket.ProcessCompanyOrders();
        var actual = ((iDemandStrategy)strategy).CalculateSupplyForPeriod(TestMarket, Period);
        //Assert
        Assert.AreEqual(expected, actual);
    }
#endregion


}