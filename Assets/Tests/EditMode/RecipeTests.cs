using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;
using System;
using UnityEngine;

[TestFixture]
public class RecipeTests
{
    private Inventory test_inventory;
    private Good lemonade;
    private Good lemon;
    private Good sugar;
    private Good water;
    private Recipe lemonade_recipe;

    private readonly Price_band price_band1= new(.5f, 1f);
    private readonly Price_band price_band2= new(1f, 5f);
    private readonly Price_band price_band3= new(6f, 8f);

    private readonly Price_band price_band4 = new(10f, 15f);

    [SetUp]
    public void SetUp()
    {
        test_inventory = new Inventory();
        lemon = Good.CreateInstance("lemon", price_band2);
        sugar = Good.CreateInstance("sugar", price_band1);
        water = Good.CreateInstance("water", price_band3);
        var lemon_inventory_entry = new InventoryEntry(lemon, 10, 1);
        var sugar_inventory_entry = new InventoryEntry(sugar, 10, 1);
        var water_inventory_entry = new InventoryEntry(water, 10, 1);
        test_inventory.Add_good_to_inventory(lemon_inventory_entry);
        test_inventory.Add_good_to_inventory(sugar_inventory_entry);
        test_inventory.Add_good_to_inventory(water_inventory_entry);

        lemonade = Good.CreateInstance("lemonade", price_band4);
        var lemonade_ingredients = new List<Ingredient> { new(lemon, 9), new(sugar, 2), new(water, 7) };
        lemonade_recipe = new Recipe(lemonade, lemonade_ingredients);
    }

    [Test]
    public void Get_Ingredients_returns_list_of_ingredients()
    {
        // Arrange
        var expected = new List<String> { "lemon", "sugar", "water" };
        // Act
        var actual = lemonade_recipe.Get_Ingredients();
        // Assert
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void Get_max_quantity_returns_goods_based_on_smallest_ingredient_available()
    {
        // Arrange
        var expected = 1;// need 9 lemon, 2 sugar, 7 water, can only make 1 lemonade
        // Act
        var actual = lemonade_recipe.Get_max_quantity(test_inventory.Get_inventory_items());
        // Assert 
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void Make_recipe_throws_Recipe_Exception_if_not_enough_stock()
    {
        //arrange
        var test_inventory = new Inventory();
        var inventory_entry1 = new InventoryEntry(lemon, 10, 1.0f);
        var inventory_entry2 = new InventoryEntry(sugar, 10, 1.0f);
        var inventory_entry3 = new InventoryEntry(water, 10, 1.0f);
        test_inventory.Add_good_to_inventory(inventory_entry1);
        test_inventory.Add_good_to_inventory(inventory_entry2);
        test_inventory.Add_good_to_inventory(inventory_entry3);
        var lemonade = Good.CreateInstance("Lemonade", price_band4);
        var lemonade_recipe = new Recipe(lemonade, new List<Ingredient> { new(lemon, 9), new(sugar, 2), new(water, 7) });
        var quantity = 2;
        //act
        //assert   
        Assert.Throws<RecipeException>(() => lemonade_recipe.Make_recipe(quantity, test_inventory));
    }

    [Test]
    public void Make_recipe_consume_stock_and_return_product_and_quantity()
    {
        //arrange
        var test_inventory = new Inventory();
        var inventory_entry1 = new InventoryEntry(lemon, 10, 1.0f);
        var inventory_entry2 = new InventoryEntry(sugar, 10, 1.0f);
        var inventory_entry3 = new InventoryEntry(water, 10, 1.0f);
        test_inventory.Add_good_to_inventory(inventory_entry1);
        test_inventory.Add_good_to_inventory(inventory_entry2);
        test_inventory.Add_good_to_inventory(inventory_entry3);
        var lemonade = Good.CreateInstance("Lemonade", price_band4);
        var lemonade_recipe = new Recipe(lemonade, new List<Ingredient> { new(lemon, 9), new(sugar, 2), new(water, 7) });
        var quantity = 1;
        var expected_remaining_lemons = 1;
        var expected_remaining_sugar = 8;
        var expected_remaining_water = 3;
        //act
        var actual = lemonade_recipe.Make_recipe(quantity, test_inventory);
        //assert
        Assert.AreEqual((lemonade, quantity), actual);
        Assert.AreEqual(expected_remaining_lemons, test_inventory.Get_inventory_items().Where(entry => entry.good == lemon).First().quantity);
        Assert.AreEqual(expected_remaining_sugar, test_inventory.Get_inventory_items().Where(entry => entry.good == sugar).First().quantity);
        Assert.AreEqual(expected_remaining_water, test_inventory.Get_inventory_items().Where(entry => entry.good == water).First().quantity);
    }
    [Test]
    public void Get_recipe_returns_list_of_ingredients_and_quantity_needed()
    {
     // Arrange
        var expected = new List<(string Name, int Quantity)>
        {
            ("lemon", 9),
            ("sugar", 2),
            ("water", 7)
        };

        // Act
        var actual = lemonade_recipe.Get_recipe()
                                    .Select(ingredient => (ingredient.Good.good_name, ingredient.Quantity_needed))
                                    .ToList();

        // Assert
        foreach (var (expectedName, expectedQuantity) in expected)
        {
            var match = actual.FirstOrDefault(a => a.good_name == expectedName && a.Quantity_needed == expectedQuantity);
            Assert.IsNotNull(match, $"Expected ingredient '{expectedName}' with quantity {expectedQuantity} was not found in the recipe.");
        }
    }
}
