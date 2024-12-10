using System.Linq;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class IntegrationTests
{
    TheEconomy TestEconomy;
    Good Lemonade;
    [SetUp]
    public void SetUp()
    {
        var EconomyObject = new GameObject();
        TestEconomy = EconomyObject.AddComponent<TheEconomy>();
        TestEconomy.Initialize(new MockLogger());
        Lemonade = Good.CreateInstance("Lemonade", new Price_band(.5m, 2m), Rarity_enum.Common);
    }
    [Test]
    public void UnleashMarketForcesFulfilsMarketDemandWhenCalledFromMarket()
    {
        //Arrange
        var marketToTest = Market.Factory.CreateStarterMarket("Starter Market",
                                                              CompanyLevelEnum.Market,
                                                              new LinearDemandStrategy() );
        var company1 = Company.Factory.Create("Company1", CompanyLevelEnum.Beginner);
        company1.GetInventory().AddGood(new InventoryEntry(Lemonade, 2000, 3m,0));
        marketToTest.RegisterCompany(company1);
        var company1Order = new Trade(marketToTest, company1, Lemonade, 1000, 3.5m);
        var company1Context = new ActionContext{TradeToSubmit = company1Order,
                                                MarketToSubmitTo = marketToTest};
        var period = 1;
        var expected = 1000;
        //Act
        company1.QueueOrder(company1Context);
        marketToTest.UnleashMarketForces(period);
        var actual=company1Order.FilledQuantity;
        //Assert
        Assert.AreEqual(expected, actual);    
    }
    [Test]
    public void UnleashMarketForcesFulfillsMarketDemandWhenCalledFromTheEconomy()
    {
        //Arrange
        var marketToTest = Market.Factory.CreateStarterMarket("Starter Market",
                                                              CompanyLevelEnum.Market,
                                                              new LinearDemandStrategy() );
        var company1 = Company.Factory.Create("Company1", CompanyLevelEnum.Beginner);
        company1.GetInventory().AddGood(new InventoryEntry(Lemonade, 2000, 3m,0));
        marketToTest.RegisterCompany(company1);
        var company1Order = new Trade(marketToTest, company1, Lemonade, 1000, 3.5m);
        var company1Context = new ActionContext{TradeToSubmit = company1Order,
                                                MarketToSubmitTo = marketToTest};
        var expected = 1000;
        //Act
        company1.QueueOrder(company1Context);
        TestEconomy.EndTradingPeriod();
        var actual=company1Order.FilledQuantity;
        //Assert
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void EndTradingPeriodSendsTradesThatMarketHasQueued()
    {
        // Arrange
        var market = Market.Factory.CreateMarket("Market Test", CompanyLevelEnum.Market, new LinearDemandStrategy());
        market.SetCash(1000000);
        var company1 = Company.Factory.Create("Company1", CompanyLevelEnum.Beginner);
        company1.GetInventory().AddGood(new InventoryEntry(Lemonade, 2000, 3m,0));
        var trade1 = new Trade(market, company1, Lemonade, 500, 2.0m);
        var trade2 = new Trade(market, company1, Lemonade, 1000, 3.5m);
        var theFirstMarket = (Market)TheEconomy.Instance.companies.Where(c => c.Name == "The First Market").FirstOrDefault();
        TheEconomy.Instance.RemoveMarket(theFirstMarket);
        TheEconomy.Instance.RegisterCompany(market);
        
        market.QueueOrder(new ActionContext { TradeToSubmit = trade1, MarketToSubmitTo = market });
        market.QueueOrder(new ActionContext { TradeToSubmit = trade2, MarketToSubmitTo = market });
        var expected = 2;
        
        // Act
        TestEconomy.EndTradingPeriod();
        TestEconomy.EndTradingPeriod();
        var actual = TestEconomy._trade_logger.GetTradeCount();
        // Assert
        Assert.AreEqual(expected, actual);
    }

     [Test]
    public void EndTradingPeriodSendsTradesThatMarketHasQueuedWhenTwoMarketsArePresent()
    {
        // Arrange
        var market = Market.Factory.CreateMarket("Market Test", CompanyLevelEnum.Market, new LinearDemandStrategy());
        market.SetCash(1000000);
        var company1 = Company.Factory.Create("Company1", CompanyLevelEnum.Beginner);
        company1.GetInventory().AddGood(new InventoryEntry(Lemonade, 2000, 3m,0));
        var trade1 = new Trade(market, company1, Lemonade, 500, 2.0m);
        var trade2 = new Trade(market, company1, Lemonade, 1000, 3.5m);
        var theFirstMarket = (Market)TheEconomy.Instance.companies.Where(c => c.Name == "The First Market").FirstOrDefault();
        TheEconomy.Instance.RegisterCompany(market);
        
        market.QueueOrder(new ActionContext { TradeToSubmit = trade1, MarketToSubmitTo = market });
        market.QueueOrder(new ActionContext { TradeToSubmit = trade2, MarketToSubmitTo = market });
        var expected = 2;
        
        // Act
        TestEconomy.EndTradingPeriod();
        TestEconomy.EndTradingPeriod();
        var actual = TestEconomy._trade_logger.GetTradeCount();
        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(TestEconomy.gameObject);
    }
}