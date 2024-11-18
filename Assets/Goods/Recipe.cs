using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Recipe
{
        private readonly List<Ingredient> ingredients;
        private readonly Good product;
        private readonly float cost;
        
        public Recipe(Good product, List<Ingredient> ingredients)
        {
            this.product = product;
            this.ingredients = ingredients;
        }
        public List<String> Get_Ingredients()
        {
            return ingredients.Select(ingredient => ingredient.Good.good_name).ToList();
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
        
        public (Good,int) Make_recipe(int quantity, Inventory inventory)
        {
            var stock = inventory.GetInventoryEntries()
                     .Where(entry => Get_Ingredients().Contains(entry.good.good_name))
                     .ToList();

            if (quantity > Get_max_quantity(stock))
            {
                throw new RecipeException("Not enough ingredients to make "+quantity+" "+product.good_name);
            }
            else
            {
                inventory.Consume_for_recipe(this, quantity);    
                return (product, quantity);
            }
        }
    public override string ToString()
    {
        return product.good_name;
    }

    public override bool Equals(object other)
    {
        if (other is Recipe other_recipe)
        {
            return product.good_name == other_recipe.product.good_name;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return product.good_name?.GetHashCode()??0;
    }

    public decimal GetCostPerUnit(Inventory inventory)
    {
        Dictionary<string, decimal> cost_per_good = new();
        foreach (var ingredient in ingredients)
        {
            var inventoryEntries = inventory.GetInventoryEntriesByGood(ingredient.Good.good_name);
            var costForThisIngredient = inventoryEntries.Sum(entry => entry.acquisition_price*entry.quantity);
            var quantityForThisIngredient = inventoryEntries.Sum(entry => entry.quantity);
            var requiredUnits = ingredient.Quantity_needed;

            var costPerUnit = Math.Round(requiredUnits*(costForThisIngredient / quantityForThisIngredient),2);
            cost_per_good.Add(ingredient.Good.good_name, costPerUnit);
        }
        return Math.Round(cost_per_good.Sum(entry => entry.Value),2);
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