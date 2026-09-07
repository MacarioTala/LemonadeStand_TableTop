using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using static TestHelpers;

[TestFixture]
public class LemonadeSellerStrategyTests
{
    private Good lemon;
    private Good sugar;
    private Good water;
    private Good lemonade;

    private Recipe lemonadeRecipe;
    private LemonadeSellerStrategy strategy;
    private EconAgent company;
    private Market TestMarket;
    [SetUp]
    public void Setup()
    {
        lemon = new GoodBuilder().Named("lemon").Costing(3).Build();
        sugar = new GoodBuilder().Named("sugar").Costing(2).Build();
        water = new GoodBuilder().Named("water").Costing(2).Build();

        lemonade = new GoodBuilder()
            .Named("lemonade")
            .Costing(20)
            .WhichIsProducedGood()
            .Build();

        lemonadeRecipe = ScriptableObject.CreateInstance<Recipe>();
        lemonadeRecipe.Initialize(
            recipeName: "Basic Lemonade",
            product: lemonade,
            ingredients: new List<Ingredient>
            {
                new(lemon, 3),
                new(sugar, 1),
                new(water, 2)
            });

        //Create Market
         TestMarket = Market.Factory.CreateMarket("Test Market")
            .EnsureDefaults();

        TestMarket.GetInventory().AddInventoryEntry(new InventoryEntry(lemon, 100, 3, TestMarket.CurrentPeriod));
        TestMarket.GetInventory().AddInventoryEntry(new InventoryEntry(sugar, 100, 2, TestMarket.CurrentPeriod));
        TestMarket.GetInventory().AddInventoryEntry(new InventoryEntry(water, 100, 2, TestMarket.CurrentPeriod));
        MarketProvidesSupplies();

        strategy = new LemonadeSellerStrategy();

        company = EconAgentBuilder.For<EconAgent>()
                        .Named("Company 1")
                        .AtLevel(AgentLevelEnum.Beginner)
                        .WithInitialCash(60)
                        .WithBehaviourStrategy(strategy)
                        .Build();

        company.SetMarket(TestMarket);
        company.AddRecipe(lemonadeRecipe);
    }

    [TearDown]
    public void TearDown()
    {
        TestMarket = null;
        company = null;
    }
    [Test]
    public void BuySupplies_QueuesOrdersForMaximumAffordableProduction()
    {
        // Arrange
        // Lemonade cost = 15
        // Cash = 45 -- 60 * .8 because of aggression level
        // Can afford 3 lemonade -- 9 lemon, 3 sugar, 6 water
        
        const int expectedOrderCount = 3;
        const int expectedLemonCount = 9;
        const int expectedSugarCount = 3;
        const int expectedWaterCount = 6;

        // Act
        strategy.BuySupplies();
        var orders = TestMarket.GetOrdersSentToMarket()
                    .Where(x=>x.SubmittingCompany.Equals(company))
                    .ToList();
        var actualOrderCount = orders.Count;

        var actualLemonOrderQuantity = orders.FirstOrDefault(x=>x.SubmittingCompany.Equals(company) && x.Good==lemon).Quantity;
        var actualSugarOrderQuantity = orders.FirstOrDefault(x=>x.SubmittingCompany.Equals(company) && x.Good==sugar).Quantity;
        var actualWaterrderQuantity = orders.FirstOrDefault(x=>x.SubmittingCompany.Equals(company) && x.Good==water).Quantity;

        // Assert
        Assert.AreEqual(expectedOrderCount,actualOrderCount);
        Assert.AreEqual(expectedLemonCount,actualLemonOrderQuantity);
        Assert.AreEqual(expectedSugarCount,actualSugarOrderQuantity);
        Assert.AreEqual(expectedWaterCount,actualWaterrderQuantity);
    }

[Test]
public void BuySupplies_FloorsFractionalProduction()
{
    // Arrange
    company.SetCash(44);

    const int expectedLemonCount = 6;
    const int expectedSugarCount = 2;
    const int expectedWaterCount = 4;

    // Act
    strategy.BuySupplies();

    var orders = GetCompanyOrders();

    // Assert
    Assert.AreEqual(expectedLemonCount, GetOrderQuantity(orders, lemon));
    Assert.AreEqual(expectedSugarCount, GetOrderQuantity(orders, sugar));
    Assert.AreEqual(expectedWaterCount, GetOrderQuantity(orders, water));
}

[Test]
public void BuySupplies_QueuesZeroQuantityOrders_WhenCashTooLow()
{
    // Arrange
    company.SetCash(14);

    // Act
    strategy.BuySupplies();

    var orders = GetCompanyOrders();

    // Assert
    Assert.AreEqual(0, GetOrderQuantity(orders, lemon));
    Assert.AreEqual(0, GetOrderQuantity(orders, sugar));
    Assert.AreEqual(0, GetOrderQuantity(orders, water));
}

[Test]
public void BuySupplies_CreatesBuyOrders()
{
    // Arrange

    // Act
    strategy.BuySupplies();

    var orders = GetCompanyOrders();

    // Assert
    Assert.That(orders.All(x => x.IsBuy()), Is.True);
}

#region TestHelpers
//Have to queue market orders otherwise prices won't be available
private void MarketProvidesSupplies()
{
         // Have Market queue up some orders
        var marketLemonSale = new Order(null, TestMarket,lemon,20,3);
        var marketSugarSale = new Order(null, TestMarket,sugar,20,2);
        var marketWaterSale = new Order(null, TestMarket,water,20,2);

        var marketLemonContext = CreateActionContext(marketLemonSale,TestMarket,0);
        marketLemonContext.SubmittingCompany = TestMarket;
        var marketSugarContext = CreateActionContext(marketSugarSale,TestMarket,0);
        marketSugarContext.SubmittingCompany = TestMarket;
        var marketWaterContext = CreateActionContext(marketWaterSale,TestMarket,0);
        marketWaterContext.SubmittingCompany = TestMarket;

        TestMarket.QueueOrder(marketLemonContext);
        TestMarket.QueueOrder(marketSugarContext);
        TestMarket.QueueOrder(marketWaterContext);
}

private List<Order> GetCompanyOrders()
{
    return TestMarket.GetOrdersSentToMarket()
        .Where(x => x.SubmittingCompany.Equals(company))
        .ToList();
}

private static int GetOrderQuantity(List<Order> orders, Good good)
{
    if(orders.Count==0) return 0;
    return orders
        .FirstOrDefault(x => x.Good == good)
        .Quantity;
}
#endregion
}