using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class BasicConsumptionManagerTests
{
    TheEconomy TestEconomy;
    Good lemon;
    Good water;
    Good sugar;
    Good lemonade;

    Recipe lemonadeRecipe;
    readonly List<Good> testGoods = new();

    //Pricing bands
    readonly Price_band band1 = new(.5m, 2m);
    readonly Price_band band2 = new(2.1m, 3m);
    readonly Price_band band3 = new(3.1m, 6m);

    
    [SetUp]
    public void SetUp()
    {
        var EconomyObject = new GameObject();
        TestEconomy = EconomyObject.AddComponent<TheEconomy>();
        TestEconomy.Initialize(new MockLogger());
        SetupGoodsAndRecipes();
    }

    private void SetupGoodsAndRecipes()
    {
        water = Good.CreateInstance("Water", band1, Rarity_enum.Common);
        sugar = Good.CreateInstance("Sugar", band1, Rarity_enum.Common);
        lemon = Good.CreateInstance("Lemon", band2, Rarity_enum.Common);
        lemonade = Good.CreateInstance("Lemonade", band3, Rarity_enum.Uncommon);
        lemonadeRecipe = new Recipe(RecipeName: "Basic Lemonade",
                                     product: lemonade, 
                                     ingredients: new List<Ingredient> { new(lemon, 9), 
                                                                        new(sugar, 2), 
                                                                        new(water, 7) });                
        testGoods.Add(lemon);
        testGoods.Add(water);
        testGoods.Add(sugar);
        testGoods.Add(lemonade);
    }
    [Test]
    public void FillOrderBasedOnPriceFillsDemandOf1000WithLowestPricedOrder()
    {
        //Arrange
        var marketToTest = Market.Factory.CreateStarterMarket("Starter Market",
                                                              CompanyLevelEnum.Market,
                                                              new LinearDemandStrategy() );
        var company1 = Company.Factory.Create("Company1", CompanyLevelEnum.Beginner);
        var company2 = Company.Factory.Create("Company2", CompanyLevelEnum.Beginner);
        company1.GetInventory().AddGood(new InventoryEntry(lemonade, 1000, 3m,0));
        company2.GetInventory().AddGood(new InventoryEntry(lemonade, 1000, 4m,0));

        marketToTest.RegisterCompany(company1);
        marketToTest.RegisterCompany(company2);

        var company1Order = new Trade(marketToTest, company1, lemonade, 1000, 3.5m);
        var company2Order = new Trade(marketToTest, company2, lemonade, 1000, 4.5m);
        var expectedFilledQuantity = 1000;
        var company1Context = new ActionContext{TradeToSubmit = company1Order,
                                                MarketToSubmitTo = marketToTest};
        var company2Context = new ActionContext{TradeToSubmit = company2Order,
                                                MarketToSubmitTo = marketToTest};  
        //Act
        company1.QueueOrder(company1Context);
        company2.QueueOrder(company2Context);
        marketToTest.FulfillDemand();
        var actualFilledQuantity=company1Order.FilledQuantity;
        //Assert
        Assert.AreEqual(expectedFilledQuantity, actualFilledQuantity);
    }
    [Test]
    public void FillOrderBasedOnPriceFillsBothOrdersWhenLessThanDemandedQuantity()
    {
        //Arrange
        var marketToTest = Market.Factory.CreateStarterMarket("Starter Market",
                                                              CompanyLevelEnum.Market,
                                                              new LinearDemandStrategy() );
        var company1 = Company.Factory.Create("Company1", CompanyLevelEnum.Beginner);
        var company2 = Company.Factory.Create("Company2", CompanyLevelEnum.Beginner);
        company1.GetInventory().AddGood(new InventoryEntry(lemonade, 1000, 3m,0));
        company2.GetInventory().AddGood(new InventoryEntry(lemonade, 1000, 3m,0));
        marketToTest.RegisterCompany(company1);
        marketToTest.RegisterCompany(company2);

        var company1Order = new Trade(marketToTest, company1, lemonade, 500, 3.5m);
        var company2Order = new Trade(marketToTest, company2, lemonade, 500, 3.5m);
        var company1Context = new ActionContext{TradeToSubmit = company1Order,
                                                MarketToSubmitTo = marketToTest};
        var company2Context = new ActionContext{TradeToSubmit = company2Order,
                                                MarketToSubmitTo = marketToTest};

        var expectedFilledQuantityForCompany1 = 500;
        var expectedFilledQuantityForCompany2 = 500;
        //Act
        company1.QueueOrder(company1Context);
        company2.QueueOrder(company2Context);
        marketToTest.FulfillDemand();
        var actualFilledQuantityForCompany1=company1Order.FilledQuantity;
        var actualFilledQuantityForCompany2=company2Order.FilledQuantity;
        //Assert
        Assert.AreEqual(expectedFilledQuantityForCompany1, actualFilledQuantityForCompany1);
        Assert.AreEqual(expectedFilledQuantityForCompany2, actualFilledQuantityForCompany2);
    }
    [Test]
    public void IfBothOrdersAreLessThanDemandedQuantityBothAreFilled()
    {
         //Arrange
        var marketToTest = Market.Factory.CreateStarterMarket("Starter Market",
                                                              CompanyLevelEnum.Market,
                                                              new LinearDemandStrategy() );
        var company1 = Company.Factory.Create("Company1", CompanyLevelEnum.Beginner);
        var company2 = Company.Factory.Create("Company2", CompanyLevelEnum.Beginner);
        company1.GetInventory().AddGood(new InventoryEntry(lemonade, 1000, 3m,0));
        company2.GetInventory().AddGood(new InventoryEntry(lemonade, 1000, 3m,0));
        marketToTest.RegisterCompany(company1);
        marketToTest.RegisterCompany(company2);

        var company1Order = new Trade(marketToTest, company1, lemonade, 400, 3.5m);
        var company2Order = new Trade(marketToTest, company2, lemonade, 400, 3.5m);
        var company1Context = new ActionContext{TradeToSubmit = company1Order,
                                                MarketToSubmitTo = marketToTest};
        var company2Context = new ActionContext{TradeToSubmit = company2Order,
                                                MarketToSubmitTo = marketToTest};

        var expectedFilledQuantityForCompany1 = 400;
        var expectedFilledQuantityForCompany2 = 400;
        //Act
        company1.QueueOrder(company1Context);
        company2.QueueOrder(company2Context);
        marketToTest.FulfillDemand();
        var actualFilledQuantityForCompany1=company1Order.FilledQuantity;
        var actualFilledQuantityForCompany2=company2Order.FilledQuantity;
        //Assert
        Assert.AreEqual(expectedFilledQuantityForCompany1, actualFilledQuantityForCompany1);
        Assert.AreEqual(expectedFilledQuantityForCompany2, actualFilledQuantityForCompany2);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(lemon);
        Object.DestroyImmediate(water);
        Object.DestroyImmediate(sugar);
        Object.DestroyImmediate(lemonade);
        Object.DestroyImmediate(TestEconomy);
    }
}