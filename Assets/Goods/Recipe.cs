using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName ="LemonadeStandAssets/Recipes")]
public class Recipe : ScriptableObject
{
    public string RecipeName;
    private List<Ingredient> _ingredients=new();
    [SerializeField]
    private Good _product;
    public Good GetProduct() => _product;
    [SerializeField]
    private List<SerializableIngredient> SerializableIngredients=new();

    public bool CanRecipeBeMadeFrom(List<InventoryEntry> entries)
    {
        return GetMaxQuantityFromInventory(entries)>0;
    }
    public Recipe(string recipeName, Good product, List<Ingredient> ingredients)
    {
        RecipeName = recipeName;
        _product = product;
        _ingredients = ingredients;
    }

    public Recipe()
    {
    }

    public void Initialize(string recipeName, Good product, List<Ingredient> ingredients)
    {
        RecipeName = recipeName;
        _product = product;
        _ingredients = ingredients;
    }
    public List<string> GetIngredientNames()
        =>_ingredients.Select(ingredient => ingredient.Good.GoodName).ToList();
    
    public List<Ingredient> GetIngredients() => _ingredients;

    public List<Ingredient> GetRequiredIngredientsForRecipe() => _ingredients;
    /// <summary>
    /// Note: This will return a list of goods with zero quantities if budget cannot make even one unit  
    /// </summary>
    /// <param name="prices"></param>
    /// <param name="budget"></param>
    /// <returns></returns>
    public List<Ingredient> GetIngredientMaximumsByBudget(Dictionary<Good,decimal> prices, decimal budget)
    {
        if(prices.Count==0) return new List<Ingredient>();
        var pricePerUnit = _ingredients.Sum(x=>x.QuantityNeeded * prices[x.Good]);

        var totalToProduce = (int)(budget/pricePerUnit);

        var returnList = new List<Ingredient>();
        foreach(var ingredient in _ingredients)
            returnList.Add(new (
                                ingredient.Good,
                                ingredient.QuantityNeeded*totalToProduce
                                )
                            );

        return returnList;
    }
    public int GetMaxQuantityFromInventory(List<InventoryEntry> stock)
    {
        var maxUnitsPerIngredient = _ingredients.Select(ingredient => stock
                                                  .Where(entry => entry.good == ingredient.Good)
                                                  .Sum(entry => entry.quantity) / ingredient.QuantityNeeded)
                                                  .ToList();
        return maxUnitsPerIngredient.Min();
    }

    public (Good, int) MakeRecipe(int quantity, Inventory inventory)
    {
        var stock = inventory.GetAvailableInventory()
                 .Where(entry => GetIngredientNames().Contains(entry.good.GoodName))
                 .ToList();

        if (quantity > GetMaxQuantityFromInventory(stock))
        {
            throw new RecipeException("Not enough ingredients to make " + quantity + " " + _product.GoodName);
        }
        else
        {
            inventory.ConsumeForRecipe(this, quantity);
            _product.IsProducedGood = true;
            return (_product, quantity);
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
            foreach (var ingredient in _ingredients)
            {
                inventory.AddGood(new(ingredient.Good, 1, ingredient.Good.GetPrice(), 0));
            }
        }
        
        foreach (var ingredient in _ingredients)
        {
            var inventoryEntries = inventory.GetInventoryEntriesByGood(ingredient.Good.GoodName);
            var costForThisIngredient = inventoryEntries.Sum(entry => entry.Cost * entry.quantity);
            var quantityForThisIngredient = inventoryEntries.Sum(entry => entry.quantity);
            var requiredUnits = ingredient.QuantityNeeded;

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
        
        if(_ingredients.Any(x=> !prices.Any(y=>y.Good == x.Good)))
            return totalCost;

        foreach (var ingredient in _ingredients)
            {
                var averagePrice = prices
                        .Where(x=>x.Good == ingredient.Good)
                        .Average(x=> x.Price);

                
                totalCost += averagePrice *ingredient.QuantityNeeded;
            }
        return totalCost;
    }
    #region Unity Stuff
    void OnEnable()
    {
        _ingredients??=new List<Ingredient>();
        _ingredients.Clear();
        if( SerializableIngredients == null) return;
        foreach(var si in SerializableIngredients)
        {
            if(Equals(si.Good,null)) continue;
            _ingredients.Add(new Ingredient(si.Good,si.QuantityNeeded));
        }
    }
    #endregion
}

public class Ingredient
{
    public readonly Good Good;
    public readonly int QuantityNeeded;
    
    public Ingredient(Good good, int quantity_needed)
    {
        Good = good;
        QuantityNeeded = quantity_needed;
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