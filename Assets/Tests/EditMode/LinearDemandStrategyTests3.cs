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

        TestMarketDataService.SetPopulationHistory(new List<PopulationHistory>()
        {
            new() {Population = 100, Period = 0, MarketId = TestMarket.MarketId},
            new() {Population = 200, Period = 1, MarketId = TestMarket.MarketId}
        });

        var expectedDemand = 170;
        
        //Act
        TestMarket.UnleashMarketForces(TestMarket.CurrentPeriod);
        var actualDemand = TestMarket.GetMarketDemand()[Lemonade].CurrentDemand;
        //Assert
        Assert.AreEqual(expectedDemand, actualDemand);
    }

    [TestCase(TestName="From Market. Incremental demand adjustments should happen when Market.ProcessCompanyOrders is called")]
    public void UMF_DemandChangesWhenDemandIsFulfilled()
    {
        //Arrange
        var initialDemand = 100;
        Lemonade.Elasticities.Add(ElasticityTypeEnum.SaturationElasticity, .7f);
        TestMarket.InitializeDemandForSpecificGood(Lemonade, initialDemand);
        TestMarket.SetMarketInstability(.1f);
        
        Company2.GetInventory().AddGood(new InventoryEntry(Lemonade, 100,1m,Period));
        var Company2SellsLemonadeToAnyone = new Order(null,Company2,Lemonade,100,1m);
        var Company2SellLemonadeContext = new ActionContext{TradeToSubmit = Company2SellsLemonadeToAnyone,MarketToSubmitTo = TestMarket,Period = Period};

        Company2.QueueOrder(Company2SellLemonadeContext);

        //Act
        TestMarket.ProcessCompanyOrders();
        TestMarket.UnleashMarketForces(TestMarket.CurrentPeriod);
        var actualDemand = TestMarket.GetMarketDemand()[Lemonade].CurrentDemand;
        //Assert
        Assert.AreNotEqual(initialDemand, actualDemand);
        Debug.Log($"Initial Demand: {initialDemand}, Actual Demand: {actualDemand}");
        
    }
    
}