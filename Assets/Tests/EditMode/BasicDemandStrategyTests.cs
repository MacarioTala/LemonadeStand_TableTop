using NUnit.Framework;

[TestFixture]
public class BasicDemandStrategyTests
{
    Good lemon;
    Good water;
    Good sugar;
    [SetUp]
    public void Setup ()
    {
        lemon = Good.CreateInstance("Lemon", new Price_band(.5m, 1.0m), Rarity_enum.Common);
        water = Good.CreateInstance("Water", new Price_band(.5m, 1.0m), Rarity_enum.Common);
        sugar = Good.CreateInstance("Sugar", new Price_band(.5m, 1.0m), Rarity_enum.Common);
    }
    [Test]
    public void GetTotalBought_returns_total_amount_of_good_bought_in_a_period()
    {
        // Arrange
        var period = 0;
        var marketToTest = Market.Factory.CreateStarterMarket("Market To Test", CompanyLevelEnum.Market, new LinearDemandStrategy());
        var lemonOrder = new Trade(marketToTest, marketToTest, lemon, 500, 3.0m);
        var lemonContext = new ActionContext { TradeToSubmit = lemonOrder, MarketToSubmitTo = marketToTest , Period = period};
        marketToTest.QueueOrder(lemonContext);
        marketToTest.QueueOrder(lemonContext);
        marketToTest.QueueOrder(lemonContext);

        const int expected = 1500;
        const int trading_period = 0;
        var strategy = new LinearDemandStrategy();
        // Act
        marketToTest.ProcessCompanyOrders();
        var actual = ((iDemandStrategy)strategy).GetTotalBought(marketToTest, lemon, trading_period);
        // Assert
        Assert.AreEqual(expected, actual);
    }
}