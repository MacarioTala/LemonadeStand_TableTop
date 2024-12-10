using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Tests")]
public class TheEconomy : MonoBehaviour
{
    //The Economy is a singleton that manages the market and all companies
    public static TheEconomy Instance { get; private set; }
    public int tradingPeriod = 0;

    //These are the goods, but not the inventory items, that will exist in the market when initialized
    public List<Good> goods = new();
    
    internal readonly List<Trade> tradeQueue = new();
    internal ITradeLogger _trade_logger;
    
    public List<iCompany> companies = new();

    //The Initial Market is a company that is always present in the market.
    //It contains the initial goods that are available in the market
    //As well as the goods sold to the market by Producers and the players
    //There will eventually be multiple markets,representing different regions

    private Market InitialMarket;

    public void Initialize(ITradeLogger trade_logger)
    {
        //Create the instance
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        Create_initial_goods(goods);
        _trade_logger = trade_logger;
        CreateInitialMarket();
    }

    public void RemoveMarket(Market market)
    {
        companies.Remove(market);
    }
    public void ClearEconomy()
    {
        companies.Clear();
        goods.Clear();
        tradeQueue.Clear();
    }

    private void CreateInitialMarket()
    {
        InitialMarket = Market.Factory.CreateStarterMarket(
                            "The First Market", 
                            CompanyLevelEnum.Market, 
                            new LinearDemandStrategy());
        RegisterCompany(InitialMarket);
    }

    public void EndTradingPeriod()
    {
        var executedTrades = new List<Trade>();
        //Update prices
        foreach (Market market in companies.OfType<Market>())
        {
            executedTrades = market.ProcessCompanyOrders();
            market.UnleashMarketForces(tradingPeriod);
        };

        tradingPeriod++;

        _trade_logger?.SaveDailySummary(executedTrades);
    }

    public void RegisterCompany(iCompany company)
    {
        if(!companies.Any(x=>x.Name == company.Name))
        {
            companies.Add(company);
        }
        else
        {
            throw new TheEconomy_CompanyException("Company already registered");
        }
        
    }
    private void Update()
    {
       throw new NotImplementedException();
    }

    public iCompany GetGlobalMarket() => InitialMarket;
    public void Create_initial_goods(List<Good> goods)//move static data to DB in future
    {
        //Limits for good quantities
        var common_range = UnityEngine.Random.Range(1, 1001);
        var uncommon_range = UnityEngine.Random.Range(1, 501);
        var rare_range = UnityEngine.Random.Range(1, 101);
        var very_rare_range = UnityEngine.Random.Range(1, 11);

        //create goods
        foreach(Good good in goods)
        {
            //Generate quantity based on rarity
            int quantity = good.GetRarity() switch
            {
                Rarity_enum.Common => common_range,
                Rarity_enum.Uncommon => uncommon_range,
                Rarity_enum.Rare => rare_range,
                Rarity_enum.Very_Rare => very_rare_range,
                _ => throw new ArgumentOutOfRangeException()
            };
            InitialMarket.BuyGood(good, quantity, good.GetPrice());
        //in the future, have a concept of rarity driving the initial price
        }
    }
}


#region Exceptions
[Serializable]
public class TheEconomy_CompanyException : Exception
{
    public TheEconomy_CompanyException(string message) : base(message)
    {
    }

}
#endregion