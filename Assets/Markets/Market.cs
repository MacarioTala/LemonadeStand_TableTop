using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
[assembly:InternalsVisibleTo("Tests")]
[CreateAssetMenu(fileName = "Market", menuName = "LemonadeStandAssets/Market", order = 1)]
public class Market : ScriptableObject, iCompany
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
    public List<MarketFeature> MarketFeatures = new();
    public List<MarketFeature> GetMarketFeatures() => MarketFeatures;
    public (int x,int y) MarketSize = (200,200);
    public int GetWidth() => MarketSize.x;
    public int GetHeight() => MarketSize.y;
    //Fields to get around Unity's limitation of not having automatic backing properties.
    [SerializeField] private string _companyName;
    public string Name
    {
        get => _companyName;
        set => _companyName = value;
    }
    public CompanyLevelEnum company_level;
    //Demand
    public List<MarketData> MarketData = new();//bid/ask spread for companies
    private readonly Dictionary<Good, DemandData> _marketDemand = new();

    //Cash and Inventory
    private decimal cash = 0;
    private readonly Inventory _inventory = new();
    private readonly List<Recipe> _recipes = new();

    //Companies
    public List<Company> CompaniesInThisMarket = new();
    public LemonadeStandResultObject RemoveCompany(Company company)
    {
        if(CompaniesInThisMarket.Contains(company))
        {
            CompaniesInThisMarket.Remove(company);
            return LemonadeStandResultObject.Success();
        }
        return LemonadeStandResultObject.Failure(ResultTypeEnum.CompanyNotFound, $"Company {company.Name} not found in Market {Name}");
    }
#endregion

#region Demographic data
    public float GetMarketInstability() => _demographicManager.GetMarketInstability();
    public LemonadeStandResultObject SetMarketInstability(float newInstability)
        =>_demographicManager.SetMarketInstability(newInstability);
    public float GetPopulationEnnui() => _demographicManager.GetPopulationEnnui();
    public string GetEnnuiLevel() => _demographicManager.GetEnnuiLevel();
    
    public int GetPopulation() => _demographicManager.GetPopulation();
    public LemonadeStandResultObject SetPopulation(int newPopulation) => _demographicManager.SetPopulation(newPopulation);
    public float GetPopulationGrowthRate()=>_demographicManager.GetPopulationGrowthRate();
    public float GetPopulationHappiness()=>_demographicManager.GetPopulationHappiness();
    public LemonadeStandResultObject SetPopulationHappiness(float newHappiness)=>
        _demographicManager.SetPopulationHappiness(newHappiness);
#endregion 
    

    //Event Handlers
    public delegate void OrderFulfillmentHandler(OrderFulfilledEvent orderFulfilledEvent);
    public event OrderFulfillmentHandler OrderFulfilled;
    public void RaiseOrderFulfilledEvent(OrderFulfilledEvent orderFulfilledEvent)
    {
        OrderFulfilled?.Invoke(orderFulfilledEvent);
    }

    //Goals
    public List<Goal> Goals {get;set;}

    [SerializeField] private ScriptableObject _demandStrategy;
    public iDemandStrategy DemandStrategy
    { get=> _demandStrategy as iDemandStrategy;
      set=> _demandStrategy = value as ScriptableObject;} 
    private iStrategy _marketStrategy;
    
    //Pricing  
    public List<FixedCost> FixedCosts { get; set; }
    public iFixedCostStrategy FixedCostStrategy {get;set;}
    private readonly List<iPriceModifier> _priceModifiers = new();
    
    //Time
    public int CurrentPeriod{get;set;}=0;
    public int StartingPeriod{get;set;}
    //Trading
    private readonly List<MarketTransaction> _marketTradesInPeriod = new();

#region Convenience Methods
    public decimal GetCash() => cash;
    public Inventory GetInventory() => _inventory;
    public List<Order>GetOrdersSentToMarket()=>_tradeProcessor.GetOrders();
    public List<Order>GetOrdersSentToMarketByCompany(Company company)=>_tradeProcessor.GetOrders().Where(x=>x.SubmittingCompany.Equals(company)).ToList();
    public List<Recipe> GetRecipes()=>_recipes;
    public Dictionary<Good,DemandData> GetMarketDemand() => _marketDemand;
    public void SetMarketDemandForGood(Good good, DemandData demandData) => _marketDemand[good] = demandData;
    public List<MarketTransaction> GetMarketTradesInPeriod(int period) => _marketTradesInPeriod.Where(x=>x.Period == period).ToList();
     
    public List<iPriceModifier> GetPriceModifiers() => _priceModifiers;
    public void RecordTrade(MarketTransaction trade) 
    {
        if(!_marketTradesInPeriod.Contains(trade))_marketTradesInPeriod.Add(trade);
    }
    public void SetCash(decimal new_cash) => cash = new_cash;
#endregion

#region Creation and Initialization
    //Instantiate Markets using a factory
    private Market ()
    {
    }
    public static class Factory
    { 
        public static readonly StarterMarketInitializer _initializer = new();
        public static Market CreateMarket(string companyName, CompanyLevelEnum companyLevel, iDemandStrategy demandStrategy)
        {
            var market = CreateInstance<Market>();
            market.DemandStrategy = demandStrategy ?? throw new ArgumentNullException("Markets must have a demand strategy");
            market.Initialize(companyName, companyLevel, null);
            return market;
        }

        public static Market CreateStarterMarket(string companyName, CompanyLevelEnum companyLevel, iDemandStrategy demandStrategy)
        {
            var market = CreateInstance<Market>();
            market.DemandStrategy = demandStrategy ?? throw new ArgumentNullException("Markets must have a demand strategy");
            market.Initialize(companyName, companyLevel, null);
            _initializer.InitializeMarket(market);
            return market;
        }
    }
    internal void Initialize (string companyName,CompanyLevelEnum companyLevel,iStrategy strategy)
    {
        Name = companyName;
        company_level = companyLevel;
        _marketStrategy = strategy;
        
        //Event Handlers
        OrderFulfilled += DemandStrategy.OnOrderFulfilled;

        //Managers
        _consumptionManager = new BasicConsumptionManager();
        _marketDataManager = new BasicMarketDataManager();
        _priceManager = new BasicPriceManager();
        _tradeProcessor = new BasicTradeProcessor();
        _transactionManager = new BasicTransactionManager();

        //Price Modifiers
        _priceModifiers.Add(new SupplyDemandModifier());
    }
#endregion
#region Managers
    private iDemographicManager _demographicManager;
    public void SetDemographicManager(iDemographicManager demographicManager) => _demographicManager = demographicManager;
    private iFeatureManager _featureManager;
    public void SetFeatureManager(iFeatureManager featureManager) => _featureManager = featureManager;
    private iConsumptionManager _consumptionManager;
    public void SetConsumptionManager(iConsumptionManager consumptionManager) => _consumptionManager = consumptionManager;
    private iMarketDataManager _marketDataManager;
    public void SetMarketDataManager(iMarketDataManager marketDataManager) => _marketDataManager = marketDataManager;
    private iPriceManager _priceManager;
    public void SetPriceManager(iPriceManager priceManager) => _priceManager = priceManager;
    private iTradeProcessor _tradeProcessor;
    public void SetTradeProcessor(iTradeProcessor tradeProcessor) => _tradeProcessor = tradeProcessor;
    private iTransactionManager _transactionManager;
    public void SetTransactionManager(iTransactionManager transactionManager) => _transactionManager = transactionManager;
    private iMarketDataService _marketDataService;
    public void SetMarketDataService(iMarketDataService marketDataService) => _marketDataService = marketDataService;

#endregion
#region Company Interactions
    public void BankruptCompany(Company company)
    {
        if(company.IsBankrupt())
        {
            TheEconomy.Instance.HandleBankruptcy(this,company);
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
        var CompanyOrdersExecuted = _tradeProcessor.ProcessCompanyOrders(this);
        return CompanyOrdersExecuted;
    }
     public LemonadeStandResultObject QueueOrder(ActionContext context)
    {
        var contextValidationResult = context.DoesContextContainValidTrade();
        if ( !contextValidationResult.Equals(LemonadeStandResultObject.Success()) )
            return context.DoesContextContainValidTrade();
        
        var queueResult = _tradeProcessor.QueueOrder(context);
        if ( !queueResult.Equals(LemonadeStandResultObject.Success()) )
            return queueResult;
        
        return LemonadeStandResultObject.Success();
    }
    public void RegisterCompany(Company company)
    {
        if(!CompaniesInThisMarket.Contains(company))
        {
            CompaniesInThisMarket.Add(company);
        }
        else
        {
            throw new TheEconomy_CompanyException("Company {company.company_name} already in Market{company_name}");
        }
        TheEconomy.Instance.RegisterCompany(company);
    }
   
    internal void UpdateCompanyStatuses(int period)
    {
        foreach (var company in CompaniesInThisMarket)
        {
            //Update Company Statuses
            company.ExpireGoods(period);
            company.SubtractFixedCostsForPeriod(period);
            company.UpdateCurrentPeriod(period+1);
            BankruptCompany(company);
        }
    }
#endregion
#region Consumption and Demand

    public Dictionary<Good,DemandData> GetDemandForPeriod()
    {
        return DemandStrategy.CalculateDemandForPeriod(this,CurrentPeriod);
    }

    public LemonadeStandResultObject FulfillDemand()
    {
        return _consumptionManager.FulfillDemand(this);
        
    }
    internal void ConsumeGoods()
        {
            //attempt to consume goods at current demand levels
            foreach(var good in _marketDemand.Keys)
            {
                var demanded_quantity = _marketDemand[good].CurrentDemand;
                //consume good
                var unfulfilledDemand = _inventory.TryConsumeGood(good.GoodName,demanded_quantity);
                // Do something with unfulfilled demand later
            }
        }
    public void ExpireGoods(int period)
        {
            foreach(var company in CompaniesInThisMarket)
                company.GetInventory().ExpireGoods(period); 
        }
#endregion
#region Fixed costs
    public decimal CalculateFixedCostsForPeriod(int period)
    {
        return FixedCostStrategy.CalculateFixedCosts(FixedCosts,period);
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
    public float GetMetricPercentageChangeInPeriod<T>
        (
            Func<Guid, IEnumerable<T>> getHistoryFunc,
            Func<T, float> getMetricValueFunc
        )
        where T : iHistorical
    {
        if(CurrentPeriod == 0) return 0;

        var history = getHistoryFunc(MarketId);

        var metricValues = history
                            .Where(x=>x.Period == CurrentPeriod
                            || x.Period == CurrentPeriod-1)
                            .ToDictionary(x=>x.Period, x=>getMetricValueFunc(x));

        var currentMetricValue = metricValues.GetValueOrDefault(CurrentPeriod,0);
        var previousMetricValue = metricValues.GetValueOrDefault(CurrentPeriod-1,0);

        var valueToReturn = (currentMetricValue - previousMetricValue)
                            /(previousMetricValue==0?1:previousMetricValue);

        return valueToReturn;
    }

    public float GetPopulationPercentageChangeInPeriod()
    {
        return GetMetricPercentageChangeInPeriod(
            _marketDataService.GetPopulationHistory,
            x=>x.Population);       
    }
#endregion
#region Inventory Management
    public void AddRecipe(Recipe recipe)
    {
        if(_recipes.Contains(recipe))
        {
            throw new System.Exception("Recipe already exists in company");
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
       return DemandStrategy.GetTotalBoughtByMarket(this,good,tradingPeriod);
    }
    public int GetTotalSoldByMarket(int tradingPeriod, Good good) //currently public for testing purposes
    {
        return DemandStrategy.GetTotalSoldByMarket(this,tradingPeriod,good);
    }
#endregion
#region Demand
     internal void CalculateFulfillmentRates(int tradingPeriod=-1)
    {
        DemandStrategy.CalculateFulfillmentRates(this,tradingPeriod);
    }

    public LemonadeStandResultObject GetEffectiveElasticityForGood(Good good, ElasticityTypeEnum elasticity)
    {
        if (!good.Elasticities.TryGetValue(elasticity, out float elasticityValue))
        {
            return LemonadeStandResultObject.Failure(ResultTypeEnum.ElasticityNotFound, "");
        }

        return LemonadeStandResultObject.Success(extraData:elasticityValue);
    }
    public int GetMarketDemandForGood(string good_name)
    {
        var good = _marketDemand.Keys.FirstOrDefault(x=>x.GoodName == good_name);
        return _marketDemand[good].CurrentDemand;
    }
    public void InitializeDemandForSpecificGood(Good good, int InitialDemand, int minDemand=iDemandStrategy.MinDemand, int maxDemand=iDemandStrategy.MaxDemand,float curvature=1f)
    {
       DemandStrategy.InitializeDemandForSpecificGood(this,good,InitialDemand,minDemand,maxDemand,curvature);
    }
    /// <summary>
    /// A Market order is an order initiated by the Market
    /// Use this in order to buy produced goods,
    /// Have the 'Population' buy goods from the market
    /// etc.
    /// </summary>
    /// <param name="context"></param>
    public LemonadeStandResultObject QueueMarketOrder(ActionContext context)
    {   context.TradeToSubmit.SubmittingCompany = this;
        var queueResult = QueueOrder(context);
        if ( !queueResult.Equals(LemonadeStandResultObject.Success()) )
            return queueResult;
        return LemonadeStandResultObject.Success();
    }
#endregion
#region Pricing
    internal void UpdatePrices()
        {
            _priceManager.UpdatePricesForMarket(this);
        }
    public void CalculateNewBidAskSpreadForMarket()
    {
        //remember to call CalculateNewBidAskSpreadForMarket 
        //as part of TheEconomy.Instance.ExecuteDailyTrades.
        //eventually
        _priceManager.CalculateNewBidAskSpreadForMarket(this);
    }
    public decimal GetMarketCostForGood(Market market, Good good)
    {
        return _priceManager.GetMarketCostForGood(market,good);
    }
#endregion
#region Supply
    public int GetTotalSupply(Good good)
    {
        return _inventory.GetInventoryEntriesByGood(good.GoodName)
            .Sum(x => x.quantity);
    }
#endregion
#region Publishing
    public List<MarketData> PublishMarketData()
    {
        return _marketDataManager.PublishMarketData(this);
    }

    public void PublishSpreadToMarket(ActionContext context)
    {
        _marketDataManager.PublishSpreadToMarket(context);
    }
#endregion
#region Interacting with the Economy
    public void UnleashMarketForces(int period)
    {
        FulfillDemand();
        CalculateFulfillmentRates(period);
        UpdatePrices();
        DemandStrategy.AdjustDemandInPeriod(this);
        ConsumeGoods();
        UpdateCompanyStatuses(period);
        CurrentPeriod++;
    }
#endregion
#region Overrides
    public override string ToString()
    {
        return Name;
    }
    public override bool Equals(object obj)
    {
        if(obj is Market market)
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
}
