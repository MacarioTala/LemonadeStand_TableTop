using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;


/// <summary>
/// These are tests for integrating LinearDemandStrategy with Market
/// </summary>
public partial class LinearDemandStrategyTests
{

    [TestCase(TestName="From Market. Demand changes only by the market instability if it's 100% filled")][Ignore("Obsolete. Demand is now agent-driven instead of centrally controlled")]
    public void UMF_DemandStableWhenDemandIsFulfilled()
    {
        //Arrange
        var initialDemand = 100;
        Lemonade.Elasticities.Add(ElasticityTypeEnum.SaturationElasticity, .7f);
        TestMarket.SetMarketInstability(.1f);

        var expectedLowerBound = initialDemand;
        var expectedUpperBound = initialDemand + (int)(initialDemand * .1f);
        
        Company2.GetInventory().AddInventoryEntry(new InventoryEntry(Lemonade, 100,1,Period));
        var Company2SellsLemonadeToAnyone = new Order(null,Company2,Lemonade,100,1);
        var Company2SellLemonadeContext = new ActionContext{TradeToSubmit = Company2SellsLemonadeToAnyone,MarketToSubmitTo = TestMarket,Period = Period};

        Company2.QueueOrder(Company2SellLemonadeContext);

        //Act
        TestMarket.ProcessCompanyOrders();
        TestMarket.UnleashMarketForces(TestMarket.CurrentPeriod);
        var actualDemand = TestMarket.GetPopulationDemand()[Lemonade].CurrentDemand;
        //Assert
        Assert.GreaterOrEqual(actualDemand, expectedLowerBound);
        Assert.LessOrEqual(actualDemand, expectedUpperBound);
    }
    
}