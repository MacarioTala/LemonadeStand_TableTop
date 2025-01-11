using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class LinearDemandStrategyTests
{
    TheEconomy TestEconomy;
    Good Lemonade;
    Market TestMarket;

    MockMarketDataService TestMarketDataService;

    LinearDemandStrategy Strategy;
    
    [SetUp]
    public void SetUp()
    {
        Lemonade = Good.CreateInstance("Lemonade", new Price_band(.5m, 2m), Rarity_enum.Uncommon);

        var EconomyObject = new GameObject();
        TestEconomy = EconomyObject.AddComponent<TheEconomy>();

        Strategy = new();

        TestMarket = Market.Factory.CreateStarterMarket("Starter Market",
                                                        CompanyLevelEnum.Market,
                                                        Strategy);
        TestMarketDataService = new MockMarketDataService();
        TestMarket.SetMarketDataService(TestMarketDataService);
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
        var expectedResult = LemonadeStandResultObject.Failure(ResultTypeEnum.ElasticityNotFound, "");
        //Act
        var actualResult = Strategy.AdjustDemandBasedOnElasticity(TestMarket, Lemonade, ElasticityTypeEnum.PopulationElasticity,dummyMetric);
        var actualDemand = TestMarket.GetMarketDemand()[Lemonade].CurrentDemand;
        //Assert
        Assert.AreEqual(expectedDemand, actualDemand);
        Assert.AreEqual(expectedResult.Result, actualResult.Result);
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
        var actualResult = Strategy.AdjustDemandBasedOnElasticity(TestMarket, Lemonade, ElasticityTypeEnum.SaturationElasticity, dummyMetric);
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
        var actualResult = Strategy.AdjustDemandBasedOnElasticity(TestMarket, Lemonade, ElasticityTypeEnum.SaturationElasticity,dummyMetric);
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
        var actualResult = Strategy.AdjustDemandBasedOnElasticity(TestMarket, Lemonade, ElasticityTypeEnum.SaturationElasticity,dummyMetric);
        var actualDemand = TestMarket.GetMarketDemand()[Lemonade].CurrentDemand;
        //Assert
        Assert.AreEqual(expectedDemand, actualDemand);
        Assert.AreEqual(expectedResult.Result, actualResult.Result);
    }

    [TestCase(TestName="If the population elasticity is .7, and population doubles, demand should increase by 70%")]
    public void ADFP_DemandIncreasesBySeventyPercentWhenPopulationDoubles()
    {
        //Arrange
        TestMarket.InitializeDemandForSpecificGood(Lemonade, 100);
        Lemonade.Elasticities.Add(ElasticityTypeEnum.PopulationElasticity, .7f);
        
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
    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(TestEconomy.gameObject);
        TestMarket = null;
        Lemonade = null;
        TestMarketDataService = null;
    }
}