using System;
using System.Collections.Generic;
using System.Linq;

public class Recipe
{
    public string RecipeName;
    private readonly List<Ingredient> ingredients;
    private readonly Good product;
    public Good GetProduct() => product;

    public Recipe(string RecipeName, Good product, List<Ingredient> ingredients)
    {
        this.RecipeName = RecipeName;
        this.product = product;
        this.ingredients = ingredients;
    }
    public List<string> GetIngredientNames()
    {
        return ingredients.Select(ingredient => ingredient.Good.GoodName).ToList();
    }
    public List<Ingredient> GetIngredients()
    {
        return ingredients;
    }

    public List<Ingredient> Get_recipe()
    {
        return ingredients;
    }

    public int Get_max_quantity(List<InventoryEntry> stock)
    {
        var max_units_per_ingredient = ingredients.Select(ingredient => stock
                                                  .Where(entry => entry.good == ingredient.Good)
                                                  .Sum(entry => entry.quantity) / ingredient.Quantity_needed)
                                                  .ToList();
        return max_units_per_ingredient.Min();
    }

    public (Good, int) Make_recipe(int quantity, Inventory inventory)
    {
        var stock = inventory.GetInventoryEntries()
                 .Where(entry => GetIngredientNames().Contains(entry.good.GoodName))
                 .ToList();

        if (quantity > Get_max_quantity(stock))
        {
            throw new RecipeException("Not enough ingredients to make " + quantity + " " + product.GoodName);
        }
        else
        {
            inventory.Consume_for_recipe(this, quantity);
            product.IsProducedGood = true;
            return (product, quantity);
        }
    }
    public override string ToString() => RecipeName;

    public override bool Equals(object other)
    {
        if (other is Recipe other_recipe)
        {
            return RecipeName == other_recipe.RecipeName;
        }
        return false;
    }

    public override int GetHashCode() => RecipeName?.GetHashCode() ?? 0;

    public decimal GetCostPerUnit(Inventory inventory)
    {
        Dictionary<string, decimal> cost_per_good = new();
        //If inventory is empty, naively use the base cost of the good
        if (inventory is null)
        {
            inventory = new();
            foreach (var ingredient in ingredients)
            {
                inventory.AddGood(new(ingredient.Good, 1, ingredient.Good.GetPrice(), 0));
            }
        }
        
        foreach (var ingredient in ingredients)
        {
            var inventoryEntries = inventory.GetInventoryEntriesByGood(ingredient.Good.GoodName);
            var costForThisIngredient = inventoryEntries.Sum(entry => entry.Cost * entry.quantity);
            var quantityForThisIngredient = inventoryEntries.Sum(entry => entry.quantity);
            var requiredUnits = ingredient.Quantity_needed;

            var costPerUnit = Math.Round(requiredUnits * (costForThisIngredient / quantityForThisIngredient), 2);
            cost_per_good.Add(ingredient.Good.GoodName, costPerUnit);
        }
        return Math.Round(cost_per_good.Sum(entry => entry.Value), 2);
    }

    public decimal GetPerceivedCostPerUnit(Dictionary<Good, decimal> asks=null)
    {
        decimal totalCost = 0;
        if (asks is null)
        {
            asks = new();
            foreach (var ingredient in ingredients)
            {
                asks.Add(ingredient.Good, ingredient.Good.GetPrice());
            }
        }
        foreach (var ingredient in ingredients)
            {
                if (asks.TryGetValue(ingredient.Good, out var askPrice))
                {
                    totalCost += askPrice * ingredient.Quantity_needed;
                }
                //what about if no price exists for the ingredient?
            }
        return Math.Round(totalCost, 2);
    }
}

public class Ingredient
{
    public readonly Good Good;
    public readonly int Quantity_needed;
    
    public Ingredient(Good good, int quantity_needed)
    {
        Good = good;
        Quantity_needed = quantity_needed;
    }
}

public class RecipeException : Exception
{
    public RecipeException(string message) : base(message)
    {
    }
}