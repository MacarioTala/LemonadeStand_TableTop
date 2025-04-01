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

    readonly TestComparer<Execution> ExecutionComparer=new(new string[] { "CounterPartyTrades" });

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
        UnityEngine.Object.DestroyImmediate(TestEconomy);
        Company1 = null;
        Company2 = null;
        TestMarket = null;
        Lemon = null;
        Lemonade = null;
    }

#region Orders get executions added 
    [TestCase(TestName = "Company 1 buys 5 lemonade from Company 2, expect 1 execution for each order")]
    public void ProcessCompanyOrdersRecordsExecutionsInOrders_1P1CP()
    {
        //Arrange
        var Company1SellsLemonadeToAnyone = new Order(Company1, null, Lemonade, 5, 10m);
        var Company2BuysLemonadeFromAnyone = new Order(null, Company2, Lemonade, 5, 10m);
        Company2.GetInventory().AddGood(new InventoryEntry(Lemonade, 5, 10m, 2));
        Company1.QueueOrder(CreateActionContext(Company1SellsLemonadeToAnyone,TestMarket,Period));
        Company2.QueueOrder(CreateActionContext(Company2BuysLemonadeFromAnyone,TestMarket,Period));
        
        const int expectedOrder1ExecutionCount = 1;
        const int expectedOrder2ExecutionCount = 1;
        var expectedOrder1 = new Execution(Company1SellsLemonadeToAnyone,Company1,Company2,5,10m,Period);
        var expectedOrder2 = new Execution(Company2BuysLemonadeFromAnyone,Company1,Company2,5,10m,Period);

        //Act
        TestMarket.ProcessCompanyOrders();
        var actualOrder1Executions = Company1SellsLemonadeToAnyone.GetExecutions();
        var actualOrder2Executions = Company2BuysLemonadeFromAnyone.GetExecutions();
        
        //Assert
        Assert.AreEqual(expectedOrder1ExecutionCount,actualOrder1Executions?.Count());
        Assert.AreEqual(expectedOrder2ExecutionCount,actualOrder2Executions?.Count());
        Assert.IsTrue(ExecutionComparer.Equals(expectedOrder1,actualOrder1Executions?.FirstOrDefault()));
        Assert.IsTrue(ExecutionComparer.Equals(expectedOrder2,actualOrder2Executions?.FirstOrDefault()));
    }
    [TestCase(TestName = "Company 1 buys 5 lemonade each from Company 2 and Company 3, expect 2 executions for Company 1 and 1 for each seller")]
     public void ProcessCompanyOrdersRecordsExecutionsInOrders_1P2CP()
     {
        //Arrange
        var Company3 = Company.Factory.Create("Company 3",CompanyLevelEnum.Beginner);
        var Company1BuysLemonadeFromAnyone = new Order(Company1, null, Lemonade, 10, 10m);
        var Company2SellsLemonadeToAnyone = new Order(null, Company2, Lemonade, 5, 10m);
        var Company3SellsLemonadeToAnyone = new Order(null, Company3, Lemonade, 5, 10m);
        Company2.GetInventory().AddGood(new InventoryEntry(Lemonade, 5, 10m, 2));
        Company3.GetInventory().AddGood(new InventoryEntry(Lemonade, 5, 10m, 2));
        Company1.QueueOrder(CreateActionContext(Company1BuysLemonadeFromAnyone,TestMarket,Period));
        Company2.QueueOrder(CreateActionContext(Company2SellsLemonadeToAnyone,TestMarket,Period));
        Company3.QueueOrder(CreateActionContext(Company3SellsLemonadeToAnyone,TestMarket,Period));

        const int expectedOrder1ExecutionCount = 2;
        const int expectedOrder2ExecutionCount = 1;
        const int expectedOrder3ExecutionCount = 1;
        var expectedCompany1Execution1 = new Execution(Company1BuysLemonadeFromAnyone,Company1,Company2,5,10m,Period);
        var expectedCompany1Execution2 = new Execution(Company1BuysLemonadeFromAnyone,Company1,Company3,5,10m,Period);
        var expectedCompany2Execution1 = new Execution(Company2SellsLemonadeToAnyone,Company1,Company2,5,10m,Period);
        var expectedCompany3Execution1 = new Execution(Company3SellsLemonadeToAnyone,Company1,Company3,5,10m,Period);
        
        //Act
        TestMarket.ProcessCompanyOrders();
        var actualCompany1Executions = Company1BuysLemonadeFromAnyone.GetExecutions();
        var actualCompany2Executions = Company2SellsLemonadeToAnyone.GetExecutions();
        var actualCompany3Executions = Company3SellsLemonadeToAnyone.GetExecutions();
        //Assert
        Assert.AreEqual(expectedOrder1ExecutionCount,actualCompany1Executions?.Count());
        Assert.AreEqual(expectedOrder2ExecutionCount,actualCompany2Executions?.Count());
        Assert.AreEqual(expectedOrder3ExecutionCount,actualCompany3Executions?.Count());

        Assert.IsTrue(ExecutionComparer.Equals(expectedCompany1Execution1,actualCompany1Executions[0]));
        Assert.IsTrue(ExecutionComparer.Equals(expectedCompany1Execution2,actualCompany1Executions[1]));
    
        Assert.IsTrue(ExecutionComparer.Equals(expectedCompany2Execution1,actualCompany2Executions?.FirstOrDefault()));
        Assert.IsTrue(ExecutionComparer.Equals(expectedCompany3Execution1,actualCompany3Executions?.FirstOrDefault()));
     }
     [TestCase(TestName = "Company 1 sells 5 lemonade to the Market, expect 1 execution for Company1")]
     public void FulfillDemandRecordsExecutionsInOrders_1P1CP()
     {
        //Arrange
        var Company1SellsLemonadeToAnyone = new Order(null,Company1 , Lemonade, 5, 10m);
        Company1.GetInventory().AddGood(new InventoryEntry(Lemonade, 5, 10m, 5));
        Company1.QueueOrder(CreateActionContext(Company1SellsLemonadeToAnyone,TestMarket,Period));
        TestMarket.InitializeDemandForSpecificGood(Lemonade, 5);

        const int expectedCompany1OrderExecutionCount = 1;
        var expectedCompany1Execution = new Execution(Company1SellsLemonadeToAnyone,TestMarket,Company1,5,10m,Period);

        //Act
        TestMarket.FulfillDemand();
        var actualCompany1Executions = Company1SellsLemonadeToAnyone.GetExecutions();
        //Note: no market executions can be querried at this time -- the order is created during fulfill demand

        //Assert
        Assert.AreEqual(expectedCompany1OrderExecutionCount,actualCompany1Executions?.Count());
        Assert.IsTrue(ExecutionComparer.Equals(expectedCompany1Execution,actualCompany1Executions?.FirstOrDefault()));
     }
#endregion

#region Execution Tests From ProcessCompanyOrders
   

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
            new(Company1SellsLemonadeToAnyone,Company1,Company2,5,10,Period),
            new(Company2BuysLemonadeFromAnyone,Company1,Company2,5,10,Period)
        };
        Company1.QueueOrder(CreateActionContext(Company1SellsLemonadeToAnyone,TestMarket,Period));
        Company2.QueueOrder(CreateActionContext(Company2BuysLemonadeFromAnyone,TestMarket,Period));
        //Act
        TestMarket.ProcessCompanyOrders();
        var actualExecutions = TestMarket.GetExecutionsInPeriod(Period);
        //Assert
        Assert.AreEqual(expectedExecutionCount,actualExecutions.Count);
        var comparer = new TestComparer<Execution>(new string[] { "CounterPartyTrades" });
        // for(var i = 0; i < expectedExecutions.Count; i++)
        // {
        //     Assert.IsTrue(comparer.Equals(expectedExecutions[i], actualExecutions[i]));
        // }
        Assert.IsTrue(comparer.ListsAreEquivalent(expectedExecutions, actualExecutions,comparer));
    }
    [TestCase(TestName = "Company trades only: One Buyer, Two Sellers, fully filled. 4 executions expected")]
    public void OneBuyerTwoSellersFullyFilled()
    {
        //Arrange
        var Company3 = Company.Factory.Create("Company 3",CompanyLevelEnum.Beginner);
        Company2.GetInventory().AddGood(new InventoryEntry(Lemon, 1, 1m, 1));
        Company3.GetInventory().AddGood(new InventoryEntry(Lemon, 1, 1m, 1));

        var Company1BuysFromAnyone = new Order(Company1, null, Lemon, 2, 1m);
        var Company2SellsToAnyone = new Order(null,Company2,Lemon,1,1m);
        var Company3SellsToAnyone = new Order(null,Company3,Lemon,1,1m);

        const int expectedExecutionCount = 4;
        var expectedExecutions = new List<Execution>
        {
            new(Company1BuysFromAnyone,Company1,Company2,1,1,Period),
            new(Company1BuysFromAnyone,Company1,Company3,1,1,Period),
            new(Company2SellsToAnyone,Company1,Company2,1,1,Period),
            new(Company3SellsToAnyone,Company1,Company3,1,1,Period)
        };
        Company1.QueueOrder(CreateActionContext(Company1BuysFromAnyone,TestMarket,Period));
        Company2.QueueOrder(CreateActionContext(Company2SellsToAnyone,TestMarket,Period));
        Company3.QueueOrder(CreateActionContext(Company3SellsToAnyone,TestMarket,Period));

        //Act
        TestMarket.ProcessCompanyOrders();
        var actualExecutions = TestMarket.GetExecutionsInPeriod(Period);
        //Assert
        Assert.AreEqual(expectedExecutionCount,actualExecutions.Count);
        var comparer = new TestComparer<Execution>(new string[] { "CounterPartyTrades" });
        Assert.IsTrue(comparer.ListsAreEquivalent(expectedExecutions, actualExecutions,comparer));
    }
    [TestCase(TestName = "Single trade, market is buyer counterparty. Expected executions: 2")]
    public void OneTradeMarketBuyerCounterparty()
    {
        //Arrange
        var Company1SellsToAnyone = new Order( null,Company1, Lemon, 5, 10m);
        var TestMarketBuysFromCompany1 = new Order(TestMarket,Company1, Lemon, 5, 10m){
            SubmittingCompany = TestMarket,
            FilledQuantity = 5
        };
        Company1.GetInventory().AddGood(new InventoryEntry(Lemon, 5, 10m, 5));
        TestMarket.InitializeDemandForSpecificGood(Lemon, 5);
        const int expectedExecutionCount = 2;
        var expectedExecutions = new List<Execution>
        {
            new(Company1SellsToAnyone,TestMarket,Company1,5,10m,Period),
            new(TestMarketBuysFromCompany1,TestMarket,Company1,5,10m,Period)
        };
        Company1.QueueOrder(CreateActionContext(Company1SellsToAnyone,TestMarket,Period));
        
        //Act
        TestMarket.ProcessCompanyOrders();
        TestMarket.FulfillDemand();
        var actualExecutions = TestMarket.GetExecutionsInPeriod(Period);
        //Assert
        Assert.AreEqual(expectedExecutionCount,actualExecutions.Count);
        Assert.IsTrue(ExecutionComparer.ListsAreEquivalent(expectedExecutions, actualExecutions,ExecutionComparer));
    }
    [TestCase(TestName = "Company1 sells 50 Lemonade, Company2 buys 10, Company 3 buys 10, market buys 30. Expected executions: 6")]
    public void BigSell2CompanyCounterParty1MarketCounterparty()
    {
        //Arrange
        Company1.GetInventory().AddGood(new InventoryEntry(Lemonade, 50, 10m, 5));
        TestMarket.InitializeDemandForSpecificGood(Lemonade, 30);

        var Company3 = Company.Factory.Create("Company 3",CompanyLevelEnum.Beginner);
        Company3.SetCash(1000);
        var Company1SellsToAnyone = new Order(null,Company1, Lemonade, 50, 10m);
        var Company2BuysFromAnyone = new Order(Company2, null, Lemonade, 10, 10m);
        var Company3BuysFromAnyone = new Order(Company3, null, Lemonade, 10, 10m);
        const int expectedExecutionCount = 6;
        var TestMarketBuysFromCompany1 = new Order(TestMarket,Company1, Lemonade, 30, 10m)
        {
            SubmittingCompany = TestMarket,
            FilledQuantity = 30
        };
        var expectedExecutions = new List<Execution>
        {
            new(Company1SellsToAnyone,Company2,Company1,10,10m,Period),
            new(Company1SellsToAnyone,Company3,Company1,10,10m,Period),
            new(Company1SellsToAnyone,TestMarket,Company1,30,10m,Period),
            new(Company2BuysFromAnyone,Company2,Company1,10,10m,Period),
            new(Company3BuysFromAnyone,Company3,Company1,10,10m,Period),
            new(TestMarketBuysFromCompany1,TestMarket,Company1,30,10m,Period)
        };
        Company1.QueueOrder(CreateActionContext(Company1SellsToAnyone,TestMarket,Period));
        Company2.QueueOrder(CreateActionContext(Company2BuysFromAnyone,TestMarket,Period));
        Company3.QueueOrder(CreateActionContext(Company3BuysFromAnyone,TestMarket,Period));
        //Act
        TestMarket.ProcessCompanyOrders();
        TestMarket.FulfillDemand();
        var actualExecutions = TestMarket.GetExecutionsInPeriod(Period);
        //Assert
        Assert.AreEqual(expectedExecutionCount,actualExecutions.Count);
        Assert.IsTrue(ExecutionComparer.ListsAreEquivalent(expectedExecutions, actualExecutions,ExecutionComparer));
    }

    [TestCase(TestName = "Single trade, market is seller counterparty. Expected executions: 2")]
    public void OneTradeMarketSellerCounterparty()
    {
        //Arrange
        var Company1BuysFromAnyone = new Order(Company1, null, Lemon, 5, 10m);
        TestMarket.GetInventory().AddGood(new InventoryEntry(Lemon, 5, 10m, 5));
        TestMarket.InitializeDemandForSpecificGood(Lemon, 5);
        const int expectedExecutionCount = 2;
        var expectedExecutions = new List<Execution>
        {
            new(Company1BuysFromAnyone,Company1,TestMarket,5,10m,Period),
            new(Company1BuysFromAnyone,Company1,TestMarket,5,10m,Period)
        };
        Company1.QueueOrder(CreateActionContext(Company1BuysFromAnyone,TestMarket,Period));
        //Act
        TestMarket.ProcessCompanyOrders();
        TestMarket.FulfillDemand();
        var actualExecutions = TestMarket.GetExecutionsInPeriod(Period);
        //Assert
        Assert.AreEqual(expectedExecutionCount,actualExecutions.Count);
        CollectionAssert.AreEqual(expectedExecutions,actualExecutions);
    }
#endregion
}

