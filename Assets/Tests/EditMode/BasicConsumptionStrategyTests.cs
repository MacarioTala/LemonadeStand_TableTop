using System.Linq;
using NUnit.Framework;

[TestFixture]
public class BasicConsumptionStrategyTests
{
    private Market testMarket;
    private PopulationAgent testPopulation;
    private BasicConsumptionStrategy testStrategy;
    private Good lemonade;

    [SetUp]
    public void SetUp()
    {
        testMarket = Market.Create()
            .Named("Basic Consumption Strategy Test Market")
            .EnsureDefaults();

        testStrategy = new BasicConsumptionStrategy();

        testPopulation = EconAgentBuilder.For<PopulationAgent>()
            .Named("Test Population")
            .AtLevel(AgentLevelEnum.Beginner)
            .WithBehaviourStrategy(testStrategy)
            .Build();

        testMarket.RegisterMarketParticipant(testPopulation);
        testStrategy.SetEconAgent(testPopulation);

        lemonade = new GoodBuilder()
            .Named("Lemonade")
            .Costing(3)
            .Build();
    }

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(testMarket);
        UnityEngine.Object.DestroyImmediate(lemonade);

        testMarket = null;
        testPopulation = null;
        testStrategy = null;
        lemonade = null;
    }

    [Test]
    public void PerformStrategy_WhenRequisiteDemandExists_SetsMarketOnQueuedBuyActionContexts()
    {
        // Arrange
        var demandData = new DemandData
        {
            MinDemand = 10
        };
        testPopulation.SetDemand(lemonade, demandData);

        // Act
        testStrategy.PerformStrategy(testPopulation);
        var queuedActions = testStrategy.GetQueuedActions();

        // Assert
        Assert.That(queuedActions, Is.Not.Empty);

        var buyAction = queuedActions.FirstOrDefault(x =>
            x.Action == ActionEnum.QueueTradeBuy &&
            x.TradeToSubmit != null &&
            x.TradeToSubmit.Good == lemonade);

        Assert.That(buyAction, Is.Not.Null);
        Assert.That(buyAction.MarketToSubmitTo, Is.EqualTo(testMarket));
    }
}