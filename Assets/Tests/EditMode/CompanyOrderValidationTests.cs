using System;
using System.Runtime.InteropServices;
using NUnit.Framework;

[TestFixture]
public class CompanyOrderValidationTests
{
    Good Lemon;
    [SetUp]
    public void Setup()
    {
        Lemon = Good.CreateInstance("Lemon", new Price_band(1, 3), Rarity_enum.Common);
    }
    [Test]
    public void CompanyQueueOrderReturnsFailureIfOrderWouldResultInNegativeCashBalance()
    {
        // Arrange
        var testMarket = Market.Factory.CreateMarket("Test Market", CompanyLevelEnum.Market, new LinearDemandStrategy());
        var company = Company.Factory.Create("Test Company", CompanyLevelEnum.Beginner);
        var order = new Order(company, testMarket, Lemon, 10000, 10m);
        var context = new ActionContext
        {
            TradeToSubmit = order,
            MarketToSubmitTo = testMarket,
            Period = 0
        };
        var expected = new LemonadeStandResultObject
        {
            Result = ResultTypeEnum.InsufficientCash,
            Message = "Insufficient Cash to queue order"
        };
        // Act
        var actual = company.QueueOrder(context);
        // Assert
        Assert.AreEqual(expected, actual);

    }
}