using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
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
        test_initial_market = (Market)test_economy.GetGlobalMarket();
        test_initial_market.InitializeDemand(lemon, 1000);
    }

    private void SetupGoodsAndRecipes()
    {
        lemon = Good.CreateInstance("Lemon", band2, Rarity_enum.Common);
        water = Good.CreateInstance("Water", band1, Rarity_enum.Common);
        sugar = Good.CreateInstance("Sugar", band1, Rarity_enum.Common);
        lemonade = Good.CreateInstance("Lemonade", band2, Rarity_enum.Uncommon);
        lemonade_recipe = new Recipe(lemonade, new List<Ingredient> { new(lemon, 9), 
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
                     where company.company_name == "The First Market"
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
        var actual = test_initial_market.MarketDemand.First();
        // Assert
        Assert.AreEqual(expected_good_name, actual.Key.good_name);
        Assert.AreEqual(expected_demand, actual.Value.CurrentDemand);
    }
    #endregion

    [Test]
    public void ConsumeGoods_should_decrease_inventory()
    {
        // Arrange
        var initialLemons = 10000;
        var lemonDemand = test_initial_market.GetDemand(lemon.good_name);
        var expected = initialLemons - lemonDemand;
        test_initial_market.BuyGood(lemon,initialLemons,3.0m);
        // Act
        test_initial_market.ConsumeGoods();
        var actual = test_initial_market.GetInventory().GetInventoryEntriesByGood(lemon.good_name).First().quantity;
        // Assert
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void Markets_should_only_have_a_single_InventoryEntry_per_good()
    {
        //As of 11/17/2023, Markets don't care about optimizing 
        //the price that they buy goods at
        //they only care about the quantity of goods they have
        //So there should only be one InventoryEntry per good
        
        // Arrange
        const int expected_number_of_entries = 1;
        var company1 = ScriptableObject.CreateInstance<Company>();
        company1.Initialize("Company1", CompanyLevelEnum.Beginner);
        test_economy.Register_Company(company1);
        var company2 = ScriptableObject.CreateInstance<Company>();
        company2.Initialize("Company2", CompanyLevelEnum.Beginner);
        test_economy.Register_Company(company2);
        company1.BuyGood(lemon, 10,3.0m);
        company2.BuyGood(lemon, 10,3.0m);
        test_initial_market.BuyGood(lemon, 1000, 3.0m);
        test_initial_market.InitializeDemand(lemon, 50);
        // Act
        var trade1 = new Trade(test_initial_market, company1, lemon, 10, 10.0m);
        var trade2 = new Trade(test_initial_market, company2, lemon, 10, 15.0m);
        TheEconomy.Instance.Queue_Trade(trade1);
        TheEconomy.Instance.Queue_Trade(trade2);
        TheEconomy.Instance.EndTradingPeriod();
        var actual_number_of_entries = test_initial_market.GetInventory().GetInventoryEntriesByGood(lemon.good_name).Count();
        // Assert
        Assert.AreEqual(expected_number_of_entries, actual_number_of_entries);
    }
   
   #region Production tests
   [Test]
   public void Companies_cannot_make_goods_without_a_recipe()
   {
       // Arrange
       var company = ScriptableObject.CreateInstance<Company>();
       company.Initialize("Company1", CompanyLevelEnum.Beginner);
       test_economy.Register_Company(company);
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
    }

   #endregion

   #region publish tests
    [Test]
    public void PublishMarketDataShouldAddOnePercentToPrice()
    {
        // Arrange
        test_initial_market.BuyGood(lemon, 1000, 3.0m);
        var initial_price = test_initial_market.GetInventory().GetInventoryEntriesByGood(lemon.good_name).First().good.GetPrice();
        var expected = initial_price * 1.01m;
        // Act
        test_initial_market.CalculateMarketData();
        var marketData = test_initial_market.MarketData;
        var actual = marketData.Where(entry => entry.Good.good_name == lemon.good_name
                                        && entry.Company.company_name == test_initial_market.company_name)
                                    .First().Ask;
        // Assert
        Assert.AreEqual(expected, actual);
    }
    [Test]
    public void PublishSpreadToMarket_should_update_existing_MarketData_if_spread_exists()
    {
        // Arrange
        var company1 = ScriptableObject.CreateInstance<Company>();
        company1.Initialize("Company1", CompanyLevelEnum.Beginner);
        test_economy.Register_Company(company1);
        var context = new ActionContext{BidToSubmit = 2.0m, AskToSubmit = 3.0m, GoodToSubmit = lemon, MarketToSubmitTo = test_initial_market};
        var expected = new List<MarketData>{new() { Good = lemon, Company = company1, Bid = 2.0m, Ask = 3.0m}};
        //Act
        company1.SubmitBidAskSpreadToMarket(context);
        test_initial_market.CalculateMarketData();
        var actual = test_initial_market.MarketData;
        // Assert
        Assert.AreEqual(expected, actual);
    }
    [Test]
    public void PublishSpreadToMarketThrowsContextExceptionIfActionContextIsIncomplete()
    {
        // Arrange
        var company1 = ScriptableObject.CreateInstance<Company>();
        company1.Initialize("Company1", CompanyLevelEnum.Beginner);
        test_economy.Register_Company(company1);
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
    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(lemon);
        UnityEngine.Object.DestroyImmediate(water);
        UnityEngine.Object.DestroyImmediate(sugar);
        UnityEngine.Object.DestroyImmediate(test_economy);
    }
}