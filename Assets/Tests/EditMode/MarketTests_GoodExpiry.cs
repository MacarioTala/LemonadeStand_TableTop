using System.Linq;
using NUnit.Framework;

public partial class MarketTests
{
[Test]
public void UnleashMarketForcesShouldExpireGoods()
{
    // Arrange
    var tradingPeriod = 1;
    var company = EconAgent.Factory.Create("Company 1", AgentLevelEnum.Beginner);
    TestMarket.RegisterMarketParticipant(company);

    var francium = Good.CreateInstance("Francium", band2, RarityEnum.Very_Rare);
    francium.ExpiresAfterPeriods = 1;
    company.GetInventory().AddGood(new InventoryEntry(francium, 1, 10000m, 0));

    var expected = 0;

    // Act
    TestMarket.UnleashMarketForces(tradingPeriod);
    var franciumEntry = company.GetInventory().GetInventoryEntriesByGood(francium.GoodName).FirstOrDefault();
    var actual = franciumEntry?.quantity ?? 0;

    // Assert
    Assert.AreEqual(expected, actual);

    // Cleanup
    TestMarket.RemoveMarketParticipant(company);
}
}