using NUnit.Framework;

[TestFixture]
public class Defaults_DemandManagerTests
{
    [Test]
    public void DemandManagerDefaultCreated()
    {
        //Arrange
        var market = Market.Factory.CreateMarket("Default Tester Market");
        var expected = typeof(DefaultDemandManager);

        //Act
        var actual = market.DemandManager.GetType();

        //Assert
        Assert.AreEqual(expected, actual);
    }

    [Test, Description("CreateStarterMarket has a default MarketEventManager")]
    public void CreateStarterMarketHasDefaultDemandManager()
    {
        //Arrange
        var market = Market.Factory.CreateStarterMarket("Dependency testing Market",  null);
        var expected = typeof(DefaultDemandManager);

        //Act
        var actual = market.DemandManager.GetType();

        //Assert
        Assert.AreEqual(expected, actual);
    }
}