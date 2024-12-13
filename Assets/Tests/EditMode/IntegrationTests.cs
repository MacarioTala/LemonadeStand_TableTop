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
#region Recording Trades
    [Test]
    public void MarketProcessCompanyOrdersDoesNotDuplicateRecordingTrades()
    {
        // Arrange
        var market = Market.Factory.CreateMarket("Test Market", CompanyLevelEnum.Market, new LinearDemandStrategy());
        var buyer = Company.Factory.Create("Buyer", CompanyLevelEnum.Beginner);
        var seller = Company.Factory.Create("Seller", CompanyLevelEnum.Beginner);
        market.RegisterCompany(buyer);
        market.RegisterCompany(seller);
        var good =Good.CreateInstance("Good", new Price_band(1m, 2m), Rarity_enum.Common);
        var sellersInventory = seller.GetInventory();
        sellersInventory.AddGood(new InventoryEntry(good, 10, 5, 0));

        var actionContext = new ActionContext
        {
            TradeToSubmit = new Order(buyer, seller, good, 10, 5),
            MarketToSubmitTo = market,
            Period = 0
        };
        buyer.QueueOrder(actionContext);
        // Act
        market.ProcessCompanyOrders();

        // Assert
        var recordedTrades = market.GetMarketTradesInPeriod(0);
        Assert.AreEqual(1, recordedTrades.Count);
        Assert.AreEqual(buyer, recordedTrades[0].RecordedTrade.Buyer);
        Assert.AreEqual(seller, recordedTrades[0].RecordedTrade.Seller);
        Assert.AreEqual(good, recordedTrades[0].RecordedTrade.Good);
        Assert.AreEqual(10, recordedTrades[0].RecordedTrade.Quantity);
    }
    [Test]
    public void MarketDoesNotAllowQueueingDuplicateTrades()
    {
        // Arrange
        var market = Market.Factory.CreateMarket("Test Market", CompanyLevelEnum.Market, new LinearDemandStrategy());
        var buyer = Company.Factory.Create("Buyer", CompanyLevelEnum.Beginner);
        var seller = Company.Factory.Create("Seller", CompanyLevelEnum.Beginner);
        market.RegisterCompany(buyer);
        market.RegisterCompany(seller);
        var good =Good.CreateInstance("Good", new Price_band(1m, 2m), Rarity_enum.Common);
        var sellersInventory = seller.GetInventory();
        sellersInventory.AddGood(new InventoryEntry(good, 10, 5, 0));

        var actionContext = new ActionContext
        {
            TradeToSubmit = new Order(buyer, seller, good, 10, 5),
            MarketToSubmitTo = market,
            Period = 0
        };
        var expected = LemonadeStandResultObject.Failure(ResultTypeEnum.DuplicateOrder, "Order already exists in the queue");
        // Act
        var actual = buyer.QueueOrder(actionContext);
        actual=buyer.QueueOrder(actionContext);

        // Assert
        Assert.AreEqual(expected, actual);
    }

#endregion
#region Time
    [Test]
    public void EndTradingPeriodIncrementsPeriod()
    {
        // Arrange
        var currentPeriod = TestEconomy.tradingPeriod;
        var expected = currentPeriod + 1;
        // Act
        TestEconomy.EndTradingPeriod();
        var actual = TestEconomy.tradingPeriod;
        // Assert
        Assert.AreEqual(expected, actual);
    }
    [Test]
    public void EndTradingPeriodIncrementsMarketPeriod()
    {
        // Arrange
        var market = Market.Factory.CreateMarket("Market Test", CompanyLevelEnum.Market, new LinearDemandStrategy());
        var currentPeriod = market.CurrentPeriod;
        var expected = currentPeriod + 1;
        TheEconomy.Instance.RegisterCompany(market);
        // Act
        TestEconomy.EndTradingPeriod();
        var actual = market.CurrentPeriod;
        // Assert
        Assert.AreEqual(expected, actual);
    }
    [Test]
    public void EndTradingPeriodIncrementsCompanyPeriod()
    {
        // Arrange
        var company = Company.Factory.Create("Company Test", CompanyLevelEnum.Beginner);
        var currentPeriod = company.CurrentPeriod;
        var expected = currentPeriod + 1;
        var market = Market.Factory.CreateMarket("Market Test", CompanyLevelEnum.Market, new LinearDemandStrategy());
        TheEconomy.Instance.RegisterCompany(market);
        market.RegisterCompany(company);
        // Act
        TestEconomy.EndTradingPeriod();
        var actual = company.CurrentPeriod;
        // Assert
        Assert.AreEqual(expected, actual);
    }
        
#endregion
#region UnleashMarketForces
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
        var company1Order = new Order(marketToTest, company1, Lemonade, 1000, 3.5m);
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
        marketToTest.InitializeDemandForSpecificGood(Lemonade, 2000);
        var company1 = Company.Factory.Create("Company1", CompanyLevelEnum.Beginner);
        company1.GetInventory().AddGood(new InventoryEntry(Lemonade, 2000, 3m,0));
        marketToTest.RegisterCompany(company1);
        var company1Order = new Order(marketToTest, company1, Lemonade, 1000, 3.5m);
        var company1Context = new ActionContext{TradeToSubmit = company1Order,
                                                MarketToSubmitTo = marketToTest};
        var expected = 1000;
        var theFirstMarket = (Market)TheEconomy.Instance.companies.Where(c => c.Name == "The First Market").FirstOrDefault();
        TestEconomy.RemoveMarket(theFirstMarket);
        TestEconomy.RegisterCompany(marketToTest);

        //Act
        company1.QueueOrder(company1Context);
        TestEconomy.EndTradingPeriod();
        var actual=company1Order.FilledQuantity;
        //Assert
        Assert.AreEqual(expected, actual);
    }
    
#endregion
#region Trades
    [Test]
    public void EndTradingPeriodProcessesNonMarketTradesQueuedInTheMarket()
    {
        // Arrange
        var market = Market.Factory.CreateMarket("Market Test", CompanyLevelEnum.Market, new LinearDemandStrategy());
        market.SetCash(1000000);
        var company1 = Company.Factory.Create("Company1", CompanyLevelEnum.Beginner);
        company1.GetInventory().AddGood(new InventoryEntry(Lemonade, 2000, 3m,0));
        var company2 = Company.Factory.Create("Company2", CompanyLevelEnum.Beginner);
        company2.SetCash(1000000);

        var trade1 = new Order(company2, company1, Lemonade, 500, 2.0m);
        var trade2 = new Order(company2, company1, Lemonade, 1000, 3.5m);
        var theFirstMarket = (Market)TheEconomy.Instance.companies.Where(c => c.Name == "The First Market").FirstOrDefault();
        TheEconomy.Instance.RemoveMarket(theFirstMarket);
        TheEconomy.Instance.RegisterCompany(market);
        
        market.QueueOrder(new ActionContext { TradeToSubmit = trade1, MarketToSubmitTo = market });
        market.QueueOrder(new ActionContext { TradeToSubmit = trade2, MarketToSubmitTo = market });
        var expected = 2;
        
        // Act
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
        TheEconomy.Instance.RegisterCompany(market);
        var company1 = Company.Factory.Create("Company1", CompanyLevelEnum.Beginner);
        company1.GetInventory().AddGood(new InventoryEntry(Lemonade, 2000, 3m,0));

        var company2 = Company.Factory.Create("Company2", CompanyLevelEnum.Beginner);
        company2.SetCash(1000000);

        var trade1 = new Order(company2, company1, Lemonade, 500, 2.0m);
        var trade2 = new Order(company2, company1, Lemonade, 1000, 3.5m);
        var theFirstMarket = (Market)TheEconomy.Instance.companies.Where(c => c.Name == "The First Market").FirstOrDefault();

        market.QueueOrder(new ActionContext { TradeToSubmit = trade1, MarketToSubmitTo = market });
        market.QueueOrder(new ActionContext { TradeToSubmit = trade2, MarketToSubmitTo = market });
        var expectedTradeCount = 2;
        var expectedMarketCount = 2;
        // Act
        TestEconomy.EndTradingPeriod();
        var actualTradeCount = TestEconomy._trade_logger.GetTradeCount();
        var actualMarketCount = TheEconomy.Instance.companies.Where(c=>c is Market).Count();

        // Assert
        Assert.AreEqual(expectedTradeCount, actualTradeCount);
        Assert.AreEqual(expectedMarketCount, actualMarketCount);
    }
#endregion
    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(TestEconomy.gameObject);
    }
}