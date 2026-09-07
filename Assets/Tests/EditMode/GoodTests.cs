
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class GoodTests
{
    readonly PriceBand price_band1 = new(1,2);
    Good Lemon;
    Good Sugar;
    Good Water;

    [SetUp]
    public void SetUp()
    {
        Lemon = new GoodBuilder()
            .Named("Lemon")
            .WithRarity(RarityEnum.Common)
            .Costing(1)
            .Build();
        Sugar = new GoodBuilder()
            .Named("Sugar")
            .WithRarity(RarityEnum.Common)
            .Costing(1)
            .Build();
        Water = new GoodBuilder()
            .Named("Water")
            .WithRarity(RarityEnum.Common)
            .Costing(1)
            .Build();
    }

    [Test]
    public void Creating_good_generates_price_based_on_price_band()
    {
        // Arrange
        var good_name = "lemon";
        var uncommon = RarityEnum.Uncommon;
        var expected_price_min = 0.5m;
        var expected_price_max = 1.0m;
        var expected_rarity = RarityEnum.Uncommon;

        // Act
        var lemon = Good.CreateInstance(good_name, price_band1,uncommon);
        // Assert
        Assert.AreEqual(good_name, lemon.GoodName);
        Assert.AreEqual(expected_rarity, lemon.GetRarity());
        Assert.IsTrue(lemon.GetPrice() >= expected_price_min && lemon.GetPrice() <= expected_price_max);
    }
    

#region Good Effects
    [Test]
    public void ApplyingEnnuiReducingGoodEffectReducesEnnui()
    {
        // Arrange
        var company = EconAgentBuilder.For<PopulationAgent>()
            .Named("Test Company")
            .WithEnnui(.99f)
            .AtLevel(AgentLevelEnum.Beginner)
            .Build();
        var goodEffect = GoodEffectBuilder.Create()
            .Named("Ennui Reducer")
            .DescribedAs("Reduces ennui by 0.1")
            .WithEffect(new MetricModifier<PopulationAgent>(
                c => c.Ennui,
                (c, newValue) => c.Ennui = newValue))
            .WithEffectMagnitude(-0.1f);
        var whoopeeCushion = ScriptableObject.CreateInstance<Good>();
        whoopeeCushion.AddEffect(goodEffect);
        var expectedEnnui = company.Ennui - 0.1f;

        // Act
        whoopeeCushion.ApplyEffects(company);
        var actualEnnui = company.Ennui;

        // Assert
        Assert.AreEqual(expectedEnnui, actualEnnui);
    }
    #endregion
#region Pricing Tests
   [Test]
   public void GetCostOfGoodReturnsCostOfRecipe()
   {
         // Arrange
        var enhancedlemonade = new GoodBuilder()
            .Named("Enhanced Lemonade")
            .WithRarity(RarityEnum.VeryRare)
            .Build();
        enhancedlemonade.IsProducedGood = true;
        var enhancedLemonadeRecipe = ScriptableObject.CreateInstance<Recipe>(); 
        enhancedLemonadeRecipe.Initialize(recipeName: "Enhanced Lemonade",
                                     product: enhancedlemonade, 
                                     ingredients: new List<Ingredient> { new(Lemon, 9), 
                                                                        new(Sugar, 2), 
                                                                        new(Water, 7) });
        var costPerUnit = 9 * Lemon.GetPrice() + 2 * Sugar.GetPrice() + 7 * 
        Water.GetPrice();
        // Act
        var actual = enhancedlemonade.GetCostOfGood(enhancedLemonadeRecipe);
        // Assert
        Assert.IsTrue(actual == costPerUnit);
        Debug.Log($"Cost per unit: {costPerUnit}" + " Ask: " + actual);
   }
   #endregion
}