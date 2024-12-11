using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
[assembly:InternalsVisibleTo("Tests")]
public class Market : ScriptableObject, iCompany
{
#region Fields, Properties, and Convenience Methods
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

    //Goals
    public List<Goal> Goals {get;set;}
    public iDemandStrategy DemandStrategy ;
    private iStrategy _marketStrategy;
    
    //Pricing  
    public List<FixedCost> FixedCosts { get; set; }
    public iFixedCostStrategy FixedCostStrategy {get;set;}
    private readonly List<iPriceModifier> _priceModifiers = new();
    
    //Time
    public int CurrentPeriod{get;set;}
    public int StartingPeriod{get;set;}
    //Trading
    private readonly List<MarketTrade> _marketTradesInPeriod = new();

    //Convenience methods
    public decimal GetCash() => cash;
    public Inventory GetInventory() => _inventory;
    public List<Order>GetOrdersSentToMarket()=>_tradeProcessor.GetOrders();
    public List<Recipe> GetRecipes()=>_recipes;
    public Dictionary<Good,DemandData> GetMarketDemand() => _marketDemand;
    public List<MarketTrade> GetMarketTradesInPeriod() => _marketTradesInPeriod;
    public void RecordMarketTrade(MarketTrade trade) => _marketTradesInPeriod.Add(trade);
    public List<iPriceModifier> GetPriceModifiers() => _priceModifiers;
    public void RecordTrade(MarketTrade trade) => _marketTradesInPeriod.Add(trade);
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
            market.Initialize(companyName, companyLevel, null);
            market.DemandStrategy = demandStrategy ?? throw new ArgumentNullException("Markets must have a demand strategy");
            return market;
        }

        public static Market CreateStarterMarket(string companyName, CompanyLevelEnum companyLevel, iDemandStrategy demandStrategy)
        {
            var market = CreateInstance<Market>();
            market.Initialize(companyName, companyLevel, null);
            market.DemandStrategy = demandStrategy ?? throw new ArgumentNullException("Markets must have a demand strategy");
            _initializer.InitializeMarket(market);
            return market;
        }
    }
    internal void Initialize (string companyName,CompanyLevelEnum companyLevel,iStrategy strategy)
    {
        Name = companyName;
        company_level = companyLevel;
        _marketStrategy = strategy;
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
#endregion
#region Company Interactions
    
    public void ProcessOrder(ActionContext context)
    {
        _transactionManager.ProcessTransaction(context);
    }
    public List<Order> ProcessCompanyOrders()
    {
        var CompanyOrdersExecuted = _tradeProcessor.ProcessCompanyOrders(this);
        return CompanyOrdersExecuted;
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
    public void QueueOrder(ActionContext context)
    {
        _tradeProcessor.QueueOrder(context);
    }

    internal void UpdateCompanyStatuses()
    {
        foreach (var company in CompaniesInThisMarket)
        {
            //Update Company Statuses
            company.UpdateCurrentPeriod(CurrentPeriod);
            company.ExpireGoods(CurrentPeriod);
        }
    }
#endregion
#region Consumption
    public void FulfillDemand()
    {
        _consumptionManager.FulfillDemand(this);
    }
    internal void ConsumeGoods()
        {
            //attempt to consume goods at current demand levels
            foreach(var good in _marketDemand.Keys)
            {
                var demanded_quantity = _marketDemand[good].CurrentDemand;
                //consume good
                var unfulfilledDemand = _inventory.TryConsumeGood(good.good_name,demanded_quantity);
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
    public void BuyGood(Good good, int quantity,decimal price,int period=0)
    {
        var context = new ActionContext
        {
            Buyer=this,
            Seller=null,//null because we're buying from the market
            GoodToBuy=good,
            Quantity=quantity,
            Price=price,
            Period=period
        };
            
        _transactionManager.ProcessTransaction(context);
    }
    public void SellGood(Good good, int quantity, decimal price,int period=0)
    {
        //period currently does nothing for companies, but is used in Market which implements iCompany
        var context = new ActionContext
        {
            Buyer=null,//null because we're selling to the market
            Seller=this,
            GoodToBuy=good,
            Quantity=quantity,
            Price=price,
            Period=period
        };
        _transactionManager.ProcessTransaction(context);
    }
    public int GetTotalBought(int tradingPeriod, Good good)//Currently public for testing purposes
    {
       return DemandStrategy.GetTotalBought(this,good,tradingPeriod);
    }
    public int GetTotalSold(int tradingPeriod, Good good) //currently public for testing purposes
    {
        return DemandStrategy.GetTotalSold(this,tradingPeriod,good);
    }
#endregion
#region Demand
    internal void AdjustDemand()
    {
        DemandStrategy.AdjustDemand(this);
    }
     internal void CalculateFulfillmentRates(int tradingPeriod=-1)
    {
        DemandStrategy.CalculateFulfillmentRates(this,tradingPeriod);
    }
    public int GetMarketDemandForGood(string good_name)
    {
        var good = _marketDemand.Keys.FirstOrDefault(x=>x.good_name == good_name);
        return _marketDemand[good].CurrentDemand;
    }
    public void InitializeDemandForSpecificGood(Good good, int InitialDemand)
    {
       DemandStrategy.InitializeDemandForSpecificGood(this,good,InitialDemand);
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
        return _inventory.GetInventoryEntriesByGood(good.good_name)
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
        AdjustDemand();
        UpdatePrices();
        ConsumeGoods();
        UpdateCompanyStatuses();
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