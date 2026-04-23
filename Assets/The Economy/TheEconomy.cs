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
                    _instance = FindFirstObjectByType<TheEconomy>();
                    if(_instance == null)
                    {
                        var economyObject = new GameObject("Lemonade Stand: Economy Episode 1");
                        _instance = economyObject.AddComponent<TheEconomy>();
                    }
                }
                return _instance;
            }
    }
    public int tradingPeriod = 0;

    //These are the goods, but not the inventory items, that will exist in the market when initialized
    public List<Good> goods = new();
    
    internal ITradeLogger _trade_logger;

    public Dictionary<Guid,List<Execution>> GetAllTransactions(int period) 
    {
        var markets = EconomicAgents.OfType<Market>().ToList();
        return _trade_logger?.GetAllTransactions(markets, period);
    }
    
    public List<iEconAgent> EconomicAgents = new();

    //The Initial Market is an EconAgent that is always present in the Economy.
    //It contains the initial goods that are available in the Economy
    //Might be time to refactor this out, since there are more robust ways to create an initial market.
    private Market InitialMarket;

    public void Initialize(ITradeLogger trade_logger)
    {
        if(_trade_logger != null)
        {
            Debug.LogWarning("The Economy is already initialized");
            return;
        }

        _trade_logger = trade_logger;
        CreateInitialMarket();
        CreateInitialGoods(goods);
        Debug.Log("Lemonade Stand Economy initialized successfully.");
    }

    public Market GetMarketByName(string marketName)
    {
        return EconomicAgents.OfType<Market>().FirstOrDefault(x => x.Name == marketName);
    }

    public void RemoveMarket(Market market)
    {
        market.OnMarketEventFired -= HandleMarketEvent;
        EconomicAgents.Remove(market);
    }
    public void ClearEconomy()
    {
        EconomicAgents.Clear();
        goods.Clear();
    }

    private void CreateInitialMarket()
    {
        InitialMarket = Market.Factory.CreateStarterMarket(
                            "The First Market", 
                            ScriptableObject.CreateInstance<LinearDemandStrategy>());
        RegisterEconomicAgent(InitialMarket);
    }
    public void StartTradingPeriod()
    {
        var markets = EconomicAgents.OfType<Market>().ToList();
        foreach (var market in markets)
        {
            market.StartTradingPeriod();
        }
    }
    public void EndTradingPeriod()
    {
        var executedTrades = new List<Order>();
        //Update prices
        foreach (Market market in EconomicAgents.OfType<Market>())
        {
            executedTrades = market.ProcessCompanyOrders();
            market.UnleashMarketForces(tradingPeriod);
        };

        tradingPeriod++;

        _trade_logger?.SaveDailySummary(executedTrades);
    }
    /// <summary>
    /// Note: ResolveTurn should be used from TextBasedStoryHandler.
    /// The current flow is:
    /// 1. Player acts
    /// 2. The trading day resolves
    /// The split phase is still required for the unit tests
    /// It also allows us to split the resolution into two, add interrupts, etc. in the future
    /// </summary>
    public void ResolveTurn()
    {
        StartTradingPeriod();
        EndTradingPeriod();
    }

    public void RegisterEconomicAgent(iEconAgent agent)
    {
         if(!EconomicAgents.Any(x=>x.Name == agent.Name))
        {
            EconomicAgents.Add(agent);
            if(agent is Market market)
            {
                market.OnMarketEventFired += HandleMarketEvent;
            }
        }
        else
        {
            Debug.LogWarning ($"{agent.Name} already registered");
        }
    }
#region Events
    public event Action<Market,ActiveMarketEvent> OnMarketEventFired;
    private void HandleMarketEvent(Market market, ActiveMarketEvent e)
    {
        OnMarketEventFired?.Invoke(market,e);
    }
#endregion

    public iEconAgent GetGlobalMarket() => InitialMarket;
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
                RarityEnum.Common => common_range,
                RarityEnum.Uncommon => uncommon_range,
                RarityEnum.Rare => rare_range,
                RarityEnum.Very_Rare => very_rare_range,
                _ => throw new ArgumentOutOfRangeException()
            };
            InitialMarket.GetInventory().AddGood(new InventoryEntry(good, quantity, good.GetPrice(), tradingPeriod));
        //in the future, have a concept of rarity driving the initial price
        }
    }
    public void HandleParticipantCollapse(Market market,EconAgent collapsedEntity)
    {
        Debug.Log($"{collapsedEntity.Name} in {market.Name} has collapsed");
        ShowCollapseSummary(collapsedEntity);
        market.RemoveMarketParticipant(collapsedEntity);
        if (collapsedEntity.IsPlayer)
        {
            EndGame();
        }
    }
    public void HandleMarketFailure(Market market)
    {
        throw new NotImplementedException();
    }
    public void ShowCollapseSummary(EconAgent bankruptAgent)
    {
        Debug.Log($"{bankruptAgent.Name} has collapsed after {tradingPeriod} trading periods");
    }

    public static void SetupForTests(ITradeLogger logger)
    {
        if (_instance != null)
        {
            DestroyImmediate(_instance.gameObject);
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
#region UnitySection
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
                        Debug.LogWarning("Duplicate Economy detected. Destroying...",this);
                        Destroy(gameObject);
                    }
            }
     }
    public void InitializeEconomyForGame()
    {
        //Refactor this at some point. No need for 'the first market'
        var testMarket = Instance.GetMarketByName("The First Market");
         if(testMarket !=null)
            {
                Instance.RemoveMarket(testMarket);
                InitialMarket = null;
            }
        LoadMarketsFromResources();
    }
    private void LoadMarketsFromResources()
    {
       

        var markets = Resources.LoadAll<Market>("Markets");
        foreach (var market in markets)
        {
            var demographicManager = new DefaultDemographicManager();
            demographicManager.SetPopulationHistoryHandler(new BasicPopulationHistoryHandler());

            market.SetDemographicManager(demographicManager);
            demographicManager.SetMarket(market);
            
            // Initialize all required managers like in CreateStarterMarket
            market.WithMarketDataManager(new DefaultMarketDataManager())
                .WithPriceManager(new DefaultPriceManager())
                .WithTradeProcessor(new DefaultTradeProcessor())
                .WithTransactionManager(new DefaultTransactionManager())
                .WithPriceModifier(new SupplyDemandModifier())
                .WithOrderFulfilledEvents()
                .EnsureDefaults();
            
            market.CurrentPeriod=0;
            market.StartingPeriod=0;

            RegisterEconomicAgent(market);
            InitialMarket = market;
            Debug.Log($"Loaded Market: {market.Name} id:{market.MarketId}");
        }
    }
    

#endregion
private void OnEnable() =>
    Debug.Log($"[EC] OnEnable id={GetInstanceID()} go={gameObject.name} scene={gameObject.scene.name}", this);

private void OnDestroy() =>
    Debug.LogError($"[EC] OnDestroy id={GetInstanceID()} go={gameObject.name} scene={gameObject.scene.name}", this);
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
