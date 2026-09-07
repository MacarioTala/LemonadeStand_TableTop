using System.Linq;
using NUnit.Framework;

[TestFixture]
public class HistoricalRecordTests
{
    private Market testMarket;
    private DefaultTradeProcessor testTradeProcessor;
    private DefaultTransactionManager testTransactionManager;
    private EconAgent testCompany;
    private EconAgent testBuyer;
    private EconAgent testSeller;
    private Good lemonade;

    [SetUp]
    public void SetUp()
    {
        testTransactionManager = new();
        testTradeProcessor = new(testTransactionManager);
        
        
        // Arrange
        testMarket = Market.Create()
            .Named("Historical Journal Test Market")
            .WithTradeProcessor(testTradeProcessor)
            .WithTransactionManager(testTransactionManager)
            .EnsureDefaults();

        testMarket.CurrentPeriod = 5;

        testCompany = EconAgentBuilder.For<EconAgent>()
            .Named("Test Company 1")
            .AtLevel(AgentLevelEnum.Beginner)
            .WithInitialCash(1000)
            .WithFixedCostStrategy(new BasicFixedCostStrategy())
            .Build();
        
        testBuyer = EconAgentBuilder.For<EconAgent>()
            .Named("Test Buyer")
            .AtLevel(AgentLevelEnum.Beginner)
            .WithInitialCash(1000)
            .WithFixedCostStrategy(new BasicFixedCostStrategy())
            .Build();

        testSeller = EconAgentBuilder.For<EconAgent>()
            .Named("Test Seller")
            .AtLevel(AgentLevelEnum.Beginner)
            .WithInitialCash(1000)
            .WithFixedCostStrategy(new BasicFixedCostStrategy())
            .Build();

        testMarket.RegisterMarketParticipant(testCompany);
        testMarket.RegisterMarketParticipant(testBuyer);
        testMarket.RegisterMarketParticipant(testSeller);

        lemonade = new GoodBuilder()
            .Named("Lemonade")
            .Costing(3)
            .Build();
    }

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(testMarket);
        UnityEngine.Object.DestroyImmediate(lemonade);

        testMarket = null;
        testTradeProcessor = null;
        testCompany = null;
        testBuyer=null;
        testSeller=null;
        lemonade = null;
    }
#region TradeProcessor.QueueOrder
 [Test]
    public void QueueOrder_WhenNoRecordsExistLog1Record()
    {
        // Arrange
        var order = new Order(null, testCompany, lemonade, 10, 2)
        {
            SubmittingCompany = testCompany
        };

        var context = new ActionContext
        {
            TradeToSubmit = order,
            MarketToSubmitTo = testMarket,
            Period = testMarket.CurrentPeriod
        };

        // Act
        var result = testTradeProcessor.QueueOrder(context);

        // Assert
        Assert.AreEqual(ResultTypeEnum.Success, result.Result);
        Assert.AreEqual(1, testTradeProcessor.GetOrders().Count);

        var records = testMarket.GetHistoricalRecordsInPeriod().ToList();
        Assert.AreEqual(1, records.Count);

        var record = records.Single();
        Assert.AreEqual(OrderResultEnum.Submitted, record.Result);
        Assert.AreEqual(testMarket.CurrentPeriod, record.CreatedInPeriod);
        Assert.AreEqual(0, record.FilledQuantity);
        Assert.AreEqual(order.Quantity, record.RemainingQuantity);
        Assert.IsTrue(string.IsNullOrEmpty(record.Message));
    }
     [Test]
    public void QueueOrder_WhenRecordExistsLog1Submitted1Rejected()
    {
        // Arrange
        var order = new Order(null, testCompany, lemonade, 10, 2)
        {
            SubmittingCompany = testCompany
        };

        var context = new ActionContext
        {
            TradeToSubmit = order,
            MarketToSubmitTo = testMarket,
            Period = testMarket.CurrentPeriod
        };

        testTradeProcessor.QueueOrder(context);

        // Act
        var result = testTradeProcessor.QueueOrder(context);

        // Assert
        Assert.AreEqual(ResultTypeEnum.DuplicateOrder, result.Result);
        Assert.AreEqual("Order already exists in the queue", result.Message);
        Assert.AreEqual(1, testTradeProcessor.GetOrders().Count);

        var records = testMarket.GetHistoricalRecordsInPeriod().ToList();
        Assert.AreEqual(2, records.Count);

        Assert.AreEqual(OrderResultEnum.Submitted, records.First().Result);
        Assert.AreEqual(OrderResultEnum.Rejected, records.Last().Result);
        Assert.AreEqual("Order already exists in the queue", records.Last().Message);
    }
#endregion 

#region TradeProcessor.ExecuteBestTradesForGood
[Test]
    public void ExecuteBestTradesForGood_WhenOnlyOneOrderExists_LogsRejectedRecordForNoMatchingCounterParty()
    {
        // Arrange
        testSeller.GetInventory().AddInventoryEntry(new InventoryEntry(lemonade, 10, 3, 0));

        var sellOrder = new Order(null, testSeller, lemonade, 10, 2)
        {
            SubmittingCompany = testSeller
        };

        var context = new ActionContext
        {
            TradeToSubmit = sellOrder,
            MarketToSubmitTo = testMarket,
            Period = testMarket.CurrentPeriod
        };

        testTradeProcessor.QueueOrder(context);

        // Act
        testTradeProcessor.ProcessCompanyOrders();

        // Assert
        var records = testMarket.GetHistoricalRecordsInPeriod().ToList();
        Assert.AreEqual(2, records.Count);

        var rejectedRecord = records.Last();
        Assert.AreEqual(OrderResultEnum.Rejected, rejectedRecord.Result);
        Assert.AreEqual(ResultTypeEnum.NoMatchingCounterParties.ToString(), rejectedRecord.Message);
        Assert.AreEqual(0, rejectedRecord.FilledQuantity);
        Assert.AreEqual(10, rejectedRecord.RemainingQuantity);
        Assert.AreEqual(sellOrder.Id, rejectedRecord.OriginalOrderSnapshot.OrderId);
        Assert.AreEqual(testSeller.Name, rejectedRecord.OriginalOrderSnapshot.SellerName);
        Assert.AreEqual("Lemonade", rejectedRecord.OriginalOrderSnapshot.GoodName);
    }

    [Test]
    public void ExecuteBestTradesForGood_WhenOneOrderIsInvalid_Logs2RejectedRecords()
    {
        // Arrange
        testBuyer.SetCash(1);
        testSeller.GetInventory().AddInventoryEntry(new InventoryEntry(lemonade, 10, 3, 0));

        var buyOrder = new Order(testBuyer, null, lemonade, 10, 2)
        {
            SubmittingCompany = testBuyer
        };

        var sellOrder = new Order(null, testSeller, lemonade, 10, 2)
        {
            SubmittingCompany = testSeller
        };

        testTradeProcessor.QueueOrder(new ActionContext
        {
            TradeToSubmit = buyOrder,
            MarketToSubmitTo = testMarket,
            Period = testMarket.CurrentPeriod
        });

        testTradeProcessor.QueueOrder(new ActionContext
        {
            TradeToSubmit = sellOrder,
            MarketToSubmitTo = testMarket,
            Period = testMarket.CurrentPeriod
        });

        // Act
        testTradeProcessor.ProcessCompanyOrders();
        var records = testMarket.GetHistoricalRecordsInPeriod().ToList();
        var rejectedBuyRecord = records.FirstOrDefault(x=>x.Result == OrderResultEnum.Rejected && x.OriginalOrderSnapshot.OrderId==buyOrder.Id);
        var rejectedSellRecord = records.FirstOrDefault(x=>x.Result == OrderResultEnum.Rejected && x.OriginalOrderSnapshot.OrderId==sellOrder.Id);

        // Assert
        Assert.AreEqual(4, records.Count);

        Assert.AreEqual(ResultTypeEnum.InsufficientCash.ToString(), rejectedBuyRecord.Message);
        Assert.AreEqual(0, rejectedBuyRecord.FilledQuantity);
        Assert.AreEqual(10, rejectedBuyRecord.RemainingQuantity);

        Assert.AreEqual(ResultTypeEnum.NoMatchingCounterParties.ToString(), rejectedSellRecord.Message);
        Assert.AreEqual(0, rejectedSellRecord.FilledQuantity);
        Assert.AreEqual(10, rejectedSellRecord.RemainingQuantity);        
    }

    [Test]
    public void ExecuteBestTradesForGood_WhenOrderFullyFills_LogsFilledRecordWithUpdatedQuantities()
    {
        // Arrange
        testSeller.GetInventory().AddInventoryEntry(new InventoryEntry(lemonade, 10, 3, 0));

        var buyOrder = new Order(testBuyer, null, lemonade, 10, 2)
        {
            SubmittingCompany = testBuyer
        };

        var sellOrder = new Order(null, testSeller, lemonade, 10, 2)
        {
            SubmittingCompany = testSeller
        };

        testTradeProcessor.QueueOrder(new ActionContext
        {
            TradeToSubmit = buyOrder,
            MarketToSubmitTo = testMarket,
            Period = testMarket.CurrentPeriod
        });

        testTradeProcessor.QueueOrder(new ActionContext
        {
            TradeToSubmit = sellOrder,
            MarketToSubmitTo = testMarket,
            Period = testMarket.CurrentPeriod
        });

        // Act
        testTradeProcessor.ProcessCompanyOrders();
        var records = testMarket.GetHistoricalRecordsInPeriod().ToList();
        var recordedBuyOrder = records.FirstOrDefault(x=>x.OriginalOrderSnapshot.OrderId==buyOrder.Id && x.Result == OrderResultEnum.Filled);
        var recordedSellOrder = records.FirstOrDefault(x=>x.OriginalOrderSnapshot.OrderId==sellOrder.Id && x.Result == OrderResultEnum.Filled);

        // Assert
        Assert.AreEqual(4, records.Count); //two queued, two filled
        Assert.AreEqual(OrderResultEnum.Filled, recordedBuyOrder.Result);
        Assert.AreEqual(OrderResultEnum.Filled, recordedSellOrder.Result);
        Assert.AreEqual(10, recordedBuyOrder.FilledQuantity);
        Assert.AreEqual(10, recordedSellOrder.FilledQuantity);
        Assert.AreEqual(0, recordedBuyOrder.RemainingQuantity);
        Assert.AreEqual(0, recordedSellOrder.RemainingQuantity);
        Assert.AreEqual(testBuyer.Name, recordedBuyOrder.OriginalOrderSnapshot.BuyerName);
        Assert.AreEqual(testSeller.Name, recordedSellOrder.OriginalOrderSnapshot.SellerName);
        Assert.AreEqual("Lemonade", recordedBuyOrder.OriginalOrderSnapshot.GoodName);
        Assert.AreEqual("Lemonade", recordedSellOrder.OriginalOrderSnapshot.GoodName);
    }

    [Test]
    public void ExecuteBestTradesForGood_WhenOrderPartiallyFills_LogsPartiallyFilledRecordWithUpdatedQuantities()
    {
        // Arrange
        testSeller.GetInventory().AddInventoryEntry(new InventoryEntry(lemonade, 10, 3, 0));

        var buyOrder = new Order(testBuyer, null, lemonade, 20, 2)
        {
            SubmittingCompany = testBuyer
        };

        var sellOrder = new Order(null, testSeller, lemonade, 10, 2)
        {
            SubmittingCompany = testSeller
        };

        testTradeProcessor.QueueOrder(new ActionContext
        {
            TradeToSubmit = buyOrder,
            MarketToSubmitTo = testMarket,
            Period = testMarket.CurrentPeriod
        });

        testTradeProcessor.QueueOrder(new ActionContext
        {
            TradeToSubmit = sellOrder,
            MarketToSubmitTo = testMarket,
            Period = testMarket.CurrentPeriod
        });

        // Act
        testTradeProcessor.ProcessCompanyOrders();
        var records = testMarket.GetHistoricalRecordsInPeriod().ToList();
        var partialRecord = records.FirstOrDefault(x=>x.Result==OrderResultEnum.PartiallyFilled);

        // Assert
        
        Assert.AreEqual(5, records.Count);
        Assert.AreEqual(10, partialRecord.FilledQuantity);
        Assert.AreEqual(10, partialRecord.RemainingQuantity);
        Assert.AreEqual(buyOrder.Id, partialRecord.OriginalOrderSnapshot.OrderId);
        Assert.AreEqual(testBuyer.Name, partialRecord.OriginalOrderSnapshot.BuyerName);
        Assert.AreEqual("Lemonade", partialRecord.OriginalOrderSnapshot.GoodName);
    }
    [Test]
public void ExecuteBestTradesForGood_WhenOrderPartiallyFillsCorrectlyLogsWhichIsFilledAndWhichIsPartiallyFilled()
{
    // Arrange
    testSeller.GetInventory().AddInventoryEntry(new InventoryEntry(lemonade, 10, 3, 0));

    var buyOrder = new Order(testBuyer, null, lemonade, 20, 2)
    {
        SubmittingCompany = testBuyer
    };

    var sellOrder = new Order(null, testSeller, lemonade, 10, 2)
    {
        SubmittingCompany = testSeller
    };

    testTradeProcessor.QueueOrder(new ActionContext
    {
        TradeToSubmit = buyOrder,
        MarketToSubmitTo = testMarket,
        Period = testMarket.CurrentPeriod
    });

    testTradeProcessor.QueueOrder(new ActionContext
    {
        TradeToSubmit = sellOrder,
        MarketToSubmitTo = testMarket,
        Period = testMarket.CurrentPeriod
    });

    // Act
    testTradeProcessor.ProcessCompanyOrders();
    var records = testMarket.GetHistoricalRecordsInPeriod().ToList();

    var recordedBuyOrder = records.FirstOrDefault(
        x => x.OriginalOrderSnapshot.OrderId == buyOrder.Id &&
             x.Result == OrderResultEnum.PartiallyFilled);

    var recordedSellOrder = records.FirstOrDefault(
        x => x.OriginalOrderSnapshot.OrderId == sellOrder.Id &&
             x.Result == OrderResultEnum.Filled);
    
    var unmatchedOrder = records.FirstOrDefault(
        x => x.OriginalOrderSnapshot.OrderId == buyOrder.Id &&
             x.Result == OrderResultEnum.Rejected &&
             x.Message == ResultTypeEnum.NoMatchingCounterParties.ToString());

    // Assert
    Assert.AreEqual(5, records.Count); // two submitted, one partial, one filled, one with no remaining counterparty
    Assert.That(unmatchedOrder is not null);

    Assert.IsNotNull(recordedBuyOrder);
    Assert.IsNotNull(recordedSellOrder);

    Assert.AreEqual(10, recordedBuyOrder.FilledQuantity);
    Assert.AreEqual(10, recordedBuyOrder.RemainingQuantity);
    Assert.AreEqual(testBuyer.Name, recordedBuyOrder.OriginalOrderSnapshot.BuyerName);

    Assert.AreEqual(10, recordedSellOrder.FilledQuantity);
    Assert.AreEqual(0, recordedSellOrder.RemainingQuantity);
    Assert.AreEqual(testSeller.Name, recordedSellOrder.OriginalOrderSnapshot.SellerName);
}
#endregion
}