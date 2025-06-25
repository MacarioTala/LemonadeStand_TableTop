using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Internal;

[TestFixture]
public class BidCreationTests_ReduceEnnuiStrategy
{
    Good Lemon;
    Good Water;
    Good Sugar;
    Good Lemonade;

    Recipe LemonadeRecipe;

    iStrategy TestReduceEnnuiStrategy;

    PopulationCompany TestPopulation;

    Market TestMarket;

    [SetUp]
    public void Setup()
    {
        var initialEnnui = .90f;
        var initialPopulation = 1000;
        Lemonade = new GoodBuilder()
                    .Named("Lemonade")
                    .WithRarity(RarityEnum.Uncommon)
                    .WhichIsProducedGood()
                    .Build();

        Lemon = new GoodBuilder()
                .Named("Lemon")
                .WithRarity(RarityEnum.Common)
                .Costing(.2m)
                .Build();
        Water = new GoodBuilder()
                .Named("Water")
                .WithRarity(RarityEnum.Common)
                .Costing(.2m)
                .Build();
        Sugar = new GoodBuilder()
                .Named("Sugar")
                .WithRarity(RarityEnum.Common)
                .Costing(.2m)
                .Build();

        LemonadeRecipe = new Recipe("Ordinary Lemonade"
                                    , Lemonade
                                    , new List<Ingredient>{
                                        new (Lemon,1),
                                        new (Water,3),
                                        new (Sugar,1)
                                        }
                                    );

        TestReduceEnnuiStrategy = StrategyBuilder.For<ReduceEnnuiStrategy>()
                                .WithAggressionLevel(.20m)
                                .Build();

        TestPopulation = CompanyBuilder.For<PopulationCompany>()
                        .Named("Test Population")
                        .WithInitialCash(2000)
                        .WithEnnui(initialEnnui)
                        .WithPopulation(initialPopulation)
                        .WithBehaviourStrategy(TestReduceEnnuiStrategy)
                        .AssumingNewGoodsCost(1m)
                        .Build();
        TestReduceEnnuiStrategy.GenerateGoals(TestPopulation);

        TestMarket = Market.Factory.CreateStarterMarket("Test Market", CompanyLevelEnum.Market, null);

        TestMarket.RegisterMarketParticipant(TestPopulation);
    }
    #region iStrategy
    [TestCase(TestName = "When no market prices are available, the bid is equal to the Naive COG")]
    public void NoMarketPricesDefaultBid()
    {
        //Arrange
        var naiveCog = TestPopulation.GetMarketIgnorantAssumedCOG();
        var expected = naiveCog;

        //Act
        var actual = iStrategy.GetCostAnchoredBid(Lemonade, TestPopulation);

        //Assert
        Assert.AreEqual(expected, actual);
    }
    [TestCase(TestName = "From iStrategy: When no market prices are available, the bid is equal to the Naive COG times the multiplier.")]
    public void NoMarketPricesDefaultBidMultiplierPassed()
    {
        //Arrange
        var naiveCog = TestPopulation.GetMarketIgnorantAssumedCOG();
        var multiplier = .8m;
        var expected = naiveCog*multiplier;

        //Act
        var actual = iStrategy.GetCostAnchoredBid(Lemonade, TestPopulation,multiplier);

        //Assert
        Assert.AreEqual(expected, actual);
    }
    #endregion
    #region GenerateBidAskSpreads
    [Test]
    public void NoProductsMatchingGoalReturnsEmptySet()
    {
        //Arrange
        var goal = TestPopulation.Goals.Find(x => x.Name.Equals("Reduce Ennui"));

        //Act
        var actualBids = (TestReduceEnnuiStrategy as ReduceEnnuiStrategy).GenerateBidAskSpreads(TestPopulation)
        .Where(x=>x.Key.Equals(Lemonade));

        //Assert
        Assert.IsNotNull(goal,"Goal not set");
        Assert.IsFalse(actualBids.Any());
    }
    #endregion
    
    #region CompanyGetPerceivedCostOfGood
    [Test]
    public void GetPerceivedCostOfGoodReturnsMarketIgnorantAssumedCOGNoRecipes()
    {
        //Arrange
        var naiveCog = TestPopulation.GetMarketIgnorantAssumedCOG();
        var expected = naiveCog;

        //Act
        var actual = TestPopulation.GetPerceivedCostOfGood(Lemonade, null);

        //Assert 
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void GetPerceivedCostOfGoodReturnsCostOfRecipePresent_NoMarketPrices()
    {
        //Arrange
        TestPopulation.Add_recipe(LemonadeRecipe);
        var expected = LemonadeRecipe.GetCostPerUnit(null);

        //Act
        var actual = TestPopulation.GetPerceivedCostOfGood(Lemonade, null);

        //Assert
        Assert.AreEqual(expected, actual);

        //TearDown
        TestPopulation.RemoveRecipe(LemonadeRecipe);
    }

    [Test]
    public void GetPerceivedCostOfGoodReturnsCostOfRecipePresent_MarketPricesPassed()
    {
        //Arrange
        TestPopulation.Add_recipe(LemonadeRecipe);
        var prices = new Dictionary<Good,decimal>
                        {
                            { Lemon,.3m },
                            { Water,.1m },
                            { Sugar,.2m }
                        };
        var expected = LemonadeRecipe.GetPerceivedCostPerUnit(prices);

        //Act
        var actual = TestPopulation.GetPerceivedCostOfGood(Lemonade, prices);

        //Assert
        Assert.AreEqual(expected, actual);

        //TearDown
        TestPopulation.RemoveRecipe(LemonadeRecipe);
    }
    [Test]
    public void GetPerceivedCostOfGoodReturnsRecipeCostFromIStrategy()
    {
        //Arrange
        TestPopulation.Add_recipe(LemonadeRecipe);
        var company1 = Company.Factory.Create("Company1", CompanyLevelEnum.Beginner);

        var prices = new Dictionary<Good, decimal>
                        {
                            { Lemon,.3m },
                            { Water,.1m },
                            { Sugar,.2m }
                        };
        TestMarket.MarketData.Add(new MarketData { Company = company1, Good = Lemon, Ask = .3m});
        TestMarket.MarketData.Add(new MarketData { Company = company1, Good = Water, Ask = .1m });
        TestMarket.MarketData.Add(new MarketData { Company = company1, Good = Sugar, Ask = .2m});

        var expected = LemonadeRecipe.GetPerceivedCostPerUnit(prices);

        //Act
        var actual = iStrategy.GetCostAnchoredBid(Lemonade, TestPopulation);

        //Assert
        Assert.AreEqual(expected, actual);

        //Teardown
        TestMarket.MarketData.Clear();
    }
    #endregion
    #region CalculateBidPerCapita
    /// <summary>
    /// Note: This is an internal call that assumes that demand for the good is already present.
    /// </summary>

    [Test]
    public void CalculateBidPerCapitaReturnsAdjustedNaiveBidWhenNoPricesExist()
    {
        //Arrange
        var aggressionLevel = TestReduceEnnuiStrategy.GetAggressionLevel();
        var naiveCog = TestPopulation.GetMarketIgnorantAssumedCOG();
        var expected = aggressionLevel * naiveCog;
        var reduceEnnuiGoal = TestPopulation.Goals
                            .Where(x => x.Name.Equals("Reduce Ennui"))
                            .FirstOrDefault();

        //Act
        var actual = (TestReduceEnnuiStrategy as ReduceEnnuiStrategy)
                        .CalculateBidPerCapita(Lemonade, TestPopulation, reduceEnnuiGoal)
                        .Bid;

        //Assert
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void CalculateBidPerCapitaReturnsPriceAdjustedNaiveBidWhenPricesExist()
    {
        //Arrange
        var company1 = Company.Factory.Create("Company 1", CompanyLevelEnum.Beginner);

        TestMarket.MarketData.Add(new MarketData { Company = company1, Good = Lemon, Ask = .3m });
        TestMarket.MarketData.Add(new MarketData { Company = company1, Good = Water, Ask = .1m });
        TestMarket.MarketData.Add(new MarketData { Company = company1, Good = Sugar, Ask = .2m });

        var aggressionLevel = TestReduceEnnuiStrategy.GetAggressionLevel();
        var pricedCog = LemonadeRecipe.GetCostPerUnit(null);
        var expected = aggressionLevel * pricedCog;
        var reduceEnnuiGoal = TestPopulation.Goals
                            .Where(x => x.Name.Equals("Reduce Ennui"))
                            .FirstOrDefault();
       

        //Act
        var actual = (TestReduceEnnuiStrategy as ReduceEnnuiStrategy)
                        .CalculateBidPerCapita(Lemonade, TestPopulation, reduceEnnuiGoal)
                        .Bid;

        //Assert
        Assert.AreEqual(expected, actual);

        //TearDown
        TestMarket.MarketData.Clear();
    }
    #endregion
    [TearDown]
    public void TearDown()
    {
        TestMarket = null;
        TheEconomy.Instance.ClearEconomy();
    }
}