using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Transactions;
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
    private decimal cash = 0;
    private readonly Inventory _inventory = new();
    private readonly List<Recipe> _recipes = new();

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

    public DemandData GetDemandFor(Good good) => _demandManager.GetDemandFor(good);
    public IEnumerable<(Good good, decimal Bid, decimal Ask)> GetBidAskSpreadsFromMarket() => _demandManager.GetBidAskSpreadsFromMarket();

    public decimal GetPerceivedCostOfGood(Good good) => _demandManager.GetPerceivedCostOfGood(good);
    internal LemonadeStandResultObject UpdateFulfillmentRates(int tradingPeriod = -1) => _demandManager.UpdateFulfillmentRates(tradingPeriod);

    public LemonadeStandResultObject GetEffectiveElasticityForGood(Good good, ElasticityTypeEnum elasticity)
    {
        if (!good.Elasticities.TryGetValue(elasticity, out float elasticityValue))
        {
            return LemonadeStandResultObject.Failure(ResultTypeEnum.ElasticityNotFound, "");
        }

        return LemonadeStandResultObject.Success(extraData: elasticityValue);
    }
    public int GetMarketDemandForGood(string goodName) => _demandManager.GetMarketDemandForGood(goodName);
    public void SetMarketDemandForGood(Good good, DemandData demandData) => _demandManager.SetMarketDemandForGood(good, demandData);

    public void InitializeDemandForSpecificGood(Good good, int initialDemand, int minDemand = iDemandStrategy.MinDemand, int maxDemand = iDemandStrategy.MaxDemand, float curvature = 1f)
        => _demandManager.InitializeDemandForSpecificGood(good, initialDemand, minDemand, maxDemand, curvature);
    #endregion

   

    //Companies
    private readonly List<EconAgent> _marketParticipants = new();
    public List<EconAgent> GetMarketParticipants() => _marketParticipants;
    public void RegisterMarketParticipant(EconAgent marketParticipant)
    {
        if (!_marketParticipants.Contains(marketParticipant))
        {
            _marketParticipants.Add(marketParticipant);
            marketParticipant.SetMarket(this);
        }
        else
        {
            throw new TheEconomy_CompanyException($"Company {marketParticipant.Name} of type {marketParticipant.GetType()} already in Market {MarketId}");
        }
        TheEconomy.Instance.RegisterCompany(marketParticipant);
    }
    public LemonadeStandResultObject RemoveMarketParticipant(EconAgent company)
    {
        if (_marketParticipants.Contains(company))
        {
            _marketParticipants.Remove(company);
            company.LeaveMarket();
            return LemonadeStandResultObject.Success();
        }
        return LemonadeStandResultObject.Failure(ResultTypeEnum.CompanyNotFound, $"Company {company.Name} not found in Market {Name}");
    }

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

    //Event Handlers
    public delegate void OrderFulfillmentHandler(OrderFulfilledEvent orderFulfilledEvent);
    public event OrderFulfillmentHandler OrderFulfilled;
    public void RaiseOrderFulfilledEvent(OrderFulfilledEvent orderFulfilledEvent)
    {
        OrderFulfilled?.Invoke(orderFulfilledEvent);
    }

    //Goals
    public List<Goal> Goals { get; set; }
    private iStrategy _marketStrategy;
    public iStrategy GetStrategy() => _marketStrategy;
    public void SetStrategy(iStrategy strategy) => _marketStrategy = strategy;

#region Market Events
    public List<(iMarketEvent Event, int PeriodStart, int duration)> GetActiveMarketEvents() => _marketEventManager.GetActiveMarketEvents();
    public List<(iMarketEvent Event, int PeriodStart, int periodEnd)> GetMarketEventHistory()=> _marketEventManager.GetMarketEventHistory();
    public void AddPotentialMarketEvent(iMarketEvent marketEvent) =>
        _marketEventManager.AddPotentialMarketEvent(marketEvent);
    
    public void RemovePotentialMarketEvent(iMarketEvent marketEvent) =>
        _marketEventManager.RemovePotentialMarketEvent(marketEvent);
    
    public void ResolveMarketEvents() => _marketEventManager.ResolveMarketEvents();
    public void RollForEvents() =>_marketEventManager.RollForEvents();
#endregion
    //Pricing  
    public List<FixedCost> FixedCosts { get; set; }
    public iFixedCostStrategy FixedCostStrategy { get; set; }
    private readonly List<iPriceModifier> _priceModifiers = new();
    public void AddPriceModifier(iPriceModifier priceModifier)
    {
        if (!_priceModifiers.Contains(priceModifier))
        {
            _priceModifiers.Add(priceModifier);
        }
    }

    //Reporting 
    readonly List<(Order Order, int Period)> OrdersSubmittedInPeriod = new(); // Read only used to get Order History. 
    public List<Order> GetOrdersSubmittedInPeriod(int period)
    {
        var ordersToReturn = OrdersSubmittedInPeriod
                            .Where(x => x.Period == period)
                            .Select(x => x.Order)
                            .ToList();
        return ordersToReturn;
    }

    public void LogOrder(Order order, int period)
    {
        if (!OrdersSubmittedInPeriod.Contains((order, period)))
        {
            OrdersSubmittedInPeriod.Add((order, period));
        }
    }
   
    //Trading
    private readonly List<Execution> _executedTradesInPeriod = new();
    private readonly List<(Order Order, int Period)> _ordersExecutedInPeriod = new();
    public List<(Order Order, int Period)> GetOrdersExecutedInPeriod(params int[] periods) => _ordersExecutedInPeriod.Where(x => periods.Contains(x.Period)).ToList();
    public List<Execution> GetExecutionsInPeriod(int period)
    {
        var executions = _ordersExecutedInPeriod
                          .Where(x => x.Period == period)
                          .SelectMany(x => x.Order.GetExecutions()).ToList();
        return executions;
    }

    #region Convenience Methods
    public decimal GetCash() => cash;
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

    public LemonadeStandResultObject RecordOrderInPeriod(Order order, int period)
    {
        if (!_ordersExecutedInPeriod.Contains((order, period)))
        {
            _ordersExecutedInPeriod.Add((order, period));
            return LemonadeStandResultObject.Success();
        }
        return LemonadeStandResultObject.Failure(ResultTypeEnum.DuplicateOrder, "Order already recorded");
    }
    public void RecordTrade(Execution trade)
    {
        if (!_executedTradesInPeriod.Contains(trade)) _executedTradesInPeriod.Add(trade);
    }
    public void SetCash(decimal new_cash) => cash = new_cash;
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
    private iPriceManager _priceManager;
    public iPriceManager PriceManager{ get=>_priceManager; }
    public void SetPriceManager(iPriceManager priceManager) => SetAndWire(ref _priceManager ,priceManager);
    private iSupplyProvider _supplyProvider;
    public void SetSupplyProvider(iSupplyProvider supplyProvider) => SetAndWire(ref _supplyProvider, supplyProvider);
    private iTradeProcessor _tradeProcessor;
    public iTradeProcessor TradeProcessor{ get => _tradeProcessor; }
    public void SetTradeProcessor(iTradeProcessor tradeProcessor) => SetAndWire(ref _tradeProcessor, tradeProcessor);
    private iTransactionManager _transactionManager;
    public iTransactionManager TransactionManager { get => _transactionManager; }
    public void SetTransactionManager(iTransactionManager transactionManager) => _transactionManager = transactionManager;
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
    #region Company Interactions
    public void EvaluateParticipantCollapse(EconAgent company)
    {
        if (company.IsBankrupt())
        {
            TheEconomy.Instance.HandleParticipantCollapse(this, company);
        }
        if (company is PopulationAgent populationCompany)
        {
            if (populationCompany.IsMaxEnnui())
            {
                TheEconomy.Instance.HandleParticipantCollapse(this, company);
            }
        }
    }
    public void ProcessOrder(ActionContext context)
    {
        _transactionManager.ProcessTransaction(context);
    }
    public LemonadeStandResultObject ProcessMarketOrder(ActionContext context)
    {
        return _transactionManager.ProcessMarketTransaction(context);
    }
    public List<Order> ProcessCompanyOrders()
    {
        var CompanyOrdersExecuted = _tradeProcessor.ProcessCompanyOrders();
        return CompanyOrdersExecuted;
    }
    public LemonadeStandResultObject QueueOrder(ActionContext context)
    {
        var contextValidationResult = context.ContainsValidTrade();
        if (!contextValidationResult.Equals(LemonadeStandResultObject.Success()))
            return context.ContainsValidTrade();

        var queueResult = _tradeProcessor.QueueOrder(context);
        if (!queueResult.Equals(LemonadeStandResultObject.Success()))
            return queueResult;
        //Record the order
        LogOrder(context.TradeToSubmit, context.Period);

        return LemonadeStandResultObject.Success();
    }

    internal void UpdateCompanyStatuses(int period)
    {
        var participantCopyforIteration = _marketParticipants.ToList();
        foreach (var company in participantCopyforIteration)
        {
            //Update Company Statuses
            company.ExpireGoods(period);
            company.SubtractFixedCostsForPeriod(period);
            company.UpdateCurrentPeriod(period + 1);
            EvaluateParticipantCollapse(company);
        }
    }
    #endregion
    #region Consumption and Demand

    public Dictionary<Good, DemandData> GetDemandForPeriod()
    {
        return DemandStrategy.GetDemandInPeriod(this, CurrentPeriod);
    }
    internal void ConsumeGoods()
    {
        foreach (var participant in _marketParticipants.OfType<PopulationAgent>())
        {
            participant.Consume();
        }
    }
    public void ExpireGoods(int period)
    {
        foreach (var company in _marketParticipants)
            company.GetInventory().ExpireGoods(period);
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
    #region Goals and strategies
    public void CompleteGoal(Goal goal)
    {
        throw new NotImplementedException();
    }

    public void CheckCompanyGoals()
    {
        throw new NotImplementedException();
    }
    #endregion
    #region History
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
    #endregion
    #region Time 
    public void UpdateCurrentPeriod(int period)
    {
        CurrentPeriod = period;
    }
    #endregion
    #region Buy and sell 
    public int GetTotalBoughtByMarket(int tradingPeriod, Good good)//Currently public for testing purposes
    {
        return DemandStrategy.GetTotalBoughtByPopulation(this, good, tradingPeriod);
    }
    public int GetTotalSoldByMarket(int tradingPeriod, Good good) //currently public for testing purposes
    {
        return DemandStrategy.GetTotalSoldByMarket(this, tradingPeriod, good);
    }
    #endregion
    /// <summary>
    /// A Market order is an order initiated by the Market
    /// Use this in order to buy produced goods,
    /// Have the 'Population' buy goods from the market
    /// etc.
    /// </summary>
    /// <param name="context"></param>
    public LemonadeStandResultObject QueueMarketOrder(ActionContext context)
    {
        context.TradeToSubmit.SubmittingCompany = this;
        var queueResult = QueueOrder(context);
        if (!queueResult.Equals(LemonadeStandResultObject.Success()))
            return queueResult;
        return LemonadeStandResultObject.Success();
    }
    #region Pricing
    internal void UpdatePrices()
    {
        _priceManager.UpdatePricesForMarket();
    }
    public void CalculateNewBidAskSpreadForMarket()
    {
        //remember to call CalculateNewBidAskSpreadForMarket 
        //as part of TheEconomy.Instance.ExecuteDailyTrades.
        //eventually
        _priceManager.CalculateNewBidAskSpreadForMarket();
    }
    public decimal GetMarketCostForGood(Market market, Good good)
    {
        return _priceManager.GetMarketCostForGood(good);
    }
    public Dictionary<Good, decimal> GetAverageMarketPrices()
    {
        var averagePrices = MarketData
                            .GroupBy(x => x.Good)
                            .ToDictionary(
                                group => group.Key,
                                group => group.Average(x => x.Ask)
                            );
        return averagePrices;
    }
    #endregion
    #region Publishing
    public List<MarketData> PublishMarketData()
    {
        return _marketDataManager.PublishMarketData();
    }

    public void PublishSpreadToMarket(ActionContext context)
    {
        _marketDataManager.PublishSpreadToMarket(context);
    }
    #endregion
    #region Supply
    public List<(Good Good, int Quantity, decimal Price)> GetSupplyInPeriod(int period)
    {
        return _supplyProvider.GetSupplyInPeriod(period);
    }

    #endregion

    #region Interacting with the Economy

    public void StartTradingPeriod()
    {
        _demographicManager.RecordDemographicSnapshot(MarketId, CurrentPeriod, TurnPhase.Beginning);
        RollForEvents();
        ResolveMarketEvents();
        //Local Agents
        LocalAgentsAct(CurrentPeriod);
    }

    public void UnleashMarketForces(int period)
    {
        UpdateFulfillmentRates(period);
        UpdatePrices();
        DemandStrategy.AdjustDemandInPeriod(this);
        ConsumeGoods();
        UpdateCompanyStatuses(period);
        RecordDemographicSnapshot(TurnPhase.End);
        CurrentPeriod++;
    }

    private void LocalAgentsAct(int period)
    {
        var marketParticipants = _marketParticipants
                                .OfType<PopulationAgent>()
                                .ToList();

        if (marketParticipants.Count() == 0)
        {
            Debug.LogWarning($"No local agents found in Market {Name}");
        }
        foreach (var participant in marketParticipants)
        {
            participant.PerformStrategy(period);
        }
    }
    #endregion
    #region Overrides
    public override string ToString()
    {
        return Name;
    }
    public override bool Equals(object obj)
    {
        if (obj is Market market)
        {
            return market.Name == Name;
        }
        return false;
    }
    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
    #endregion
    #region NotImplementeds
    /// Here lie notImplementeds that serve as arguments
    /// For markets not to be iCompanies

    public decimal GetAggressionLevel()
    {
        throw new NotImplementedException();
    }
    public void SetAggressionLevel(decimal aggressionLevel)
    {
        throw new NotImplementedException();
    }
    #endregion
}
