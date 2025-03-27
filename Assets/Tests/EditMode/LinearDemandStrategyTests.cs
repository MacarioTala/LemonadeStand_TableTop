using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public partial class LinearDemandStrategyTests
{
    TheEconomy TestEconomy;
    Good Lemonade;
    Market TestMarket;
    Company Company1;
    Company Company2;

    MockMarketDataService TestMarketDataService;

    LinearDemandStrategy Strategy;

    iDemographicManager TestDemographicManager;

    int Period = 0;
    
    [SetUp]
    public void SetUp()
    {
        Lemonade = Good.CreateInstance("Lemonade", new PriceBand(.5m, 2m), RarityEnum.Uncommon);
        Lemonade.IsProducedGood = true;

        TheEconomy.SetupForTests(new MockLogger());
        TestEconomy = TheEconomy.Instance;

        Strategy = new();

        TestMarket = Market.Factory.CreateStarterMarket("Starter Market",
                                                        CompanyLevelEnum.Market,
                                                        Strategy);
        TestMarketDataService = new MockMarketDataService();
        TestDemographicManager = new MockDemographicManager();
        TestMarket.SetDemographicManager(TestDemographicManager);
        TestMarket.SetMarketDataService(TestMarketDataService);

        Company1 = Company.Factory.Create("Company1", CompanyLevelEnum.Beginner);
        Company2 = Company.Factory.Create("Company2", CompanyLevelEnum.Beginner);
        TestMarket.RegisterCompany(Company1);
        TestMarket.RegisterCompany(Company2);

    }
    [Test]
    public void InitializeDemandForSpecificGoodReplacesDemandForExistingGood()
    {
        //Arrange
        var expected = 500;
        var expectedDemandLinesForLemonade = 1;
        //Act
        TestMarket.InitializeDemandForSpecificGood(Lemonade, expected);
        var actual = TestMarket.GetMarketDemand()[Lemonade].CurrentDemand;
        var demandLinesForLemonade = TestMarket.GetMarketDemand().Where(x=>x.Key.Equals(Lemonade)).Count();
        //Assert
        Assert.AreEqual(expected, actual);
        Assert.AreEqual(expectedDemandLinesForLemonade, demandLinesForLemonade);
    }
#region Adjust Demand Based On Elasticity
    [TestCase(TestName="If the good does not have the passed elasticity, demand should not change")]
    public void ADBE_IfElasticityDNEDoNotAdjustDemand()
    {
        //Arrange
        TestMarket.InitializeDemandForSpecificGood(Lemonade, 500);

        var expectedDemand = 500;
        var dummyMetric = 1;
        //Act
        var actualResult = Strategy.AdjustDemandBasedOnElasticityAndHistory(TestMarket, Lemonade, ElasticityTypeEnum.PopulationElasticity,dummyMetric);
        var actualDemand = TestMarket.GetMarketDemand()[Lemonade].CurrentDemand;
        //Assert
        Assert.AreEqual(expectedDemand, actualDemand);
    }

    [TestCase(TestName="If the saturation elasticity is 1, the change in metric is 100%, and increase is passed, demand should double")]
    public void ADBE_IfSaturationElasticityIsOneDemandShouldIncrease()
    {
        //Arrange
        TestMarket.InitializeDemandForSpecificGood(Lemonade, 500);
        Lemonade.Elasticities.Add(ElasticityTypeEnum.SaturationElasticity, 1);
        var dummyMetric = 1;

        var expectedDemand = 1000;
        var expectedResult = LemonadeStandResultObject.Success();
        //Act
        var actualResult = Strategy.AdjustDemandBasedOnElasticityAndHistory(TestMarket, Lemonade, ElasticityTypeEnum.SaturationElasticity, dummyMetric);
        var actualDemand = TestMarket.GetMarketDemand()[Lemonade].CurrentDemand;
        //Assert
        Assert.AreEqual(expectedDemand, actualDemand);
        Assert.AreEqual(expectedResult.Result, actualResult.Result);
    }
    [TestCase(TestName="If the saturation elasticity is .5, and increase is passed, demand should increase by 50%")]
    public void ADBE_IfSaturationElasticityIsPointFiveDemandShouldIncrease()
    {
        //Arrange
        TestMarket.InitializeDemandForSpecificGood(Lemonade, 500);
        Lemonade.Elasticities.Add(ElasticityTypeEnum.SaturationElasticity, .5f);
        var dummyMetric = 1;

        var expectedDemand = 750;
        var expectedResult = LemonadeStandResultObject.Success();
        //Act
        var actualResult = Strategy.AdjustDemandBasedOnElasticityAndHistory(TestMarket, Lemonade, ElasticityTypeEnum.SaturationElasticity,dummyMetric);
        var actualDemand = TestMarket.GetMarketDemand()[Lemonade].CurrentDemand;
        //Assert
        Assert.AreEqual(expectedDemand, actualDemand);
        Assert.AreEqual(expectedResult.Result, actualResult.Result);
    }
    [TestCase(TestName="If the saturation elasticity is 1, and decrease is passed, demand should be eliminated")]
    public void ADBE_IfSaturationElasticityIsOneDemandShouldDisappear()
    {
        //Arrange
        TestMarket.InitializeDemandForSpecificGood(Lemonade, 500);
        Lemonade.Elasticities.Add(ElasticityTypeEnum.SaturationElasticity, 1);
        var dummyMetric = -1;

        var expectedDemand = 0;
        var expectedResult = LemonadeStandResultObject.Success();
        //Act
        var actualResult = Strategy.AdjustDemandBasedOnElasticityAndHistory(TestMarket, Lemonade, ElasticityTypeEnum.SaturationElasticity,dummyMetric);
        var actualDemand = TestMarket.GetMarketDemand()[Lemonade].CurrentDemand;
        //Assert
        Assert.AreEqual(expectedDemand, actualDemand);
        Assert.AreEqual(expectedResult.Result, actualResult.Result);
    }
#endregion

#region Population tests
    [TestCase(TestName="If the population elasticity is .7, and population doubles, demand should increase by 70%")]
    public void ADFP_DemandIncreasesBySeventyPercentWhenPopulationDoubles()
    {
        //Arrange
        Lemonade.Elasticities.Add(ElasticityTypeEnum.PopulationElasticity, .7f);
        TestMarket.InitializeDemandForSpecificGood(Lemonade, 100);
        
        TestMarket.CurrentPeriod = 1;

        TestMarketDataService.SetPopulationHistory(new List<PopulationHistory>()
        {
            new() {Population = 100, Period = 0, MarketId = TestMarket.MarketId},
            new() {Population = 200, Period = 1, MarketId = TestMarket.MarketId}
        });

        var expectedDemand = 170;
        
        //Act
        Strategy.AdjustDemandForPopulation(TestMarket, Lemonade);
        var actualDemand = TestMarket.GetMarketDemand()[Lemonade].CurrentDemand;
        //Assert
        Assert.AreEqual(expectedDemand, actualDemand);
    }
    [TestCase(TestName="If the population elasticity is .7, and population halves, demand should decrease by 35%")]
    public void ADFP_DemandDecreasesByThirtyPercentWhenPopulationHalves()
    {
        //Arrange
        TestMarket.InitializeDemandForSpecificGood(Lemonade, 100);
        Lemonade.Elasticities.Add(ElasticityTypeEnum.PopulationElasticity, .7f);
        
        TestMarket.CurrentPeriod = 1;

        TestMarketDataService.SetPopulationHistory(new List<PopulationHistory>()
        {
            new() {Population = 200, Period = 0, MarketId = TestMarket.MarketId},
            new() {Population = 100, Period = 1, MarketId = TestMarket.MarketId}
        });

        var expectedDemand = 65;
        
        //Act
        Strategy.AdjustDemandForPopulation(TestMarket, Lemonade);
        var actualDemand = TestMarket.GetMarketDemand()[Lemonade].CurrentDemand;
        //Assert
        Assert.AreEqual(expectedDemand, actualDemand);
    }
#endregion
#region Saturation tests
    [TestCase(TestName="A good's demand should not change if the saturation elasticity is 1 and the good supply is equal to demand")]
    public void SaturatedGoodElasticityOneDemandUnchanged()
    {
        //Arrange
        TestMarket.InitializeDemandForSpecificGood(Lemonade, 100);
            var marketDemand = TestMarket.GetMarketDemand();
            var lemonadeDemand = marketDemand[Lemonade];
            lemonadeDemand.Curvature = 1;

        Lemonade.Elasticities.Add(ElasticityTypeEnum.SaturationElasticity, 1);
        var Company1SellsLemonadeToAnyone = new Order(TestMarket,Company1,Lemonade,100,1){SubmittingCompany=Company1};
        TestMarket.RecordTrade(new Execution(Company1SellsLemonadeToAnyone,0));

        var expectedDemand = 100;
        //Act
        LinearDemandStrategy.AdjustDemandForSaturation(TestMarket, Lemonade);
        var actualDemand = TestMarket.GetMarketDemand()[Lemonade].CurrentDemand;
        //Assert
        Assert.AreEqual(expectedDemand, actualDemand);
    }
    [TestCase(TestName="A good's demand should double if the saturation elasticity is 1 and the good is supplied at zero")]
    public void UndersuppliedGoodElasticityOneDemandDoubles()
    {
        //Arrange
        TestMarket.InitializeDemandForSpecificGood(Lemonade, 100);
            var marketDemand = TestMarket.GetMarketDemand();
            var lemonadeDemand = marketDemand[Lemonade];
            lemonadeDemand.Curvature = 1;

        Lemonade.Elasticities.Add(ElasticityTypeEnum.SaturationElasticity, 1);
        var expectedDemand = 200;
        //Act
        LinearDemandStrategy.AdjustDemandForSaturation(TestMarket, Lemonade);
        var actualDemand = TestMarket.GetMarketDemand()[Lemonade].CurrentDemand;
        //Assert
        Assert.AreEqual(expectedDemand, actualDemand);
    }

    [TestCase(TestName="A good's demand should decrease if the saturation elasticity is 1 and the good is supplied at greater than demand")]
    public void OverSuppliedDemandFalls()
    {
        //Arrange
        const int initialDemand = 100;
        Lemonade.Elasticities.Add(ElasticityTypeEnum.SaturationElasticity, 1);
        TestMarket.InitializeDemandForSpecificGood(Lemonade, initialDemand);
            var marketDemand = TestMarket.GetMarketDemand();
            var lemonadeDemand = marketDemand[Lemonade];
            lemonadeDemand.Curvature = 1;
        
        var Company1SellsLemonadeToAnyone = new Order(TestMarket,Company1,Lemonade,200,1){SubmittingCompany=Company1};
        TestMarket.RecordTrade(new Execution(Company1SellsLemonadeToAnyone,0));
        //Act
        LinearDemandStrategy.AdjustDemandForSaturation(TestMarket, Lemonade);
        var actualDemand = TestMarket.GetMarketDemand()[Lemonade].CurrentDemand;
        //Assert
        Assert.IsTrue(actualDemand < initialDemand,$"Demand was expected to decrease from {initialDemand} but was {actualDemand}");
    }
    [TestCase(TestName="A good's demand should increase,but not double if the saturation elasticity is .5 and the good is supplied at zero")]
    public void UndersuppliedGoodElasticityPointFiveDemandIncreases()
    {
        //Arrange
        TestMarket.InitializeDemandForSpecificGood(Lemonade, 100);
            var marketDemand = TestMarket.GetMarketDemand();
            var lemonadeDemand = marketDemand[Lemonade];
            lemonadeDemand.Curvature = 1;
        Lemonade.Elasticities.Add(ElasticityTypeEnum.SaturationElasticity, .5f);
        var doubleDemand = 200;

        //Act
        LinearDemandStrategy.AdjustDemandForSaturation(TestMarket, Lemonade);
        var actualDemand = TestMarket.GetMarketDemand()[Lemonade].CurrentDemand;
        //Assert
        Assert.IsTrue(0< actualDemand && actualDemand < doubleDemand,$"Demand was expected to increase less than double from 100, but was {actualDemand}");
    }
#endregion
    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(TestEconomy.gameObject);
        TestMarket = null;
        Lemonade = null;
        TestMarketDataService = null;
        Company1 = null;
        Company2 = null;
    }
}