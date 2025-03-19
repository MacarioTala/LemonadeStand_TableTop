using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using UnityEngine;
using static TestHelpers;

[TestFixture]
public partial class MarketTests
{
    private TheEconomy TestEconomy;

    private Market test_initial_market;
    Company Company1;
    Company Company2;
    Market TestMarket;

    const int Period = 0;

    //Goods
    Good lemon;
    Good water;
    Good sugar;
    Good lemonade;

    //Market Dependencies
    iDemandStrategy TestDemandStrategy;

    //Recipes
    private Recipe lemonade_recipe;

    readonly PriceBand band1 = new(.5m, 1.0m);
    readonly PriceBand band2 = new(1.0m, 3.0m);
    readonly ITradeLogger trade_logger = new MockLogger();
    readonly List<Good> test_goods = new();

    [SetUp]
    public void SetUp()
    {
        //Create the economy
        TheEconomy.SetupForTests(new MockLogger());
        TestEconomy = TheEconomy.Instance;

        //Setup Market Dependencies
        TestDemandStrategy = ScriptableObject.CreateInstance<LinearDemandStrategy>();

        //Setup Market
        TestMarket = Market.Factory.CreateMarket("Test Market", CompanyLevelEnum.Market, TestDemandStrategy);

        //Setup Companies
        Company1 = Company.Factory.Create("Company1", CompanyLevelEnum.Beginner);
        Company2 = Company.Factory.Create("Company2", CompanyLevelEnum.Beginner);
        TestMarket.RegisterCompany(Company1);
        TestMarket.RegisterCompany(Company2);

        SetupGoodsAndRecipes();

        //Setup the initial market
        SetupInitialMarket();
    }

    private void SetupInitialMarket()
    {
        test_initial_market = Market.Factory.CreateStarterMarket(companyName: "The First Market", 
                                                    companyLevel: CompanyLevelEnum.Market,
                                                    demandStrategy: TestDemandStrategy);
        test_initial_market.InitializeDemandForSpecificGood(lemon, 1000);
    }

    private void SetupGoodsAndRecipes()
    {
        lemon = Good.CreateInstance("Lemon", band2, RarityEnum.Common);
        water = Good.CreateInstance("Water", band1, RarityEnum.Common);
        sugar = Good.CreateInstance("Sugar", band1, RarityEnum.Common);
        lemonade = Good.CreateInstance("Lemonade", band2, RarityEnum.Uncommon);
        lemonade.IsProducedGood = true;
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
        var actual = from company in TestEconomy.companies
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
        Assert.AreEqual(expected_good_name, actual.Key.GoodName);
        Assert.AreEqual(expected_demand, actual.Value.CurrentDemand);
    }
    #endregion
#region Consumption tests
    [Test]
    public void ConsumeGoods_should_decrease_inventory()
    {
        // Arrange
        var lemonDemand = test_initial_market.GetMarketDemandForGood(lemon.GoodName);
        var initialLemons = test_initial_market.GetInventory().GetInventoryEntriesByGood(lemon.GoodName).First().quantity;
        var expected = initialLemons - lemonDemand;
        // Act
        test_initial_market.ConsumeGoods();
        var actual = test_initial_market.GetInventory().GetInventoryEntriesByGood(lemon.GoodName).First().quantity;
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
        
        TestMarket.SetCash(100000);
        TestMarket.GetInventory().AddGood(new InventoryEntry(lemon, initialLemons, 3.0m, period));
        TestMarket.InitializeDemandForSpecificGood(lemon, 1000);

        Company1.GetInventory().AddGood(new InventoryEntry(lemon, 2000, 2.0m, period));
        
        //next line is necessary because of different demand strategies 
        //that will change the demand
        var expected = 500;
        // Act
        var lemonSale = new Order(TestMarket, Company1, lemon, lemonsCompanyWillSellToMarket, 3.0m);
        Company1.QueueOrder(CreateActionContext(lemonSale,TestMarket,0));
        TestMarket.ProcessCompanyOrders();
        TestMarket.FulfillDemand();
        TestMarket.ConsumeGoods();
        
        //only one inventory entry per good in Markets
        var actualInventory = TestMarket.GetInventory();
        var actual = actualInventory.GetInventoryEntriesByGood(lemon.GoodName).FirstOrDefault();
        // Assert
        Assert.AreEqual(expected, actual.quantity);
    }
    
#endregion
#region Perishability tests
    [Test]
    public void PerishableGoodsShouldExpire()
    {
        // Arrange
        var tradingPeriod = 1;
        var testMarket = Market.Factory.CreateMarket("Test Market", CompanyLevelEnum.Market,TestDemandStrategy);
        var company = Company.Factory.Create("Company1", CompanyLevelEnum.Beginner);
        testMarket.RegisterCompany(company);
        var francium = Good.CreateInstance("Uranium", band2, RarityEnum.Very_Rare);
        francium.ExpiresAfterPeriods = 1;
        company.GetInventory().AddGood(new InventoryEntry(francium, 1,10000m,0));
        var expected = 0;
        // Act
        testMarket.ExpireGoods(tradingPeriod);
        var franciumEntry = company.GetInventory().GetInventoryEntriesByGood(francium.GoodName).FirstOrDefault();
        var actual = franciumEntry?.quantity??0;
        // Assert
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void NonPerishableGoodsShouldNotExpire()
    {
        // Arrange
        var period = 1;
        var testMarket = Market.Factory.CreateMarket("Test Market", CompanyLevelEnum.Market,TestDemandStrategy);
        var company = Company.Factory.Create("Company1", CompanyLevelEnum.Beginner);
        testMarket.RegisterCompany(company);
        var ripeLemon = Good.CreateInstance("Ripe Lemon", band2, RarityEnum.Common);
        ripeLemon.ExpiresAfterPeriods = 1;
        var ripeLemonInventoryEntry = new InventoryEntry(ripeLemon, 10, 3.0m,0);
        var waterInventoryEntry = new InventoryEntry(water, 10, 1.0m,0);
        var sugarInventoryEntry = new InventoryEntry(sugar, 10, 1.0m,0);
        company.GetInventory().AddGood(ripeLemonInventoryEntry);
        company.GetInventory().AddGood(waterInventoryEntry);
        company.GetInventory().AddGood(sugarInventoryEntry);
        var expectedWaterQuantity = 10;
        var expectedRipeLemonQuantity = 0;
        // Act
        testMarket.ExpireGoods(period);
        var actualWaterQuantity = company.GetInventory().GetInventoryEntriesByGood(water.GoodName).FirstOrDefault().quantity;
        var ripeLemonEntries = company.GetInventory().GetInventoryEntriesByGood(ripeLemon.GoodName).FirstOrDefault();
        var actualRipeLemonQuantity = ripeLemonEntries?.quantity??0;
        // Assert
        Assert.IsTrue(expectedWaterQuantity==actualWaterQuantity && expectedRipeLemonQuantity==actualRipeLemonQuantity);
    }

    [Test]
    public void OnlyPerishableGoodsAtTheirExpiryPeriodShouldExpire()
    {
        // Arrange
        var tradingPeriod = 1;
        var company = Company.Factory.Create("Company1", CompanyLevelEnum.Beginner);
        var testMarket = Market.Factory.CreateMarket("Test Market", CompanyLevelEnum.Market,TestDemandStrategy);
        testMarket.RegisterCompany(company);
        company.GetInventory().AddGood(new InventoryEntry(lemon, 10, 3.0m,0));
        lemon.ExpiresAfterPeriods=2;
        var ripeLemon = Good.CreateInstance("Ripe Lemon", band2, RarityEnum.Common);
        ripeLemon.ExpiresAfterPeriods=1;
        company.GetInventory().AddGood(new InventoryEntry(ripeLemon, 10, 3.0m,0));
        var expectedLemonQuantity = 10;
        var expectedRipeLemonQuantity = 0;
        // Act
        testMarket.ExpireGoods(tradingPeriod);
        var actualLemonEntries = company.GetInventory().GetInventoryEntriesByGood(lemon.GoodName).FirstOrDefault();
        var actualRipeLemonEntries = company.GetInventory().GetInventoryEntriesByGood(ripeLemon.GoodName).FirstOrDefault();
        var actualLemonQuantity = actualLemonEntries?.quantity??0;
        var actualRipeLemonQuantity = actualRipeLemonEntries?.quantity??0;
        // Assert
        Assert.AreEqual(expectedLemonQuantity, actualLemonQuantity);
        Assert.AreEqual(expectedRipeLemonQuantity, actualRipeLemonQuantity);
    }

    [Test]
    public void TheSameGoodBoughtAtDifferentTimesExpiresAtDifferentPeriods()
    {
        // Arrange
        var tradingPeriod = 1;
        var company = Company.Factory.Create("Company1", CompanyLevelEnum.Beginner);
        var testMarket = Market.Factory.CreateMarket("Test Market", CompanyLevelEnum.Market,TestDemandStrategy);
        testMarket.RegisterCompany(company);
        
        var apple = Good.CreateInstance("Apple", band2, RarityEnum.Common);
        apple.ExpiresAfterPeriods=1;
        company.GetInventory().AddGood(new InventoryEntry(apple, 10, 3.0m,0));
        company.GetInventory().AddGood(new InventoryEntry(apple, 10, 3.0m,1));
        var expectedAppleQuantity = 10;
        // Act
        testMarket.ExpireGoods(tradingPeriod); //only 1 batch of apples expires
        var actualAppleEntry = company.GetInventory().GetInventoryEntriesByGood(apple.GoodName).FirstOrDefault();
        var actualAppleQuantity = actualAppleEntry?.quantity??0;
        // Assert
        Assert.AreEqual(expectedAppleQuantity, actualAppleQuantity);
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
                                                    demandStrategy: TestDemandStrategy);
        testMarket.GetInventory().AddGood(new InventoryEntry(lemon, 1000, 3.0m, 0));
        var initialPrice = testMarket.GetInventory().GetInventoryEntriesByGood(lemon.GoodName).First().good.GetPrice();
        var expected = initialPrice * 1.01m;
        // Act
        testMarket.CalculateNewBidAskSpreadForMarket();
        var marketData = testMarket.MarketData;
        var actual = marketData.Where(entry => entry.Good.GoodName == lemon.GoodName
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
        TestEconomy.RegisterCompany(company1);
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
    public void PublishSpreadToMarketReturnsErrorIfActionContextIsIncomplete()
    {
        // Arrange
        var company1 = Company.Factory.Create("Company1", CompanyLevelEnum.Beginner);
        TestEconomy.RegisterCompany(company1);
        var context = new ActionContext{BidToSubmit = 2.0m, AskToSubmit = 3.0m, GoodToSubmit = lemon};
        
        var expected = LemonadeStandResultObject.Failure(ResultTypeEnum.MarketNotSet, "Market not set");
        //Act
        var actual = company1.SubmitBidAskSpreadToMarket(context);
        
        // Assert
        Assert.AreEqual(expected, actual);
    }
                               
   #endregion
   #region Pricing Tests
   [Test]
   public void AskForAGoodShouldExceedCost()
   {
         // Arrange
        var testMarket = Market.Factory.CreateMarket(companyName: "TestMarket", 
                                                    companyLevel: CompanyLevelEnum.Market,
                                                    demandStrategy: TestDemandStrategy);
        var enhancedlemonade = Good.CreateInstance("Enhanced Lemonade", band2, RarityEnum.Uncommon);
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
    public void ACompanyCannotBuyGoodsWithInsufficientCash()
    {
        // Arrange
        Company2.SetCash(0);
        var company2BuysLemons = new Order(Company2, null, lemon, 4000, 3.0m);
        var expected= LemonadeStandResultObject.Failure(ResultTypeEnum.InsufficientCash, "").Result;
        // Act
        var actual = Company2.QueueOrder(CreateActionContext(company2BuysLemons,TestMarket,0)).Result;
        TestMarket.ProcessCompanyOrders();
        var marketTradesInPeriod = TestMarket.GetMarketTradesInPeriod(0);
        // Assert
        Assert.AreEqual(expected, actual);
        Assert.AreEqual(0, marketTradesInPeriod.Count);
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
        var marketToTest = TestMarket;
        const int expected_number_of_entries = 1;
        var company1 = Company.Factory.Create("Company 1", CompanyLevelEnum.Beginner);
        marketToTest.RegisterCompany(company1);
        var company2 = Company.Factory.Create("Company 2", CompanyLevelEnum.Beginner);
        marketToTest.RegisterCompany(company2);
        company1.GetInventory().AddGood(new InventoryEntry(lemon, 20, 3m,tradingPeriod));
        company2.GetInventory().AddGood(new InventoryEntry(lemon, 20, 3m,tradingPeriod));
        marketToTest.InitializeDemandForSpecificGood(lemon, 50);
        // Act
        var trade1 = new Order(test_initial_market, company1, lemon, 10, 10.0m);
        var trade2 = new Order(test_initial_market, company2, lemon, 10, 15.0m);
        var trade1Context = new ActionContext{TradeToSubmit = trade1,
                                                MarketToSubmitTo = marketToTest};   
        var trade2Context = new ActionContext{TradeToSubmit = trade2,
                                                MarketToSubmitTo = marketToTest};
        marketToTest.QueueOrder(trade1Context);
        marketToTest.QueueOrder(trade2Context);
        marketToTest.ProcessCompanyOrders();
        var actual_number_of_entries = test_initial_market.GetInventory().GetInventoryEntriesByGood(lemon.GoodName).Count();
        // Assert
        Assert.AreEqual(expected_number_of_entries, actual_number_of_entries);

        //remove companies from Market
        marketToTest.RemoveCompany(company1);
        marketToTest.RemoveCompany(company2);
    }
     [Test]
    public void CompaniesCanBuyGoodsFromEachOtherViaMatchingQueuedOrders()
    {
        // Arrange
        var period = 0;
        Company1.GetInventory().AddGood(new InventoryEntry(sugar,10,2.0m,period));
        Company2.GetInventory().AddGood(new InventoryEntry(sugar,10, 2.0m,period));
        
        var expectedCompany1Cash = Company1.GetCash() - 2;
        var expectedCompany2Cash = Company2.GetCash() + 2;
        var expectedCompany1SugarQuantity = 11;
        var expectedCompany2SugarQuantity = 10 - 1;

        var Company1BuysSugarFromCompany2 = new Order(Company1, Company2, sugar, 1, 2.0m);
        var Company2SellsSugarToCompany1 = new Order(Company1,Company2, sugar, 1, 2.0m);
        
        // Act
        Company1.QueueOrder(CreateActionContext(Company1BuysSugarFromCompany2,TestMarket,0));
        Company2.QueueOrder(CreateActionContext(Company2SellsSugarToCompany1,TestMarket,0));
        TestMarket.ProcessCompanyOrders();
        var actualCompany1Cash = Company1.GetCash();
        var actualCompany2Cash = Company2.GetCash();
        var actualCompany1SugarQuantity = Company1.GetInventory().GetInventoryEntries().FirstOrDefault(x => x.good == sugar && x.Cost==2.0m).quantity;
        var actualCompany2SugarQuantity = Company2.GetInventory().GetInventoryEntries().FirstOrDefault(x => x.good == sugar).quantity;
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
        var expected = LemonadeStandResultObject.Failure(ResultTypeEnum.InsufficientGoods, "").Result;
        var Company1BuysLemons = new Order(Company1, null, lemon, 10, 3.0m);
        var Company2SellsLemons = new Order(null, Company2, lemon, 10, 3.0m);
        Company2.GetInventory().Clear();

        // Act
        Company1.QueueOrder(CreateActionContext(Company1BuysLemons,TestMarket,0));
        var actual=Company2.QueueOrder(CreateActionContext(Company2SellsLemons,TestMarket,0)).Result;
        TestMarket.ProcessCompanyOrders();
        var marketTradesInPeriod = TestMarket.GetMarketTradesInPeriod(0);
        
        // Assert
        Assert.AreEqual(expected, actual);
        Assert.AreEqual(0, marketTradesInPeriod.Count);
    }

    [Test]
    public void MarketsShouldIgnoreTradesWhereMarketIsTheBuyerWhenCallingProcessCompanyOrders()
    {
        // Arrange
        lemonade.IsProducedGood = true;
        var radioactiveLemonade = Good.CreateInstance("Radioactive Lemonade", band2, RarityEnum.Very_Rare);
        radioactiveLemonade.IsProducedGood = true;

        Company1.GetInventory().AddGood(new InventoryEntry(radioactiveLemonade, 10, 3.0m, Period));
        Company2.GetInventory().AddGood(new InventoryEntry(lemonade, 10, 3.0m, Period));

        var radioactiveLemonadeTrade = new Order(TestMarket, Company1, radioactiveLemonade, 10, 3.0m);
        var radioactiveLemonadeContext = new ActionContext{TradeToSubmit = radioactiveLemonadeTrade, MarketToSubmitTo = TestMarket, Period = Period};

        var Company1BuysLemonadeFromCompany2 = new Order( Company1,Company2, lemonade, 10, 3.0m);
        var Company2SellsLemonadeToCompany1 = new Order(Company1,Company2, lemonade, 10, 3.0m);

        var expectedFilledQuantityForRadioactiveLemonade = 0;
        var expectedFilledQuantityForLemonade = 10;

        // Act
        TestMarket.QueueMarketOrder(radioactiveLemonadeContext);
        Company1.QueueOrder(CreateActionContext(Company1BuysLemonadeFromCompany2,TestMarket,Period));
        Company2.QueueOrder(CreateActionContext(Company2SellsLemonadeToCompany1,TestMarket,Period));

        TestMarket.ProcessCompanyOrders();
        var actualFilledQuantityForRadioactiveLemonade = radioactiveLemonadeTrade.FilledQuantity;
        var actualFilledQuantityForLemonade = Company1BuysLemonadeFromCompany2.FilledQuantity;
        // Assert
        Assert.AreEqual(expectedFilledQuantityForRadioactiveLemonade, actualFilledQuantityForRadioactiveLemonade);
        Assert.AreEqual(expectedFilledQuantityForLemonade, actualFilledQuantityForLemonade);
    }
   #endregion

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(lemon);
        UnityEngine.Object.DestroyImmediate(water);
        UnityEngine.Object.DestroyImmediate(sugar);
        UnityEngine.Object.DestroyImmediate(TestEconomy);
        UnityEngine.Object.DestroyImmediate(test_initial_market);
        Company1 = null;
        Company2 = null;
        TestMarket = null;
    }
}