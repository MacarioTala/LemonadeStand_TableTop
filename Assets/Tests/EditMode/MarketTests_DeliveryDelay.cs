using System.Linq;
using NUnit.Framework;

public partial class MarketTests
{
    [Test]
public void UnleashMarketForcesShouldResolveDeliveriesForAllParticipants()
{
    // Arrange
    var delayedGood = new GoodBuilder()
                        .Named("Delayed Lemon")
                        .WithRarity(RarityEnum.Common)
                        .Costing(1)
                        .WithDeliveryDelay(2)
                        .WithExpiryAfter(5)
                        .Build();

    var entry = new InventoryEntry(delayedGood, 10, 3, 0);
    Company1.GetInventory().AddInventoryEntry(entry);

    var expected = 1;

    // Act
    TestMarket.UnleashMarketForces(0);

    var actualGood = Company1.GetInventory()
                         .GetInventoryEntriesByGood(delayedGood.GoodName)
                         .First();

    var actual = actualGood.RemainingDelay;

    // Assert
    Assert.AreEqual(expected, actual);
}
}