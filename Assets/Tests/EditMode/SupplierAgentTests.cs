using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

[TestFixture]
public class SupplierAgentTests
{
    TheEconomy TestEconomy;
    Good Water;
    Good Lemon;
    Good Uranium;
    Good Francium;
    Good Unobtanium;

    const string lemonName = "Lemon";
    const string waterName = "Water";
    const string uraniumName = "Uranium";
    const string franciumName = "Francium";
    const string unobtaniumName = "Unobtanium";

    const decimal waterCost = 1m;
    const decimal lemonCost = 10m;
    const decimal uraniumCost = 100m;
    const decimal franciumCost = 1000m;
    const decimal unobtaniumCost = 10000m;

    const int commonQuantity = 10000;
    const int uncommonQuantity = 1000;
    const int rareQuantity = 100;
    const int veryRareQuantity = 10;
    
    readonly List<Good> initialGoods = new();
    readonly Dictionary<RarityEnum, int> RarityQuantities = new();
    

    Market TestMarket;
    [SetUp]
    public void Setup()
    {
        SetupGoods();
        SetupMarket();
        SetupRarities();
    }

    private void SetupRarities()
    {
        RarityQuantities[RarityEnum.Common] = commonQuantity;
        RarityQuantities[RarityEnum.Uncommon] = uncommonQuantity;
        RarityQuantities[RarityEnum.Rare] = rareQuantity;
        RarityQuantities[RarityEnum.Very_Rare] = veryRareQuantity;
        RarityQuantities[RarityEnum.Unique] = 1;
    }

    private void SetupMarket()
    {
       TestMarket=Market.Factory.CreateStarterMarket("SupplierAgent Test Market", null)
                        .WithSupplyProvider(new MockSupplyProvider())
                        .WithDemographicManager(new MockDemographicManager())
                        ;
    }

    private void SetupGoods()
    {
        Water = new GoodBuilder()
                .Named(waterName)
                .Costing(waterCost)
                .WithRarity(RarityEnum.Common)
                .Build();

        Lemon = new GoodBuilder()
                .Named(lemonName)
                .Costing(lemonCost)
                .WithRarity(RarityEnum.Uncommon)
                .Build();
        Uranium = new GoodBuilder()
                .Named(uraniumName)
                .Costing(uraniumCost)
                .WithRarity(RarityEnum.Rare)
                .Build();
        Francium = new GoodBuilder()
                .Named(franciumName)
                .Costing(franciumCost)
                .WithRarity(RarityEnum.Very_Rare)
                .Build();
        Unobtanium = new GoodBuilder()
                .Named(unobtaniumName)
                .Costing(unobtaniumCost)
                .WithRarity(RarityEnum.Unique)
                .Build();

        initialGoods.Add(Water);
        initialGoods.Add(Lemon);
        initialGoods.Add(Uranium);
        initialGoods.Add(Francium);
        initialGoods.Add(Unobtanium);

    }

    [TearDown]
    public void TearDown()
    {
        Lemon = null;
        Water = null;
        Uranium = null;
        Francium = null;
        Unobtanium = null;
        initialGoods.Clear();
        TestMarket = null;
    }

    [Test]
    public void SeedSuppliedGoodsAddsGoodsToBeSupplied()
    {
        //Arrange
        var testSupplier = SupplierAgent.SupplierAgentBuilder.Create()
            .Named("Supplier")
            .AtLevel(AgentLevelEnum.Beginner)
            .WithInitialCash(1000)
            .Build();

        TestMarket.RegisterMarketParticipant(testSupplier);

        testSupplier.SeedSuppliedGoods(initialGoods);
        var expected = initialGoods;

        //Act
        var actual = testSupplier.GetSuppliedGoods();

        //Assert
        Assert.AreEqual(expected, actual);

        //Cleanup
        testSupplier.LeaveMarket();
    }

    [Test]
    public void ReplenishSuppliesAddsGoodsToInventoryBasedOnRarityDictionary()
    {
        //Arrange
        var testSupplier = SupplierAgent.SupplierAgentBuilder.Create()
            .Named("Supplier")
            .AtLevel(AgentLevelEnum.Beginner)
            .WithInitialCash(1000)
            .Build();

        TestMarket.RegisterMarketParticipant(testSupplier);

        testSupplier.SeedSuppliedGoods(initialGoods);

        testSupplier.InitializeRarityQuantities(
                RarityQuantities[RarityEnum.Common],
                RarityQuantities[RarityEnum.Uncommon],
                RarityQuantities[RarityEnum.Rare],
                RarityQuantities[RarityEnum.Very_Rare]
                );

        var expectedWater = RarityQuantities[Water.GetRarity()];
        var expectedLemon = RarityQuantities[Lemon.GetRarity()];
        var expectedUranium = RarityQuantities[Uranium.GetRarity()];
        var expectedFrancium = RarityQuantities[Francium.GetRarity()];
        var expectedUnobtanium = RarityQuantities[Unobtanium.GetRarity()];

        //Act
        testSupplier.ReplenishSupplies();
        var actualWater = testSupplier.GetInventory().GetInventoryEntries().FirstOrDefault(x => x.good == Water).quantity;
        var actualLemon = testSupplier.GetInventory().GetInventoryEntries().FirstOrDefault(x => x.good == Lemon).quantity;
        var actualUranium = testSupplier.GetInventory().GetInventoryEntries().FirstOrDefault(x => x.good == Uranium).quantity;
        var actualFrancium = testSupplier.GetInventory().GetInventoryEntries().FirstOrDefault(x => x.good == Francium).quantity;
        var actualUnobtanium = testSupplier.GetInventory().GetInventoryEntries().FirstOrDefault(x => x.good == Unobtanium).quantity;

        //Assert
        Assert.AreEqual(expectedWater, actualWater, $"Expected {expectedWater} water, but got {actualWater} instead");
        Assert.AreEqual(expectedLemon, actualLemon, $"Expected {expectedLemon} Lemon, but got {actualLemon} instead");
        Assert.AreEqual(expectedUranium, actualUranium, $"Expected {expectedUranium} Uranium, but got {actualUranium} instead");
        Assert.AreEqual(expectedFrancium, actualFrancium, $"Expected {expectedFrancium} Francium, but got {actualFrancium} instead");
        Assert.AreEqual(expectedUnobtanium, actualUnobtanium, $"Expected {expectedUnobtanium} Unobtanium, but got {actualUnobtanium} instead");


        //Cleanup
        testSupplier.LeaveMarket();
    }

    [Test]
    public void SupplyGoodsCreatesOrders()
    {
        //Arrange
        var testSupplier = SupplierAgent.SupplierAgentBuilder.Create()
            .Named("Test Supplier")
            .AtLevel(AgentLevelEnum.Beginner)
            .Build();

        TestMarket.RegisterMarketParticipant(testSupplier);
        testSupplier.SeedSuppliedGoods(initialGoods);

        testSupplier.InitializeRarityQuantities(
                RarityQuantities[RarityEnum.Common],
                RarityQuantities[RarityEnum.Uncommon],
                RarityQuantities[RarityEnum.Rare],
                RarityQuantities[RarityEnum.Very_Rare]
                );

        const int expectedBeforeOrderCount = 0;
        int expectedAfterOrderCount = initialGoods.Count();

        //Act
        var actualBeforeOrderCount = TestMarket.GetOrdersSentToMarket().Count();
        testSupplier.ReplenishSupplies();
        testSupplier.SupplyGoods();
        var actualAfterOrderCount = TestMarket.GetOrdersSentToMarket().Count();

        //Assert
        Assert.AreEqual(expectedBeforeOrderCount, actualBeforeOrderCount);
        Assert.AreEqual(expectedAfterOrderCount,actualAfterOrderCount);
    }
}