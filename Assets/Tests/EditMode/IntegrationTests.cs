using System.Linq;
using NUnit.Framework;
using UnityEngine;
using static TestHelpers;

[TestFixture]
public class IntegrationTests
{
    TheEconomy TestEconomy;
    Market TestMarket;
    Company Company1;
    Company Company2;

    Good Lemonade;
    const int Period = 0;
    readonly iDemandStrategy TestDemandStrategy = ScriptableObject.CreateInstance<LinearDemandStrategy>();
    readonly iMarketDataService TestMarketDataService = new MockMarketDataService();
    readonly iSupplyProvider TestSupplyProvider = new MockSupplyProvider();

    readonly ITradeLogger TestTradeLogger = new TradeLoggerV1();

    [SetUp]
    public void SetUp()
    {
        TheEconomy.SetupForTests(TestTradeLogger);
        TestEconomy = TheEconomy.Instance;

        var existingMarket = TheEconomy.Instance.GetMarketByName("The First Market");
        TheEconomy.Instance.RemoveMarket(existingMarket);

        TestMarket = Market.Factory.CreateMarket("The First Market", CompanyLevelEnum.Market, TestDemandStrategy);
        TestMarket.SetMarketDataService(TestMarketDataService);
        TestMarket.SetSupplyProvider(TestSupplyProvider);
        TheEconomy.Instance.RegisterCompany(TestMarket);

        Company1 = Company.Factory.Create("Company1", CompanyLevelEnum.Beginner);
        Company2 = Company.Factory.Create("Company2", CompanyLevelEnum.Beginner);

        TestMarket.RegisterCompany(Company1);
        TestMarket.RegisterCompany(Company2);

        Lemonade = Good.CreateInstance("Lemonade", new PriceBand(.5m, 2m), RarityEnum.Common);
    }
#region Recording Trades
    [Test]
    public void MarketRecordsBothSidesOfTrade()
    {
        // Arrange
        var buyer = Company1;
        var seller = Company2;
        var good =Good.CreateInstance("Good", new PriceBand(1m, 2m), RarityEnum.Common);
        var sellersInventory = seller.GetInventory();
        sellersInventory.AddGood(new InventoryEntry(good, 10, 5, 0));
        
        var buyerBuysGoodFromSeller = new Order(buyer, seller, good, 10, 5);
        var sellerSellsGoodToBuyer = new Order(buyer, seller, good, 10, 5);
        var queueOrderResult = buyer.QueueOrder(CreateActionContext(buyerBuysGoodFromSeller, TestMarket,Period));
        var queueOrderResult2 = seller.QueueOrder(CreateActionContext(sellerSellsGoodToBuyer, TestMarket,Period));
        // Act
        TestMarket.ProcessCompanyOrders();

        // Assert
        var recordedTrades = TestMarket.GetExecutionsInPeriod(0);
        Assert.AreEqual(queueOrderResult.Result, LemonadeStandResultObject.Success(ResultTypeEnum.Success).Result);
        Assert.AreEqual(queueOrderResult2.Result, LemonadeStandResultObject.Success(ResultTypeEnum.Success).Result);
        Assert.AreEqual(2, recordedTrades.Count);
        Assert.AreEqual(buyer, recordedTrades[0].RecordedTrade.Buyer);
        Assert.AreEqual(seller, recordedTrades[0].RecordedTrade.Seller);
        Assert.AreEqual(good, recordedTrades[0].RecordedTrade.Good);
        Assert.AreEqual(10, recordedTrades[0].RecordedTrade.Quantity);
    }
    [Test]
    public void MarketDoesNotAllowQueueingDuplicateTrades()
    {
        // Arrange
        var market = Market.Factory.CreateMarket("Test Market", CompanyLevelEnum.Market, TestDemandStrategy);
        var buyer = Company.Factory.Create("Buyer", CompanyLevelEnum.Beginner);
        var seller = Company.Factory.Create("Seller", CompanyLevelEnum.Beginner);
        market.RegisterCompany(buyer);
        market.RegisterCompany(seller);
        var good =Good.CreateInstance("Good", new PriceBand(1m, 2m), RarityEnum.Common);
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
        var market = TestMarket;
        var currentPeriod = market.CurrentPeriod;
        var expected = currentPeriod + 1;
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
        var company = Company1;
        var currentPeriod = company.CurrentPeriod;
        var expected = currentPeriod + 1;
        
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
        var marketToTest = TestMarket;
        var company1 = Company1;
        marketToTest.InitializeDemandForSpecificGood(Lemonade, 1000);
        company1.GetInventory().AddGood(new InventoryEntry(Lemonade, 2000, 3m,0));
        var company1Order = new Order(null, company1, Lemonade, 1000, 3.5m);
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
        var marketToTest = TestMarket;
        marketToTest.SetMarketDataService(TestMarketDataService);
        marketToTest.InitializeDemandForSpecificGood(Lemonade, 2000);
        var company1 = Company.Factory.Create("TestCompany", CompanyLevelEnum.Beginner);
        company1.GetInventory().AddGood(new InventoryEntry(Lemonade, 2000, 3m,0));
        marketToTest.RegisterCompany(company1);

        var company1Order = new Order(null, company1, Lemonade, 1000, 3.5m);
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
    
#endregion
#region Trades
    [Test]
    public void EndTradingPeriodProcessesNonMarketTradesQueuedInTheMarket()
    {
        // Arrange
        var market = TestMarket;
        market.SetCash(1000000);
        
        Company1.GetInventory().AddGood(new InventoryEntry(Lemonade, 2000, 3m,0));
        Company2.SetCash(1000000);

        var trade1 = new Order(null, Company1, Lemonade, 500, 2.0m);
        var trade2 = new Order(Company2, null, Lemonade, 1000, 3.5m);
        
        Company1.QueueOrder(new ActionContext { TradeToSubmit = trade1, MarketToSubmitTo = market });
        Company2.QueueOrder(new ActionContext { TradeToSubmit = trade2, MarketToSubmitTo = market });
        var expected = 2;
        
        // Act
        TestEconomy.EndTradingPeriod();
        int actual=TestEconomy.GetAllTransactions(0)
                    .Sum(x=> x.Value.Count());
        // Assert
        Assert.AreEqual(expected, actual);
    }

     [Test]
    public void EndTradingPeriodSendsTradesThatMarketHasQueuedWhenTwoMarketsArePresent()
    {
        // Arrange
        var SecondMarket = Market.Factory.CreateMarket("Second Market", CompanyLevelEnum.Market, TestDemandStrategy);
        SecondMarket.SetMarketDataService(TestMarketDataService);
        SecondMarket.SetSupplyProvider(TestSupplyProvider);
        TheEconomy.Instance.RegisterCompany(SecondMarket);
        TestMarket.SetCash(1000000);
        Company1.GetInventory().AddGood(new InventoryEntry(Lemonade, 2000, 3m,0));
        Company2.SetCash(1000000);

        var trade1 = new Order(null, Company1, Lemonade, 500, 2.0m);
        var trade2 = new Order(Company2, null, Lemonade, 1000, 3.5m);

        Company1.QueueOrder(new ActionContext { TradeToSubmit = trade1, MarketToSubmitTo = TestMarket });
        Company2.QueueOrder(new ActionContext { TradeToSubmit = trade2, MarketToSubmitTo = TestMarket });
        var expectedTradeCount = 2;
        var expectedMarketCount = 2;
        // Act
        TestEconomy.EndTradingPeriod();
        var actualTradeCount = TestEconomy.GetAllTransactions(0).Sum(x=> x.Value.Count());
        var actualMarketCount = TheEconomy.Instance.companies.Where(c=>c is Market).Count();

        // Assert
        Assert.AreEqual(expectedTradeCount, actualTradeCount,$"Expected {expectedTradeCount} trades, got {actualTradeCount}");
        Assert.AreEqual(expectedMarketCount, actualMarketCount,$"Expected {expectedMarketCount} markets, got {actualMarketCount}");

        // Clean up
        TheEconomy.Instance.RemoveMarket(SecondMarket);
    }

    [Test]
    public void CompaniesCannotQueueTradesToThemselves()
    {
        //Arrange
        var period = 0;
        var good = Good.CreateInstance("Good", new PriceBand(1m, 2m), RarityEnum.Common);
        var company = Company.Factory.Create("Company", CompanyLevelEnum.Beginner);
        var market = Market.Factory.CreateMarket("Market", CompanyLevelEnum.Market, TestDemandStrategy);
        market.RegisterCompany(company);
        company.GetInventory().AddGood(new InventoryEntry(good, 10, 1, 0));

        var goodOrder = new Order(company, company, good, 10, 1);
        var actionContext = new ActionContext
        {
            TradeToSubmit = goodOrder,
            MarketToSubmitTo = market,
            Period = period
        };
        var expected = LemonadeStandResultObject.Failure(ResultTypeEnum.SelfTrade, "Cannot trade with yourself");
        //Act
        var actual = company.QueueOrder(actionContext);
        //Assert
        Assert.AreEqual(expected.Result, actual.Result);
    }
    [Test]
    public void IfTradeInvolvesMarketThenTheSubmittingCompanyIsTheMarket()
    {
        //Arrange
        var period = 0;
        var good = Good.CreateInstance("Good", new PriceBand(1m, 2m), RarityEnum.Common);
        var TestMarket = Market.Factory.CreateMarket("Market", CompanyLevelEnum.Market, TestDemandStrategy);
        var company = Company.Factory.Create("Company", CompanyLevelEnum.Beginner);
        TestMarket.RegisterCompany(company);
        company.GetInventory().AddGood(new InventoryEntry(good, 10, 1, 0));
        var MarketBuysGoodFromCompany1 = new Order(TestMarket, company, good, 10, 1);
        var actionContext = new ActionContext
        {
            TradeToSubmit = MarketBuysGoodFromCompany1,
            MarketToSubmitTo = TestMarket,
            Period = period
        };
        var expected = TestMarket;
        //Act
        TestMarket.QueueMarketOrder(actionContext);
        var actual = MarketBuysGoodFromCompany1.SubmittingCompany;
        //Assert
        Assert.AreEqual(expected, actual);
    }
#endregion
    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(TestEconomy.gameObject);
        TestMarket = null;
        Company1 = null;
        Company2 = null;
    }
}