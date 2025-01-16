using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Tests")]
public class TheEconomy : MonoBehaviour
{
    //The Economy is a singleton that manages the market and all companies
    private static TheEconomy _instance;
    public static TheEconomy Instance 
    {
         get
            {
                if(_instance == null)
                {
                    var economyObject = new GameObject("Lemonade Stand Economy");
                    _instance = economyObject.AddComponent<TheEconomy>();
                }
                return _instance;
            }
    }
    public int tradingPeriod = 0;

    //These are the goods, but not the inventory items, that will exist in the market when initialized
    public List<Good> goods = new();
    
    internal ITradeLogger _trade_logger;
    
    public List<iCompany> companies = new();

    //The Initial Market is a company that is always present in the market.
    //It contains the initial goods that are available in the market
    //As well as the goods sold to the market by Producers and the players
    //There will eventually be multiple markets,representing different regions

    private Market InitialMarket;

    public void Initialize(ITradeLogger trade_logger)
    {
        if(_trade_logger != null)
        {
            Debug.LogWarning("The Economy is already initialized");
            return;
        }

        _trade_logger = trade_logger;
        CreateInitialGoods(goods);
        CreateInitialMarket();
        Debug.Log("Lemonade Stand Economy initialized successfully.");
    }

    public void RemoveMarket(Market market)
    {
        companies.Remove(market);
    }
    public void ClearEconomy()
    {
        companies.Clear();
        goods.Clear();
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
        var executedTrades = new List<Order>();
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
    private void Awake()
    {
        if (Application.isPlaying)
            {
                if (_instance == null)
                    {
                        _instance = this;
                        DontDestroyOnLoad(gameObject);
                    }
                else if (_instance != this)
                    {
                        Debug.LogWarning("Duplicate Economy detected. Destroying...");
                        Destroy(gameObject);
                    }
            }
    }
    private void Update()
    {
       Debug.Log("The Economy is running");
    }

    public iCompany GetGlobalMarket() => InitialMarket;
    public void CreateInitialGoods(List<Good> goods)//move static data to DB in future
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
            InitialMarket.GetInventory().AddGood(new InventoryEntry(good, quantity, good.GetPrice(), tradingPeriod));
        //in the future, have a concept of rarity driving the initial price
        }
    }
    public void HandleBankruptcy(Market market,Company bankruptCompany)
    {
        Debug.Log($"{bankruptCompany.Name} in {market.Name} has gone bankrupt");
        ShowBankruptcySummary(bankruptCompany);
        if(bankruptCompany.IsPlayer)
        {
            EndGame();
        }
    }
    public void HandleMarketFailure(Market market)
    {
        throw new NotImplementedException();
    }
    public void ShowBankruptcySummary(Company bankruptCompany)
    {
        Debug.Log($"{bankruptCompany.Name} has gone bankrupt after {tradingPeriod} trading periods");
    }

    public static void SetupForTests(ITradeLogger logger)
    {
        if (_instance != null)
        {
            UnityEngine.Object.DestroyImmediate(_instance.gameObject);
        }

        var obj = new GameObject("TestEconomy");
        _instance = obj.AddComponent<TheEconomy>();
        _instance.Initialize(logger);
    }

    private void EndGame()
    {
        Debug.Log("Game Over");
        Time.timeScale = 0;
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