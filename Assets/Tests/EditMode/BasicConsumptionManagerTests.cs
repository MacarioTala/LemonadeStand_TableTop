using System.Collections.Generic;
using System.Linq;
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
    iDemandStrategy TestDemandStrategy ;
    iMarketDataService TestMarketDataService;
    iDemographicManager TestDemographicManager;
    iSupplyProvider TestSupplyProvider;
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
    readonly PriceBand band1 = new(.5m, 2m);
    readonly PriceBand band2 = new(2.1m, 3m);
    readonly PriceBand band3 = new(3.1m, 6m);

    
    [SetUp]
    public void SetUp()
    {
        TheEconomy.SetupForTests(new MockLogger());
        TestEconomy = TheEconomy.Instance;

        SetupGoodsAndRecipes();

        TestConsumptionManager = new BasicConsumptionManager();
        TestDemandStrategy = ScriptableObject.CreateInstance<LinearDemandStrategy>();
        TestMarketDataService = new MockMarketDataService();
        TestDemographicManager = new MockDemographicManager();
        TestSupplyProvider= new MockSupplyProvider();
        TestMarket = Market.Factory.CreateMarket("Test Market", CompanyLevelEnum.Market)
                            .WithDemandStrategy(TestDemandStrategy)
                            .WithConsumptionManager(TestConsumptionManager)
                            .WithTradeProcessor(new BasicTradeProcessor())
                            .WithTransactionManager(new BasicTransactionManager())
                            .WithPriceManager(new BasicPriceManager())
                            .WithDataService(TestMarketDataService)
                            .WithSupplyProvider(TestSupplyProvider)
                            .WithDemographicManager(TestDemographicManager);
        TestMarket.InitializeDemandForSpecificGood(lemonade, 1000);

        Company1 = Company.Factory.Create("Company1", CompanyLevelEnum.Beginner);
        Company2 = Company.Factory.Create("Company2", CompanyLevelEnum.Beginner);
        TestMarket.RegisterMarketParticipant(Company1);
        TestMarket.RegisterMarketParticipant(Company2);
    }

    private void SetupGoodsAndRecipes()
    {
        water = Good.CreateInstance("Water", band1, RarityEnum.Common);
        sugar = Good.CreateInstance("Sugar", band1, RarityEnum.Common);
        lemon = Good.CreateInstance("Lemon", band2, RarityEnum.Common);
        lemonade = Good.CreateInstance("Lemonade", band3, RarityEnum.Uncommon);
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
#region Fulfill Demand
    [Test]
    public void FD_FillsDemandOf1000WithLowestPricedOrder()
    {
        //Arrange
        Company1.GetInventory().AddGood(new InventoryEntry(lemonade, 1000, 3m,0));
        Company2.GetInventory().AddGood(new InventoryEntry(lemonade, 1000, 4m,0));

        var company1Order = new Order(null, Company1, lemonade, 1000, 3.5m);
        var company2Order = new Order(null, Company2, lemonade, 1000, 4.5m);
        var expectedFilledQuantity = 1000;
        //Act
        Company1.QueueOrder(CreateActionContext(company1Order, TestMarket,Period));
        Company2.QueueOrder(CreateActionContext(company2Order, TestMarket,Period));
        TestMarket.ProcessCompanyOrders();
        TestMarket.FulfillDemand();
        var actualFilledQuantity=company1Order.FilledQuantity;
        //Assert
        Assert.AreEqual(expectedFilledQuantity, actualFilledQuantity);
    }
    [Test]
    public void FD_FillsBothOrdersWhenLessThanDemandedQuantity()
    {
        //Arrange
        var company1Order = new Order(null, Company1, lemonade, 500, 3.5m);
        Company1.GetInventory().AddGood(new InventoryEntry(lemonade, 500, 3m,0));
        var company2Order = new Order(null, Company2, lemonade, 500, 3.5m);
        Company2.GetInventory().AddGood(new InventoryEntry(lemonade, 500, 3m,0));
        TestMarket.InitializeDemandForSpecificGood(lemonade, 2000);
        
        var expectedFilledQuantityForCompany1 = 500;
        var expectedFilledQuantityForCompany2 = 500;
        //Act
        var result1 = Company1.QueueOrder(CreateActionContext(company1Order, TestMarket,Period));
        var result2 = Company2.QueueOrder(CreateActionContext(company2Order, TestMarket,Period));
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
        TestMarket.InitializeDemandForSpecificGood(lemonade, 1000);
        Company1.GetInventory().AddGood(new InventoryEntry(lemonade, 1000, 3m,0));
        Company2.GetInventory().AddGood(new InventoryEntry(lemonade, 1000, 3m,0));

        var company1Order = new Order(null, Company1, lemonade, 400, 3.5m);
        var company2Order = new Order(null, Company2, lemonade, 400, 3.5m);
    
        var expectedFilledQuantityForCompany1 = 400;
        var expectedFilledQuantityForCompany2 = 400;
        //Act
        Company1.QueueOrder(CreateActionContext(company1Order, TestMarket,Period));
        Company2.QueueOrder(CreateActionContext(company2Order, TestMarket,Period));
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
        var ExpectedMarketFillQuantity = 900;
        
        //Act
        Company1.QueueOrder(CreateActionContext(Company1SellsLemonadeToAnyone, TestMarket,Period));
        Company2.QueueOrder(CreateActionContext(Company2BuysLemonadeFromAnyone, TestMarket,Period));
        TestMarket.ProcessCompanyOrders();
        TestMarket.FulfillDemand();
        var ActualCompany1LemonadeSellFillQuantity = Company1SellsLemonadeToAnyone.FilledQuantity;
        var ActualCompany2LemonadeBuyFillQuantity = Company2BuysLemonadeFromAnyone.FilledQuantity;
        var ActualMarketTrade = TestMarket.GetOrdersSubmittedInPeriod(Period)
            .Where(x => x.SubmittingCompany is Market)
            .FirstOrDefault();

        var ActualMarketFillQuantity = ActualMarketTrade.FilledQuantity;

        //Assert
        Assert.AreEqual(ExpectedCompany1LemonadeSellFillQuantity, 
                        ActualCompany1LemonadeSellFillQuantity,
                        $"Expected {ExpectedCompany1LemonadeSellFillQuantity} but got {ActualCompany1LemonadeSellFillQuantity}");
        Assert.AreEqual( ExpectedCompany2LemonadeBuyFillQuantity,
                         ActualCompany2LemonadeBuyFillQuantity,
                         $"Expected {ExpectedCompany2LemonadeBuyFillQuantity} but got {ActualCompany2LemonadeBuyFillQuantity}");
        Assert.AreEqual(ExpectedMarketFillQuantity, 
                        ActualMarketFillQuantity,
                        $"Expected {ExpectedMarketFillQuantity} but got {ActualMarketFillQuantity}");
    }
    [Test]
    public void FD_OnlyPartiallyFillsWhenSupplyExceedsDemand()
    {
        //Arrange
        Company1.GetInventory().AddGood(new InventoryEntry(lemonade, 2000, 3m,0));
        
        TestMarket.InitializeDemandForSpecificGood(lemonade, 1000);
        var company1SellsLemonadeToAnyone = new Order(null, Company1, lemonade, 1500, 3.5m);
        var expected = 1000;
        //Act
        Company1.QueueOrder(CreateActionContext(company1SellsLemonadeToAnyone, TestMarket,Period));
        TestMarket.ProcessCompanyOrders();
        TestMarket.FulfillDemand();
        var actual=company1SellsLemonadeToAnyone.FilledQuantity;
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
        var actualMarketTransactions = TestMarket.GetExecutionsInPeriod(Period);
        //Assert
        Assert.AreEqual(expectedMarketTransactions, actualMarketTransactions.Count);
    }

    [Test]
    public void FD_RecordsTradeWhenMarketOrderIsFilled_MarketAndCompanyOrders()
    {
        //Arrange
        TestMarket.InitializeDemandForSpecificGood(lemonade, 1000);
        Company1.GetInventory().AddGood(new InventoryEntry(lemonade, 1000, 3m,0));

        var Company1SellsLemonadeToAnyone = new Order(null, Company1, lemonade, 1000, 3.5m);
        var Company2BuysLemonadeFromAnyone = new Order(Company2,null, lemonade, 100, 3.5m);

        var expectedMarketTransactions = 4;
        //Act
        Company1.QueueOrder(CreateActionContext(Company1SellsLemonadeToAnyone, TestMarket,Period));
        Company2.QueueOrder(CreateActionContext(Company2BuysLemonadeFromAnyone, TestMarket,Period));
        TestMarket.ProcessCompanyOrders();
        TestMarket.FulfillDemand();
        var actualMarketTransactions = TestMarket.GetExecutionsInPeriod(Period);
        //Assert
        Assert.AreEqual(expectedMarketTransactions, actualMarketTransactions.Count);
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
    [Test]
    public void FD_MarketCashDecrementsAfterFulfilingDemand()
    {
        //Arrange
        TestMarket.InitializeDemandForSpecificGood(lemonade, 1000);
        Company1.GetInventory().AddGood(new InventoryEntry(lemonade, 1000, 3m,0));

        var Company1SellsLemonadeToAnyone = new Order(null, Company1, lemonade, 1000, 3.5m);
        var expectedMarketCash = TestMarket.GetCash()-(1000 * 3.5m);

        Company1.QueueOrder(CreateActionContext(Company1SellsLemonadeToAnyone, TestMarket,Period));
        //Act
        TestMarket.ProcessCompanyOrders();
        TestMarket.FulfillDemand();
        var actualMarketCash = TestMarket.GetCash();
        //Assert
        Assert.AreEqual(expectedMarketCash, actualMarketCash);
    }
#endregion

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