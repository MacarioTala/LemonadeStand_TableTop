using System.Collections.Generic;
using System.Linq;
using System;
public class Inventory
{
    private readonly List<InventoryEntry> inventory_items = new(); 

    public List<InventoryEntry> Get_inventory_items()
    {
        return inventory_items;
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

    public void Sell_goods(Good good, int quantity, float price) 
    {
        var goods_to_remove = Generate_goods_to_remove(good, quantity, price);
        remove_goods(goods_to_remove);
    } 
    public List<InventoryEntry> Generate_goods_to_remove(Good good, int quantity, float price)
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
    private void remove_goods(List<InventoryEntry> entries_to_remove)
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