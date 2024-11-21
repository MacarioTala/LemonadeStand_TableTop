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
        var items = inventory_items.Where(x => x.good.good_name == goodName)
                              .OrderBy(x => x.acquisition_price)
                              .ToList();
        if (items.Count == 0)
        {
            new List<InventoryEntry>();
        }
        return items;
    }
    public void Add_good_to_inventory(InventoryEntry entry)
    {
        var existing_good_at_price = inventory_items.Find(item=> item.good.good_name == entry.good.good_name && item.acquisition_price == entry.acquisition_price);
        if(existing_good_at_price == null)
        {
            inventory_items.Add(entry);
        }
        else
        {
            existing_good_at_price.quantity += entry.quantity;
        }

    }

    public void Sell_goods(Good good, int quantity, decimal price) 
    {
        var goods_to_remove = Generate_goods_to_remove(good, quantity, price);
        Remove_goods(goods_to_remove);
    } 
    public List<InventoryEntry> Generate_goods_to_remove(Good good, int quantity, decimal price)
    {
        var eligible_goods = inventory_items
            .Where(item=> item.good == good && item.acquisition_price <= price)
            .OrderBy(item=> item.acquisition_price)
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
            
            goods_to_remove.Add(new InventoryEntry(current_entry.good, current_entry.quantity, current_entry.acquisition_price));   
            
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
            var inventory_entry = inventory_items.Find(x=> x.good.good_name == ingredient.Good.good_name);
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
                var item_to_remove = inventory_items.Find(item=> item.good == entry.good && item.acquisition_price == entry.acquisition_price);
                inventory_items.Remove(item_to_remove);
            }
        }
    }


}