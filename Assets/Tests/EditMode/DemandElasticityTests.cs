using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class DemandElasticityTests
{
    TheEconomy TestEconomy;
    Good lemon;
    Good water;
    Good sugar;
    [SetUp]
    public void Setup ()
    {
        var economyObject = new GameObject();
        TestEconomy = economyObject.AddComponent<TheEconomy>();
        TestEconomy.Initialize(new MockLogger());
        lemon = Good.CreateInstance("Lemon", new Price_band(.5m, 1.0m), Rarity_enum.Common);
        water = Good.CreateInstance("Water", new Price_band(.5m, 1.0m), Rarity_enum.Common);
        sugar = Good.CreateInstance("Sugar", new Price_band(.5m, 1.0m), Rarity_enum.Common);

        water.AddElasticity(ElasticityTypeEnum.SaturationElasticity,0f);
    }
   
    [Test]
    public void DemandInelasticGoodsDoNotChangeDemand()
    {
        // Arrange
        var period = 0;
        var TestMarket = Market.Factory.CreateMarket("TestMarket", CompanyLevelEnum.Market,new LinearDemandStrategy());
        TestMarket.SetCash(1000000);
        var Company1 = Company.Factory.Create("Company1", CompanyLevelEnum.Beginner);
        Company1.SetCash(10000);
        var Company2 = Company.Factory.Create("Company2", CompanyLevelEnum.Beginner);
        Company2.SetCash(10000);
        Company1.GetInventory().AddGood(new InventoryEntry(water, 10000,1m,0));
        Company2.GetInventory().AddGood(new InventoryEntry(lemon, 1000,2m,0));
        TestMarket.RegisterCompany(Company1);
        TestMarket.RegisterCompany(Company2);
        var initialLemonDemand = 1000;
        var initialWaterDemand = 1000;
        TestMarket.InitializeDemandForSpecificGood(lemon,initialLemonDemand);
        TestMarket.InitializeDemandForSpecificGood(water,initialWaterDemand);
        var expectedWaterDemand = initialWaterDemand;

        var waterOrder = new Order(Company2,Company1,water,10000,1m);
        var lemonOrder = new Order(Company1,Company2,lemon,1000,2m);

        var waterContext = new ActionContext{TradeToSubmit = waterOrder,MarketToSubmitTo = TestMarket,Period = period};
        var lemonContext = new ActionContext{TradeToSubmit = lemonOrder,MarketToSubmitTo = TestMarket,Period = period};
        //Act
        Company1.QueueOrder(waterContext);
        Company2.QueueOrder(lemonContext);
        TestMarket.ProcessCompanyOrders();
        throw new System.Exception("Test not implemented");
        var actualWaterDemand = TestMarket.GetMarketDemand()[water].CurrentDemand;
        var actualLemonDemand = TestMarket.GetMarketDemand()[lemon].CurrentDemand;
        //Assert
        Assert.AreEqual(expectedWaterDemand,actualWaterDemand);
        Assert.AreNotEqual(initialLemonDemand,actualLemonDemand);
    }

    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(TestEconomy.gameObject);
    }
}