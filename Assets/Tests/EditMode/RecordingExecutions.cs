using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using static TestHelpers;

[TestFixture]
public class RecordingExecutions
{
    TheEconomy TestEconomy;
    Good Lemon;
    Good Lemonade;
    Market TestMarket;
    int Period;

    Company Company1;
    Company Company2;
    readonly PriceBand PriceBand1 = new(.5m, 1.0m);
    readonly PriceBand PriceBand2 = new(5.0m, 10m);

    readonly iDemandStrategy TestDemandStrategy = ScriptableObject.CreateInstance<LinearDemandStrategy>();
    [SetUp]
    public void Setup()
    {
        TheEconomy.SetupForTests(new MockLogger());
        TestEconomy = TheEconomy.Instance;

        TestMarket = Market.Factory.CreateMarket("Test Market", CompanyLevelEnum.Market, TestDemandStrategy);
        Company1 = Company.Factory.Create("Company 1", CompanyLevelEnum.Beginner);
        Company2 = Company.Factory.Create("Company 2", CompanyLevelEnum.Beginner);
        TestMarket.RegisterCompany(Company1);
        TestMarket.RegisterCompany(Company2);

        Period=0;

        Lemon = Good.CreateInstance("Lemon", PriceBand1, RarityEnum.Common);
        Lemonade = Good.CreateInstance("Lemonade", PriceBand2, RarityEnum.Uncommon);
    }
    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(TestEconomy);
        Company1 = null;
        Company2 = null;
        TestMarket = null;
        Lemon = null;
        Lemonade = null;
    }
#region ExecutionsTests
    [TestCase(TestName = "Company trades only: Company 1 sells Lemonade to Company2, expect 2 executions")]
    public void MarketExecutionsContainsPartyAndCounterParty_OnePair()
    {
        //Arrange
        var Company1SellsLemonadeToAnyone = new Order(Company1, null, Lemonade, 5, 10m);
        var Company2BuysLemonadeFromAnyone = new Order(null, Company2, Lemonade, 5, 10m);
        Company2.GetInventory().AddGood(new InventoryEntry(Lemonade, 5, 10m, 2));

        const int expectedExecutionCount = 2;
        var expectedExecutions = new List<Execution>
        {
            new(Company1SellsLemonadeToAnyone,Period),
            new(Company2BuysLemonadeFromAnyone,Period)
        };
        Company1.QueueOrder(CreateActionContext(Company1SellsLemonadeToAnyone,TestMarket,Period));
        Company2.QueueOrder(CreateActionContext(Company2BuysLemonadeFromAnyone,TestMarket,Period));
        //Act
        TestMarket.ProcessCompanyOrders();
        var actualExecutions = TestMarket.GetExecutionsInPeriod(Period);
        //Assert
        Assert.AreEqual(expectedExecutionCount,actualExecutions.Count);
        CollectionAssert.AreEqual(expectedExecutions,actualExecutions);
    }
#endregion
#region RecordTrade Tests
    [Test]
    public void RT_RecordsOrderAndCounterPartyOrderWhenCalledInIsolationOnePairOnly()
    {
        //Arrange
        var order = new Order(Company1, Company2, Lemon, 1, 1m)
        {
            SubmittingCompany = Company1
        };
        var counterPartyOrder = new Order(Company1,Company2,Lemon,1,1m)
        {
            SubmittingCompany = Company2
        };
        var counterPartyOrders = new List<Order>
        {
            counterPartyOrder
        };
        
        var expectedOrder = order;
        var expectedCounterPartyOrder = counterPartyOrder;
        //Act
        BasicTransactionManager.RecordTransaction(order, TestMarket, Period, counterPartyOrders);
        var actualOrder = TestMarket.GetExecutionsInPeriod(Period).FirstOrDefault()?.RecordedTrade;
        var actualCounterPartyOrder = TestMarket.GetExecutionsInPeriod(Period).FirstOrDefault()?.CounterPartyTrades.FirstOrDefault();
        //Assert
        Assert.AreEqual(expectedOrder, actualOrder);
        Assert.AreEqual(expectedCounterPartyOrder, actualCounterPartyOrder);
    }
    [Test]
    public void RT_RecordsOrderAndCounterPartyOrdersWithTwoCounterpartyOrders()
    {
        //Arrange
        var Company3 = Company.Factory.Create("Company 3",CompanyLevelEnum.Beginner);
        
        var Company1BuysFromCompany2Order = new Order(Company1, Company2, Lemon, 1, 1m)
        {
            SubmittingCompany = Company1
        };
        var Company2SellsToCompany1Order = new Order(Company2,Company1,Lemon,1,1m)
        {
            SubmittingCompany = Company2
        };
        var Company3SellsToCompany1Order = new Order(Company3,Company1,Lemon,1,1m)
        {
            SubmittingCompany = Company3
        };
        var counterPartyOrders = new List<Order>
        {
            Company2SellsToCompany1Order,
            Company3SellsToCompany1Order
        };
        var expectedPrimaryOrder = Company1BuysFromCompany2Order;
        //Act
        BasicTransactionManager.RecordTransaction(Company1BuysFromCompany2Order, TestMarket, Period, counterPartyOrders);
        var actualPrimaryOrder = TestMarket.GetExecutionsInPeriod(Period).FirstOrDefault()?.RecordedTrade;
        var actualCounterPartyOrders = TestMarket.GetExecutionsInPeriod(Period).FirstOrDefault()?.CounterPartyTrades;
        //Assert
        Assert.AreEqual(expectedPrimaryOrder, actualPrimaryOrder);
        Assert.Contains(Company2SellsToCompany1Order, actualCounterPartyOrders);
        Assert.Contains(Company3SellsToCompany1Order, actualCounterPartyOrders);
        Assert.AreEqual(2, actualCounterPartyOrders.Count);
    }
    [Test]
    public void ProcessTransactionRecordsOrderAndCounterPartyOnePairOnly()
    {
        //Arrange
        var basicTransactionManager = new BasicTransactionManager();
        Company2.GetInventory().AddGood(new InventoryEntry(Lemon, 1, 1m, 1));
        var order = new Order(Company1, Company2, Lemon, 1, 1m)
        {
            SubmittingCompany = Company1
        };
        var counterPartyOrders = new List<Order>();
        var counterPartyOrder = new Order(Company1,Company2,Lemon,1,1m)
        {
            SubmittingCompany = Company2
        };
        counterPartyOrders.Add(counterPartyOrder); 

        var orderContext = new ActionContext{PrimaryOrder = order
                ,CounterPartyOrders=counterPartyOrders,
                MarketToSubmitTo = TestMarket,
                Period = Period};

        var expectedOrder = order;
        var expectedCounterPartyOrder = counterPartyOrder;
        //Act
        basicTransactionManager.ProcessPairedOrders(orderContext);
        var actualOrder = TestMarket.GetExecutionsInPeriod(Period).FirstOrDefault()?.RecordedTrade;
        var actualCounterPartyOrder = TestMarket.GetExecutionsInPeriod(Period).FirstOrDefault()?.CounterPartyTrades.FirstOrDefault();
        //Assert
        Assert.AreEqual(expectedOrder, actualOrder);
        Assert.AreEqual(expectedCounterPartyOrder, actualCounterPartyOrder);
    }
    [Test]
    public void RT_StaticOneBuyerOneSellerCounterPartyIsSeller()
    {
        //Arrange
        var PrimaryOrder = new Order(Company1, null, Lemonade, 5, 10m)
        {
            SubmittingCompany = Company1
        };
        var CounterPartyOrder = new Order(null, Company2, Lemonade, 5, 10m)
        {
            SubmittingCompany = Company2
        };

        var CounterPartyOrders = new List<Order>{CounterPartyOrder};

        //Act
        BasicTransactionManager.RecordTransaction(PrimaryOrder,TestMarket,Period,CounterPartyOrders);
        
        //Assert
        var transactionsRecorded = TestMarket.GetExecutionsInPeriod(Period);
        var recordedTransaction = transactionsRecorded?.FirstOrDefault();
        Assert.IsNotNull(recordedTransaction);
        Assert.IsTrue(recordedTransaction.CounterPartyTrades.Count == 1);
        Assert.AreEqual(Company2,recordedTransaction.CounterPartyTrades.FirstOrDefault().Seller);
        Assert.AreEqual(Company1,recordedTransaction.RecordedTrade.Buyer);
    }

    [Test]
    public void RT_StaticOneBuyerOneSellerCounterPartyIsBuyer()
    {
        //Arrange
        var PrimaryOrder = new Order(null, Company1, Lemonade, 5, 10m)
        {
            SubmittingCompany = Company1
        };
        var CounterPartyOrder = new Order(Company2, null, Lemonade, 5, 10m)
        {
            SubmittingCompany = Company2
        };

        var CounterPartyOrders = new List<Order>{CounterPartyOrder};

        //Act
        BasicTransactionManager.RecordTransaction(PrimaryOrder,TestMarket,Period,CounterPartyOrders);
        
        //Assert
        var transactionsRecorded = TestMarket.GetExecutionsInPeriod(Period);
        var recordedTransaction = transactionsRecorded?.FirstOrDefault();
        Assert.IsNotNull(recordedTransaction);
        Assert.IsTrue(recordedTransaction.CounterPartyTrades.Count == 1);
        Assert.AreEqual(Company2,recordedTransaction.CounterPartyTrades.FirstOrDefault().Buyer);
        Assert.AreEqual(Company1,recordedTransaction.RecordedTrade.Seller);
    }
    [Test]
    public void RecordTrade_OneOrderOneCounterpartyTwoMarketTrades()
    {
        //A single primary order that has a single counterparty
        //results in two market trades being recorded in the Market
        //Arrange
        var PrimaryOrder = new Order(Company1, null, Lemonade, 5, 10m)
        {
            SubmittingCompany = Company1
        };
        var CounterPartyOrder = new Order(null, Company2, Lemonade, 5, 10m)
        {
            SubmittingCompany = Company2
        };

        var expectedMarketTransactionCount = 2;
        var expectedMarketTransactions = new List<Execution>()
        {
            new(PrimaryOrder,Period),
            new(CounterPartyOrder,Period)
        };
        //Act
        BasicTransactionManager.RecordTransaction(PrimaryOrder,TestMarket,
                                            Period,
                                            new List<Order>{CounterPartyOrder});
        var transactionsRecorded = TestMarket.GetExecutionsInPeriod(Period);
        //Assert
        Assert.AreEqual(expectedMarketTransactionCount,transactionsRecorded.Count);
        CollectionAssert.AreEqual(expectedMarketTransactions,transactionsRecorded);
    }
    [Test]
    public void RT_OneOrderTwoCounterpartiesThreeMarketTrades()
    {
        //A single primary order that has two counterparties
        //results in three market trades being recorded in the Market
        //Arrange
        var PrimaryOrder = new Order(Company1, null, Lemonade, 10, 10m)
        {
            SubmittingCompany = Company1
        };
        var CounterPartyOrder1 = new Order(null, Company2, Lemonade, 5, 10m)
        {
            SubmittingCompany = Company2
        };
        var Company3 = Company.Factory.Create("Company 3",CompanyLevelEnum.Beginner);
        var CounterPartyOrder2 = new Order(null, Company3, Lemonade, 5, 10m)
        {
            SubmittingCompany = Company3
        };
        var expectedMarketTransactionCount = 3;
        var expectedMarketTransactions = new List<Execution>()
        {
            new(PrimaryOrder,Period),
            new(CounterPartyOrder1,Period),
            new(CounterPartyOrder2,Period)
        };
        //Act
        BasicTransactionManager.RecordTransaction(PrimaryOrder,TestMarket,
                                            Period,
                                            new List<Order>{CounterPartyOrder1,CounterPartyOrder2});
        var transactionsRecorded = TestMarket.GetExecutionsInPeriod(Period);
        //Assert
        Assert.AreEqual(expectedMarketTransactionCount,transactionsRecorded.Count);
        Assert.AreEqual(expectedMarketTransactions,transactionsRecorded);
    }
    [Test]
    public void RT_OneOrderOneCounterPartyEachIsRecordedCounterparty()
    {
        //Two market trades should be recorded
        //The counterparty in each MarketTrade.RecordedTrade should be the other PrimaryOrder
        //Arrange
        var PrimaryOrder = new Order(Company1, null, Lemonade, 5, 10m)
        {
            SubmittingCompany = Company1
        };
        var CounterPartyOrder = new Order(null, Company2, Lemonade, 5, 10m)
        {
            SubmittingCompany = Company2
        };
        
        var expectedCounterPartyOrderForFirstTrade = CounterPartyOrder;
        var expectedCounterPartyOrderForSecondTrade = PrimaryOrder;
        //Act
        BasicTransactionManager.RecordTransaction(PrimaryOrder,TestMarket,
                                            Period,
                                            new List<Order>{CounterPartyOrder});
        var transactionsRecorded = TestMarket.GetExecutionsInPeriod(Period);
        var firstTrade = transactionsRecorded.FirstOrDefault();
        var secondTrade = transactionsRecorded.LastOrDefault();
        var actualCounterPartyOrderForFirstTrade = firstTrade.CounterPartyTrades.FirstOrDefault();
        var actualCounterPartyOrderForSecondTrade = secondTrade.CounterPartyTrades.FirstOrDefault();
        //Assert
        Assert.AreEqual(expectedCounterPartyOrderForFirstTrade,actualCounterPartyOrderForFirstTrade);
        Assert.AreEqual(expectedCounterPartyOrderForSecondTrade,actualCounterPartyOrderForSecondTrade);
    }
#endregion
}

