using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Unity.VisualScripting.YamlDotNet.Core;
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
    [SetUp]
    public void Setup ()
    {
        TheEconomy.SetupForTests(new MockLogger());
        TestEconomy = TheEconomy.Instance;

        TestMarket = Market.Factory.CreateMarket("TestMarket", CompanyLevelEnum.Market,new LinearDemandStrategy());

        Company1 = Company.Factory.Create("Company1", CompanyLevelEnum.Beginner);
        Company2 = Company.Factory.Create("Company2", CompanyLevelEnum.Beginner);

        TestMarket.RegisterCompany(Company1);
        TestMarket.RegisterCompany(Company2);

        lemon = Good.CreateInstance("Lemon", new Price_band(.5m, 1.0m), Rarity_enum.Common);
        water = Good.CreateInstance("Water", new Price_band(.5m, 1.0m), Rarity_enum.Common);
        sugar = Good.CreateInstance("Sugar", new Price_band(.5m, 1.0m), Rarity_enum.Common);

        water.AddElasticity(ElasticityTypeEnum.SaturationElasticity,0f);
    }
   
    [Test]
    public void DemandInelasticGoodsDoNotChangeDemand()
    {
        // Arrange
        var TestMarket = Market.Factory.CreateMarket("TestMarket", CompanyLevelEnum.Market,new LinearDemandStrategy());
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