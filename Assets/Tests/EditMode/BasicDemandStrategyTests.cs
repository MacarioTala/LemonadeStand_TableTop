using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class BasicDemandStrategyTests
{
    TheEconomy TestEconomy;
    Good lemon;
    Good water;
    Good sugar;
    [SetUp]
    public void Setup ()
    {
        var economyObject = new GameObject();
        TestEconomy = economyObject.AddComponent<TheEconomy>();
        TestEconomy.Initialize(new MockLogger());
        lemon = Good.CreateInstance("Lemon", new Price_band(.5m, 1.0m), Rarity_enum.Common);
        water = Good.CreateInstance("Water", new Price_band(.5m, 1.0m), Rarity_enum.Common);
        sugar = Good.CreateInstance("Sugar", new Price_band(.5m, 1.0m), Rarity_enum.Common);
    }
    [Test]
    public void GetTotalBoughtReturnsTotalAmountOfGoodBoughtInAPeriodWithNoMarketOrders()
    {
        // Arrange
        var period = 0;
        var strategy = new LinearDemandStrategy();
        var marketToTest = Market.Factory.CreateStarterMarket("Market To Test", CompanyLevelEnum.Market,strategy);
        var company1 = Company.Factory.Create("Company 1", CompanyLevelEnum.Beginner);
        marketToTest.RegisterCompany(company1);
        var company2 = Company.Factory.Create("Company 2", CompanyLevelEnum.Beginner);
        marketToTest.RegisterCompany(company2);
        company1.GetInventory().AddGood(new InventoryEntry(lemon, 2000,3m,period));

        var lemonOrder = new Order(company2, company1, lemon, 500, 3.0m);
        var lemonContext = new ActionContext { TradeToSubmit = lemonOrder, MarketToSubmitTo = marketToTest , Period = period};
        marketToTest.QueueOrder(lemonContext);
        marketToTest.QueueOrder(lemonContext);
        marketToTest.QueueOrder(lemonContext);

        const int expected = 1500;
        const int tradingPeriod = 0;
        
        // Act
        marketToTest.ProcessCompanyOrders();
        marketToTest.FulfillDemand();
        var actual = ((iDemandStrategy)strategy).GetTotalBought(marketToTest, lemon, tradingPeriod);
        // Assert
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void GetTotalBoughtReturnsTotalAmountOfGoodBoughtInAPeriodWithMarketOrders()
    {
        throw new System.NotImplementedException();
    }
    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(TestEconomy.gameObject);
    }
}