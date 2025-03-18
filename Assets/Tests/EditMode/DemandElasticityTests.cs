using System.Data.Common;
using NUnit.Framework;
using NUnit.Framework.Internal;
using UnityEngine;

[TestFixture]
public class DemandElasticityTests
{
    TheEconomy TestEconomy;
    Market TestMarket;
    Company Company1;
    Company Company2;

    int Period = 0;
    Good lemon;
    Good water;
    Good sugar;

    iDemandStrategy TestDemandStrategy;

    iDemographicManager TestDemographicManager;

    [SetUp]
    public void Setup ()
    {
        TheEconomy.SetupForTests(new MockLogger());
        TestEconomy = TheEconomy.Instance;

        TestDemographicManager = new MockDemographicManager();
        TestDemandStrategy = ScriptableObject.CreateInstance<LinearDemandStrategy>();
        TestMarket = Market.Factory.CreateMarket("TestMarket", CompanyLevelEnum.Market,TestDemandStrategy);
        TestMarket.SetDemographicManager(TestDemographicManager);

        Company1 = Company.Factory.Create("Company1", CompanyLevelEnum.Beginner);
        Company2 = Company.Factory.Create("Company2", CompanyLevelEnum.Beginner);

        TestMarket.RegisterCompany(Company1);
        TestMarket.RegisterCompany(Company2);

        lemon = Good.CreateInstance("Lemon", new PriceBand(.5m, 1.0m), RarityEnum.Common);
        water = Good.CreateInstance("Water", new PriceBand(.5m, 1.0m), RarityEnum.Common);
        sugar = Good.CreateInstance("Sugar", new PriceBand(.5m, 1.0m), RarityEnum.Common);

        water.AddElasticity(ElasticityTypeEnum.SaturationElasticity,0f);
    }
   
    [Test]
    public void DemandInelasticGoodsDoNotChangeDemandInStableMarkets()
    {
        // Arrange
        TestMarket.SetCash(1000000);
        var Company1 = Company.Factory.Create("Company1", CompanyLevelEnum.Beginner);
        Company1.SetCash(10000);
        Company1.GetInventory().AddGood(new InventoryEntry(water, 10000,1m,Period));

        var initialWaterDemand = 1000;
        
        TestMarket.InitializeDemandForSpecificGood(water,initialWaterDemand);

        var Company1SellsWaterToAnyone = new Order(null,Company1,water,10000,1m);

        var waterContext = new ActionContext{TradeToSubmit = Company1SellsWaterToAnyone,MarketToSubmitTo = TestMarket,Period = Period};

        //Act
        Company1.QueueOrder(waterContext);
        TestMarket.ProcessCompanyOrders();
        TestMarket.FulfillDemand();
        var actualWaterDemand = TestMarket.GetMarketDemand()[water].CurrentDemand;
        //Assert
        Assert.AreEqual(initialWaterDemand,actualWaterDemand);
    }

    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(TestEconomy.gameObject);
        TestEconomy = null;
        TestMarket = null;
        Company1 = null;
        Company2 = null;
        lemon = null;
        water = null;
        sugar = null;
    }
}

public class MockDemographicManager : iDemographicManager
{
    public float GetMarketInstability()
    {
        return 0f; // Stable market
    }

    public int GetPopulation()
    {
        throw new System.NotImplementedException();
    }

    public float GetPopulationEnnui()
    {
        throw new System.NotImplementedException();
    }

    public float GetPopulationGrowthRate()
    {
        throw new System.NotImplementedException();
    }

    public float GetPopulationHappiness()
    {
        throw new System.NotImplementedException();
    }

    public LemonadeStandResultObject SetMarketInstability(float newInstability)
    {
        throw new System.NotImplementedException();
    }

    public LemonadeStandResultObject SetPopulation(int newPopulation)
    {
        throw new System.NotImplementedException();
    }

    public LemonadeStandResultObject SetPopulationHappiness(float newHappiness)
    {
        throw new System.NotImplementedException();
    }
}