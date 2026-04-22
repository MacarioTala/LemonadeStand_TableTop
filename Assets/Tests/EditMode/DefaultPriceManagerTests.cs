using System;
using System.Linq;
using NUnit.Framework;

[TestFixture]
public class DefaultPriceManagerTests
{
    Market TestMarket;
    Good Lemon;

    [SetUp]
    public void SetUp()
    {
        TestMarket = Market.Factory.CreateMarket("Test Market").EnsureDefaults();        
        Lemon = new GoodBuilder()
                .Named("Lemon")
                .WithRarity(RarityEnum.Common)
                .Costing(1.0m)
                .WithDeliveryDelay(0)
                .Build();
        var entry = new InventoryEntry(Lemon,10,10,0);
        entry.SetPrice(10);
        TestMarket.GetInventory().AddGood(entry);
    }
    [TearDown]
    public void TearDown()
    {
        TestMarket=null;
    }

    [Test]
    public void PricesDoNotCycleByDefault()
    {
        // Arrange


        // Act / Assert
        Assert.That(TestMarket.GetPricesFluctuateEvery(), Is.EqualTo(0));
        Assert.That(TestMarket.ShouldPricesCycleThisPeriod(), Is.False);
    }

    [TestCase(1, 0, false, TestName = "PFE 1 does not cycle on period 0")]
    [TestCase(1, 1, true,  TestName = "PFE 1 cycles on period 1")]
    [TestCase(1, 2, true,  TestName = "PFE 1 cycles on period 2")]
    [TestCase(3, 1, false, TestName = "PFE 3 does not cycle on period 1")]
    [TestCase(3, 2, false, TestName = "PFE 3 does not cycle on period 2")]
    [TestCase(3, 3, true,  TestName = "PFE 3 cycles on period 3")]
    [TestCase(3, 4, false, TestName = "PFE 3 does not cycle on period 4")]
    [TestCase(3, 6, true,  TestName = "PFE 3 cycles on period 6")]
    public void PricesCycleEveryPfePeriods(int pfe, int currentPeriod, bool expected)
    {
        // Arrange
        TestMarket.SetPricesFluctuateEvery(pfe);
        TestMarket.CurrentPeriod = currentPeriod;

        // Act
        var result = TestMarket.ShouldPricesCycleThisPeriod();

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void PriceShouldCycleTenPercentIfPriceStabilityForGoodIsTenPercent_CalledFromCyclePrices()
    {
        //Arrange
        var flex = .1f;
        TestMarket.AddPriceStabilityEntry(new(){Good=Lemon,Flex=flex});
        TestMarket.SetPricesFluctuateEvery(1);
        TestMarket.CurrentPeriod=1;
        var marketInventoryEntries=TestMarket.GetInventory().GetInventoryEntries();
        var originalLemonPrice = marketInventoryEntries.FirstOrDefault(x=>x.good.Equals(Lemon)).Price;
        var expected = originalLemonPrice*(decimal)flex;

        //Act
        TestMarket.CyclePrices();
        var actualPrice = marketInventoryEntries.FirstOrDefault(x=>x.good.Equals(Lemon)).Price;
        var actual = Math.Abs(originalLemonPrice-actualPrice);

        //Assert
        Assert.AreEqual(expected,actual);
    }

    [Test]
    public void PriceShouldCycleTenPercentIfPriceStabilityForGoodIsTenPercent_CalledFromUnleashMarketForces()
    {
        //Arrange
        var flex = .1f;
        TestMarket.AddPriceStabilityEntry(new(){Good=Lemon,Flex=flex});
        TestMarket.SetPricesFluctuateEvery(1);
        TestMarket.CurrentPeriod=1;
        var marketInventoryEntries=TestMarket.GetInventory().GetInventoryEntries();
        var originalLemonPrice = marketInventoryEntries.FirstOrDefault(x=>x.good.Equals(Lemon)).Price;
        var expected = originalLemonPrice*(decimal)flex;

        //Act
        TestMarket.UnleashMarketForces(1);
        var actualPrice = marketInventoryEntries.FirstOrDefault(x=>x.good.Equals(Lemon)).Price;
        var actual = Math.Abs(originalLemonPrice-actualPrice);

        //Assert
        Assert.AreEqual(expected,actual);
    }
}