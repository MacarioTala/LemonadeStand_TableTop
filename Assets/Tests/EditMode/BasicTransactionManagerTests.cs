using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class BasicTransactionManagerTests
{
    TheEconomy TestEconomy;
    Good Lemon;
    Good Lemonade;
    Market TestMarket;
    int Period;

    Company Company1;
    Company Company2;
    readonly Price_band PriceBand1 = new(.5m, 1.0m);
    readonly Price_band PriceBand2 = new(5.0m, 10m);

    [SetUp]
    public void Setup()
    {
        var TestEconomyObject = new GameObject();
        TestEconomy = TestEconomyObject.AddComponent<TheEconomy>();
        TestEconomy.Initialize(new MockLogger());

        TestMarket = Market.Factory.CreateMarket("Test Market", CompanyLevelEnum.Market, new LinearDemandStrategy());
        Company1 = Company.Factory.Create("Company 1", CompanyLevelEnum.Beginner);
        Company2 = Company.Factory.Create("Company 2", CompanyLevelEnum.Beginner);
        TestMarket.RegisterCompany(Company1);
        TestMarket.RegisterCompany(Company2);

        Period=0;

        Lemon = Good.CreateInstance("Lemon", PriceBand1, Rarity_enum.Common);
        Lemonade = Good.CreateInstance("Lemonade", PriceBand2, Rarity_enum.Uncommon);
    }
#region ValidateTransactionTests
    [Test]
    public void ValidateTransactionShouldReturnSelfTradeWhenCompanySubmitsTwoIdenticalTrades()
    {
        //Arrange
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
        var expected = LemonadeStandResultObject.Failure(ResultTypeEnum.InsufficientCash, "");
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
        var actual = transactionManager.ProcessTransactionPair(order, counterPartyOrder, Period);
        //Assert
        Assert.AreEqual(expected.Result, actual.Result);
    }
    [Test]
    public void FullyFilledTradesTransferCashAndGoodsCorrectly()
    {
        //Arrange
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
        transactionManager.ProcessTransactionPair(order, counterPartyOrder, Period);
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
        transactionManager.ProcessTransactionPair(order, counterPartyOrder, Period);
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
    [Test]
    public void ProcessTransactionRecordsTwoCounterPartiesWhenTwoCounterPartiesArePresent()
    {
        //Arrange
        var basicTransactionManager = new BasicTransactionManager();
        var Company3 = Company.Factory.Create("Company 3",CompanyLevelEnum.Beginner);
        
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
            MarketToSubmitTo = TestMarket,
            Period = Period};

        var expectedOrder = Company1Buys2LemonFromMultiple;
        //Act
        basicTransactionManager.ProcessPairedOrders(orderContext);
        var actualOrder = TestMarket.GetMarketTradesInPeriod(Period).FirstOrDefault()?.RecordedTrade;
        var actualCounterPartyOrders = TestMarket.GetMarketTradesInPeriod(Period).FirstOrDefault()?.CounterPartyTrades;
        //Assert
        Assert.AreEqual(expectedOrder, actualOrder);
        Assert.Contains(company2Sells1LemonToCompany1, actualCounterPartyOrders);
        Assert.Contains(company3Sells1LemonToCompany1, actualCounterPartyOrders);
        Assert.AreEqual(2, actualCounterPartyOrders.Count);
    }
#endregion
#region ProcessMarketTransactionTests
    [Test]
    public void ProcessMarketTransactionShouldReturnSuccessForValidOrderPair()
    {
        //Arrange
        Company1.GetInventory().AddGood(new InventoryEntry(Lemon, 1, 1m, 1));
        var Company1SellsLemonsToAnyone = new Order(Company1, null, Lemon, 1, 1m)
        {
            SubmittingCompany = Company1
        };
        var marketGeneratedCounterPartyOrder = new Order(Company1,Company2,Lemon,1,1m)
        {
            SubmittingCompany = Company2
        };
        var context = new ActionContext
        {
            PrimaryOrder = Company1SellsLemonsToAnyone,
            CounterPartyOrders = new List<Order>{marketGeneratedCounterPartyOrder},
            MarketToSubmitTo = TestMarket,
            Period = Period
        };
        var expected = LemonadeStandResultObject.Success();
        var transactionManager = new BasicTransactionManager();
        //Act
        var actual = transactionManager.ProcessMarketTransaction(context);
        //Assert
        Assert.AreEqual(expected.Result, actual.Result);
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
        BasicTransactionManager.RecordTrade(order, TestMarket, Period, counterPartyOrders);
        var actualOrder = TestMarket.GetMarketTradesInPeriod(Period).FirstOrDefault()?.RecordedTrade;
        var actualCounterPartyOrder = TestMarket.GetMarketTradesInPeriod(Period).FirstOrDefault()?.CounterPartyTrades.FirstOrDefault();
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
        BasicTransactionManager.RecordTrade(Company1BuysFromCompany2Order, TestMarket, Period, counterPartyOrders);
        var actualPrimaryOrder = TestMarket.GetMarketTradesInPeriod(Period).FirstOrDefault()?.RecordedTrade;
        var actualCounterPartyOrders = TestMarket.GetMarketTradesInPeriod(Period).FirstOrDefault()?.CounterPartyTrades;
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
        var actualOrder = TestMarket.GetMarketTradesInPeriod(Period).FirstOrDefault()?.RecordedTrade;
        var actualCounterPartyOrder = TestMarket.GetMarketTradesInPeriod(Period).FirstOrDefault()?.CounterPartyTrades.FirstOrDefault();
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
        BasicTransactionManager.RecordTrade(PrimaryOrder,TestMarket,Period,CounterPartyOrders);
        
        //Assert
        var transactionsRecorded = TestMarket.GetMarketTradesInPeriod(Period);
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
        BasicTransactionManager.RecordTrade(PrimaryOrder,TestMarket,Period,CounterPartyOrders);
        
        //Assert
        var transactionsRecorded = TestMarket.GetMarketTradesInPeriod(Period);
        var recordedTransaction = transactionsRecorded?.FirstOrDefault();
        Assert.IsNotNull(recordedTransaction);
        Assert.IsTrue(recordedTransaction.CounterPartyTrades.Count == 1);
        Assert.AreEqual(Company2,recordedTransaction.CounterPartyTrades.FirstOrDefault().Buyer);
        Assert.AreEqual(Company1,recordedTransaction.RecordedTrade.Seller);
    }
    [Test]
    public void RT_OneOrderOneCounterpartyTwoMarketTrades()
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
        var expectedMarketTransactions = new List<MarketTransaction>()
        {
            new(PrimaryOrder,Period),
            new(CounterPartyOrder,Period)
        };
        //Act
        BasicTransactionManager.RecordTrade(PrimaryOrder,TestMarket,
                                            Period,
                                            new List<Order>{CounterPartyOrder});
        var transactionsRecorded = TestMarket.GetMarketTradesInPeriod(Period);
        //Assert
        Assert.AreEqual(expectedMarketTransactionCount,transactionsRecorded.Count);
        Assert.AreEqual(expectedMarketTransactions,transactionsRecorded);
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
        var expectedMarketTransactions = new List<MarketTransaction>()
        {
            new(PrimaryOrder,Period),
            new(CounterPartyOrder1,Period),
            new(CounterPartyOrder2,Period)
        };
        //Act
        BasicTransactionManager.RecordTrade(PrimaryOrder,TestMarket,
                                            Period,
                                            new List<Order>{CounterPartyOrder1,CounterPartyOrder2});
        var transactionsRecorded = TestMarket.GetMarketTradesInPeriod(Period);
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
        BasicTransactionManager.RecordTrade(PrimaryOrder,TestMarket,
                                            Period,
                                            new List<Order>{CounterPartyOrder});
        var transactionsRecorded = TestMarket.GetMarketTradesInPeriod(Period);
        var firstTrade = transactionsRecorded.FirstOrDefault();
        var secondTrade = transactionsRecorded.LastOrDefault();
        var actualCounterPartyOrderForFirstTrade = firstTrade.CounterPartyTrades.FirstOrDefault();
        var actualCounterPartyOrderForSecondTrade = secondTrade.CounterPartyTrades.FirstOrDefault();
        //Assert
        Assert.AreEqual(expectedCounterPartyOrderForFirstTrade,actualCounterPartyOrderForFirstTrade);
        Assert.AreEqual(expectedCounterPartyOrderForSecondTrade,actualCounterPartyOrderForSecondTrade);
    }
#endregion

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(TestEconomy.gameObject);
        TestMarket = null;
        Lemon = null;
        Company1 = null;
        Company2 = null;
    }
}