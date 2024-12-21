using NUnit.Framework;
using NUnit.Framework.Internal;
using UnityEngine;

[TestFixture]
public partial class BasicTradeProcessorTests
{
    TheEconomy TestEconomy;
    int Period = 0;
    Market TestMarket;
    BasicTradeProcessor TestTradeProcessor;

    Good Lemonade;
    Good RadioactiveLemonade;

    Good Lemon;

    Company Company1;
    Company Company2;

    [SetUp]
    public void SetUp()
    {
        var economyObject = new GameObject();
        TestEconomy = economyObject.AddComponent<TheEconomy>();
        TestEconomy.Initialize(new MockLogger());

        Company1 = Company.Factory.Create("Company 1", CompanyLevelEnum.Beginner);
        Company2 = Company.Factory.Create("Company 2", CompanyLevelEnum.Beginner);

        TestMarket = Market.Factory.CreateMarket("Test Market", CompanyLevelEnum.Market, new LinearDemandStrategy());
        TestTradeProcessor = new BasicTradeProcessor();
        TestMarket.SetTradeProcessor(TestTradeProcessor);
        TestMarket.SetCash(1000000);
        TestMarket.RegisterCompany(Company1);
        TestMarket.RegisterCompany(Company2);
        
        Lemon = Good.CreateInstance("Lemons", new Price_band(1, 3), Rarity_enum.Common);
        Lemonade = Good.CreateInstance("Lemonade", new Price_band(1, 3), Rarity_enum.Uncommon);
        RadioactiveLemonade = Good.CreateInstance("Radioactive Lemonade", new Price_band(10, 20), Rarity_enum.Very_Rare);
    }
    
#region ProcessCompanyOrders Tests
    [Test]
    public void ProcessCompanyOrdersIgnoresOrdersWhereSellerIsMarket()
    {
        // Arrange
        Company1.GetInventory().AddGood(new InventoryEntry(Lemonade, 100, 1m, Period));
        var testOrder = new Order(TestMarket, Company1, Lemonade, 100, 10m);
        var testContext = new ActionContext{
                    TradeToSubmit = testOrder,
                    MarketToSubmitTo = TestMarket,
                    Period = Period};
        var expected=0;
        // Act
        Company1.QueueOrder(testContext);
        TestMarket.ProcessCompanyOrders();
        var actual = testOrder.FilledQuantity;
        // Assert
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void GetOrdersGetsUpdatedFillsForOrdersBetweenCompanies()
    {
        // Arrange
        Company1.GetInventory().AddGood(new InventoryEntry(Lemonade, 100, 1m, Period));
        Company2.GetInventory().AddGood(new InventoryEntry(RadioactiveLemonade, 10, 10m, Period));

        var testOrder = new Order(Company2,Company1, Lemonade, 100, 10m);
        var testOrder2 = new Order(Company1,Company2, RadioactiveLemonade, 10, 10m);
        var testContext = new ActionContext{
                    TradeToSubmit = testOrder,
                    MarketToSubmitTo = TestMarket,
                    Period = Period
                                            };
        var testContext2 = new ActionContext{
                    TradeToSubmit = testOrder2,
                    MarketToSubmitTo = TestMarket,
                    Period = Period
                                            };
        var expectedLemonadeFill=100;
        var expectedRadioactiveLemonadeFill=10;
        Company1.QueueOrder(testContext);
        Company2.QueueOrder(testContext2);
        // Act
        TestTradeProcessor.ProcessCompanyOrders(TestMarket);
        
        var actualLemonadeFill = testOrder.FilledQuantity;
        var actualRadioactiveLemonadeFill = testOrder2.FilledQuantity;
        // Assert
        Assert.AreEqual(expectedLemonadeFill, actualLemonadeFill);
        Assert.AreEqual(expectedRadioactiveLemonadeFill, actualRadioactiveLemonadeFill);
    }
#endregion


    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(TestEconomy.gameObject);
        TestMarket = null;
        TestTradeProcessor = null;
        Company1 = null;
        Company2 = null;
        Lemonade = null;
        RadioactiveLemonade = null;
    }
}