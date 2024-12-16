using System.Linq;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class FillsAndPartialFillsTests
{
    TheEconomy testEconomy;
    Good Lemon;
    Good Water;
    Good Sugar;
    Good Lemonade;

    [SetUp]
    public void Setup()
    {
        var economyObject = new GameObject();
        testEconomy = economyObject.AddComponent<TheEconomy>();
        testEconomy.Initialize(new MockLogger());
        Lemon = Good.CreateInstance("Lemon", new Price_band(.5m, 2m), Rarity_enum.Common);
        Water = Good.CreateInstance("Water", new Price_band(.5m, 1m), Rarity_enum.Common);
        Sugar = Good.CreateInstance("Sugar", new Price_band(.5m, 1m), Rarity_enum.Common);
    }

    [Test]
    public void TestThatOrdersAreNotFilledIfThereIsNoCounterParty()
    {
        //Arrange
        var period =0;
        var TestMarket = Market.Factory.CreateMarket("TestMarket", CompanyLevelEnum.Market,new LinearDemandStrategy());
        
        var TestCompany1 = Company.Factory.Create("TestCompany1", CompanyLevelEnum.Beginner);
        TestCompany1.GetInventory().AddGood(new InventoryEntry(Lemon,100,2m,0));

        var TestCompany2 = Company.Factory.Create("TestCompany2", CompanyLevelEnum.Beginner);
        TestCompany2.GetInventory().AddGood(new InventoryEntry(Water,100,1m,0));

        TestMarket.RegisterCompany(TestCompany1);
        TestMarket.RegisterCompany(TestCompany2);

        var Company1BuysWaterFromCompany2 = new Order(TestCompany1, TestCompany2, Water, 50, 1m);
        var orderContext = new ActionContext{
            TradeToSubmit=Company1BuysWaterFromCompany2,
            MarketToSubmitTo=TestMarket,
            Period=period
            };
        TestCompany1.QueueOrder(orderContext);
        var expected = LemonadeStandResultObject
                .Failure(ResultTypeEnum.NoMatchingCounterParties, "No counterparty was found for this offer");
        //Act
        TestMarket.ProcessCompanyOrders();
        var actual = Company1BuysWaterFromCompany2.OrderStatus;
        //Assert
        Assert.AreEqual(expected.Result, actual.Result);
    }
    [Test]
    public void TestThatOrderFullyFillsIfCounterPartyMatchesQuantity()
    {
        //Arrange
        var period =0;
        var TestMarket = Market.Factory.CreateMarket("TestMarket", CompanyLevelEnum.Market,new LinearDemandStrategy());
        
        var TestCompany1 = Company.Factory.Create("TestCompany1", CompanyLevelEnum.Beginner);
        TestCompany1.GetInventory().AddGood(new InventoryEntry(Lemon,100,2m,0));
        var TestCompany2 = Company.Factory.Create("TestCompany2", CompanyLevelEnum.Beginner);

        TestMarket.RegisterCompany(TestCompany1);
        TestMarket.RegisterCompany(TestCompany2);

        var Company2BuysLemonFromCompany1 = new Order(TestCompany2, TestCompany1, Lemon, 50, 2m);
        var Company1SellsLemonToCompany2 = new Order(TestCompany2, TestCompany1, Lemon, 50, 2m);

        var Company1Context = new ActionContext{
            TradeToSubmit=Company2BuysLemonFromCompany1,
            MarketToSubmitTo=TestMarket,
            Period=period
            };
        var Company2Context = new ActionContext{
            TradeToSubmit=Company1SellsLemonToCompany2,
            MarketToSubmitTo=TestMarket,
            Period=period
            };
        
        TestCompany1.QueueOrder(Company1Context);
        TestCompany2.QueueOrder(Company2Context);
        var expectedLemonBuyFillQuantity = 50;
        var expectedLemonSellFillQuantity = 50;
        var expectedCompany1Lemons = 50;
        var expectedCompany2Lemons = 50;
        //Act
        TestMarket.ProcessCompanyOrders();
        var actualLemonBuyFillQuantity = Company2BuysLemonFromCompany1.FilledQuantity;
        var actualLemonSellFillQuantity = Company1SellsLemonToCompany2.FilledQuantity;
        var actualCompany1Lemons = TestCompany1.GetInventory().GetInventoryEntriesByGood(Lemon.good_name)?.FirstOrDefault()?.quantity??0;
        var actualCompany2Lemons = TestCompany2.GetInventory().GetInventoryEntriesByGood(Lemon.good_name)?.FirstOrDefault()?.quantity??0;
        //Assert
        Assert.AreEqual(expectedLemonBuyFillQuantity, actualLemonBuyFillQuantity,$"{TestCompany1}'s order was filled with {actualLemonBuyFillQuantity} lemons");
        Assert.AreEqual(expectedLemonSellFillQuantity, actualLemonSellFillQuantity,$"{TestCompany2}'s order was filled with {actualLemonSellFillQuantity} lemons");
        Assert.AreEqual(expectedCompany1Lemons, actualCompany1Lemons,$"{TestCompany1} had {actualCompany1Lemons} lemons at the end of the transaction");
        Assert.AreEqual(expectedCompany2Lemons, actualCompany2Lemons,$"{TestCompany2} had {actualCompany2Lemons} lemons at the end of the transaction");
    }

    [Test]
    public void TestThatMarketPartiallyFillsOrderIfBuyingCompanyDoesntWantEntireQuantity()
    { 
        throw new System.NotImplementedException(); 
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(testEconomy.gameObject);
    }
}
