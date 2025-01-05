using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Internal;
using UnityEngine;
using static TestHelpers;

[TestFixture]
public class BasicConsumptionManagerTests
{
    TheEconomy TestEconomy;
    Market TestMarket;
    iConsumptionManager TestConsumptionManager;
    const int Period = 0;

    Company Company1;
    Company Company2;

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

        TestConsumptionManager = new BasicConsumptionManager();
        TestMarket = Market.Factory.CreateStarterMarket("Test Market", CompanyLevelEnum.Market, new LinearDemandStrategy());
        TestMarket.SetConsumptionManager(TestConsumptionManager); 

        Company1 = Company.Factory.Create("Company1", CompanyLevelEnum.Beginner);
        Company2 = Company.Factory.Create("Company2", CompanyLevelEnum.Beginner);
        TestMarket.RegisterCompany(Company1);
        TestMarket.RegisterCompany(Company2);

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
    public void FD_FillsDemandOf1000WithLowestPricedOrder()
    {
        //Arrange
        Company1.GetInventory().AddGood(new InventoryEntry(lemonade, 1000, 3m,0));
        Company2.GetInventory().AddGood(new InventoryEntry(lemonade, 1000, 4m,0));

        var company1Order = new Order(TestMarket, Company1, lemonade, 1000, 3.5m);
        var company2Order = new Order(TestMarket, Company2, lemonade, 1000, 4.5m);
        var expectedFilledQuantity = 1000;
        //Act
        TestMarket.QueueMarketOrder(CreateActionContext(company1Order, TestMarket,Period));
        TestMarket.QueueMarketOrder(CreateActionContext(company2Order, TestMarket,Period));
        TestMarket.FulfillDemand();
        var actualFilledQuantity=company1Order.FilledQuantity;
        //Assert
        Assert.AreEqual(expectedFilledQuantity, actualFilledQuantity);
    }
    [Test]
    public void FD_FillsBothOrdersWhenLessThanDemandedQuantity()
    {
        //Arrange
        var company1Order = new Order(TestMarket, Company1, lemonade, 500, 3.5m);
        var company2Order = new Order(TestMarket, Company2, lemonade, 500, 3.5m);
        
        var expectedFilledQuantityForCompany1 = 500;
        var expectedFilledQuantityForCompany2 = 500;
        //Act
        TestMarket.QueueMarketOrder(CreateActionContext(company1Order, TestMarket,Period));
        TestMarket.QueueMarketOrder(CreateActionContext(company2Order, TestMarket,Period));
        TestMarket.FulfillDemand();
        var actualFilledQuantityForCompany1=company1Order.FilledQuantity;
        var actualFilledQuantityForCompany2=company2Order.FilledQuantity;
        //Assert
        Assert.AreEqual(expectedFilledQuantityForCompany1, actualFilledQuantityForCompany1);
        Assert.AreEqual(expectedFilledQuantityForCompany2, actualFilledQuantityForCompany2);
    }
    [Test]
    public void FD_IfBothOrdersAreLessThanDemandedQuantityBothAreFilled()
    {
         //Arrange
        Company1.GetInventory().AddGood(new InventoryEntry(lemonade, 1000, 3m,0));
        Company2.GetInventory().AddGood(new InventoryEntry(lemonade, 1000, 3m,0));

        var company1Order = new Order(TestMarket, Company1, lemonade, 400, 3.5m);
        var company2Order = new Order(TestMarket, Company2, lemonade, 400, 3.5m);
    
        var expectedFilledQuantityForCompany1 = 400;
        var expectedFilledQuantityForCompany2 = 400;
        //Act
        TestMarket.QueueMarketOrder(CreateActionContext(company1Order, TestMarket,Period));
        TestMarket.QueueMarketOrder(CreateActionContext(company2Order, TestMarket,Period));
        TestMarket.FulfillDemand();
        var actualFilledQuantityForCompany1=company1Order.FilledQuantity;
        var actualFilledQuantityForCompany2=company2Order.FilledQuantity;
        //Assert
        Assert.AreEqual(expectedFilledQuantityForCompany1, actualFilledQuantityForCompany1);
        Assert.AreEqual(expectedFilledQuantityForCompany2, actualFilledQuantityForCompany2);
    }
    [Test]
    public void FD_FillsWhenExcessSupplyFromCompanyTradesOccurs()
    {
        //Arrange
        TestMarket.InitializeDemandForSpecificGood(lemonade, 1000);
        Company1.GetInventory().AddGood(new InventoryEntry(lemonade, 2000, 3m,0));

        var Company1SellsLemonadeToAnyone = new Order(null, Company1, lemonade, 1000, 3.5m);
        var Company2BuysLemonadeFromAnyone = new Order(Company2,null, lemonade, 100, 3.5m);

        var ExpectedCompany1LemonadeSellFillQuantity = 1000;
        var ExpectedCompany2LemonadeBuyFillQuantity = 100;
        
        //Act
        Company1.QueueOrder(CreateActionContext(Company1SellsLemonadeToAnyone, TestMarket,Period));
        Company2.QueueOrder(CreateActionContext(Company2BuysLemonadeFromAnyone, TestMarket,Period));
        TestMarket.ProcessCompanyOrders();
        TestMarket.FulfillDemand();
        var ActualCompany1LemonadeSellFillQuantity = Company1SellsLemonadeToAnyone.FilledQuantity;
        var ActualCompany2LemonadeBuyFillQuantity = Company2BuysLemonadeFromAnyone.FilledQuantity;
        //Assert
        Assert.AreEqual(ExpectedCompany1LemonadeSellFillQuantity, ActualCompany1LemonadeSellFillQuantity);
        Assert.AreEqual(ExpectedCompany2LemonadeBuyFillQuantity, ActualCompany2LemonadeBuyFillQuantity);
    }
    [Test]
    public void FD_OnlyPartiallyFillsWhenSupplyExceedsDemand()
    {
        //Arrange
        Company1.GetInventory().AddGood(new InventoryEntry(lemonade, 2000, 3m,0));
        
        TestMarket.InitializeDemandForSpecificGood(lemonade, 1000);
        var company1Order = new Order(TestMarket, Company1, lemonade, 1500, 3.5m);
        var expected = 1000;
        //Act
        Company1.QueueOrder(CreateActionContext(company1Order, TestMarket,Period));
        TestMarket.ProcessCompanyOrders();
        TestMarket.FulfillDemand();
        var actual=company1Order.FilledQuantity;
        //Assert
        Assert.AreEqual(expected, actual);
    }
    [Test]
    public void FD_RecordsTradeWhenMarketOrderIsFilled_MarketOrdersOnly()
    {
        //Arrange
        TestMarket.InitializeDemandForSpecificGood(lemonade, 1000);
        Company1.GetInventory().AddGood(new InventoryEntry(lemonade, 1000, 3m,0));

        var Company1SellsLemonadeToAnyone = new Order(null, Company1, lemonade, 1000, 3.5m);

        var expectedMarketTransactions = 2;
        //Act
        Company1.QueueOrder(CreateActionContext(Company1SellsLemonadeToAnyone, TestMarket,Period));
        TestMarket.ProcessCompanyOrders();
        TestMarket.FulfillDemand();
        var actualMarketTransactions = TestMarket.GetMarketTradesInPeriod(Period).Count;
        //Assert
        Assert.AreEqual(expectedMarketTransactions, actualMarketTransactions);
    }

    [Test]
    public void FD_DoesNotFillOrderWhenDemandIsZero()
    {
        //Arrange
        TestMarket.InitializeDemandForSpecificGood(lemonade, 0);
        Company1.GetInventory().AddGood(new InventoryEntry(lemonade, 2000, 3m,0));
        
        var company1Order = new Order(TestMarket, Company1, lemonade, 1000, 3.5m);
        
        var expected = 0;
        //Act
        TestMarket.QueueMarketOrder(CreateActionContext(company1Order, TestMarket,Period));
        TestMarket.FulfillDemand();
        var actual=company1Order.FilledQuantity;
        //Assert
        Assert.AreEqual(expected, actual);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(lemon);
        Object.DestroyImmediate(water);
        Object.DestroyImmediate(sugar);
        Object.DestroyImmediate(lemonade);
        Object.DestroyImmediate(TestEconomy);
        TestMarket = null;
        TestConsumptionManager = null;
        Company1 = null;
        Company2 = null;
    }
}