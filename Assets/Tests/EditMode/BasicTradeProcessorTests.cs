using NUnit.Framework;
using NUnit.Framework.Internal;
using UnityEngine;

[TestFixture]
public class BasicTradeProcessorTests
{
    TheEconomy TestEconomy;
    [SetUp]
    public void SetUp()
    {
        var economyObject = new GameObject();
        TestEconomy = economyObject.AddComponent<TheEconomy>();
        TestEconomy.Initialize(new MockLogger());
        
    }
    [Test]
    public void ProcessCompanyOrdersIgnoresOrdersWhereSellerIsMarket()
    {
        // Arrange
        var period = 0;
        var demandStrategy = new LinearDemandStrategy();
        var testMarket = Market.Factory.CreateMarket("Test Market", CompanyLevelEnum.Market,demandStrategy);
        var testCompany = Company.Factory.Create("Test Company", CompanyLevelEnum.Beginner);
        testMarket.RegisterCompany(testCompany);
        var lemonade = Good.CreateInstance("Lemonade", new Price_band(1, 3), Rarity_enum.Uncommon);
        testCompany.GetInventory().AddGood(new InventoryEntry(lemonade, 100, 1m, period));
        var testOrder = new Trade(testMarket, testCompany, lemonade, 100, 10m);
        var testContext = new ActionContext{
                    TradeToSubmit = testOrder,
                    MarketToSubmitTo = testMarket,
                    Period = period
                                            };
        var expected=0;
        // Act
        testCompany.QueueOrder(testContext);
        testMarket.ProcessCompanyOrders();
        var actual = testOrder.FilledQuantity;
        // Assert
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void GetOrdersGetsUpdatedFillsForOrdersBetweenCompanies()
    {
        // Arrange
        var period = 0;
        var lemonade = Good.CreateInstance("Lemonade", new Price_band(1, 3), Rarity_enum.Uncommon);
        var radioactiveLemonade = Good.CreateInstance("Radioactive Lemonade", new Price_band(10,20), Rarity_enum.Very_Rare);

        var demandStrategy = new LinearDemandStrategy();
        var testMarket = Market.Factory.CreateMarket("Test Market", CompanyLevelEnum.Market,demandStrategy);
        testMarket.SetCash(1000000);
        
        var testCompany = Company.Factory.Create("Test Company", CompanyLevelEnum.Beginner);
        testMarket.RegisterCompany(testCompany);
        testCompany.SetCash(100000);
        
        var testCompany2 = Company.Factory.Create("Test Company 2", CompanyLevelEnum.Beginner);
        testMarket.RegisterCompany(testCompany2);
        testCompany2.SetCash(100000);

        testCompany.GetInventory().AddGood(new InventoryEntry(lemonade, 100, 1m, period));
        testCompany2.GetInventory().AddGood(new InventoryEntry(radioactiveLemonade, 10, 10m, period));

        var testOrder = new Trade(testCompany2,testCompany, lemonade, 100, 10m);
        var testOrder2 = new Trade(testCompany,testCompany2, radioactiveLemonade, 10, 10m);
        var testContext = new ActionContext{
                    TradeToSubmit = testOrder,
                    MarketToSubmitTo = testMarket,
                    Period = period
                                            };
        var testContext2 = new ActionContext{
                    TradeToSubmit = testOrder2,
                    MarketToSubmitTo = testMarket,
                    Period = period
                                            };
        var expectedLemonadeFill=100;
        var expectedRadioactiveLemonadeFill=10;
        // Act
        testCompany.QueueOrder(testContext);
        testCompany2.QueueOrder(testContext2);
        testMarket.ProcessCompanyOrders();
        
        var OrdersSentToMarket = testMarket.GetOrdersSentToMarket();
        var actualLemonadeFill = OrdersSentToMarket.Find(x => x.Good == lemonade).FilledQuantity;
        var actualRadioactiveLemonadeFill = OrdersSentToMarket.Find(x => x.Good == radioactiveLemonade).FilledQuantity;
        // Assert
        Assert.AreEqual(expectedLemonadeFill, actualLemonadeFill);
        Assert.AreEqual(expectedRadioactiveLemonadeFill, actualRadioactiveLemonadeFill);
    }
    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(TestEconomy.gameObject);
    }
}