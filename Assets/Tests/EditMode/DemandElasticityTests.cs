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
    EconAgent Company1;
    EconAgent Company2;
    PopulationAgent TestPopulation;
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
        TestMarket = Market.Factory.CreateMarket("TestMarket")
            .WithTradeProcessor(new DefaultTradeProcessor())
            .WithTransactionManager(new DefaultTransactionManager())
            .WithDemandStrategy(TestDemandStrategy)
            .WithDemographicManager(TestDemographicManager);
            
        Company1 = EconAgent.Factory.Create("Company1", AgentLevelEnum.Beginner);
        Company2 = EconAgent.Factory.Create("Company2", AgentLevelEnum.Beginner);

        TestMarket.RegisterMarketParticipant(Company1);
        TestMarket.RegisterMarketParticipant(Company2);

        lemon = Good.CreateInstance("Lemon", new PriceBand(.5m, 1.0m), RarityEnum.Common);
        water = Good.CreateInstance("Water", new PriceBand(.5m, 1.0m), RarityEnum.Common);
        sugar = Good.CreateInstance("Sugar", new PriceBand(.5m, 1.0m), RarityEnum.Common);

        water.AddElasticity(ElasticityTypeEnum.SaturationElasticity,0f);
    }
   
    [Test]
    [Ignore("Test needs to be implemented")]
    public void DemandInelasticGoodsDoNotChangeDemandRegardlessOfPrice()
    {
        throw new NotImplementedException();
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
