using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class TheEconomyTests
{
    private TheEconomy test_economy;

    private Market test_initial_market;
    Good lemon;
    Good water;
    Good sugar;

    readonly Price_band band1 = new(.5m, 1.0m);
    readonly Price_band band2 = new(1.0m, 3.0m);
    readonly ITradeLogger trade_logger = new MockLogger();
    readonly List<Good> test_goods = new();

    [SetUp]
    public void SetUp()
    {
        //Create the economy
        var market_object = new GameObject();
        test_economy = market_object.AddComponent<TheEconomy>();
        test_economy.Initialize(trade_logger);
        test_initial_market = (Market)test_economy.GetGlobalMarket(); 

        lemon = Good.CreateInstance("Lemon", band2, Rarity_enum.Common);
        water = Good.CreateInstance("Water", band1, Rarity_enum.Common);
        sugar = Good.CreateInstance("Sugar", band1, Rarity_enum.Common);
        lemon.ExpiresAfterPeriods = 1;
        test_goods.Add(lemon);
        test_goods.Add(water);
        test_goods.Add(sugar);

        //make the market demand a thousand lemons
        test_initial_market.InitializeDemand(lemon, 1000);
    }
#region  Initialization tests
  [Test]
    public void Make_sure_the_first_market_exists_in_TheEconomy_with_proper_params()
    {
        // Arrange
        var expected_name = "The First Market";
        // Act
        var actual = test_economy.GetGlobalMarket().company_name;
        // Assert
        Assert.AreEqual(expected_name, actual);
    }

    [Test]
    public void The_initial_market_should_be_instantiated_as_a_market()
    {
        // Arrange
        var expected = typeof(Market);
        // Act
        var actual = test_initial_market.GetType();
        // Assert
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void When_TheEconomy_is_initialized_goods_are_created_in_InitialMarket()
    {
        // Arrange
        var expected = "Lemon";
        // Act
        test_economy.Create_initial_goods(test_goods);
        var actual = test_initial_market.GetInventory().GetInventoryEntries().FirstOrDefault(x => x.good.good_name == "Lemon").good.good_name;
        // Assert
        Assert.AreEqual(expected, actual);
    }
#endregion
    [Test]
    public void Register_Company_adds_company_to_companies_list_if_no_companies_are_registered()
    {
        // Arrange
        var company = ScriptableObject.CreateInstance<Company>();
        company.company_name = "Test Company";
        var expected = test_economy.companies.Count + 1;
        // Act
        test_economy.RegisterCompany(company);
        var actual = test_economy.companies.Count;
        
        // Assert
        Assert.AreEqual(expected, actual);
    }
    [Test]
    public void Register_Company_does_not_add_company_to_companies_list_if_company_already_registered()
    {
        // Arrange
        var company = ScriptableObject.CreateInstance<Company>();
        company.company_name = "Test Company";
        test_economy.RegisterCompany(company);
        var company2 = ScriptableObject.CreateInstance<Company>();
        company2.company_name = "Test Company";
        // Act
        // Assert
        Assert.Throws<TheEconomy_CompanyException>(() => test_economy.RegisterCompany(company2));
    }

    [Test]
    public void Create_initial_goods_creates_1to1000_goods_if_rarity_is_common()
    {
        // Arrange
        const int expected_floor = 1;
        const int expected_ceiling = 1000;
        // Act
        test_economy.Create_initial_goods(test_goods);
        var actual_good = test_initial_market.GetInventory().GetInventoryEntriesByGood(lemon.good_name).FirstOrDefault();
        var actual_quantity = actual_good.quantity;
        // Assert
        Assert.IsTrue(actual_quantity >= expected_floor && actual_quantity <= expected_ceiling, 
        "Expected between:"+expected_floor+" and "+
        expected_ceiling + 
        "Actual quantity: " + actual_quantity ); 
    }

    [Test]
    public void ExecuteDailyTrades_should_consider_market_buys_when_consuming_goods()
    {
        //For instance, if the demand for lemons is 1000
        //and the market buys 500 lemons, 
        //ConsumeGoods should only consume 500 lemons
        // Arrange
        var initialLemons = 1000;
        var lemonsCompanyWillSellToMarket = 500;
        test_initial_market.BuyGood(lemon,initialLemons,3.0m);
        var test_company = Company.Factory.Create("Test Company", CompanyLevelEnum.Beginner);
        test_economy.RegisterCompany(test_company);
        test_company.BuyGood(lemon,1000,2.0m);
        var lemonDemand = test_initial_market.GetDemand(lemon.good_name);
        //next line is necessary because of different demand strategies 
        //that will change the demand
        var lemon_quantity_if_ConsumeGoods_ignores_market_buys = initialLemons - lemonDemand;
        // Act
        var lemonSale = new Trade(test_initial_market, test_company, lemon, lemonsCompanyWillSellToMarket, 3.0m);
        TheEconomy.Instance.Queue_Trade(lemonSale);
        TheEconomy.Instance.EndTradingPeriod();
        //only one inventory entry per good in Markets
        var actual_final_market_lemons = test_initial_market.GetInventory().GetInventoryEntriesByGood(lemon.good_name).FirstOrDefault();
        // Assert
        Assert.AreNotEqual(lemon_quantity_if_ConsumeGoods_ignores_market_buys, actual_final_market_lemons.quantity);
    }

#region TradeTests
    [Test]
    public void A_Company_buying_a_good_from_another_company_via_queue_can_be_initiated_by_queue_trade()
    {
        // Arrange
        var company1 = Company.Factory.Create("Company1", CompanyLevelEnum.Beginner);
        var company2 = Company.Factory.Create("Company2", CompanyLevelEnum.Beginner);
        test_economy.RegisterCompany(company1);
        test_economy.RegisterCompany(company2);
        company1.BuyGood(sugar, 10, 2.0m);
        company2.BuyGood(sugar, 10, 2.0m);
        var expected_company1_cash = company1.Get_cash() - 2;
        var expected_company2_cash = company2.Get_cash() + 2;
        var expected_company1_sugar_quantity = 11;
        var expected_company2_sugar_quantity = 10 - 1;

        // Act
        var trade = new Trade(company1, company2, sugar, 1, 2.0m);
        test_economy.Queue_Trade(trade);
        test_economy.EndTradingPeriod();
        var actual_company1_cash = company1.Get_cash();
        var actual_company2_cash = company2.Get_cash();
        var actual_company1_sugar_quantity = company1.GetInventory().GetInventoryEntries().FirstOrDefault(x => x.good == sugar && x.acquisition_price==2.0m).quantity;
        var actual_company2_sugar_quantity = company2.GetInventory().GetInventoryEntries().FirstOrDefault(x => x.good == sugar).quantity;
        // Assert
        Assert.AreEqual(expected_company1_cash, actual_company1_cash);
        Assert.AreEqual(expected_company2_cash, actual_company2_cash);
        Assert.AreEqual(expected_company1_sugar_quantity, actual_company1_sugar_quantity);
        Assert.AreEqual(expected_company2_sugar_quantity, actual_company2_sugar_quantity);
    }

    [Test]
    public void A_Company_cannot_sell_a_good_if_it_has_insufficient_inventory()
    {
        // Arrange
        var company1 = Company.Factory.Create("Company1", CompanyLevelEnum.Beginner);
        var company2 = Company.Factory.Create("Company2", CompanyLevelEnum.Beginner);
        test_economy.RegisterCompany(company1);
        test_economy.RegisterCompany(company2);
        var expected = "Company does not have enough of the good to sell";
        string actual = null;
        
        // Act
        try{
        TheEconomy.Instance.Queue_Trade(new Trade(company1, company2, lemon, 10, 3.0m));
        TheEconomy.Instance.EndTradingPeriod();
        }
        catch(Exception e)
        {
            actual = e.Message;
        }
        // Assert
        Assert.AreEqual(expected, actual);


    }

    [Test]
    public void A_Company_cannot_buy_a_good_if_it_has_insufficient_cash()
    {
        // Arrange
        var company1 = Company.Factory.Create("Company1", CompanyLevelEnum.Beginner);
        var company2 = Company.Factory.Create("Company2", CompanyLevelEnum.Beginner);
        test_economy.RegisterCompany(company1);
        test_economy.RegisterCompany(company2);
        company1.BuyGood(lemon, 4000, 1.0m);
        const string expected="Insufficient funds to buy good";
        string actual=null;
        // Act
        try{
        TheEconomy.Instance.Queue_Trade(new Trade(company2, company1, lemon, 4000, 3.0m));
        TheEconomy.Instance.EndTradingPeriod();
        }
        catch(Exception e)
        {
            // Assert
            actual=e.Message;    
        }
        Assert.AreEqual(expected, actual);
    }

#endregion
#region Perishability tests
[Test]
public void PerishableGoodsShouldExpire()
{
    // Arrange
    var company = Company.Factory.Create("Company1", CompanyLevelEnum.Beginner);
    test_economy.RegisterCompany(company);
    company.BuyGood(lemon, 10, 3.0m,0);
    company.BuyGood(water, 10, 1.0m,0);
    company.BuyGood(sugar, 10, 1.0m,0);
    var expected_lemon_quantity = 0;
    // Act
    test_economy.tradingPeriod = 0;
    test_economy.EndTradingPeriod(); //nothing expires yet, they just bought the goods
    test_economy.EndTradingPeriod(); //lemons should expire
    var lemon_entry = company.GetInventory().GetInventoryEntriesByGood(lemon.good_name).FirstOrDefault();
    var actual_lemon_quantity = lemon_entry?.quantity??0;
    // Assert
    Assert.AreEqual(expected_lemon_quantity, actual_lemon_quantity);
}

[Test]
public void NonPerishableGoodsShouldNotExpire()
{
     // Arrange
    var company = Company.Factory.Create("Company1", CompanyLevelEnum.Beginner);
    test_economy.RegisterCompany(company);
    company.BuyGood(lemon, 10, 3.0m,0);
    company.BuyGood(water, 10, 1.0m,0);
    company.BuyGood(sugar, 10, 1.0m,0);
    var expected_water_quantity = 10;
    // Act
    test_economy.tradingPeriod = 0;
    test_economy.EndTradingPeriod(); //lemons should expire
    var actual_water_quantity = company.GetInventory().GetInventoryEntriesByGood(water.good_name).FirstOrDefault().quantity;
    // Assert
    Assert.AreEqual(expected_water_quantity, actual_water_quantity);
}

[Test]
public void OnlyPerishableGoodsAtTheirExpiryPeriodShouldExpire()
{
    // Arrange
    var company = Company.Factory.Create("Company1", CompanyLevelEnum.Beginner);
    test_economy.RegisterCompany(company);
    company.BuyGood(lemon, 10, 3.0m,0);
    lemon.ExpiresAfterPeriods=2;
    var ripeLemon = Good.CreateInstance("Ripe Lemon", band2, Rarity_enum.Common);
    ripeLemon.ExpiresAfterPeriods=1;
    company.BuyGood(ripeLemon, 10, 3.0m,0);
    var expected_lemon_quantity = 10;
    var expected_ripeLemon_quantity = 0;
    // Act
    test_economy.tradingPeriod = 0;
    test_economy.EndTradingPeriod(); //nothing expires yet, they just bought the goods
    test_economy.EndTradingPeriod(); //ripe lemons should expire
    var actualLemonEntries = company.GetInventory().GetInventoryEntriesByGood(lemon.good_name).FirstOrDefault();
    var actualRipeLemonEntries = company.GetInventory().GetInventoryEntriesByGood(ripeLemon.good_name).FirstOrDefault();
    var actual_lemon_quantity = actualLemonEntries?.quantity??0;
    var actual_ripeLemon_quantity = actualRipeLemonEntries?.quantity??0;
    // Assert
    Assert.AreEqual(expected_lemon_quantity, actual_lemon_quantity);
    Assert.AreEqual(expected_ripeLemon_quantity, actual_ripeLemon_quantity);
}

[Test]
public void TheSameGoodBoughtAtDifferentTimesExpiresAtDifferentPeriods()
{
    // Arrange
    var company = Company.Factory.Create("Company1", CompanyLevelEnum.Beginner);
    test_economy.RegisterCompany(company);
    company.BuyGood(lemon, 10, 3.0m,0);
    lemon.ExpiresAfterPeriods=2;
    company.BuyGood(lemon, 10, 3.0m,1);
    var expected_lemon_quantity = 10;
    // Act
    test_economy.tradingPeriod = 0;
    test_economy.EndTradingPeriod(); //First batch of lemons expires
    var actual_lemon_entry = company.GetInventory().GetInventoryEntriesByGood(lemon.good_name).FirstOrDefault();
    var actual_lemon_quantity = actual_lemon_entry?.quantity??0;
    // Assert
    Assert.AreEqual(expected_lemon_quantity, actual_lemon_quantity);
    }

#endregion
    [TearDown]
    public void TearDown()
    {
        test_economy.ClearEconomy();
        UnityEngine.Object.DestroyImmediate(test_economy.gameObject);
        UnityEngine.Object.DestroyImmediate(test_initial_market);
    }


}
#region stubs
public class MockLogger : ITradeLogger
{
    public void LogTrade(Trade trade)
    {
    }
    public void SaveDailySummary(List<Trade> trade_queue)
    {
    }
}
#endregion