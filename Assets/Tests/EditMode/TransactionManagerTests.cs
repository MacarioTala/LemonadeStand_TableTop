using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class TransactionManagerTests
{
    TheEconomy TestEconomy;
    Good Lemon;
    readonly Price_band PriceBand1 = new(.5m, 1.0m);

    [SetUp]
    public void Setup()
    {
        var TestEconomyObject = new GameObject();
        TestEconomy = TestEconomyObject.AddComponent<TheEconomy>();
        TestEconomy.Initialize(new MockLogger());

        Lemon = Good.CreateInstance("Lemon", PriceBand1, Rarity_enum.Common);
    }
#region ValidateTransactionTests
    [Test]
    public void ValidateTransactionShouldReturnSelfTradeWhenCompanySubmitsTwoIdenticalTrades()
    {
        //Arrange
        var Company1 = Company.Factory.Create("Company 1",CompanyLevelEnum.Beginner);
        var Company2 = Company.Factory.Create("Company 2",CompanyLevelEnum.Beginner);
        Company2.GetInventory().AddGood(new InventoryEntry(Lemon, 1, 1m, 1));
        var order = new Order(Company1,Company2,Lemon,1,1m)
        {
            SubmittingCompany = Company1
        };
        var counterPartyOrder = new Order(Company1,Company2,Lemon,1,1m)
        {
            SubmittingCompany = Company1
        };
        var costOfThisLeg = 1m;

        var expected = LemonadeStandResultObject.Failure(ResultTypeEnum.SelfTrade, "Company cannot trade with itself");
        var transactionManager = new BasicTransactionManager();
        //Act
        var actual = transactionManager.ValidateTransaction(order, counterPartyOrder, costOfThisLeg);
        //Assert
        Assert.AreEqual(expected.Result, actual.Result);
    }

    [Test]
    public void ValidateTransactionShouldReturnSelfTradeWhenCompanyTriesToTradeWithItself()
    {
        //Arrange
        var Company1 = Company.Factory.Create("Company 1",CompanyLevelEnum.Beginner);
        var order = new Order(Company1,Company1,Lemon,1,1m)
        {
            SubmittingCompany = Company1
        };
        
        var costOfThisLeg = 1m;

        var expected = LemonadeStandResultObject.Failure(ResultTypeEnum.SelfTrade, "Company cannot trade with itself");
        var transactionManager = new BasicTransactionManager();
        //Act
        var actual = transactionManager.ValidateTransaction(order, order, costOfThisLeg);
        //Assert
        Assert.AreEqual(expected.Result, actual.Result);
    }

    [Test]
    public void ValidateTransactionReturnsSuccessForValidOrderPair()
    {
        //Arrange
        var Company1 = Company.Factory.Create("Company 1",CompanyLevelEnum.Beginner);
        var Company2 = Company.Factory.Create("Company 2",CompanyLevelEnum.Beginner);
        Company1.SetCash(1000m);
        Company2.GetInventory().AddGood(new InventoryEntry(Lemon, 1, 1m, 1));
        var order = new Order(Company1, Company2, Lemon, 1, 1m)
        {
            SubmittingCompany = Company1
        };
        var counterPartyOrder = new Order(Company1,Company2,Lemon,1,1m)
        {
            SubmittingCompany = Company2
        };
        var costOfThisLeg = 1m;
        var expected = LemonadeStandResultObject.Success();
        var transactionManager = new BasicTransactionManager();
        //Act
        var actual = transactionManager.ValidateTransaction(order, counterPartyOrder, costOfThisLeg);
        //Assert
        Assert.AreEqual(expected.Result, actual.Result);
    }
    
    [Test]
    public void ValidateTransactionReturnsInsufficientFundsWhenBuyerDoesNotHaveEnoughCash()
    {
        //Arrange
        var Company1 = Company.Factory.Create("Company 1",CompanyLevelEnum.Beginner);
        var Company2 = Company.Factory.Create("Company 2",CompanyLevelEnum.Beginner);
        Company1.SetCash(0m);
        var order = new Order(Company1, Company2, Lemon, 1, 1m)
        {
            SubmittingCompany = Company1
        };
        var counterPartyOrder = new Order(Company1,Company2,Lemon,1,1m)
        {
            SubmittingCompany = Company2
        };
        var costOfThisLeg = 1m;
        var expected = LemonadeStandResultObject.Failure(ResultTypeEnum.InsufficientFunds, "Buyer does not have enough cash to complete the transaction");
        var transactionManager = new BasicTransactionManager();
        //Act
        var actual = transactionManager.ValidateTransaction(order, counterPartyOrder, costOfThisLeg);
        //Assert
        Assert.AreEqual(expected.Result, actual.Result);
    }

    [Test]
    public void ValidateTransactionReturnsInsufficientGoodsWhenSellerDoesNotHaveEnoughGoods()
    {
        //Arrange
        var Company1 = Company.Factory.Create("Company 1",CompanyLevelEnum.Beginner);
        var Company2 = Company.Factory.Create("Company 2",CompanyLevelEnum.Beginner);
        Company1.SetCash(1000m);
        var order = new Order(Company1, Company2, Lemon, 1, 1m)
        {
            SubmittingCompany = Company1
        };
        var counterPartyOrder = new Order(Company1,Company2,Lemon,1,1m)
        {
            SubmittingCompany = Company2
        };
        var costOfThisLeg = 1m;
        var expected = LemonadeStandResultObject.Failure(ResultTypeEnum.InsufficientGoods, "Seller does not have enough goods to complete the transaction");
        var transactionManager = new BasicTransactionManager();
        //Act
        var actual = transactionManager.ValidateTransaction(order, counterPartyOrder, costOfThisLeg);
        //Assert
        Assert.AreEqual(expected.Result, actual.Result);
    }
#endregion
#region ProcessTransactionTests
    [Test]
    public void ForFullyFilledTradesProcessTransactionShouldReturnSuccess()
    {
        //Arrange
        var period = 0;
        var Company1 = Company.Factory.Create("Company 1",CompanyLevelEnum.Beginner);
        var Company2 = Company.Factory.Create("Company 2",CompanyLevelEnum.Beginner);
        Company1.SetCash(1000m);
        Company2.GetInventory().AddGood(new InventoryEntry(Lemon, 1, 1m, 1));
        var order = new Order(Company1, Company2, Lemon, 1, 1m)
        {
            SubmittingCompany = Company1
        };
        var counterPartyOrder = new Order(Company1,Company2,Lemon,1,1m)
        {
            SubmittingCompany = Company2
        };
        var expected = LemonadeStandResultObject.Success();
        var transactionManager = new BasicTransactionManager();
        //Act
        var actual = transactionManager.ProcessTransactionPair(order, counterPartyOrder, period);
        //Assert
        Assert.AreEqual(expected.Result, actual.Result);
    }
    [Test]
    public void FullyFilledTradesTransferCashAndGoodsCorrectly()
    {
        //Arrange
        var period = 0;
        var Company1 = Company.Factory.Create("Company 1",CompanyLevelEnum.Beginner);
        var Company2 = Company.Factory.Create("Company 2",CompanyLevelEnum.Beginner);
        Company1.SetCash(1000m);
        Company2.SetCash(0);
        Company1.GetInventory().Clear();
        Company2.GetInventory().AddGood(new InventoryEntry(Lemon, 1, 1m, 1));
        var order = new Order(Company1, Company2, Lemon, 1, 1m)
        {
            SubmittingCompany = Company1
        };
        var counterPartyOrder = new Order(Company1,Company2,Lemon,1,1m)
        {
            SubmittingCompany = Company2
        };
        var transactionManager = new BasicTransactionManager();
        var expectedBuyerCash = 999m;
        var expectedSellerCash = 1m;
        var expectedBuyerLemonQuantity = 1;
        var expectedSellerLemonQuantity = 0;
        //Act
        transactionManager.ProcessTransactionPair(order, counterPartyOrder, period);
        var actualBuyerCash = Company1.GetCash();
        var actualSellerCash = Company2.GetCash();
        var actualBuyerLemonQuantity = Company1.GetInventory()
                                    .GetInventoryEntriesByGood(Lemon.good_name)
                                    ?.FirstOrDefault()?.quantity ?? 0;
        var actualSellerLemonQuantity = Company2.GetInventory()
                                    .GetInventoryEntriesByGood(Lemon.good_name)
                                    ?.FirstOrDefault()?.quantity ?? 0;
        //Assert
        Assert.AreEqual(expectedBuyerCash, actualBuyerCash, "Buyer cash not as expected");
        Assert.AreEqual(expectedSellerCash, actualSellerCash, "Seller cash not as expected");
        Assert.AreEqual(expectedBuyerLemonQuantity, actualBuyerLemonQuantity, "Buyer lemon quantity not as expected");
        Assert.AreEqual(expectedSellerLemonQuantity, actualSellerLemonQuantity, "Seller lemon quantity not as expected");
    }
    [Test]
    public void PartiallyFilledTradesTransferCashAndGoodsCorrectly()
    {
        //Arrange
        var period = 0;
        var Company1 = Company.Factory.Create("Company 1",CompanyLevelEnum.Beginner);
        var Company2 = Company.Factory.Create("Company 2",CompanyLevelEnum.Beginner);
        Company1.SetCash(1000m);
        Company2.SetCash(0);
        Company1.GetInventory().Clear();
        Company2.GetInventory().AddGood(new InventoryEntry(Lemon, 1, 1m, 1));
        var order = new Order(Company1, Company2, Lemon, 2, 1m)
        {
            SubmittingCompany = Company1
        };
        var counterPartyOrder = new Order(Company1,Company2,Lemon,1,1m)
        {
            SubmittingCompany = Company2
        };
        var transactionManager = new BasicTransactionManager();
        var expectedBuyerCash = 999m;
        var expectedSellerCash = 1m;
        var expectedBuyerLemonQuantity = 1;
        var expectedSellerLemonQuantity = 0;
        var expectedBuyerOrderFullyFilledStatus = false;
        var expectedBuyerOrderPartiallyFilledStatus = true;
        //Act
        transactionManager.ProcessTransactionPair(order, counterPartyOrder, period);
        var actualBuyerCash = Company1.GetCash();
        var actualSellerCash = Company2.GetCash();
        var actualBuyerLemonQuantity = Company1.GetInventory()
                                    .GetInventoryEntriesByGood(Lemon.good_name)
                                    ?.FirstOrDefault()?.quantity ?? 0;
        var actualSellerLemonQuantity = Company2.GetInventory()
                                    .GetInventoryEntriesByGood(Lemon.good_name)
                                    ?.FirstOrDefault()?.quantity ?? 0;
        var actualBuyerOrderFullyFilledStatus = order.IsFullyFilled;
        var actualBuyerOrderPartiallyFilledStatus = order.IsPartiallyFilled;
        //Assert
        Assert.AreEqual(expectedBuyerCash, actualBuyerCash, "Buyer cash not as expected");
        Assert.AreEqual(expectedSellerCash, actualSellerCash, "Seller cash not as expected");
        Assert.AreEqual(expectedBuyerLemonQuantity, actualBuyerLemonQuantity, "Buyer lemon quantity not as expected");
        Assert.AreEqual(expectedSellerLemonQuantity, actualSellerLemonQuantity, "Seller lemon quantity not as expected");
        Assert.AreEqual(expectedBuyerOrderFullyFilledStatus, actualBuyerOrderFullyFilledStatus, "Buyer order fully filled status not as expected");
        Assert.AreEqual(expectedBuyerOrderPartiallyFilledStatus, actualBuyerOrderPartiallyFilledStatus, "Buyer order partially filled status not as expected");
    }
#endregion
#region RecordTrade Tests
    [Test]
    public void RecordTradeRecordsOrderAndCounterPartyOrderWhenCalledInIsolationOnePairOnly()
    {
        //Arrange
        var period = 0;
        var Company1 = Company.Factory.Create("Company 1",CompanyLevelEnum.Beginner);
        var Company2 = Company.Factory.Create("Company 2",CompanyLevelEnum.Beginner);
        var testMarket = Market.Factory.CreateMarket("Test Market", CompanyLevelEnum.Market,new LinearDemandStrategy());
        
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
        BasicTransactionManager.RecordTrade(order, testMarket, period, counterPartyOrders);
        var actualOrder = testMarket.GetMarketTradesInPeriod(period).FirstOrDefault()?.RecordedTrade;
        var actualCounterPartyOrder = testMarket.GetMarketTradesInPeriod(period).FirstOrDefault()?.CounterPartyTrades.FirstOrDefault();
        //Assert
        Assert.AreEqual(expectedOrder, actualOrder);
        Assert.AreEqual(expectedCounterPartyOrder, actualCounterPartyOrder);
    }
    [Test]
    public void RecordTradeRecordsOrderAndCounterPartyOrdersWithTwoCounterpartyOrders()
    {
        //Arrange
        var period = 0;
        var Company1 = Company.Factory.Create("Company 1",CompanyLevelEnum.Beginner);
        var Company2 = Company.Factory.Create("Company 2",CompanyLevelEnum.Beginner);
        var Company3 = Company.Factory.Create("Company 3",CompanyLevelEnum.Beginner);
        var testMarket = Market.Factory.CreateMarket("Test Market", CompanyLevelEnum.Market,new LinearDemandStrategy());
        
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
        BasicTransactionManager.RecordTrade(Company1BuysFromCompany2Order, testMarket, period, counterPartyOrders);
        var actualPrimaryOrder = testMarket.GetMarketTradesInPeriod(period).FirstOrDefault()?.RecordedTrade;
        var actualCounterPartyOrders = testMarket.GetMarketTradesInPeriod(period).FirstOrDefault()?.CounterPartyTrades;
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
        var period = 0;
        var Company1 = Company.Factory.Create("Company 1",CompanyLevelEnum.Beginner);
        var Company2 = Company.Factory.Create("Company 2",CompanyLevelEnum.Beginner);
        var testMarket = Market.Factory.CreateMarket("Test Market", CompanyLevelEnum.Market,new LinearDemandStrategy());
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
                MarketToSubmitTo = testMarket,
                Period = period};

        var expectedOrder = order;
        var expectedCounterPartyOrder = counterPartyOrder;
        //Act
        basicTransactionManager.ProcessPairedOrders(orderContext);
        var actualOrder = testMarket.GetMarketTradesInPeriod(period).FirstOrDefault()?.RecordedTrade;
        var actualCounterPartyOrder = testMarket.GetMarketTradesInPeriod(period).FirstOrDefault()?.CounterPartyTrades.FirstOrDefault();
        //Assert
        Assert.AreEqual(expectedOrder, actualOrder);
        Assert.AreEqual(expectedCounterPartyOrder, actualCounterPartyOrder);
    }
    [Test]
    public void ProcessTransactionRecordsTwoCounterPartiesWhenTwoCounterPartiesArePresent()
    {
        //Arrange
        var basicTransactionManager = new BasicTransactionManager();
        var period = 0;
        var Company1 = Company.Factory.Create("Company 1",CompanyLevelEnum.Beginner);
        var Company2 = Company.Factory.Create("Company 2",CompanyLevelEnum.Beginner);
        var Company3 = Company.Factory.Create("Company 3",CompanyLevelEnum.Beginner);
        var testMarket = Market.Factory.CreateMarket("Test Market", CompanyLevelEnum.Market,new LinearDemandStrategy());
        Company2.GetInventory().AddGood(new InventoryEntry(Lemon, 1, 1m, 1));
        Company3.GetInventory().AddGood(new InventoryEntry(Lemon, 1, 1m, 1));
        var Company1Buys2LemonFromMultiple = new Order(Company1, Company2, Lemon, 2, 1m)
        {
            SubmittingCompany = Company1
        };
        var counterPartyOrders = new List<Order>();
        var company2Sells1LemonToCompany1 = new Order(Company1,Company2,Lemon,1,1m)
        {
            SubmittingCompany = Company2
        };
        var company3Sells1LemonToCompany1 = new Order(Company1,Company3,Lemon,1,1m)
        {
            SubmittingCompany = Company3
        };
        counterPartyOrders.Add(company2Sells1LemonToCompany1);
        counterPartyOrders.Add(company3Sells1LemonToCompany1);

        var orderContext = new ActionContext{PrimaryOrder = Company1Buys2LemonFromMultiple
            ,CounterPartyOrders=counterPartyOrders,
            MarketToSubmitTo = testMarket,
            Period = period};

        var expectedOrder = Company1Buys2LemonFromMultiple;
        //Act
        basicTransactionManager.ProcessPairedOrders(orderContext);
        var actualOrder = testMarket.GetMarketTradesInPeriod(period).FirstOrDefault()?.RecordedTrade;
        var actualCounterPartyOrders = testMarket.GetMarketTradesInPeriod(period).FirstOrDefault()?.CounterPartyTrades;
        //Assert
        Assert.AreEqual(expectedOrder, actualOrder);
        Assert.Contains(company2Sells1LemonToCompany1, actualCounterPartyOrders);
        Assert.Contains(company3Sells1LemonToCompany1, actualCounterPartyOrders);
        Assert.AreEqual(2, actualCounterPartyOrders.Count);
    }
#endregion

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(TestEconomy.gameObject);
    }
}