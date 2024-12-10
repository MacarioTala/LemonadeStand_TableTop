using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class MarketTests
{
    private TheEconomy test_economy;

    private Market test_initial_market;
    //Goods
    Good lemon;
    Good water;
    Good sugar;
    Good lemonade;

    //Recipes
    private Recipe lemonade_recipe;

    readonly Price_band band1 = new(.5m, 1.0m);
    readonly Price_band band2 = new(1.0m, 3.0m);
    readonly ITradeLogger trade_logger = new MockLogger();
    readonly List<Good> test_goods = new();

    [SetUp]
    public void SetUp()
    {
        var economy_object = new GameObject();
        test_economy = economy_object.AddComponent<TheEconomy>();
        test_economy.Initialize(trade_logger);
        SetupGoodsAndRecipes();

        //Setup the initial market
        SetupInitialMarket();
    }

    private void SetupInitialMarket()
    {
        test_initial_market = Market.Factory.CreateStarterMarket(companyName: "The First Market", 
                                                    companyLevel: CompanyLevelEnum.Market,
                                                    demandStrategy: new LinearDemandStrategy());
        test_initial_market.InitializeDemandForSpecificGood(lemon, 1000);
    }

    private void SetupGoodsAndRecipes()
    {
        lemon = Good.CreateInstance("Lemon", band2, Rarity_enum.Common);
        water = Good.CreateInstance("Water", band1, Rarity_enum.Common);
        sugar = Good.CreateInstance("Sugar", band1, Rarity_enum.Common);
        lemonade = Good.CreateInstance("Lemonade", band2, Rarity_enum.Uncommon);
        lemonade_recipe = new Recipe(RecipeName: "Basic Lemonade",
                                     product: lemonade, 
                                     ingredients: new List<Ingredient> { new(lemon, 9), 
                                                                        new(sugar, 2), 
                                                                        new(water, 7) });                
        test_goods.Add(lemon);
        test_goods.Add(water);
        test_goods.Add(sugar);
        test_goods.Add(lemonade);
    }

    #region Initialization tests
    [Test]
    public void When_an_economy_is_created_it_should_have_a_market()
    {
        // Arrange
        var expected = typeof(Market);
        // Act
        var actual = from company in test_economy.companies
                     where company.Name == "The First Market"
                     select company.GetType();
        // Assert
        Assert.AreEqual(expected, actual.First());
    }
    [Test]
    public void When_a_market_is_created_without_passing_initial_demand_it_should_demand_lemonade()
    {
        // Arrange
        var expected_good_name = "Lemonade";
        var expected_demand = 1000;
        // Act
        var actual = test_initial_market.GetMarketDemand().First();
        // Assert
        Assert.AreEqual(expected_good_name, actual.Key.good_name);
        Assert.AreEqual(expected_demand, actual.Value.CurrentDemand);
    }
    #endregion
#region Consumption tests
    [Test]
    public void ConsumeGoods_should_decrease_inventory()
    {
        // Arrange
        var lemonDemand = test_initial_market.GetMarketDemandForGood(lemon.good_name);
        var initialLemons = test_initial_market.GetInventory().GetInventoryEntriesByGood(lemon.good_name).First().quantity;
        var expected = initialLemons - lemonDemand;
        // Act
        test_initial_market.ConsumeGoods();
        var actual = test_initial_market.GetInventory().GetInventoryEntriesByGood(lemon.good_name).First().quantity;
        // Assert
        Assert.AreEqual(expected, actual);
    }
    [Test]
    public void MarketsShouldConsiderMarketBuysWhenCallingConsumeGoods()
    {
        //For instance, if the demand for lemons is 1000
        //and the market buys 500 lemons, 
        //ConsumeGoods should only consume 500 lemons
        // Arrange
        var period = 0;
        var initialLemons = 1000;
        var lemonsCompanyWillSellToMarket = 500;
        var testMarket = Market.Factory.CreateMarket("Test Market", CompanyLevelEnum.Market, new LinearDemandStrategy());
        testMarket.SetCash(100000);
        testMarket.GetInventory().AddGood(new InventoryEntry(lemon, initialLemons, 3.0m, period));
        testMarket.InitializeDemandForSpecificGood(lemon, 1000);

        var testCompany = Company.Factory.Create("Test Company", CompanyLevelEnum.Beginner);
        testMarket.RegisterCompany(testCompany);
        testCompany.GetInventory().AddGood(new InventoryEntry(lemon, 2000, 2.0m, period));
        
        //next line is necessary because of different demand strategies 
        //that will change the demand
        var expected = 500;
        // Act
        var lemonSale = new Trade(testMarket, testCompany, lemon, lemonsCompanyWillSellToMarket, 3.0m);
        var lemonContext = new ActionContext { TradeToSubmit = lemonSale, MarketToSubmitTo = testMarket, Period = period };
        testMarket.QueueOrder(lemonContext);
        testMarket.ProcessCompanyOrders();
        testMarket.ConsumeGoods();
        
        //only one inventory entry per good in Markets
        var actual = testMarket.GetInventory().GetInventoryEntriesByGood(lemon.good_name).FirstOrDefault();
        // Assert
        Assert.AreEqual(expected, actual.quantity);
    }
#endregion
   
   #region Production tests
   [Test]
   public void Companies_cannot_make_goods_without_a_recipe()
   {
       // Arrange
       var company = Company.Factory.Create("Company1", CompanyLevelEnum.Beginner);
       company.BuyGood(lemon, 10,3.0m);
       company.BuyGood(sugar, 10,3.0m);
       company.BuyGood(water, 10,3.0m);
       var expectedText = "not found in company's recipe book";
       System.Exception actual=null;
       var context = new ActionContext{Recipe = lemonade_recipe, QuantityToMake = 1};
       // Act
       try
       {
        company.MakeRecipe(context);
        }
        catch(System.Exception e)
        {
            actual = e;
        }
        // Assert
        Assert.That(actual, Is.TypeOf<RecipeException>());
        StringAssert.Contains(expectedText, actual.Message);
    }

   #endregion

   #region publish tests
    [Test]
    public void PublishMarketDataShouldAddOnePercentToPrice()
    {
        // Arrange
        var testMarket = Market.Factory.CreateMarket(companyName: "TestMarket", 
                                                    companyLevel: CompanyLevelEnum.Market,
                                                    demandStrategy: new LinearDemandStrategy());
        testMarket.GetInventory().AddGood(new InventoryEntry(lemon, 1000, 3.0m, 0));
        var initialPrice = testMarket.GetInventory().GetInventoryEntriesByGood(lemon.good_name).First().good.GetPrice();
        var expected = initialPrice * 1.01m;
        // Act
        testMarket.CalculateNewBidAskSpreadForMarket();
        var marketData = testMarket.MarketData;
        var actual = marketData.Where(entry => entry.Good.good_name == lemon.good_name
                                        && entry.Company.Name == testMarket.Name)
                                    .First().Ask;
        // Assert
        Assert.AreEqual(expected, actual);
    }
    [Test]
    public void PublishSpreadToMarket_should_update_existing_MarketData_if_spread_exists()
    {
        // Arrange
        var company1 = Company.Factory.Create("Company1", CompanyLevelEnum.Beginner);
        test_economy.RegisterCompany(company1);
        var context = new ActionContext{BidToSubmit = 2.0m, AskToSubmit = 3.0m, GoodToSubmit = lemon, MarketToSubmitTo = test_initial_market};
        var expected = new List<MarketData>{new() { Good = lemon, Company = company1, Bid = 2.0m, Ask = 3.0m}};
        //Act
        company1.SubmitBidAskSpreadToMarket(context);
        test_initial_market.CalculateNewBidAskSpreadForMarket();
        var actual = test_initial_market.MarketData.Where(x=>x.Good == lemon).ToList();
        // Assert
        Assert.AreEqual(expected, actual);
    }
    [Test]
    public void PublishSpreadToMarketThrowsContextExceptionIfActionContextIsIncomplete()
    {
        // Arrange
        var company1 = Company.Factory.Create("Company1", CompanyLevelEnum.Beginner);
        test_economy.RegisterCompany(company1);
        var context = new ActionContext{BidToSubmit = 2.0m, AskToSubmit = 3.0m, GoodToSubmit = lemon};
        System.Exception actual=null;
        //Act
        try
        {
            company1.SubmitBidAskSpreadToMarket(context);
        }
        catch(System.Exception e)
        {
            actual = e;
        }
        // Assert
        Assert.That(actual, Is.TypeOf<ContextException>());
    }
                               
   #endregion
   #region Pricing Tests
   [Test]
   public void AskForAGoodShouldExceedCost()
   {
         // Arrange
        var testMarket = Market.Factory.CreateMarket(companyName: "TestMarket", 
                                                    companyLevel: CompanyLevelEnum.Market,
                                                    demandStrategy: new LinearDemandStrategy());
        var enhancedlemonade = Good.CreateInstance("Enhanced Lemonade", band2, Rarity_enum.Uncommon);
        testMarket.SetCash(100000);
        enhancedlemonade.IsProducedGood = true;
        var enhancedLemonadeRecipe = new Recipe(RecipeName: "Enhanced Lemonade",
                                     product: enhancedlemonade, 
                                     ingredients: new List<Ingredient> { new(lemon, 9), 
                                                                        new(sugar, 2), 
                                                                        new(water, 7) });
        var lemonEntry = new InventoryEntry(lemon, 1000, 3.0m, 0);
        var sugarEntry = new InventoryEntry(sugar, 1000, 3.0m, 0);
        var waterEntry = new InventoryEntry(water, 1000, 3.0m, 0);
        testMarket.GetInventory().AddGood(lemonEntry);
        testMarket.GetInventory().AddGood(sugarEntry);
        testMarket.GetInventory().AddGood(waterEntry);
        testMarket.AddRecipe(enhancedLemonadeRecipe);
        testMarket.InitializeDemandForSpecificGood(enhancedlemonade, 1000);
        var costPerUnit = 9 * 3.0m + 2 * 3.0m + 7 * 3.0m;
        // Act
        var actual = testMarket.GetMarketDemand()[enhancedlemonade].Ask;
        // Assert
        Assert.Greater(actual, costPerUnit);
        Debug.Log($"Cost per unit: {costPerUnit}" + " Ask: " + actual);
   }
   #endregion
   #region Trade Tests
   [Test]
    public void A_Company_cannot_buy_a_good_if_it_has_insufficient_cash()
    {
        // Arrange
        var company1 = Company.Factory.Create("Company1", CompanyLevelEnum.Beginner);
        var company2 = Company.Factory.Create("Company2", CompanyLevelEnum.Beginner);
        var testMarket = Market.Factory.CreateStarterMarket("Test Market", CompanyLevelEnum.Market, new LinearDemandStrategy());
        testMarket.RegisterCompany(company1);
        testMarket.RegisterCompany(company2);
        
        company2.SetCash(0);
        const string expected="Insufficient funds to buy good";
        string actual=null;
        var company2BuysLemons = new Trade(company2, company1, lemon, 4000, 3.0m);
        var company2Context = new ActionContext{TradeToSubmit = company2BuysLemons,
                                                MarketToSubmitTo = testMarket};
        // Act
        try{
            testMarket.QueueOrder(company2Context);
            testMarket.ProcessCompanyOrders();
        }
        catch(Exception e)
        {
            // Assert
            actual=e.Message;    
        }
        Assert.AreEqual(expected, actual);
    }
     [Test]
    public void MarketsShouldOnlyHaveASingleInventoryEntryPerGoodEvenWithMultipleBuys()
    {
        //As of 11/17/2023, Markets don't care about optimizing 
        //the price that they buy goods at
        //they only care about the quantity of goods they have
        //So there should only be one InventoryEntry per good
        
        // Arrange
        var tradingPeriod = 0;
        var marketToTest = Market.Factory.CreateStarterMarket("Starter Market",
                                                              CompanyLevelEnum.Market,
                                                              new LinearDemandStrategy() );
        const int expected_number_of_entries = 1;
        var company1 = Company.Factory.Create("Company1", CompanyLevelEnum.Beginner);
        marketToTest.RegisterCompany(company1);
        var company2 = Company.Factory.Create("Company2", CompanyLevelEnum.Beginner);
        marketToTest.RegisterCompany(company2);
        company1.GetInventory().AddGood(new InventoryEntry(lemon, 20, 3m,tradingPeriod));
        company2.GetInventory().AddGood(new InventoryEntry(lemon, 20, 3m,tradingPeriod));
        marketToTest.InitializeDemandForSpecificGood(lemon, 50);
        // Act
        var trade1 = new Trade(test_initial_market, company1, lemon, 10, 10.0m);
        var trade2 = new Trade(test_initial_market, company2, lemon, 10, 15.0m);
        var trade1Context = new ActionContext{TradeToSubmit = trade1,
                                                MarketToSubmitTo = marketToTest};   
        var trade2Context = new ActionContext{TradeToSubmit = trade2,
                                                MarketToSubmitTo = marketToTest};
        marketToTest.QueueOrder(trade1Context);
        marketToTest.QueueOrder(trade2Context);
        marketToTest.ProcessCompanyOrders();
        var actual_number_of_entries = test_initial_market.GetInventory().GetInventoryEntriesByGood(lemon.good_name).Count();
        // Assert
        Assert.AreEqual(expected_number_of_entries, actual_number_of_entries);
    }
     [Test]
    public void CompaniesCanBuyGoodsFromEachOtherViaQueueTrade()
    {
        // Arrange
        var period = 0;
        var testMarket = Market.Factory.CreateStarterMarket("Test Market", CompanyLevelEnum.Market, new LinearDemandStrategy());
        var company1 = Company.Factory.Create("Company1", CompanyLevelEnum.Beginner);
        var company2 = Company.Factory.Create("Company2", CompanyLevelEnum.Beginner);
        testMarket.RegisterCompany(company1);
        testMarket.RegisterCompany(company2);
        company1.GetInventory().AddGood(new InventoryEntry(sugar,10,2.0m,period));
        company2.GetInventory().AddGood(new InventoryEntry(sugar,10, 2.0m,period));
        
        var expectedCompany1Cash = company1.GetCash() - 2;
        var expectedCompany2Cash = company2.GetCash() + 2;
        var expectedCompany1SugarQuantity = 11;
        var expectedCompany2SugarQuantity = 10 - 1;

        // Act
        var trade = new Trade(company1, company2, sugar, 1, 2.0m);
        var sugarTradeContext = new ActionContext{TradeToSubmit = trade, MarketToSubmitTo = testMarket};
        testMarket.QueueOrder(sugarTradeContext);
        testMarket.ProcessCompanyOrders();
        var actualCompany1Cash = company1.GetCash();
        var actualCompany2Cash = company2.GetCash();
        var actualCompany1SugarQuantity = company1.GetInventory().GetInventoryEntries().FirstOrDefault(x => x.good == sugar && x.Cost==2.0m).quantity;
        var actualCompany2SugarQuantity = company2.GetInventory().GetInventoryEntries().FirstOrDefault(x => x.good == sugar).quantity;
        // Assert
        Assert.AreEqual(expectedCompany1Cash, actualCompany1Cash);
        Assert.AreEqual(expectedCompany2Cash, actualCompany2Cash);
        Assert.AreEqual(expectedCompany1SugarQuantity, actualCompany1SugarQuantity);
        Assert.AreEqual(expectedCompany2SugarQuantity, actualCompany2SugarQuantity);
    }
    [Test]
    public void ACompanyCannotSellAGoodIfItHasInsufficientInventory()
    {
        // Arrange
        var testMarket = Market.Factory.CreateStarterMarket("Test Market", CompanyLevelEnum.Market, new LinearDemandStrategy());
        var company1 = Company.Factory.Create("Company1", CompanyLevelEnum.Beginner);
        var company2 = Company.Factory.Create("Company2", CompanyLevelEnum.Beginner);
        testMarket.RegisterCompany(company1);
        testMarket.RegisterCompany(company2);
        var expected = "Company does not have enough of the good to sell";
        string actual = null;
        var company1BuysLemons = new Trade(company1, company2, lemon, 10, 3.0m);
        var lemonBuyingContext = new ActionContext { TradeToSubmit = company1BuysLemons, MarketToSubmitTo = testMarket };

        // Act
        try{

        testMarket.QueueOrder(lemonBuyingContext);
        testMarket.ProcessCompanyOrders();
        }
        catch(Exception e)
        {
            actual = e.Message;
        }
        // Assert
        Assert.AreEqual(expected, actual);
    }
   #endregion

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(lemon);
        UnityEngine.Object.DestroyImmediate(water);
        UnityEngine.Object.DestroyImmediate(sugar);
        UnityEngine.Object.DestroyImmediate(test_economy);
    }
}