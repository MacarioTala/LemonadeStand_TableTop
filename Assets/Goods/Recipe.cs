using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using UnityEngine;

[CreateAssetMenu(menuName ="LemonadeStandAssets/Recipes")]
public class Recipe : ScriptableObject
{
    public string RecipeName;
    private List<Ingredient> ingredients=new();
    [SerializeField]
    private Good product;
    public Good GetProduct() => product;
    [SerializeField]
    private List<SerializableIngredient> SerializableIngredients=new();

    public bool CanRecipeBeMadeFrom(List<InventoryEntry> entries)
    {
        return Get_max_quantity(entries)>0;
    }
    public Recipe(string RecipeName, Good product, List<Ingredient> ingredients)
    {
        this.RecipeName = RecipeName;
        this.product = product;
        this.ingredients = ingredients;
    }
    public List<string> GetIngredientNames()
        =>ingredients.Select(ingredient => ingredient.Good.GoodName).ToList();
    
    public List<Ingredient> GetIngredients() => ingredients;

    public List<Ingredient> GetRequiredIngredientsForRecipe() => ingredients;

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
        var stock = inventory.GetAvailableInventory()
                 .Where(entry => GetIngredientNames().Contains(entry.good.GoodName))
                 .ToList();

        if (quantity > Get_max_quantity(stock))
        {
            throw new RecipeException("Not enough ingredients to make " + quantity + " " + product.GoodName);
        }
        else
        {
            inventory.ConsumeForRecipe(this, quantity);
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

    public decimal GetPerceivedCostPerUnit(List<KnownPrice> prices)
    {
        //Changed prices to required -- when would anyone ever know how to price something if they
        //didn't know the prices of the ingredients?
        decimal totalCost = 0m;
        
        if(ingredients.Any(x=> !prices.Any(y=>y.Good == x.Good)))
            return totalCost;

        foreach (var ingredient in ingredients)
            {
                var averagePrice = prices
                        .Where(x=>x.Good == ingredient.Good)
                        .Average(x=> x.Price);

                
                totalCost += averagePrice *ingredient.Quantity_needed;
            }
        return totalCost;
    }
    #region Unity Stuff
    void OnEnable()
    {
        ingredients??=new List<Ingredient>();
        ingredients.Clear();
        if( SerializableIngredients == null) return;
        foreach(var si in SerializableIngredients)
        {
            if(Equals(si.Good,null)) continue;
            ingredients.Add(new Ingredient(si.Good,si.QuantityNeeded));
        }
    }
    #endregion
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

[Serializable]
public struct SerializableIngredient
{
    public Good Good;
    [Min(1)]public int QuantityNeeded;
}