using System.Collections.Generic;
using System.Linq;
using System;
public class Inventory
{
    private readonly List<InventoryEntry> inventory_items = new(); 

    public void Clear()
    {
        inventory_items.Clear();
    }
    public List<InventoryEntry> GetInventoryEntries() => inventory_items;

    public List<InventoryEntry> GetInventoryEntriesByGood(string goodName)
    {
        var items = inventory_items.Where(x => x.good.GoodName == goodName)
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
        var existingGoodAtPriceAndExpiry = inventory_items.Find(item=> item.good.GoodName == entry.good.GoodName 
                                            && item.Cost == entry.Cost
                                            && item.PeriodAcquired == entry.PeriodAcquired
                                            );
        if(existingGoodAtPriceAndExpiry == null)
        {
            inventory_items.Add(entry);
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
        var eligible_goods = inventory_items
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
            inventory_items.Remove(entry);
        }
        return remaining_quantity;
    }
    
    public void Consume_for_recipe(Recipe recipe, int quantity)
    {
        //Assumes quantity that is passed in is valid. Maybe need a token system to check if quantity is valid
        var ingredients = recipe.Get_recipe();
        foreach(var ingredient in ingredients)
        {
            var quantity_to_remove = ingredient.Quantity_needed * quantity;
            var inventory_entry = inventory_items.Find(x=> x.good.GoodName == ingredient.Good.GoodName);
            inventory_entry.quantity -= quantity_to_remove;

            if(inventory_entry.quantity == 0)
            {
                inventory_items.Remove(inventory_entry);
            }

        }
    }
    private void Remove_goods(List<InventoryEntry> entries_to_remove)
    {
        foreach(var entry in entries_to_remove)
        {
            if(entry.quantity == 0)
            {
                var item_to_remove = inventory_items.Find(item=> item.good == entry.good && item.Cost == entry.Cost);
                inventory_items.Remove(item_to_remove);
            }
        }
    }

    public void ExpireGoods(int period)
    {
        var expired_goods = inventory_items.Where(item=> item.PeriodAcquired+item.good.ExpiresAfterPeriods <= period).ToList();
        foreach(var entry in expired_goods)
        {
            inventory_items.Remove(entry);
        }
    }

}