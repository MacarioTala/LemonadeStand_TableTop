using System.Linq;
using NUnit.Framework;
using UnityEngine;
using static TestHelpers;
[TestFixture]
public class DeliveryDelayTests
{
[Test]
public void GoodsWithDeliveryDelayGreaterThanZeroShouldDecrementTheirRemainingDelay()
{
    // Arrange
    var delayedGood = new GoodBuilder()
                        .Named("Delayed Lemon")
                        .WithRarity(RarityEnum.Common)
                        .Costing(1)
                        .WithDeliveryDelay(2)
                        .Build();
    
    var delayedEntry = new InventoryEntry(delayedGood, 10, 3, 0);
    
    var Company1 = EconAgentBuilder.For<EconAgent>()
                    .Named("Company1")
                    .Build();
    
    Company1.GetInventory().AddInventoryEntry(delayedEntry);

    var expected = 1;

    // Act
    Company1.ResolveDeliveries();
    var actual = Company1.GetInventory()
                         .GetInventoryEntriesByGood(delayedGood.GoodName)
                         .FirstOrDefault()
                         ?.RemainingDelay;            
    // Assert
    Assert.AreEqual(expected, actual);
}

[Test]
public void GoodsWithZeroDeliveryDelayShouldNotGoNegativeWhenResolvingDeliveries()
{
    // Arrange
    var instantGood = new GoodBuilder()
                        .Named("Instant Water")
                        .WithRarity(RarityEnum.Common)
                        .Costing(1)
                        .WithDeliveryDelay(0)
                        .Build();
    
    var instantEntry = new InventoryEntry(instantGood, 10, 1, 0);

    var Company1 = EconAgentBuilder.For<EconAgent>()
                    .Named("Company1")
                    .Build();

    Company1.GetInventory().AddInventoryEntry(instantEntry);

    var expected = 0;

    // Act
    Company1.ResolveDeliveries();
    var actual = Company1.GetInventory()
                         .GetInventoryEntriesByGood(instantGood.GoodName)
                         .FirstOrDefault()
                         ?.RemainingDelay;

    // Assert
    Assert.AreEqual(expected, actual);
}
}