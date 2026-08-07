using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class CompanyOrderValidationTests
{
    Good Lemon;
    Market TestMarket;
    iDemandStrategy TestDemandStrategy;
    [SetUp]
    public void Setup()
    {
        TestDemandStrategy = ScriptableObject.CreateInstance<LinearDemandStrategy>();
        Lemon = Good.CreateInstance("Lemon", new PriceBand(1, 3), RarityEnum.Common);
        TestMarket = Market.Factory.CreateMarket("Test Market")
            .WithTradeProcessor(new DefaultTradeProcessor())
            .WithTransactionManager(new DefaultTransactionManager())
            .WithDemographicManager(new DefaultDemographicManager())
            .WithDemandStrategy(TestDemandStrategy);
    }

    [TearDown]
    public void TearDown()
    {
        TestMarket = null;
        TestDemandStrategy = null;
        Lemon = null;
    }

    [Test]
    public void CompanyQueueOrderReturnsFailureIfOrderWouldResultInNegativeCashBalance()
    {
        // Arrange
        var company = EconAgent.Factory.Create("Test Company", AgentLevelEnum.Beginner);
        var order = new Order(company, TestMarket, Lemon, 10000, 10);
        var context = new ActionContext
        {
            TradeToSubmit = order,
            MarketToSubmitTo = TestMarket,
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