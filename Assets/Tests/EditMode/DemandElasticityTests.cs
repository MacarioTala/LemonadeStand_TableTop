using System;
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
    PopulationCompany TestPopulation;
    iStrategy TestReduceEnnuiStrategy;

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
        TestMarket = Market.Factory.CreateMarket("TestMarket", CompanyLevelEnum.Market)
            .WithTradeProcessor(new BasicTradeProcessor())
            .WithTransactionManager(new BasicTransactionManager())
            .WithDemandStrategy(TestDemandStrategy)
            .WithDemographicManager(TestDemographicManager);
            
        Company1 = Company.Factory.Create("Company1", CompanyLevelEnum.Beginner);
        Company2 = Company.Factory.Create("Company2", CompanyLevelEnum.Beginner);

        TestMarket.RegisterMarketParticipant(Company1);
        TestMarket.RegisterMarketParticipant(Company2);

        lemon = Good.CreateInstance("Lemon", new PriceBand(.5m, 1.0m), RarityEnum.Common);
        water = Good.CreateInstance("Water", new PriceBand(.5m, 1.0m), RarityEnum.Common);
        sugar = Good.CreateInstance("Sugar", new PriceBand(.5m, 1.0m), RarityEnum.Common);

        water.AddElasticity(ElasticityTypeEnum.SaturationElasticity,0f);
    }
   
    [Test]
    public void DemandInelasticGoodsDoNotChangeDemandInStableMarkets()
    {
        throw new NotImplementedException("currently doesn't make sense till populations create bids");
        // Arrange
        var Company1 = Company.Factory.Create("Company1", CompanyLevelEnum.Beginner);
        Company1.SetCash(10000);
        Company1.GetInventory().AddGood(new InventoryEntry(water, 10000,1m,Period));

        var initialWaterDemand = 1000;
        var testPopulation = CompanyBuilder.For<PopulationCompany>()
                            .Named("Test Population")
                            .WithInitialCash(5000)
                            .AssumingNewGoodsCost(1)
                            .AtLevel(CompanyLevelEnum.Beginner)
                            .Build();
        

        var Company1SellsWaterToAnyone = new Order(null,Company1,water,10000,1m);
        var waterContext = new ActionContext{TradeToSubmit = Company1SellsWaterToAnyone,MarketToSubmitTo = TestMarket,Period = Period};

        //Act
        Company1.QueueOrder(waterContext);
        TestMarket.ProcessCompanyOrders();
        var actualWaterDemand = TestMarket.GetPopulationDemand()[water].CurrentDemand;
        //Assert
        Assert.IsTrue(water.isDemandInelastic);
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
