using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
public class Inventory
{
    private readonly List<InventoryEntry> inventoryEntries = new(); 

    public void Clear()
    {
        inventoryEntries.Clear();
    }
    public List<InventoryEntry> GetInventoryEntries() => inventoryEntries ?? Enumerable.Empty<InventoryEntry>().ToList();

    public List<InventoryEntry> GetAvailableInventory() => GetInventoryEntries().Where(x=>x.RemainingDelay==0).ToList();

    public List<InventoryEntry> GetInventoryEntriesByGood(string goodName)
    {
        var items = inventoryEntries.Where(x => x.good.GoodName == goodName)
                              .OrderBy(x => x.Cost)
                              .ToList();
        if (items.Count == 0)
        {
            new List<InventoryEntry>();
        }
        return items;
    }
    public InventoryEntry AddGood(InventoryEntry entry)
    {
        var existingGoodAtPriceAndExpiry = inventoryEntries.Find(item=> item.good.GoodName == entry.good.GoodName 
                                            && item.Cost == entry.Cost
                                            && item.PeriodAcquired == entry.PeriodAcquired
                                            );
        if(existingGoodAtPriceAndExpiry == null)
        {
            inventoryEntries.Add(entry);
        }
        else
        {
            existingGoodAtPriceAndExpiry.quantity += entry.quantity;
        }
        return entry;
    }

    public void RemoveGood(Good good, int quantity, decimal price) 
    {
        var goods_to_remove = Generate_goods_to_remove(good, quantity, price);
        Remove_goods(goods_to_remove);
    } 
    public List<InventoryEntry> Generate_goods_to_remove(Good good, int quantity, decimal price)
    {
        var eligible_goods = inventoryEntries
            .Where(item=> item.good == good && item.Cost <= price)
            .OrderBy(item=> item.Cost)
            .ToList();
        
        var goods_to_remove = new List<InventoryEntry>();
        var remaining_quantity = quantity;

        foreach(var current_entry in eligible_goods)
        {
            if(remaining_quantity<=0)
            {
                break;
            }
            
            var quantity_to_remove = Math.Min(current_entry.quantity, remaining_quantity);
            remaining_quantity -= quantity_to_remove;
            current_entry.quantity -= quantity_to_remove;
            
            const int nullPeriod = 0;
            goods_to_remove.Add(new InventoryEntry(current_entry.good, current_entry.quantity, current_entry.Cost,nullPeriod));   
            
        }
        return goods_to_remove;
    }

    public int TryConsumeGood(string goodName, int quantity)
    {
        //tries to consume good, returns quantity that was not consumed
        //do not call after SellGood or ConsumeForRecipe. Those already consume goods
        var inventory_entries = GetInventoryEntriesByGood(goodName);
        var remaining_quantity = quantity;
        var entries_to_remove = new List<InventoryEntry>();
        
        if (inventory_entries == null)
        {
            return quantity;
        }   
     
        foreach(var entry in inventory_entries)
        {
            if(remaining_quantity <= 0)
            {
                break;
            }
            if(entry.quantity <= remaining_quantity)
            {
                remaining_quantity -= entry.quantity;
                entries_to_remove.Add(entry);
            }
            else //consume remaining quantity
            {
                entry.quantity -= remaining_quantity;
                remaining_quantity = 0;
            }
        }
        foreach(var entry in entries_to_remove)
        {
            inventoryEntries.Remove(entry);
        }
        return remaining_quantity;
    }
    
    public void ConsumeForRecipe(Recipe recipe, int quantity)
    {
        var ingredients = recipe.GetRequiredIngredientsForRecipe();
        

        foreach(var ingredient in ingredients)
        {
            var remainingQuantityToRemove = ingredient.Quantity_needed * quantity;

            var availableEntries = GetAvailableInventory()
                                    .Where(x=>x.good.GoodName == ingredient.Good.GoodName)
                                    .OrderBy(x=>x.Cost)
                                    .ToList();
                                    
            foreach(var entry in availableEntries)
            {
                if(remainingQuantityToRemove <= 0)
                    break;
                
                var quantityToRemove = Math.Min(entry.quantity,remainingQuantityToRemove);

                entry.quantity -= quantityToRemove;
                remainingQuantityToRemove -= quantityToRemove;

                if(entry.quantity == 0)
                {
                    inventoryEntries.Remove(entry);
                }
            }

            if(remainingQuantityToRemove >0)
            {
                throw new InventoryException($"Failed to consume enough {ingredient.Good.GoodName} for recipe: {recipe.RecipeName}");
            }

        }
    }
    private void Remove_goods(List<InventoryEntry> entries_to_remove)
    {
        foreach(var entry in entries_to_remove)
        {
            if(entry.quantity == 0)
            {
                var item_to_remove = inventoryEntries.Find(item=> item.good == entry.good && item.Cost == entry.Cost);
                inventoryEntries.Remove(item_to_remove);
            }
        }
    }

    public void ExpireGoods(int period)
    {
        var expired_goods = inventoryEntries.Where(item=> item.PeriodAcquired+item.good.ExpiresAfterPeriods <= period).ToList();
        foreach(var entry in expired_goods)
        {
            inventoryEntries.Remove(entry);
        }
    }

    public void ResolveDeliveries()
    {
        foreach(var entry in GetInventoryEntries())
        {
            if(entry.RemainingDelay>0) entry.RemainingDelay--;
        }
    }
}