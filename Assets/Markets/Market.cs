using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class Market : ScriptableObject,iCompany,iPriceSetter
{
    //Fields to get around Unity's limitation of not having automatic backing properties.
    [SerializeField]private string _company_name;

    public string company_name
    {
        get => _company_name;
        set => _company_name = value;
    }

    //Intrinsic members
    private decimal cash = 0;
    private readonly Inventory inventory = new();

    public CompanyLevelEnum company_level;

    //Market-specific members
    internal readonly List<MarketTrade> marketTradesInPeriod= new();
    private readonly List<iPriceModifier> price_modifiers= new();
    public iDemandStrategy DemandStrategy;

    //Instantiate Markets using a factory
    private Market()
    {
    }

    public static class Factory
    {
        public static Market CreateMarket(string company_name, CompanyLevelEnum company_level,iDemandStrategy demandStrategy)
        {
            var market = ScriptableObject.CreateInstance<Market>();
            market.Initialize(company_name, company_level);
            market.DemandStrategy = demandStrategy ?? throw new ArgumentNullException("Markets must have a demand strategy");
            return market;
        }
    }

    //demand_data represents the base demand for each good
    //outside of that demanded by companies
    //It is used to 'seed' the market with an initial demand that will
    //then be affected by market forces
    public Dictionary<Good, DemandData> MarketDemand = new();

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
        InitializeDemand(Good.CreateInstance("Lemonade", new Price_band(8.0m, 13.0m), Rarity_enum.Uncommon), 1000);
    }

    public Inventory GetInventory() => inventory;
    
    public decimal Get_cash() => cash;

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

    public void SetPrice(Good good, decimal new_price)
    {
        good.Set_price(new_price);
    }

     public void UpdatePrices(){
        foreach(var entry in inventory.GetInventoryEntries())
        {   
            var new_price = CalculateNewPrice(entry.good); 
            SetPrice(entry.good,new_price); 
        }
    }

    private decimal CalculateNewPrice(Good good)
    {
        decimal price = good.GetPrice();
        foreach(var modifier in price_modifiers)
        {
            price = modifier.Apply(price,good,this);
        }
        return price;
    }

#region  buy/sell, and helpers
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
    public void BuyGood(Good good, int quantity,decimal price,int period=0)
    //Currently public for testing purposes
    //Make private or internal afterwards
    //period currently does nothing for companies, but is used in Market which implements iCompany
    {
        var money_needed = price * quantity;
        if(HasMoney(money_needed))
        {
            if(inventory.GetInventoryEntriesByGood(good.good_name).Count > 0)
            {
                var inventory_entry = inventory.GetInventoryEntriesByGood(good.good_name).First();
                inventory_entry.quantity += quantity;
                cash -= money_needed;
            }
            else
            {
                var inventory_entry = new InventoryEntry(good, quantity, price);
                inventory.Add_good_to_inventory(inventory_entry);
                cash -= money_needed;
                marketTradesInPeriod.Add(new MarketTrade(inventory_entry, period,TradeType.Buy));
            }
        }
        else
        {
            throw new Company_InsufficientFundsException("Insufficient funds to buy good");
        }
    }

    public void SellGood(Good good, int quantity, decimal price,int period=0)
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
#endregion
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
        if(MarketDemand.ContainsKey(good))
        {
            MarketDemand[good].CurrentDemand = InitialDemand;
            MarketDemand[good].MinDemand = MinDemand;
            MarketDemand[good].MaxDemand = MaxDemand;
        }
        else
        {
            var demandData = new DemandData
                            { 
                                CurrentDemand = InitialDemand,
                                FulfilmentRate = 0f,
                                MinDemand = MinDemand,
                                MaxDemand = MaxDemand
                            };
            MarketDemand.Add(good, demandData);
        }
        
    }

    public void CalculateFulfillmentRates(int tradingPeriod=-1)
    {
        if (tradingPeriod == -1)//-1 is a sentinel value meaning no parameter was passed
        {
            //if no parameter was passed, always look at the previous trading period
            tradingPeriod = TheEconomy.Instance.tradingPeriod-1;
        }
        
        foreach(var good in MarketDemand.Keys)
        {
            var demanded_quantity = MarketDemand[good].CurrentDemand;
            var supplied_quantity = GetTotalBought(tradingPeriod, good);
            var FulfilmentRate = (float)supplied_quantity/demanded_quantity;
            MarketDemand[good].FulfilmentRate = FulfilmentRate;
        }
    }
    public void ConsumeGoods()
    {
        //attempt to consume goods at current demand levels
        foreach(var good in MarketDemand.Keys)
        {
            var demanded_quantity = MarketDemand[good].CurrentDemand;
            //consume good
            var unfulfilledDemand = inventory.TryConsumeGood(good.good_name,demanded_quantity);
            // Do something with unfulfilled demand later
        }
    }
    public void AdjustDemand()
    {
        DemandStrategy.AdjustDemand(this);
    }
    public int GetDemand(string good_name)
    {
        var good = MarketDemand.Keys.FirstOrDefault(x=>x.good_name == good_name);
        return MarketDemand[good].CurrentDemand;
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