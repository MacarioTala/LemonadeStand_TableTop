using NUnit.Framework;

[TestFixture]
public class DefaultTradeProcessorInternalsTests
{
    private Good Lemon;
    private Good Sugar;
    private EconAgent Company1;
    private EconAgent Company2;
    
    private Market TestMarket;
    private DefaultTradeProcessor TestTradeProcessor;

    [SetUp]
    public void Setup()
    {
        Lemon = new GoodBuilder().Named("Lemon").Costing(1).Build();
        Sugar = new GoodBuilder().Named("Sugar").Costing(1).Build();

        Company1 = EconAgent.Factory.Create("Buyer", AgentLevelEnum.Beginner);
        Company2 = EconAgent.Factory.Create("Seller", AgentLevelEnum.Beginner);

        TestTradeProcessor = new DefaultTradeProcessor();
        TestMarket = Market.Factory.CreateMarket("Test Market")
                .WithTradeProcessor(TestTradeProcessor)
                .WithPriceManager(new DefaultPriceManager())
                .WithTransactionManager(new DefaultTransactionManager())
                .EnsureDefaults();
    }
    [Test]
    public void IfSellOrderPriceSmallerThanBuyOrderPriceTradesCross()
    {
        // Arrange
        Company1.GetInventory().AddInventoryEntry(new InventoryEntry(Lemon, 100, 1, 0));
        Company2.SetCash(1000);

        var sellOrder = new Order(null, Company1, Lemon, 10, 9);
        var buyOrder = new Order(Company2, null, Lemon, 10, 10);

        Company1.QueueOrder(TestHelpers.CreateActionContext(sellOrder, TestMarket, TestMarket.CurrentPeriod));
        Company2.QueueOrder(TestHelpers.CreateActionContext(buyOrder, TestMarket, TestMarket.CurrentPeriod));

        // Act
        var actual = TestTradeProcessor.IsValidCounterParty(buyOrder, sellOrder);

        // Assert
        Assert.IsTrue(actual);
    }
    [Test]
    public void IfSellOrderPriceEqualToBuyOrderPriceTradesCross()
    {
        // Arrange
        Company1.GetInventory().AddInventoryEntry(new InventoryEntry(Lemon, 100, 1, 0));
        Company2.SetCash(1000);

        var sellOrder = new Order(null, Company1, Lemon, 10, 10);
        var buyOrder = new Order(Company2, null, Lemon, 10, 10);

        Company1.QueueOrder(TestHelpers.CreateActionContext(sellOrder, TestMarket, TestMarket.CurrentPeriod));
        Company2.QueueOrder(TestHelpers.CreateActionContext(buyOrder, TestMarket, TestMarket.CurrentPeriod));

        // Act
        var actual = TestTradeProcessor.IsValidCounterParty(buyOrder, sellOrder);

        // Assert
        Assert.IsTrue(actual);
    }
    [Test]
    public void IfSellOrderPriceLargerThanBuyOrderPriceTradesDontCross()
    {
        // Arrange
        Company1.GetInventory().AddInventoryEntry(new InventoryEntry(Lemon, 100, 1, 0));
        Company2.SetCash(1000);

        var sellOrder = new Order(null, Company1, Lemon, 10, 19);
        var buyOrder = new Order(Company2, null, Lemon, 10, 10);

        Company1.QueueOrder(TestHelpers.CreateActionContext(sellOrder, TestMarket, TestMarket.CurrentPeriod));
        Company2.QueueOrder(TestHelpers.CreateActionContext(buyOrder, TestMarket, TestMarket.CurrentPeriod));

        // Act
        var actual = TestTradeProcessor.IsValidCounterParty(buyOrder, sellOrder);

        // Assert
        Assert.IsFalse(actual);
    }

[Test]
public void IfSellOrderPriceSmallerThanBuyOrderPriceTradesCrossRegardlessOfOrderDirection()
{
    // Arrange
    Company1.GetInventory().AddInventoryEntry(new InventoryEntry(Lemon, 100, 1, 0));
    Company2.SetCash(1000);

    var sellOrder = new Order(null, Company1, Lemon, 10, 9);
    var buyOrder = new Order(Company2, null, Lemon, 10, 10);

    Company1.QueueOrder(TestHelpers.CreateActionContext(sellOrder, TestMarket, TestMarket.CurrentPeriod));
    Company2.QueueOrder(TestHelpers.CreateActionContext(buyOrder, TestMarket, TestMarket.CurrentPeriod));

    // Act
    var buyThenSell = TestTradeProcessor.IsValidCounterParty(buyOrder, sellOrder);
    var sellThenBuy = TestTradeProcessor.IsValidCounterParty(sellOrder, buyOrder);

    // Assert
    Assert.IsTrue(buyThenSell);
    Assert.IsTrue(sellThenBuy);
}

[Test]
public void IfSellOrderPriceLargerThanBuyOrderPriceTradesDontCrossRegardlessOfOrderDirection()
{
    // Arrange
    Company1.GetInventory().AddInventoryEntry(new InventoryEntry(Lemon, 100, 1, 0));
    Company2.SetCash(1000);

    var sellOrder = new Order(null, Company1, Lemon, 10, 19);
    var buyOrder = new Order(Company2, null, Lemon, 10, 10);

    Company1.QueueOrder(TestHelpers.CreateActionContext(sellOrder, TestMarket, TestMarket.CurrentPeriod));
    Company2.QueueOrder(TestHelpers.CreateActionContext(buyOrder, TestMarket, TestMarket.CurrentPeriod));

    // Act
    var buyThenSell = TestTradeProcessor.IsValidCounterParty(buyOrder, sellOrder);
    var sellThenBuy = TestTradeProcessor.IsValidCounterParty(sellOrder, buyOrder);

    // Assert
    Assert.IsFalse(buyThenSell);
    Assert.IsFalse(sellThenBuy);
}

[Test]
public void IfBothOrdersBuyNoValidCounterparties()
{
    // Arrange
    Company1.SetCash(1000);
    Company2.SetCash(1000);

    var buyOrder = new Order(Company2, null, Lemon, 10, 10);
    var otherBuyOrder = new Order(Company1,null , Lemon, 10, 19);

    Company1.QueueOrder(TestHelpers.CreateActionContext(otherBuyOrder, TestMarket, TestMarket.CurrentPeriod));
    Company2.QueueOrder(TestHelpers.CreateActionContext(buyOrder, TestMarket, TestMarket.CurrentPeriod));

    // Act
    var actual = TestTradeProcessor.IsValidCounterParty(buyOrder, otherBuyOrder);

    // Assert
    Assert.IsFalse(actual);
}
[Test]
public void IfBothOrdersSellNoValidCounterparties()
{
    // Arrange
    Company1.GetInventory().AddInventoryEntry(new InventoryEntry(Lemon, 100, 1, 0));
    Company2.GetInventory().AddInventoryEntry(new InventoryEntry(Lemon, 100, 1, 0));

    var sellOrder = new Order(null, Company2, Lemon, 10, 10);
    var otherSellOrder = new Order(null,Company1 , Lemon, 10, 19);

    Company1.QueueOrder(TestHelpers.CreateActionContext(otherSellOrder, TestMarket, TestMarket.CurrentPeriod));
    Company2.QueueOrder(TestHelpers.CreateActionContext(sellOrder, TestMarket, TestMarket.CurrentPeriod));

    // Act
    var actual = TestTradeProcessor.IsValidCounterParty(sellOrder, otherSellOrder);

    // Assert
    Assert.IsFalse(actual);
}
[Test]
public void IfAnyOrderFilledNoValidCounterparties()
{
    // Arrange
    Company1.GetInventory().AddInventoryEntry(new InventoryEntry(Lemon, 100, 1, 0));
    Company2.GetInventory().AddInventoryEntry(new InventoryEntry(Lemon, 100, 1, 0));

        var sellOrder = new Order(null, Company2, Lemon, 10, 10);
        var otherSellOrder = new Order(null,Company1 , Lemon, 10, 19)
        {
            FilledQuantity = 10
        };

    Company1.QueueOrder(TestHelpers.CreateActionContext(otherSellOrder, TestMarket, TestMarket.CurrentPeriod));
    Company2.QueueOrder(TestHelpers.CreateActionContext(sellOrder, TestMarket, TestMarket.CurrentPeriod));

    // Act
    var actual = TestTradeProcessor.IsValidCounterParty(sellOrder, otherSellOrder);

    // Assert
    Assert.IsFalse(actual);
}
[Test]
public void IfBothOrdersFullyFilledNoValidCounterparties()
{
    // Arrange
    Company1.GetInventory().AddInventoryEntry(new InventoryEntry(Lemon, 100, 1, 0));
    Company2.GetInventory().AddInventoryEntry(new InventoryEntry(Lemon, 100, 1, 0));

        var sellOrder = new Order(null, Company2, Lemon, 10, 10)
        {
            FilledQuantity = 10
        };
        var otherSellOrder = new Order(null,Company1 , Lemon, 10, 19)
        {
            FilledQuantity = 10
        };

    Company1.QueueOrder(TestHelpers.CreateActionContext(otherSellOrder, TestMarket, TestMarket.CurrentPeriod));
    Company2.QueueOrder(TestHelpers.CreateActionContext(sellOrder, TestMarket, TestMarket.CurrentPeriod));

    // Act
    var actual = TestTradeProcessor.IsValidCounterParty(sellOrder, otherSellOrder);

    // Assert
    Assert.IsFalse(actual);
}
[Test]
public void IfBothOrdersBySameCompanyNoValidCounterparties()
{
    // Arrange
    Company1.GetInventory().AddInventoryEntry(new InventoryEntry(Lemon, 100, 1, 0));
    Company1.SetCash(1000);

    var sellOrder = new Order(null, Company1, Lemon, 10, 19);
    var buyOrder = new Order(Company1, null, Lemon, 10, 10);

    Company1.QueueOrder(TestHelpers.CreateActionContext(sellOrder, TestMarket, TestMarket.CurrentPeriod));
    Company1.QueueOrder(TestHelpers.CreateActionContext(buyOrder, TestMarket, TestMarket.CurrentPeriod));

    // Act
    var actual=TestTradeProcessor.IsValidCounterParty(buyOrder, sellOrder);

    // Assert
    Assert.IsFalse(actual);
}
[Test]
    public void IfGoodsAreDifferentNoValidCounterparties()
    {
        // Arrange
        Company1.GetInventory().AddInventoryEntry(new InventoryEntry(Lemon, 100, 1, 0));
        Company2.SetCash(1000);

        var sellOrder = new Order(null, Company1, Lemon, 10, 9);
        var buyOrder = new Order(Company2, null, Sugar, 10, 10);

        Company1.QueueOrder(TestHelpers.CreateActionContext(sellOrder, TestMarket, TestMarket.CurrentPeriod));
        Company2.QueueOrder(TestHelpers.CreateActionContext(buyOrder, TestMarket, TestMarket.CurrentPeriod));

        // Act
        var actual = TestTradeProcessor.IsValidCounterParty(buyOrder, sellOrder);

        // Assert
        Assert.IsFalse(actual);
    }
}