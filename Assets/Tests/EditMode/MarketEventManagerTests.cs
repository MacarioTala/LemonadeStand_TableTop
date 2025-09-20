using NUnit.Framework;

[TestFixture]
public class MarketEventManagerTests
{
    [Test, Description("If no MarketEventManager is specified in Market.Create, the default one is passed")]
    public void MarketEventManagerDefaultCreated()
    {
        //Arrange
        var market = Market.Factory.CreateMarket("Default Tester Market");
        var expected = typeof(DefaultMarketEventManager);

        //Act
        var actual = market.MarketEventManager.GetType();

        //Assert
        Assert.AreEqual(expected, actual);
    }

    [Test, Description("CreateStarterMarket has a default MarketEventManager")]
    public void CreateStarterMarketHasDefaultEventManager()
    {
        //Arrange
        var market = Market.Factory.CreateStarterMarket("Dependency testing Market", null);
        var expected = typeof(DefaultMarketEventManager);

        //Act
        var actual = market.MarketEventManager.GetType();

        //Assert
        Assert.AreEqual(expected, actual);
    }
}