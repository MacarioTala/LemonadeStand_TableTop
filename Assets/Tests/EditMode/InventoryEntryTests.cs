using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class InventoryEntryTests
{
    [Test]
    public void SetRecipeReturnsNullIfGoodIsNotProducedGood()
    {
        // Arrange
        var TestGood = Good.CreateInstance("TestGood", new PriceBand(1, 3), RarityEnum.Common);
        var TestInventoryEntry = new InventoryEntry(good: TestGood, 
                                                    quantity: 1, 
                                                    acquisition_price: 1.0m, 
                                                    period: 0);
        TestGood.IsProducedGood = false;
        Recipe expected = null;
        // Act
        var actual = TestInventoryEntry.GetRecipe();
        // Assert
        Assert.AreEqual(expected, actual);
    }
    [Test]
    public void CannotSetRecipeIfGoodIsNotProducedGood()
    {
        // Arrange
        var TestGood = Good.CreateInstance("TestGood", new PriceBand(1, 3), RarityEnum.Common);
        var TestInventoryEntry = new InventoryEntry(good: TestGood, 
                                                    quantity: 1, 
                                                    acquisition_price: 1.0m, 
                                                    period: 0);
        TestGood.IsProducedGood = false;

        Recipe TestRecipe = ScriptableObject.CreateInstance<Recipe>();
        TestRecipe.Initialize(recipeName: "Test Recipe", 
                                product: TestGood, 
                                ingredients: new List<Ingredient>());
        // Act
        // Assert
        Assert.Throws<System.Exception>(() => TestInventoryEntry.SetRecipe(TestRecipe));
    }
    [Test]
    public void SetRecipeSetsRecipeIfGoodIsProducedGood()
    {
        // Arrange
        var TestGood = Good.CreateInstance("TestGood", new PriceBand(1, 3), RarityEnum.Common);
        var TestInventoryEntry = new InventoryEntry(good: TestGood, 
                                                    quantity: 1, 
                                                    acquisition_price: 1.0m, 
                                                    period: 0);
        TestGood.IsProducedGood = true;
        Recipe TestRecipe = ScriptableObject.CreateInstance<Recipe>();
        TestRecipe.Initialize(recipeName: "Test Good", 
                                product:TestGood, 
                                ingredients: new List<Ingredient>());
        var expected = TestRecipe;
        // Act
        TestInventoryEntry.SetRecipe(TestRecipe);
        var actual = TestInventoryEntry.GetRecipe();
        // Assert
        Assert.AreEqual(expected, actual);
    }

}