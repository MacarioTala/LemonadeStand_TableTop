using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor.VersionControl;

[TestFixture]
public class PopulationBasedDemandTests
{
    Good Lemon;
    Good Water;
    Good Sugar;
    Good Lemonade;
    Recipe LemonadeRecipe;

    [SetUp]
    public void Setup()
    {
        Lemon = new GoodBuilder() 
                .Named("Lemon") 
                .WithRarity(RarityEnum.Common) 
                .Costing(.2m) 
                .Build(); 

        Water = new GoodBuilder() 
                .Named("Water") 
                .WithRarity(RarityEnum.Common) 
                .Costing(.2m) .Build(); 
        
        Sugar = new GoodBuilder() 
                .Named("Sugar") 
                .WithRarity(RarityEnum.Common) 
                .Costing(.2m) 
                .Build(); 
        
        Lemonade = new GoodBuilder()
                .Named("Lemonade")
                .WithRarity(RarityEnum.Uncommon)
                .Costing(1m)
                .Build();
        
        LemonadeRecipe = new Recipe("Ordinary Lemonade" , Lemonade , new List<Ingredient>{ new (Lemon,1), new (Water,3), new (Sugar,1) } );
    }

    [Test]
    public void GetRequisiteDemandReturns5WhenMinimumDemandIs5()
    {
        //Arrange
        var expectedDemand = 5;
        var lemonadeDemand = new Dictionary<Good, DemandData>
        {
            { Lemonade, new() { MinDemand = expectedDemand } }
        };
        var pop = EconAgentBuilder.For<PopulationAgent>()
            .Named("TestPop")
            .Demanding(lemonadeDemand)
            .WithBehaviourStrategy(new BasicConsumptionStrategy())
            .Build();

        //Act
        var actualDemand = pop.GetDemandFor(Lemonade).GetRequisiteDemand();

        //Assert
        Assert.AreEqual(expectedDemand,actualDemand);
    }
#region BasicConsumptionStrategy
    [Test]
    public void BasicConsumptionStrategy_PerformStrategyCreatesOneBuyActionWhenRequisiteDemandIsPositive()
    {
        var lemonadeDemand = new Dictionary<Good, DemandData>
        {
            { Lemonade, new DemandData { MinDemand = 5 } }
        };

        var strategy = new BasicConsumptionStrategy();

        var pop = EconAgentBuilder.For<PopulationAgent>()
            .Named("TestPop")
            .Demanding(lemonadeDemand)
            .WithBehaviourStrategy(strategy)
            .Build();

        pop.AddRecipe(LemonadeRecipe);
        strategy.SetEconAgent(pop);

        strategy.PerformStrategy(pop);

        var actions = strategy.GetQueuedActions();

        Assert.AreEqual(1, actions.Count);
    }

    [Test]
    public void BasicConsumptionStrategy_PerformStrategyCreatesNoBuyActionsWhenRequisiteDemandIsZero()
    {
        var lemonadeDemand = new Dictionary<Good, DemandData>
        {
            { Lemonade, new DemandData { MinDemand = 0 } }
        };

        var strategy = new BasicConsumptionStrategy();

        var pop = EconAgentBuilder.For<PopulationAgent>()
            .Named("TestPop")
            .Demanding(lemonadeDemand)
            .WithBehaviourStrategy(strategy)
            .Build();

        pop.AddRecipe(LemonadeRecipe);
        strategy.SetEconAgent(pop);

        strategy.PerformStrategy(pop);

        var actions = strategy.GetQueuedActions();

        Assert.AreEqual(0, actions.Count);
    }
      [Test]
    public void BasicConsumptionStrategy_PerformStrategyCreatesBuyActionQuantity5_WhenMinimumDemandIs5()
    {
        var expectedDemand = 5;

        var lemonadeDemand = new Dictionary<Good, DemandData>
        {
            { Lemonade, new DemandData { MinDemand = expectedDemand } }
        };

        var strategy = new BasicConsumptionStrategy();

        var pop = EconAgentBuilder.For<PopulationAgent>()
            .Named("TestPop")
            .Demanding(lemonadeDemand)
            .WithBehaviourStrategy(strategy)
            .Build();

        pop.AddRecipe(LemonadeRecipe);
        strategy.SetEconAgent(pop);

        strategy.PerformStrategy(pop);

        var action = strategy.GetQueuedActions().Single();
        Assert.AreEqual(expectedDemand, action.TradeToSubmit.Quantity);
    }
    [Test]
    public void BasicConsumptionStrategy_PerformStrategyCreatesBuyActionForLemonadeWhenDemandIsOnlyLemonade()
    {
        var lemonadeDemand = new Dictionary<Good, DemandData>
        {
            { Lemonade, new DemandData { MinDemand = 5 } }
        };

        var strategy = new BasicConsumptionStrategy();

        var pop = EconAgentBuilder.For<PopulationAgent>()
            .Named("TestPop")
            .Demanding(lemonadeDemand)
            .WithBehaviourStrategy(strategy)
            .Build();

        pop.AddRecipe(LemonadeRecipe);
        strategy.SetEconAgent(pop);

        strategy.PerformStrategy(pop);

        var action = strategy.GetQueuedActions().Single();
        Assert.AreEqual(Lemonade, action.TradeToSubmit.Good);
    }
  [Test]
    public void BasicConsumptionStrategy_LimitPriceForCreateBuysIsPerceivedCostOfLemonade()
    {
        var lemonadeDemand = new Dictionary<Good, DemandData>
        {
            { Lemonade, new DemandData { MinDemand = 5 } }
        };

        var strategy = new BasicConsumptionStrategy();

        var pop = EconAgentBuilder.For<PopulationAgent>()
            .Named("TestPop")
            .Demanding(lemonadeDemand)
            .WithBehaviourStrategy(strategy)
            .Build();

        pop.AddRecipe(LemonadeRecipe);
        strategy.SetEconAgent(pop);

        var expectedPrice = pop.GetPerceivedCostOfGood(Lemonade);

        strategy.PerformStrategy(pop);

        var action = strategy.GetQueuedActions().Single();
        Assert.AreEqual(expectedPrice, action.TradeToSubmit.Price);
    }
    [Test]
    public void BasicConsumptionStrategy_PerformStrategyCreatesBuyActionsForEachGoodWithPositiveRequisiteDemand()
    {
        var bread = new GoodBuilder()
            .Named("Bread")
            .WithRarity(RarityEnum.Common)
            .Costing(.5m)
            .Build();

        var demand = new Dictionary<Good, DemandData>
        {
            { Lemonade, new DemandData { MinDemand = 5 } },
            { bread, new DemandData { MinDemand = 2 } }
        };

        var strategy = new BasicConsumptionStrategy();

        var pop = EconAgentBuilder.For<PopulationAgent>()
            .Named("TestPop")
            .Demanding(demand)
            .WithBehaviourStrategy(strategy)
            .Build();

        pop.AddRecipe(LemonadeRecipe);
        strategy.SetEconAgent(pop);

        strategy.PerformStrategy(pop);

        var actions = strategy.GetQueuedActions();

        Assert.AreEqual(2, actions.Count);
        Assert.IsTrue(actions.Any(x => x.TradeToSubmit.Good == Lemonade && x.TradeToSubmit.Quantity == 5));
        Assert.IsTrue(actions.Any(x => x.TradeToSubmit.Good == bread && x.TradeToSubmit.Quantity == 2));
    }
#endregion
    
}