using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using UnityEngine;

[TestFixture]
public class MarketStatusTests
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
    public void GetMarketTradesInPeriodReturnsAllExecutedTrades()
    {
        // Arrange
        var period = 0;
        var strategy = new LinearDemandStrategy();
        var marketToTest = Market.Factory.CreateStarterMarket("Market To Test", CompanyLevelEnum.Market,strategy);
        
        var company1 = Company.Factory.Create("Company 1", CompanyLevelEnum.Beginner);
        marketToTest.RegisterCompany(company1);
        company1.GetInventory().AddGood(new InventoryEntry(lemon, 2000,3m,period));
        company1.SetCash(5000);

        var company2 = Company.Factory.Create("Company 2", CompanyLevelEnum.Beginner);
        marketToTest.RegisterCompany(company2);
        company2.GetInventory().AddGood(new InventoryEntry(water, 2000,3m,period));
        company2.SetCash(5000);

        var company1Order = new Order(company2, company1, lemon, 500, 3.0m);
        var company1Context = new ActionContext { TradeToSubmit = company1Order, MarketToSubmitTo = marketToTest , Period = period};

        var company2Order = new Order(company1, company2, water, 500, 3.0m);
        var company2Context = new ActionContext { TradeToSubmit = company2Order, MarketToSubmitTo = marketToTest , Period = period};

        marketToTest.QueueOrder(company1Context);
        marketToTest.QueueOrder(company2Context);

        var expected = new List<MarketTrade>
        {
            new(company1Order, period),
            new(company2Order, period)
        };
        // Act
        marketToTest.ProcessCompanyOrders();
        var actual=marketToTest.GetMarketTradesInPeriod(period);
        // Assert
        var inExpectedNotInActual = expected.Except(actual).ToList();
        var inActualNotInExpected = actual.Except(expected).ToList();
        var listsAreEqual = inExpectedNotInActual.Count == 0 && inActualNotInExpected.Count == 0;
        
        Debug.Log("Expected: " + string.Join(", ", expected));
        Debug.Log("Actual: " + string.Join(", ", actual));
        Assert.IsTrue(listsAreEqual);
    }
    [Test]
    public void GetMarketTradesInPeriodOnlyReturnsTradesForTheCurrentPeriod()
    {
        // Arrange
        var startingPeriod = 0;
        var strategy = new LinearDemandStrategy();
        var marketToTest = Market.Factory.CreateStarterMarket("Market To Test", CompanyLevelEnum.Market,strategy);
        
        var company1 = Company.Factory.Create("Company 1", CompanyLevelEnum.Beginner);
        marketToTest.RegisterCompany(company1);
        company1.GetInventory().AddGood(new InventoryEntry(lemon, 2000,3m,startingPeriod));
        company1.SetCash(5000);

        var company2 = Company.Factory.Create("Company 2", CompanyLevelEnum.Beginner);
        marketToTest.RegisterCompany(company2);
        company2.GetInventory().AddGood(new InventoryEntry(water, 2000,3m,startingPeriod));
        company2.SetCash(5000);

        var company1Order = new Order(company2, company1, lemon, 500, 3.0m);
        var company1Context = new ActionContext { TradeToSubmit = company1Order, MarketToSubmitTo = marketToTest , Period = startingPeriod};
        marketToTest.QueueOrder(company1Context);
        
        var company2Order = new Order(company1, company2, water, 500, 3.0m);
        var company2Context = new ActionContext { TradeToSubmit = company2Order, MarketToSubmitTo = marketToTest , Period = startingPeriod};

        var expected = new List<MarketTrade>
        {
            new(company1Order, startingPeriod)
        };
        // Act
        marketToTest.ProcessCompanyOrders();
        company2.QueueOrder(company2Context);
        marketToTest.CurrentPeriod++;
        marketToTest.ProcessCompanyOrders();
        var actual=marketToTest.GetMarketTradesInPeriod(startingPeriod);
        // Assert
        var inExpectedNotInActual = expected.Except(actual).ToList();
        var inActualNotInExpected = actual.Except(expected).ToList();
        var listsAreEqual = inExpectedNotInActual.Count == 0 && inActualNotInExpected.Count == 0;
        
        Debug.Log("Expected: " + string.Join(", ", expected));
        Debug.Log("Actual: " + string.Join(", ", actual));
        Assert.IsTrue(listsAreEqual);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(TestEconomy.gameObject);
    }
}