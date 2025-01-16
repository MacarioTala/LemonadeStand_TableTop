using System.Collections.Generic;
using NUnit.Framework;

public partial class LinearDemandStrategyTests
{
#region Integration Tests for AdjustDemandInPeriod

    [TestCase(TestName = "AdjustDemandInPeriod adjusts demand when population changes")]
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
#endregion
}