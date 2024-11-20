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
    
    #region Action Economy
    private List<AllowedAction> allowedActions = new();
    private int actionsPerCycle;
    private int actionsRemaining = 0;
    public void ResetActions() => actionsRemaining = actionsPerCycle;
    public void SetActionsPerCycle(int actions) => actionsPerCycle = actions;
    public void AddAllowedAction(AllowedAction action) => allowedActions.Add(action);

    public void SetInitialActions()
    {
        switch(companyLevel)
        {
            case CompanyLevelEnum.Beginner:
                actionsPerCycle = 3;
                break;
            case CompanyLevelEnum.Intermediate:
                actionsPerCycle = 2;
                break;
            case CompanyLevelEnum.Advanced:
                actionsPerCycle = 1;
                break;
            case CompanyLevelEnum.Market:
                actionsPerCycle = 1000;
                break;
        }
        ResetActions();
    }

    #endregion

    private readonly float share_price;
    private readonly int shares_outstanding;
    private decimal cash = 0;
    private readonly Inventory inventory = new();

    public List<Recipe> Recipes{get; private set;} = new();

    public void Add_recipe(Recipe recipe)=>Recipes.Add(recipe);
    public CompanyLevelEnum companyLevel;

    public void Initialize (string company_name, CompanyLevelEnum company_level)
    {
        this.company_name = company_name;
        this.companyLevel = company_level;
        //setup
        SetInitialCash();
        SetInitialActions();
    }

    public Inventory GetInventory()
    {
        return inventory;
    }

    public decimal Get_cash()
    {
        return cash;
    }

    private void SetInitialCash()
    {
        switch(companyLevel)
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

#region Buy/Sell and helper methods
    public void BuyGood(Good good, int quantity,decimal price,int period=0)
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

    internal bool HasMoney(decimal money_needed)
    {
       return cash >= money_needed;
    }

    internal bool HasGood(Good good, int quantity)
    {
        var goods = inventory.GetInventoryEntries();
        var good_in_inventory = goods.Find(item=> item.good.good_name == good.good_name);
        return good_in_inventory != null && good_in_inventory.quantity >= quantity;
    }

    public void SellGood(Good good, int quantity, decimal price,int period=0)
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
#endregion

    public void MakeRecipe(ActionContext context)
    {
        var recipe = context.Recipe;
        var quantity = context.QuantityToMake;
        if(!Recipes.Contains(recipe))
        {
            throw new RecipeException("Recipe for "+recipe.ToString()+" not found in company's recipe book");
        }
        try
        {
            var totalCost = recipe.GetCostPerUnit(inventory)*quantity;
            var (product, product_quantity) = recipe.Make_recipe(quantity, inventory);
            inventory.Add_good_to_inventory(new InventoryEntry(product, product_quantity, totalCost));
        }
        catch(RecipeException e)
        {
            throw new RecipeException(e.Message);
        }
    }

#region Market Actions
    public void QueueTrade(ActionContext context)
    {
        var seller = context.Seller;
        Trade trade;
        if(context.Seller == null || context.GoodToBuy == null || context.Quantity == 0 || context.Price == 0)
        {
            throw new ContextException("Seller, Good, Quantity, or Price not set in context");
        }

        if (context.IsBuy)
        {
            trade = new Trade(this, seller, context.GoodToBuy, context.Quantity, context.Price);
        }
        else
        {
            trade = new Trade(seller, this, context.GoodToBuy, context.Quantity, context.Price);
        }
        TheEconomy.Instance.Queue_Trade(trade);
    }
    public void SubmitBidAskSpreadToMarket(ActionContext context)
    {
        context.SubmittingCompany = this;
        if(context.BidToSubmit==0||context.AskToSubmit==0||context.GoodToSubmit==null||context.MarketToSubmitTo==null)
        {
            throw new ContextException("Bid, Ask, Good, or Market not set in context");
        }
        context.MarketToSubmitTo.PublishSpreadToMarket(context);
    }
#endregion
#region Overrides
    public override string ToString()
    {
        return company_name;
    }

    public override bool Equals(object other)
    {
        if(other is Company company)
        {
            return company_name == company.company_name;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return company_name.GetHashCode();
    }
#endregion
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