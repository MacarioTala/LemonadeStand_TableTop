using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
[assembly: InternalsVisibleTo("Tests")]
[CreateAssetMenu(fileName = "Market", menuName = "LemonadeStandAssets/Market", order = 1)]
public class Market : ScriptableObject, iEconAgent
{
    #region Fields, Properties

    [SerializeField] private string _marketId;
    public Guid MarketId
    {
        get
        {
            if (_marketId == null)
            {
                _marketId = Guid.NewGuid().ToString();

#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
            return Guid.Parse(_marketId);
        }
    }
    //Fields to get around Unity's limitation of not having automatic backing properties.
    [SerializeField] private string _companyName;
    public string Name
    {
        get => _companyName;
        set => _companyName = value;
    }
    public AgentLevelEnum company_level;
    //Cash and Inventory
    public long InitialCashInCents;
    private decimal cash = 0;
    private readonly Inventory _inventory = new();
    private readonly List<Recipe> _recipes = new();
    private int minMarketDelay=0; // minimum amount of time that goods get delivered in this market
    public int GetMinMarketDeliveryDelay()=>minMarketDelay;
    public void SetMinMarketDeliveryDelay(int delay)=> minMarketDelay = delay;

    //Graphics and Market Features
    public List<MarketFeature> MarketFeatures = new();
    public List<MarketFeature> GetMarketFeatures() => MarketFeatures;
    public (int x, int y) MarketSize = (200, 200);
    public int GetWidth() => MarketSize.x;
    public int GetHeight() => MarketSize.y;

    //Time
    public int CurrentPeriod { get; set; } = 0;
    public int StartingPeriod { get; set; }

    #endregion

    #region Demand
    public List<MarketData> MarketData { get; } = new();//bid/ask spread for companies
    public Dictionary<Good, DemandData> GetDemandForPeriod()
        => DemandStrategy.GetDemandInPeriod(this, CurrentPeriod);
    public DemandData GetDemandFor(Good good) => _demandManager.GetDemandFor(good);
    public IEnumerable<(Good good, decimal Bid, decimal Ask)> GetBidAskSpreadsFromMarket()
        => _demandManager.GetBidAskSpreadsFromMarket();
    public decimal GetPerceivedCostOfGood(Good good) 
        => _demandManager.GetPerceivedCostOfGood(good);
    internal LemonadeStandResultObject UpdateFulfillmentRates(int tradingPeriod = -1)
        => _demandManager.UpdateFulfillmentRates(tradingPeriod);

    public LemonadeStandResultObject GetEffectiveElasticityForGood(Good good, ElasticityTypeEnum elasticityType)
        => _demandManager.GetEffectiveElasticityForGood(good, elasticityType);
    public int GetMarketDemandForGood(Good good) => _demandManager.GetMarketDemandForGood(good);
     public int GetTotalBoughtByMarket(int tradingPeriod, Good good)//Currently public for testing purposes
        => DemandStrategy.GetTotalBoughtByPopulation(this, good, tradingPeriod);
    public int GetTotalSoldByMarket(int tradingPeriod, Good good) //currently public for testing purposes
        => DemandStrategy.GetTotalSoldByMarket(this, tradingPeriod, good);

    #endregion
    //Econ Agents
    private readonly List<EconAgent> _marketParticipants = new();
    public List<EconAgent> GetMarketParticipants() => _marketParticipants;
    public void RegisterMarketParticipant(EconAgent marketParticipant)
        => _marketInteractionManager.RegisterMarketParticipant(marketParticipant);
    public LemonadeStandResultObject RemoveMarketParticipant(EconAgent marketParticipant)
        => _marketInteractionManager.RemoveMarketParticipant(marketParticipant);
    
    

    #region Demographics
    public float GetMarketInstability() => _demographicManager.GetMarketInstability();
    public LemonadeStandResultObject SetMarketInstability(float newInstability)
        => _demographicManager.SetMarketInstability(newInstability);
    public float GetPopulationEnnui() => _demographicManager.GetPopulationEnnui();
    public string GetEnnuiLevel() => _demographicManager.GetEnnuiLevel();
    //Population
    public int GetPopulation() => _demographicManager.GetPopulation();
    public List<PopulationHistory> GetPopulationHistory() => _demographicManager.GetPopulationHistory(MarketId);

    public LemonadeStandResultObject SetPopulation(int newPopulation, PopulationAgent marketParticipant)
        => _demographicManager.SetPopulation(newPopulation, marketParticipant);
        
    public float GetPopulationGrowthRate() => _demographicManager.GetPopulationGrowthRate(0, CurrentPeriod);

    //Population Happiness
    public float GetPopulationHappiness() => _demographicManager.GetPopulationHappiness();
    public LemonadeStandResultObject SetPopulationHappiness(float newHappiness) =>
        _demographicManager.SetPopulationHappiness(newHappiness);

    public LemonadeStandResultObject RecordDemographicSnapshot(TurnPhase phase)
    {
        _demographicManager.RecordDemographicSnapshot(MarketId, CurrentPeriod, phase);
        return LemonadeStandResultObject.Success();
    }
    #endregion

#region EventBus and Handlers
    //Event Handlers
    //TODO: Refactor this to use the unified bus whenever there's some free time
    public delegate void OrderFulfillmentHandler(OrderFulfilledEvent orderFulfilledEvent);
    public event OrderFulfillmentHandler OrderFulfilled;
    public void RaiseOrderFulfilledEvent(OrderFulfilledEvent orderFulfilledEvent)
    {
        OrderFulfilled?.Invoke(orderFulfilledEvent);
    }
#endregion
    //Goals
    public List<Goal> Goals { get; set; }
    private iStrategy _marketStrategy;
    public iStrategy GetStrategy() => _marketStrategy;
    public void SetStrategy(iStrategy strategy) => _marketStrategy = strategy;

#region Market Events
    public List<ActiveMarketEvent> GetActiveMarketEvents() => _marketEventManager.GetActiveMarketEvents();
    public List<(MarketEventSO Event, int PeriodStart, int periodEnd)> GetMarketEventHistory()=> _marketEventManager.GetMarketEventHistory();
    public void AddPotentialMarketEvent(MarketEventSO marketEvent) =>
        _marketEventManager.AddPotentialMarketEvent(marketEvent);
    
    public void RemovePotentialMarketEvent(MarketEventSO marketEvent) =>
        _marketEventManager.RemovePotentialMarketEvent(marketEvent);
    
    public void ResolveMarketEvents() => _marketEventManager.ResolveMarketEvents();
    public event Action<Market, ActiveMarketEvent> OnMarketEventFired;
    public void FireEvent(ActiveMarketEvent e)
    {
        OnMarketEventFired?.Invoke(this,e);
    }
#endregion
    #region Pricing
    [SerializeField] List<PriceStabilityEntry> priceStability=new();
    [SerializeField] int pricesFluctuateEvery;
    public int GetPricesFluctuateEvery()=> pricesFluctuateEvery;
    public void SetPricesFluctuateEvery(int value)=>pricesFluctuateEvery=value;
    public bool ShouldPricesCycleThisPeriod()
    {   
        return  CurrentPeriod>0 &&
                pricesFluctuateEvery>0 &&
                CurrentPeriod%pricesFluctuateEvery==0;
    }
    
    public IEnumerable<PriceStabilityEntry> GetPriceStability()=>priceStability;
    public void AddPriceStabilityEntry(PriceStabilityEntry entry)=> priceStability.Add(entry);
    public void CyclePrices ()
        => _priceManager.CyclePrices();
    public List<FixedCostInstance> FixedCosts { get; set; }
    public iFixedCostStrategy FixedCostStrategy { get; set; }
    private readonly List<iPriceModifier> _priceModifiers = new();
    public List<iPriceModifier> PriceModifiers { get => _priceModifiers; }
    public void AddPriceModifier(iPriceModifier priceModifier)
    {
        if (!_priceModifiers.Contains(priceModifier))
        {
            _priceModifiers.Add(priceModifier);
        }
    }
    #endregion

    #region Convenience Methods
    public decimal GetCash() => cash;
    public void SetCash(decimal new_cash) => cash = new_cash;
    public Inventory GetInventory() => _inventory;
    public List<Order> GetOrdersSentToMarket() => _tradeProcessor.GetOrders();
    public List<Order> GetOrdersSentToMarketByCompany(EconAgent company) => _tradeProcessor?.GetOrders()?.Where(x => x.SubmittingCompany.Equals(company)).ToList()?? new List<Order>();
    public List<Recipe> GetRecipes() => _recipes;
    public Dictionary<Good, DemandData> GetPopulationDemand()
    {
        // Currently assumes that only one PopulationCompany
        // will exist in the market.
        // In the future, we'll need to know how to merge the different demands
        // for the same good across multiple market segments/populationCompanies.
        var participantDemand = _marketParticipants
               .OfType<PopulationAgent>()
               .SelectMany(x => x.GetDemand())
               .ToDictionary(x => x.Key, x => x.Value);

        return participantDemand;
    }

    public List<iPriceModifier> GetPriceModifiers() => _priceModifiers;
    #endregion

    #region Creation and Initialization
    //Instantiate Markets using a factory
    private Market()
    {
        // Intentionally blank. Do not add a constructor.
        // We will use ScriptableObject.CreateInstance<Market>() to create instances of this class.
        // Market.Create() will be used as syntactic sugar for tests.
    }
    public static Market Create()
    {
        return CreateInstance<Market>();
    }

    public static class Factory
    {
        public static readonly StarterMarketInitializer starterMarketInitializer = new();

        public static Market CreateMarket(string companyName)
        {
            var market = CreateInstance<Market>();
            market.Name = companyName;
            market.company_level = AgentLevelEnum.Market;
            if (market == null)
            {
                throw new Exception("Market could not be created");
            }
            return market.EnsureDefaults();
        }

        public static Market CreateStarterMarket(string companyName, iDemandStrategy demandStrategy)
        {
            demandStrategy ??= CreateInstance<LinearDemandStrategy>();
            var market = CreateInstance<Market>()
                    .WithDemandStrategy(demandStrategy)
                    .WithOrderFulfilledEvents()
                    .Named(companyName)
                    .EnsureDefaults()
                    .InitializedWith(starterMarketInitializer);
            market.DemographicManager.SetPopulationHistoryHandler(new BasicPopulationHistoryHandler());
            return market;
        }
    }

    #endregion

    #region Company Interactions
    public List<Order> ProcessCompanyOrders() => _tradeProcessor.ProcessCompanyOrders();
    public LemonadeStandResultObject QueueOrder(ActionContext context)
        => _marketInteractionManager.QueueOrder(context);
    public LemonadeStandResultObject QueueMarketOrder(ActionContext context)
        => _marketInteractionManager.QueueMarketOrder(context);
    internal void UpdateCompanyStatuses(int period)
        => _marketInteractionManager.UpdateCompanyStatuses(period);
    #endregion

    #region Consumption
    internal void ConsumeGoods()
    {
        foreach (var participant in _marketParticipants.OfType<PopulationAgent>())
        {
            participant.Consume();
        }
    }
    public void ExpireGoods(int period)
    {
        foreach (var participant in _marketParticipants)
            participant.ExpireGoods(period);
    }
    #endregion
    #region Fixed costs 
    public decimal CalculateFixedCostsForPeriod(int period)
    {
        if (FixedCostStrategy is not null)
        {
            return FixedCostStrategy.CalculateFixedCosts(FixedCosts, period);
        }
        else
        {
            Debug.Log("Fixed Cost Strategy not set");
            return 0;
        }
    }
    #endregion
    #region History
    readonly List<HistoricalRecord> MarketJournal=new();
    public IEnumerable<HistoricalRecord> GetHistoricalRecordsInPeriod(int? period=null)
    {
        period??=CurrentPeriod;
        return MarketJournal.Where(x=>x.CreatedInPeriod==period);
    }
    public void LogHistoricalRecord(HistoricalRecord record) => MarketJournal.Add(record);
    public float GetPopulationPercentageChangeInPeriod()
    {
        return MathHelper.GetMetricPercentageChangeInPeriod(
            _demographicManager.GetPopulationHistory,
            x => x.Population,
            CurrentPeriod,
            MarketId);
    }
    #endregion
    #region Inventory Management
    public void AddRecipe(Recipe recipe)
    {
        if (_recipes.Contains(recipe))
        {
            throw new Exception("Recipe already exists in company");
        }
        else
        {
            _recipes.Add(recipe);
        }
    }
    public void RemoveRecipe(Recipe recipe)
    {
        if(_recipes.Contains(recipe))
            _recipes.Remove(recipe);
    }
    #endregion
    #region Time 
    public void UpdateCurrentPeriod(int period)
        => CurrentPeriod = period;
    
    #endregion
    #region Pricing
    internal void UpdatePrices()
    {
        _priceManager.UpdatePricesForMarket();
        CyclePrices();
    }
    public void CalculateNewBidAskSpreadForMarket()
        // Originally, markets acted monolithically, but now various populations
        // can set their own bid/ask spreads. Is this still needed?
        // #RefactorCandidate
        =>_priceManager.CalculateNewBidAskSpreadForMarket();
    public decimal GetMarketCostForGood(Market market, Good good)
        => _priceManager.GetMarketCostForGood(good);

    public List<KnownPrice> GetAverageMarketPrices()
        => _priceManager.GetAverageMarketPrices();
    #endregion

    #region Publishing
    public List<AvailableGood> GetGoodsAvailableInPeriod(iEconAgent requester,int? period = null)
    {
        period ??= CurrentPeriod;
        var goodsAvailable = GetOrdersSubmittedInPeriod(period)
            .Where(x=>x.IsSell()&& x.IsVisibleTo(requester))
            .Select(x=> new AvailableGood(
                         x.SubmittingCompany,
                         x.Good,
                         x.Quantity,
                         x.Price))
            .ToList();
        return goodsAvailable;
    }
    public List<MarketData> PublishMarketData()
        => _marketDataManager.PublishMarketData();
    public void PublishSpreadToMarket(ActionContext context)
        => _marketDataManager.PublishSpreadToMarket(context);
    #endregion

    #region Reporting
    public LemonadeStandResultObject RecordOrderInPeriod(Order order, int period)
        => _marketDataManager.RecordOrderInPeriod(order, period);
    public void RecordExecution(Execution trade)
        => _marketDataManager.RecordExecution(trade);
    
    public List<Order> GetOrdersSubmittedInPeriod(int? period)
        => _marketDataManager.GetOrdersSubmittedInPeriod(period);
    public void LogOrder(Order order, int period) => _marketDataManager.LogOrder(order, period);
    public List<(Order Order, int Period)> GetOrdersExecutedInPeriod(params int[] periods)
        => _marketDataManager.GetOrdersExecutedInPeriod(periods);
    public List<Execution> GetExecutionsInPeriod(int period)
        => _marketDataManager.GetExecutionsInPeriod(period);
    #endregion

    #region Supply
    public List<(Good Good, int Quantity, decimal Price)> GetSupplyInPeriod(int period)
        => _supplyProvider.GetSupplyInPeriod(period);
    #endregion

    #region Interacting with the Economy

    public void StartTradingPeriod()
    {
        _demographicManager.RecordDemographicSnapshot(MarketId, CurrentPeriod, TurnPhase.Beginning);
        ResolveMarketEvents();
        //Local Agents
        _marketInteractionManager.MarketsProvideLiquidityOfLastResort();
        _marketInteractionManager.NPCsAct(CurrentPeriod);
    }

    private void ResolveDeliveries()
    {
        foreach(var marketParticipant in _marketParticipants)
        {
            marketParticipant.ResolveDeliveries();
        }
        
    }

    public void UnleashMarketForces(int period)
    {
        ProcessCompanyOrders();
        ResolveDeliveries();
        UpdateFulfillmentRates(period);
        UpdatePrices();
        ConsumeGoods();
        ExpireGoods(period);
        UpdateCompanyStatuses(period);
        RecordDemographicSnapshot(TurnPhase.End);
        CurrentPeriod++;
    }
    #endregion
    
    #region Managers
    private void SetAndWire<T>(ref T managerFieldToSet, T newValue) where T : class
    {
        managerFieldToSet = newValue;
        if (newValue is iMarketAware aware) aware.SetMarket(this);
    }
    private iDemographicManager _demographicManager;
    public iDemographicManager DemographicManager { get => _demographicManager; }
    public void SetDemographicManager(iDemographicManager demographicManager) => SetAndWire(ref _demographicManager, demographicManager);
    private iMarketEventManager _marketEventManager;
    public iMarketEventManager MarketEventManager{ get => _marketEventManager; }
    public void SetMarketEventManager(iMarketEventManager manager) => SetAndWire(ref _marketEventManager, manager);
    private iFeatureManager _featureManager;
    public void SetFeatureManager(iFeatureManager featureManager) => SetAndWire(ref _featureManager ,featureManager);
    public iMarketDataManager MarketDataManager{ get=>_marketDataManager; }
    private iMarketDataManager _marketDataManager;
    public void SetMarketDataManager(iMarketDataManager marketDataManager) => SetAndWire(ref _marketDataManager,marketDataManager);
    private iMarketInteractionManager _marketInteractionManager;
    public iMarketInteractionManager MarketInteractionManager{ get => _marketInteractionManager; }
    public void SetMarketInteractionManager(iMarketInteractionManager manager) => SetAndWire(ref _marketInteractionManager, manager);
    private iPriceManager _priceManager;
    public iPriceManager PriceManager{ get=>_priceManager; }
    public void SetPriceManager(iPriceManager priceManager) => SetAndWire(ref _priceManager ,priceManager);
    private iSupplyHelper _supplyProvider;
    public iSupplyHelper SupplyProvider { get => _supplyProvider; }
    public void SetSupplyProvider(iSupplyHelper supplyProvider) => SetAndWire(ref _supplyProvider, supplyProvider);
    private iTradeProcessor _tradeProcessor;
    public iTradeProcessor TradeProcessor{ get => _tradeProcessor; }
    public void SetTradeProcessor(iTradeProcessor tradeProcessor) => SetAndWire(ref _tradeProcessor, tradeProcessor);
    private iTransactionManager _transactionManager;
    public iTransactionManager TransactionManager { get => _transactionManager; }
    public void SetTransactionManager(iTransactionManager transactionManager) => SetAndWire(ref _transactionManager,transactionManager);
    private iMarketDataService _marketDataService;
    public void SetMarketDataService(iMarketDataService marketDataService) => _marketDataService = marketDataService;
    [SerializeField] private ScriptableObject _demandStrategy;
    public iDemandStrategy DemandStrategy { get => _demandStrategy as iDemandStrategy; }
    public void SetDemandStrategy(iDemandStrategy demandStrategy) 
        => SetAndWire(ref _demandStrategy , demandStrategy as ScriptableObject); 
    private iDemandManager _demandManager;
    public iDemandManager DemandManager{ get => _demandManager; }
    public void SetDemandManager(iDemandManager manager) => SetAndWire(ref _demandManager ,manager);
    #endregion
    #region Overrides
    public override string ToString()
        => Name;
    public override bool Equals(object obj)
    {
        if (obj is Market market)
        {
            return market.Name == Name;
        }
        return false;
    }
    public override int GetHashCode()
        => Name.GetHashCode();
    #endregion
    #region NotImplementeds
    /// Here lie notImplementeds that serve as arguments
    /// For markets not to be iCompanies
    public void CompleteGoal(Goal goal)
        => throw new NotImplementedException();
    public void CheckAgentGoals()
        => throw new NotImplementedException();
    public decimal GetAggressionLevel()
        =>throw new NotImplementedException();
    public void SetAggressionLevel(decimal aggressionLevel)
        => throw new NotImplementedException();

    #endregion
}
