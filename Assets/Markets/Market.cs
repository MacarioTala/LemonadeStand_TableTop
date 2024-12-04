using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Market : ScriptableObject, iCompany, iPriceSetter
{
#region Identity
    //Fields to get around Unity's limitation of not having automatic backing properties.
    [SerializeField] private string _company_name;
    public string Name
    {
        get => _company_name;
        set => _company_name = value;
    }
    public CompanyLevelEnum company_level;
#endregion

#region fixed_costs
    public List<FixedCost> FixedCosts { get; set; }
    public iFixedCostStrategy FixedCostStrategy {get;set;}
    public decimal CalculateFixedCostsForPeriod(int period)
    {
        return FixedCostStrategy.CalculateFixedCosts(FixedCosts,period);
    }
#endregion

#region Financials
    private decimal cash = 0;
    public decimal GetCash() => cash;
    private void SetInitialCash()
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
#endregion

#region goals and strategies
    public List<Goal> Goals {get;set;}
    public iDemandStrategy DemandStrategy ;
    private iStrategy _marketStrategy;
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
    private readonly Inventory _inventory = new();
    public Inventory GetInventory() => _inventory;
    private readonly List<Recipe> _recipes = new();
    public List<Recipe> GetRecipes()=>_recipes;
    private Recipe GetRecipeForGood(Good good)
    {
       return _recipes.Where(recipe=>recipe.GetProduct().Equals(good)).FirstOrDefault();
    }
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
    public int StartingPeriod{get;set;}
    public int CurrentPeriod{get;set;}
    public void UpdateCurrentPeriod(int period)
    {
        CurrentPeriod = period;
    }
#endregion

    //Market-specific members
    private readonly List<MarketTrade> _marketTradesInPeriod = new();
    private readonly List<iPriceModifier> _priceModifiers = new();

#region Creation
    //Instantiate Markets using a factory
    private Market ()
    {
    }

    public static class Factory
    {
        public static Market CreateMarket(string companyName, CompanyLevelEnum companyLevel, iDemandStrategy demandStrategy, iStrategy marketStrategy)
        {
            var market = CreateInstance<Market>();
            market.Initialize(companyName, companyLevel, marketStrategy);
            market.DemandStrategy = demandStrategy ?? throw new ArgumentNullException("Markets must have a demand strategy");
            return market;
        }
        public static Market CreateMarket(string companyName, CompanyLevelEnum companyLevel, iDemandStrategy demandStrategy)
        {
            var market = CreateInstance<Market>();
            market.Initialize(companyName, companyLevel, null);
            market.DemandStrategy = demandStrategy ?? throw new ArgumentNullException("Markets must have a demand strategy");
            return market;
        }
        
        public static Market CreateMarket(string companyName, CompanyLevelEnum companyLevel, iDemandStrategy demandStrategy, iStrategy marketStrategy, iFixedCostStrategy fixedCostStrategy)
        {
            var market = CreateInstance<Market>();
            market.Initialize(companyName, companyLevel, marketStrategy);
            market.DemandStrategy = demandStrategy ?? throw new ArgumentNullException("Markets must have a demand strategy");
            market.FixedCostStrategy = fixedCostStrategy ?? throw new ArgumentNullException("Markets must have a fixed cost strategy");
            return market;
        }

        public static Market CreateStarterMarket(string companyName, CompanyLevelEnum companyLevel, iDemandStrategy demandStrategy)
        {
            var market = CreateInstance<Market>();
            market.SeedWithInitialGoods();
            market.Initialize(companyName, companyLevel, null);
            market.DemandStrategy = demandStrategy ?? throw new ArgumentNullException("Markets must have a demand strategy");
            return market;
        }
    }
    internal void Initialize(
        string companyName,
        CompanyLevelEnum companyLevel,
        iStrategy strategy)
    {
        Name = companyName;
        company_level = companyLevel;
        _marketStrategy = strategy;
        //setup
        SetInitialCash();
        _priceModifiers.Add(new SupplyDemandModifier());
        CreateStarterDemand();

    }

    private void CreateStarterDemand ()
    {
        //Initialize demand data
        //If no demand data is passed, demand defaults to 1000 units of Lemonade
        //This is a placeholder and will be replaced with a more sophisticated system
        var lemonade = Good.CreateInstance("Lemonade", new Price_band(8.0m, 13.0m), Rarity_enum.Uncommon);
        lemonade.IsProducedGood = true;
        InitializeDemand(lemonade, 1000);
    }

    private void SeedWithInitialGoods ()
    {
        var Lemonade = Good.CreateInstance("Lemonade", new Price_band(8.0m, 13.0m), Rarity_enum.Uncommon);
        var Lemon = Good.CreateInstance("Lemon", new Price_band(1.0m, 3.0m), Rarity_enum.Common);
        var Sugar = Good.CreateInstance("Sugar", new Price_band(1.0m, 2.0m), Rarity_enum.Common);
        var Water = Good.CreateInstance("Water", new Price_band(.5m, 1.0m), Rarity_enum.Common);
        _inventory.Add_good_to_inventory(new InventoryEntry(Lemon, 10000, 2.0m, 0));
        _inventory.Add_good_to_inventory(new InventoryEntry(Sugar, 10000, 1.5m, 0));
        _inventory.Add_good_to_inventory(new InventoryEntry(Water, 10000, .75m, 0));
        var LemonadeRecipe = new Recipe(RecipeName: "Basic Lemonade",
                                        product: Lemonade,
                                        ingredients: new List<Ingredient> { new(Lemon, 9),
                                                                           new(Sugar, 2),
                                                                           new(Water, 7) });
        AddRecipe(LemonadeRecipe);
    }
#endregion
#region Price Setting
    private decimal CalculateAskForProducedGood (Good good)
    {
        var recipeToUse = GetRecipeForGood(good);

        if(recipeToUse == null)
        {
            throw new Exception("No recipe found for "+good.good_name);
        }
        var costPerUnit = recipeToUse.GetCostPerUnit(_inventory);
        var rng = (double)UnityEngine.Random.Range(.01f,.15f);
        var ask = costPerUnit * 1+(decimal)rng;
        return ask;
    }
        
    private decimal CalculateNewPrice (Good good)
    {
        decimal price = good.GetPrice();
        foreach(var modifier in _priceModifiers)
        {
            price = modifier.Apply(price,good,this);
        }
        return price;
    }
    public void SetPrice (Good good, decimal new_price)
    {
        good.Set_price(new_price);
    }

     public void UpdatePrices(){
        foreach(var entry in _inventory.GetInventoryEntries())
        {   
            var new_price = CalculateNewPrice(entry.good); 
            SetPrice(entry.good,new_price); 
        }
    }
#endregion
#region buy/sell, and helpers
    internal bool HasMoney(decimal money_needed)
    {
       return cash >= money_needed;
    }

    internal bool HasGood(Good good, int quantity)
    {
        var goods = _inventory.GetInventoryEntries();
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
            if(_inventory.GetInventoryEntriesByGood(good.good_name).Count > 0)
            {
                var inventory_entry = _inventory.GetInventoryEntriesByGood(good.good_name).First();
                inventory_entry.quantity += quantity;
                cash -= money_needed;
            }
            else
            {
                var inventory_entry = new InventoryEntry(good, quantity, price, period);
                _inventory.Add_good_to_inventory(inventory_entry);
                cash -= money_needed;
                _marketTradesInPeriod.Add(new MarketTrade(inventory_entry, period,TradeType.Buy));
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
            
            _inventory.Sell_goods(good, quantity, price);
            cash += price * quantity;
            _marketTradesInPeriod.Add(new MarketTrade(new InventoryEntry(good, quantity, price,period), period,TradeType.Sell));
        }
        else
        {
            throw new Company_InventoryException("Company does not have enough of the good to sell");
        }
    }
    public int GetTotalBought(int tradingPeriod, Good good)//Currently public for testing purposes
    {
        var total_bought = 
            _marketTradesInPeriod
            .Where(x => x.Period == tradingPeriod
                        && x.InventoryEntry.good.Equals(good)
                        && x.TradeType == TradeType.Buy)
            .Sum(x => x.InventoryEntry.quantity);
        return total_bought;
    }

    public int GetTotalSold(int tradingPeriod, Good good) //currently public for testing purposes
    {
        var total_sold=_marketTradesInPeriod
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
    //demand_data represents the base demand for each good
    //outside of that demanded by companies
    //It is used to 'seed' the market with an initial demand that will
    //then be affected by market forces
    public Dictionary<Good, DemandData> MarketDemand = new();
    public void InitializeDemand(Good good, int InitialDemand,int MinDemand=0, int MaxDemand=1000000)
    {
        decimal ask;
        if(good.IsProducedGood && GetRecipeForGood(good)!=null)
        {
            ask=CalculateAskForProducedGood(good);
        }
        else
        {
            ask = good.GetPrice();
        }

        if(MarketDemand.ContainsKey(good))
        {
            MarketDemand[good].CurrentDemand = InitialDemand;
            MarketDemand[good].MinDemand = MinDemand;
            MarketDemand[good].MaxDemand = MaxDemand;
            MarketDemand[good].Ask = ask;
        }
        else
        {
            var demandData = new DemandData
                            { 
                                CurrentDemand = InitialDemand,
                                FulfilmentRate = 0f,
                                MinDemand = MinDemand,
                                MaxDemand = MaxDemand,
                                Ask = ask
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
            var unfulfilledDemand = _inventory.TryConsumeGood(good.good_name,demanded_quantity);
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
#region Publishing
    public List<MarketData> MarketData = new();//bid/ask spread for companies
    //remember to call CalculateMarketData as part of TheEconomy.Instance.ExecuteDailyTrades.
    public void CalculateMarketData()
    {
        var temporaryPriceIncrease = .01m;
        foreach(var entry in _inventory.GetInventoryEntries())
        {
            var data = new MarketData
            {
                Company = this,
                Bid = entry.good.GetPrice(),
                Ask = entry.good.GetPrice() * (1 + temporaryPriceIncrease),
                Good = entry.good
            };
            MarketData.Add(data);
        }
    }
    public List<MarketData> PublishMarketData()=>MarketData;

    public void PublishSpreadToMarket(ActionContext context)
    {
        var MarketToSubmitTo = this;
        var good = context.GoodToSubmit;
        var bid = context.BidToSubmit;
        var ask = context.AskToSubmit;
        var submittingCompany = context.SubmittingCompany;

        var isGoodInMarketData = MarketData.Any(x=>x.Good==good && x.Company.Equals(submittingCompany));
        if(isGoodInMarketData)
        {
            var marketData = MarketData.First(x=>x.Good==good && x.Company.Equals(submittingCompany));
            marketData.Bid = bid;
            marketData.Ask = ask;
        }
        else
        {
            var data = new MarketData
            {
                Company = submittingCompany,
                Good = good,
                Bid = bid,
                Ask = ask
            };
            MarketData.Add(data);
        }
    }
#endregion
#region Interacting with the Economy
    private iTradeProcessor _tradeProcessor;
    private iConsumptionManager _consumptionManager;
    public void SetTradeProcessor(iTradeProcessor tradeProcessor)
    {
        _tradeProcessor = tradeProcessor;
    }
    public void SetConsumptionManager(iConsumptionManager consumptionManager)
    {
        _consumptionManager = consumptionManager;
    }
    public void QueueOrder(ActionContext context)
    {
        _tradeProcessor.QueueOrder(context);
    }

    public List<Company> CompaniesInThisMarket = new();
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
    
   

    // public List<Trade> FillOrders()
    // {
    //     var ordersToReturn = new List<Trade>();
    // }
        
#endregion
public void ExpireGoods(int period)
{
    //inventory.ExpireGoods(period); //maybe goods in market just don't expire?
}
}