using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Market : ScriptableObject,iCompany
{
    //Fields to get around Unity's limitation of not having automatic backing properties.
    [SerializeField]private string _company_name;

    public string company_name
    {
        get => _company_name;
        set => _company_name = value;
    }

    //Intrinsic members
    private float cash = 0;
    private readonly Inventory inventory = new();

    public CompanyLevelEnum company_level;

    //Market-specific members
    internal readonly List<MarketTrade> marketTradesInPeriod= new();
    private readonly List<iPriceModifier> price_modifiers= new();

    //demand_data represents the base demand for each good
    //outside of that demanded by companies
    //It is used to 'seed' the market with an initial demand that will
    //then be affected by market forces
    public Dictionary<Good, DemandData> demand_data = new();

    public void Initialize(string company_name, CompanyLevelEnum company_level)
    {
        this.company_name = company_name;
        this.company_level = company_level;
        //setup
        Set_Initial_Cash();
        price_modifiers.Add(new SupplyDemandModifier());
        //Initialize demand data
        //If no demand data is passed, demand defaults to 1000 units of Lemonade
        //This is a placeholder and will be replaced with a more sophisticated system
        InitializeDemand(Good.CreateInstance("Lemonade", new Price_band(8f, 13f), Rarity_enum.Uncommon), 1000);
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
    public int GetTotalBought(int tradingPeriod, Good good)//Currently public for testing purposes
    {
        var total_bought = 
            marketTradesInPeriod
            .Where(x => x.Period == tradingPeriod
                        && x.InventoryEntry.good.good_name == good.good_name
                        && x.TradeType == TradeType.Buy)
            .Sum(x => x.InventoryEntry.quantity);
        return total_bought;
    }

    public int GetTotalSold(int tradingPeriod, Good good) //currently public for testing purposes
    {
        var total_sold=marketTradesInPeriod
            .Where(x => x.Period == tradingPeriod
                        && x.InventoryEntry.good.good_name == good.good_name
                        && x.TradeType == TradeType.Sell)
            .Sum(x => x.InventoryEntry.quantity);
        return total_sold;
    }

#region Supply
    public int GetTotalSupply(int tradingPeriod, Good good)
    {
        //Currently, supply is the total bought
        //This will change when we introduce production
        //And other things that increase supply, like substitute goods, imports, etc.
        return GetTotalBought(tradingPeriod, good);
    }
#endregion
#region Demand

    public void InitializeDemand(Good good, int InitialDemand,int MinDemand=0, int MaxDemand=1000000)
    {
        var demandData = new DemandData
                            { 
                                CurrentDemand = InitialDemand,
                                FulfilmentRate = 0f,
                                MinDemand = MinDemand,
                                MaxDemand = MaxDemand
                            };
        demand_data.Add(good, demandData);
    }

    public void CalculateFulfillmentRates(int tradingPeriod=-1)
    {
        if (tradingPeriod == -1)//-1 is a sentinel value meaning no parameter was passed
        {
            //if no parameter was passed, always look at the previous trading period
            tradingPeriod = TheEconomy.Instance.tradingPeriod-1;
        }
        
        foreach(var good in demand_data.Keys)
        {
            var demanded_quantity = demand_data[good].CurrentDemand;
            var supplied_quantity = GetTotalBought(tradingPeriod, good);
            var FulfilmentRate = (float)supplied_quantity/demanded_quantity;
            demand_data[good].FulfilmentRate = FulfilmentRate;
        }
    }

    public void AdjustDemand()
    {
        foreach(var good in demand_data.Keys)
        {
            
            var demandData = demand_data[good];
            var elasticity = good.DemandElasticity;
            
            //Calculate adjustment factor
            var adjustment_factor = 1f;

            //increase demand if fulfilment rate is 60% or lower
            if(demandData.FulfilmentRate <= .6f)
            {
                adjustment_factor += (1f- demandData.FulfilmentRate) * elasticity;  
            }
            //make adjustment_factor equal elasticity if fulfilment rate is 60 to 95%
            else if(demandData.FulfilmentRate > .6f && demandData.FulfilmentRate < .95f)
            {
                adjustment_factor = elasticity;
            }
            //decrease demand if fulfilment rate is 95% or higher
            else if(demandData.FulfilmentRate >= .95f)
            {
                adjustment_factor -= .1f * elasticity;
            }

            demandData.CurrentDemand = Mathf.Clamp(
                Mathf.RoundToInt(demandData.CurrentDemand * adjustment_factor)
                                ,demandData.MinDemand
                                ,demandData.MaxDemand
                                );
        }
    }
    #endregion
}
public class DemandData
{
    public int CurrentDemand{get; set;}
    public float FulfilmentRate{get; set;}
    public float DemandElasticity{get; set;}
    public int MinDemand{get; set;}
    public int MaxDemand{get; set;}
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