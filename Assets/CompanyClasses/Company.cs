using System;
using System.Collections.Generic;
using UnityEngine;
public class Company : ScriptableObject, iCompany
{
    //Fields to get around Unity's limitation of not having automatic backing properties.
    [SerializeField]private string _company_name;
    public string company_name
    {
        get => _company_name;
        set => _company_name = value;
    }

    private readonly float share_price;
    private readonly int shares_outstanding;
    private float cash = 0;
    private readonly Inventory inventory = new();

    public List<Recipe> Recipes{get; private set;} = new();

    public void Add_recipe(Recipe recipe)=>Recipes.Add(recipe);
    public CompanyLevelEnum company_level;

    public void Initialize (string company_name, CompanyLevelEnum company_level)
    {
        this.company_name = company_name;
        this.company_level = company_level;
        //setup
        Set_Initial_Cash();
    }

    public Inventory GetInventory()
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
            case CompanyLevelEnum.Market:
                cash = 1000000000000;
                break;
        }
    }


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
        var goods = inventory.GetInventoryEntries();
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
        }
        else
        {
            throw new Company_InventoryException("Company does not have enough of the good to sell");
        }
    }
}
#region enums
public enum CompanyLevelEnum
{
    Beginner,
    Intermediate,
    Advanced,
    Market
    }
#endregion
#region Exceptions
    [Serializable]
    public class Company_InventoryException : Exception
    {
        public Company_InventoryException(string message) : base(message)
        {
        }
    }

    [Serializable]
    public class Company_InsufficientFundsException : Exception
    {
        public Company_InsufficientFundsException(string message) : base(message)
        {
        }
    }
    #endregion