using NUnit.Framework;
using NUnit.Framework.Internal;
using UnityEngine;

[TestFixture]
public class MarketEventTests
{
    TheEconomy TestEconomy;
    Good Lemonade;
    Market TestMarket;
    Company Company1;
    Company Company2;

    MockMarketDataService TestMarketDataService;

    LinearDemandStrategy Strategy;

    iDemographicManager TestDemographicManager;

    iSupplyProvider TestSupplyProvider;

    int Period;

    MarketEventSO MaraudersAttack;
    MarketEventSO GodzillaAttack;

    [SetUp]
    public void Setup()
    {
        Period = 0;
        Lemonade = Good.CreateInstance("Lemonade", new PriceBand(.5m, 2m), RarityEnum.Uncommon);
        Lemonade.IsProducedGood = true;

        TheEconomy.SetupForTests(new MockLogger());
        TestEconomy = TheEconomy.Instance;

        Strategy = ScriptableObject.CreateInstance<LinearDemandStrategy>();
        TestSupplyProvider= new BasicSupplyProvider();

        TestMarket = Market.Factory.CreateStarterMarket("Starter Market",
                                                        CompanyLevelEnum.Market,
                                                        Strategy);
        TestMarketDataService = new MockMarketDataService();
        TestDemographicManager = new MockDemographicManager();
        TestMarket.SetDemographicManager(TestDemographicManager);
        TestMarket.SetMarketDataService(TestMarketDataService);
        TestMarket.SetSupplyProvider(TestSupplyProvider);
        TestSupplyProvider.Initialize(TestMarket);

        Company1 = Company.Factory.Create("Company1", CompanyLevelEnum.Beginner);
        Company2 = Company.Factory.Create("Company2", CompanyLevelEnum.Beginner);
        TestMarket.RegisterCompany(Company1);
        TestMarket.RegisterCompany(Company2);

        MaraudersAttack = ScriptableObject.CreateInstance<MarketEventSO>();
        GodzillaAttack = ScriptableObject.CreateInstance<MarketEventSO>();

        MaraudersAttack.Initialize( eventName: "Marauders Attack",
                                    eventDescription: "Marauders attack the neighbourhood, reducing population.",
                                    eventChance: 100f,
                                    eventDuration: 2);
        MaraudersAttack.Effects.Add(new ChangePopulationEffect(-10));

        GodzillaAttack.Initialize( eventName: "Godzilla Attack",
                                    eventDescription: "Godzilla attacks the neighbourhood, reducing population.",
                                    eventChance: 100f,
                                    eventDuration: 2);
        GodzillaAttack.Effects.Add(new ChangePopulationEffect(-30));
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(TestEconomy.gameObject);
        Object.DestroyImmediate(Strategy);
        TestMarket = null;
        Company1 = null;
        Company2 = null;
        TestMarketDataService = null;
        TestDemographicManager = null;
        TestSupplyProvider = null;
        MaraudersAttack = null;
        GodzillaAttack = null;
    }

    [Test]
    public void MaraudersAttackNeighbourhoodReducesPopulation()
    {
        //Arrange
        const int initialPopulation = 100;
        var expectedPopulation = 90;
        TestMarket.SetPopulation(initialPopulation);

        //Act
        MaraudersAttack.Invoke(TestMarket);
        var newPopulation = TestMarket.GetPopulation();
        
        //Assert
        Assert.AreEqual(expectedPopulation, newPopulation);
    }
#region MarketEventDuration tests
    [Test]
    public void IfEventStillActiveReapplyEffect()
    {
        //Arrange
        const int initialPopulation = 100;
        var expectedPopulation = 81;
        TestMarket.AddPotentialMarketEvent(MaraudersAttack);
        TestMarket.SetPopulation(initialPopulation);

        //Act
        TestMarket.StartTradingPeriod();
        TestMarket.UnleashMarketForces(TestMarket.CurrentPeriod);
        TestMarket.StartTradingPeriod();
        var actualPopulation = TestMarket.GetPopulation();

        //Assert
        Assert.AreEqual(expectedPopulation, actualPopulation);
    }
    [Test]
    public void MarketEventCanBeAddedToActiveEventWithoutMarketEffect()
    {
        //Arrange
        var AppleTreesGrow = ScriptableObject.CreateInstance<MarketEventSO>();
        AppleTreesGrow.Initialize( eventName: "Apple Trees Grow",
                                    eventDescription: "Apple trees grow in the neighbourhood, increasing population.",
                                    eventChance: 100f,
                                    eventDuration: 1);
        var initialPopulation = 100;
        TestMarket.SetPopulation(initialPopulation);
        var expectedPopulation = initialPopulation;
        TestMarket.AddPotentialMarketEvent(AppleTreesGrow);

        //Act
        TestMarket.StartTradingPeriod();

        //Assert
        Assert.AreEqual(expectedPopulation, TestMarket.GetPopulation());

    }
    [Test]
    public void MarketEventExpiresAfterDuration()
    {
        //Arrange
        const int initialPopulation = 100;
        var expectedPopulation = 81;
        TestMarket.AddPotentialMarketEvent(MaraudersAttack);
        TestMarket.SetPopulation(initialPopulation);

        //Act
        TestMarket.StartTradingPeriod();
        TestMarket.UnleashMarketForces(TestMarket.CurrentPeriod);
        TestMarket.StartTradingPeriod();
        TestMarket.UnleashMarketForces(TestMarket.CurrentPeriod);
        TestMarket.StartTradingPeriod(); //Period should be 2 here. No additional population reduction.
        var actualPopulation = TestMarket.GetPopulation();

        //Assert
        Assert.AreEqual(2,TestMarket.CurrentPeriod);
        Assert.AreEqual(expectedPopulation, actualPopulation);
    }
    [Test]
    public void CannotAddADuplicateOfACurrentlyActiveEvent()
    {
        //Arrange
        var initialNumberOfActiveEvents = TestMarket.GetActiveMarketEvents().Count;
        var expectedNumberOfActiveEvents = initialNumberOfActiveEvents+1;
        TestMarket.AddPotentialMarketEvent(MaraudersAttack);

        //Act
        TestMarket.RollForEvents();
        TestMarket.RollForEvents(); //try to add it twice
        var actualNumberOfActiveEvents = TestMarket.GetActiveMarketEvents().Count;

        //Assert
        Assert.AreEqual(expectedNumberOfActiveEvents, actualNumberOfActiveEvents);  
    }
    [Test]
    public void CanAddADifferentEventEvenWithActiveEvents()
    {
        //Arrange
        var expectedNumberOfActiveEvents = 2;

        //Act
        TestMarket.AddPotentialMarketEvent(MaraudersAttack);
        TestMarket.RollForEvents();
        TestMarket.AddPotentialMarketEvent(GodzillaAttack);
        TestMarket.RollForEvents(); 
        var actualNumberOfActiveEvents = TestMarket.GetActiveMarketEvents().Count;
        //Assert
        Assert.AreEqual(expectedNumberOfActiveEvents, actualNumberOfActiveEvents);
    }
#endregion
#region Integration tests with Market
    [Test]
    public void MaraudersAttackNeighbourhoodReducesPopulationFromMarket()
    {
        //Arrange
        const int initialPopulation = 100;
        var expectedPopulation = 90;

        TestMarket.SetPopulation(initialPopulation);
        TestMarket.AddPotentialMarketEvent(MaraudersAttack);
        
        //Act
        TestMarket.StartTradingPeriod();
        var actual = TestMarket.GetPopulation();
        //Assert
        Assert.AreEqual(expectedPopulation, actual);

    }
    
#endregion

}