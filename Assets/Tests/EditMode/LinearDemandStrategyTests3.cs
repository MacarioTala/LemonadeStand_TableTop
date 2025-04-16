using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;


/// <summary>
/// These are tests for integrating LinearDemandStrategy with Market
/// </summary>
public partial class LinearDemandStrategyTests
{

    [TestCase(TestName = "From Market.UnleashMarketForces: AdjustDemandInPeriod increases demand by 70% when population doubles")]
     public void UMF_DemandIncreasesBySeventyPercentWhenPopulationDoubles()
    {
        //Arrange
        Lemonade.Elasticities.Add(ElasticityTypeEnum.PopulationElasticity, .7f);
        TestMarket.InitializeDemandForSpecificGood(Lemonade, 100);
        
        TestMarket.CurrentPeriod = 1;

        ((MockDemographicManager)TestDemographicManager).SetPopulationHistory(new List<PopulationHistory>()
        {
            new() {Population = 100, Period = 0, MarketId = TestMarket.MarketId, Phase = TurnPhase.Beginning},
            new() {Population = 100, Period = 0, MarketId = TestMarket.MarketId, Phase = TurnPhase.End},
            new() {Population = 200, Period = 1, MarketId = TestMarket.MarketId, Phase = TurnPhase.Beginning},
            new() {Population = 200, Period = 1, MarketId = TestMarket.MarketId, Phase = TurnPhase.End}
        });

        var expectedDemand = 170;
        
        //Act
        TestMarket.UnleashMarketForces(TestMarket.CurrentPeriod);
        var actualDemand = TestMarket.GetMarketDemand()[Lemonade].CurrentDemand;
        //Assert
        Assert.AreEqual(expectedDemand, actualDemand);
    }

    [TestCase(TestName="From Market. Demand changes only by the market instability if it's 100% filled")]
    public void UMF_DemandStableWhenDemandIsFulfilled()
    {
        //Arrange
        var initialDemand = 100;
        Lemonade.Elasticities.Add(ElasticityTypeEnum.SaturationElasticity, .7f);
        TestMarket.InitializeDemandForSpecificGood(Lemonade, initialDemand);
        TestMarket.SetMarketInstability(.1f);

        var expectedLowerBound = initialDemand;
        var expectedUpperBound = initialDemand + (int)(initialDemand * .1f);
        
        Company2.GetInventory().AddGood(new InventoryEntry(Lemonade, 100,1m,Period));
        var Company2SellsLemonadeToAnyone = new Order(null,Company2,Lemonade,100,1m);
        var Company2SellLemonadeContext = new ActionContext{TradeToSubmit = Company2SellsLemonadeToAnyone,MarketToSubmitTo = TestMarket,Period = Period};

        Company2.QueueOrder(Company2SellLemonadeContext);

        //Act
        TestMarket.ProcessCompanyOrders();
        TestMarket.UnleashMarketForces(TestMarket.CurrentPeriod);
        var actualDemand = TestMarket.GetMarketDemand()[Lemonade].CurrentDemand;
        //Assert
        Assert.GreaterOrEqual(actualDemand, expectedLowerBound);
        Assert.LessOrEqual(actualDemand, expectedUpperBound);
    }
    
}