using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Market : ScriptableObject,iCompany
{
    //Intrinsic members
    public string company_name;

    private float cash = 0;
    private readonly Inventory inventory = new();

    public CompanyLevelEnum company_level;

    //Market-specific members
    internal readonly List<MarketTrade> marketTradesInPeriod= new();
    private readonly List<iPriceModifier> price_modifiers= new();

    public void Initialize (string company_name, CompanyLevelEnum company_level)
    {
        this.company_name = company_name;
        this.company_level = company_level;
        //setup
        Set_Initial_Cash();
        price_modifiers.Add(new SupplyDemandModifier());
    }

    public Inventory Get_inventory()
    {
        return inventory;
    }

    public float Get_cash()
    {
        return cash;
    }

    private void Set_Initial_Cash()
    {
        switch(company_level)
        {
            case CompanyLevelEnum.Beginner:
                cash = 10000;
                break;
            case CompanyLevelEnum.Intermediate:
                cash = 5000;
                break;
            case CompanyLevelEnum.Advanced:
                cash = 1000;
                break;  
            case CompanyLevelEnum.Global:
                cash = 1000000000000;
                break;
        }
    }

     public void UpdatePrices(){
        foreach(var entry in inventory.Get_inventory_items())
        {   
            var new_price = CalculateNewPrice(entry.good); 
            entry.acquisition_price = new_price;
            
        }
    }

    private float CalculateNewPrice(Good good)
    {
        float price = good.Get_price();
        foreach(var modifier in price_modifiers)
        {
            price = modifier.Apply(price,good,this);
        }
        return price;
    }

#region  buy/sell, and helpers
//Note: these are currently duplicated in Company.cs. Find a way to refactor
    public void BuyGood(Good good, int quantity,float price,int period=0)
    //Currently public for testing purposes
    //Make private or internal afterwards
    //period currently does nothing for companies, but is used in Market which implements iCompany
    {
        var money_needed = price * quantity;
        if(HasMoney(money_needed))
        {
            var inventory_entry = new InventoryEntry(good, quantity, price);
            inventory.Add_good_to_inventory(inventory_entry);
            cash -= money_needed;
            marketTradesInPeriod.Add(new MarketTrade(inventory_entry, period,TradeType.Buy));
        }
        else
        {
            throw new Company_InsufficientFundsException("Insufficient funds to buy good");
        }
    }

    internal bool HasMoney(float money_needed)
    {
       return cash >= money_needed;
    }

    internal bool HasGood(Good good, int quantity)
    {
        var goods = inventory.Get_inventory_items();
        var good_in_inventory = goods.Find(item=> item.good.good_name == good.good_name);
        return good_in_inventory != null && good_in_inventory.quantity >= quantity;
    }

    public void SellGood(Good good, int quantity, float price,int period=0)
    {
        //period currently does nothing for companies, but is used in Market which implements iCompany
        if(HasGood(good, quantity))
        {
            
            inventory.Sell_goods(good, quantity, price);
            cash += price * quantity;
            marketTradesInPeriod.Add(new MarketTrade(new InventoryEntry(good, quantity, price), period,TradeType.Sell));
        }
        else
        {
            throw new Company_InventoryException("Company does not have enough of the good to sell");
        }
    }
#endregion
    internal int GetTotalBought(int tradingPeriod, Good good)
    {
        return marketTradesInPeriod
            .Where(x => x.Period == tradingPeriod
                        && x.InventoryEntry.good.good_name == good.good_name
                        && x.TradeType == TradeType.Buy)
            .Sum(x => x.InventoryEntry.quantity);

    }

    internal int GetTotalSold(int tradingPeriod, Good good)
    {
        return marketTradesInPeriod
            .Where(x => x.Period == tradingPeriod
                        && x.InventoryEntry.good.good_name == good.good_name
                        && x.TradeType == TradeType.Sell)
            .Sum(x => x.InventoryEntry.quantity);
    }
}

public enum TradeType
{
    Buy,
    Sell
}
public class MarketTrade
{
    public InventoryEntry InventoryEntry{get; private set;}
    public int Period{get; private set;}

    public TradeType TradeType{get; private set;}

    public MarketTrade(InventoryEntry inventoryEntry, int period,TradeType tradeType)
    {
        InventoryEntry = inventoryEntry;
        Period = period;
        TradeType = tradeType;
    }
}