using System.Collections.Generic;
using NUnit.Framework;

public partial class LinearDemandStrategyTests
{
#region Integration Tests for AdjustDemandInPeriod

    [TestCase(TestName = "AdjustDemandInPeriod increases demand by 70% when population doubles")]
    public void ADIP_DemandIncreasesBySeventyPercentWhenPopulationDoubles()
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
        Strategy.AdjustDemandInPeriod(TestMarket);
        var actualDemand = TestMarket.GetMarketDemand()[Lemonade].CurrentDemand;
        //Assert
        Assert.AreEqual(expectedDemand, actualDemand);
    }
   [TestCase(TestName="AdjustDemandInPeriod decreases demand by 35% when population halves")]
    public void ADIP_DemandDecreasesByThirtyPercentWhenPopulationHalves()
    {
        //Arrange
        Lemonade.Elasticities.Add(ElasticityTypeEnum.PopulationElasticity, .7f);
        TestMarket.InitializeDemandForSpecificGood(Lemonade, 100);
        
        TestMarket.CurrentPeriod = 1;

        TestMarketDataService.SetPopulationHistory(new List<PopulationHistory>()
        {
            new() {Population = 200, Period = 0, MarketId = TestMarket.MarketId},
            new() {Population = 100, Period = 1, MarketId = TestMarket.MarketId}
        });

        var expectedDemand = 65;
        
        //Act
        Strategy.AdjustDemandInPeriod(TestMarket);
        var actualDemand = TestMarket.GetMarketDemand()[Lemonade].CurrentDemand;
        //Assert
        Assert.AreEqual(expectedDemand, actualDemand);
    }

    [TestCase(TestName="AdjustDemandInPeriod: A good's demand should not change if the saturation elasticity is 1 and the good supply is equal to demand")]
    public void ADIP_SaturatedGoodElasticityOneDemandUnchanged()
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
        Strategy.AdjustDemandInPeriod(TestMarket);
        var actualDemand = TestMarket.GetMarketDemand()[Lemonade].CurrentDemand;
        //Assert
        Assert.AreEqual(expectedDemand, actualDemand);
    }
    [TestCase(TestName="AdjutDemandInPeriod: A good's demand should double if the saturation elasticity is 1 and the good is supplied at zero")]
    public void ADIP_UndersuppliedGoodElasticityOneDemandDoubles()
    {
        //Arrange
        TestMarket.InitializeDemandForSpecificGood(Lemonade, 100);
            var marketDemand = TestMarket.GetMarketDemand();
            var lemonadeDemand = marketDemand[Lemonade];
            lemonadeDemand.Curvature = 1;

        Lemonade.Elasticities.Add(ElasticityTypeEnum.SaturationElasticity, 1);
        var expectedDemand = 200;
        //Act
        Strategy.AdjustDemandInPeriod(TestMarket);
        var actualDemand = TestMarket.GetMarketDemand()[Lemonade].CurrentDemand;
        //Assert
        Assert.AreEqual(expectedDemand, actualDemand);
    }
#endregion
}