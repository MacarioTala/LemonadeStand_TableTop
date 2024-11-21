using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

[TestFixture]
public class SupplyAndDemandTests
{
    public Market test_market;
    Good lemon;
    Good water;
    Good sugar;
    Good lemonade;
    readonly Price_band band1 = new(.5m, 1.0m);
    readonly Price_band band2 = new(1.0m, 3.0m);
    readonly Price_band band3 = new(3.0m, 5.0m);
    readonly Price_band band4 = new(5.0m, 10.0m);
    readonly ITradeLogger trade_logger = new MockLogger();

    GameObject TestEconomy;

    [SetUp]
    public void Setup()
    {
        // Set up the Economy
        TestEconomy = new GameObject("TestEconomy");
        var economyComponent=TestEconomy.AddComponent<TheEconomy>();
        if (TheEconomy.Instance == null)
        {
            economyComponent.Initialize(trade_logger);
        }
        // Set up goods
        lemon = Good.CreateInstance("Lemon", band2, Rarity_enum.Common);
        water = Good.CreateInstance("Water", band1, Rarity_enum.Common);
        sugar = Good.CreateInstance("Sugar", band1, Rarity_enum.Common);
        lemonade = Good.CreateInstance("Lemonade", band3, Rarity_enum.Uncommon);

        // set test_market to the Initial Market
        test_market = TheEconomy.Instance.companies.Find(company => company.company_name == "The First Market") as Market;
        //Make the market demand lemons and lemonade
        test_market.InitializeDemand(lemonade,1000,10,10000);
        test_market.InitializeDemand(lemon,1000,10,10000);
    }
#region UpdatePrices tests
    [Test]
    public void UpdatePrices_increases_price_by_price_increment_rate_when_demand_threshold_is_reached()
    {
        // Arrange
        var test_period = 0;
        test_market.BuyGood(lemon, 500,3.0m, test_period);
        var current_lemon_price = lemon.GetPrice();
        var price_increment_rate = lemon.Get_price_increment_rate();
        var expected_lemon_price = Math.Round(current_lemon_price * (1 + price_increment_rate), 2);
        // Act
        test_market.UpdatePrices();   
        // Only one entry per good in market inventories
        var actual_lemon = test_market.GetInventory().GetInventoryEntriesByGood(lemon.good_name).FirstOrDefault();
        var actual_lemon_price = Math.Round(actual_lemon.good.GetPrice(),2);
        // Assert
        Assert.AreEqual(expected_lemon_price, actual_lemon_price);
    }

    [Test]
    public void If_market_buying_in_a_period_exceeds_demand_threshold_increase_prices()
    {
        // Arrange
        var test_period = 0;
        var current_lemon_price = lemon.GetPrice();
        var price_increment_rate = lemon.Get_price_increment_rate();
        var expected_lemon_price = Math.Round(current_lemon_price * (1 + price_increment_rate), 2);
        test_market.BuyGood(lemon, 500,3.0m, test_period);
        test_market.BuyGood(lemon, 500,3.0m, test_period);
        test_market.BuyGood(lemon, 500,3.0m, test_period);
        // Act
        test_market.UpdatePrices();
        var actual_lemon = test_market.GetInventory().GetInventoryEntriesByGood(lemon.good_name).FirstOrDefault();
        var actual_lemon_price = Math.Round(actual_lemon.good.GetPrice(),2);
        // Assert
        Assert.AreEqual(expected_lemon_price, actual_lemon_price);
    }
    #endregion
    [Test]
    public void GetTotalBought_returns_total_amount_of_good_bought_in_a_period()
    {
        // Arrange
        test_market.BuyGood(lemon, 500,3.0m, 0);
        test_market.BuyGood(lemon, 500,3.0m, 1);
        test_market.BuyGood(lemon, 500,3.0m, 2);
        const int expected = 1500;
        const int trading_period = 0;
        // Act
        var actual = test_market.GetTotalBought(trading_period,lemon);
        // Assert
        Assert.AreEqual(expected, actual);
    }
    [Test]
    public void GetTotalSold_returns_total_amount_of_good_sold_in_a_period()
    {
        // Arrange
        test_market.BuyGood(lemon, 5000,3.0m, 0);
        test_market.SellGood(lemon, 500,3.0m, 0);
        test_market.SellGood(lemon, 500,3.0m, 0);
        test_market.SellGood(lemon, 500,3.0m, 0);
        const int expected = 1500;
        const int trading_period = 0;
        // Act
        var actual = test_market.GetTotalSold(trading_period,lemon);
        // Assert
        Assert.AreEqual(expected, actual);
    }
#region CalculateFulfillmentRate tests
    [Test]
    public void CalculateFulfillmentRate_returns_1_when_demand_is_met()
    {
        // Arrange
        var selling_company = ScriptableObject.CreateInstance<Company>();
        selling_company.Initialize("Test Company", CompanyLevelEnum.Beginner);
        // Set up a market with demand for lemons
        Market MarketThatDemandsLemons = Market.Factory.CreateMarket("Market That Demands Lemons", CompanyLevelEnum.Market, new LinearDemandStrategy());
        MarketThatDemandsLemons.InitializeDemand(lemon, 1000);
        TheEconomy.Instance.Register_Company(MarketThatDemandsLemons);

        //give the selling company some lemons
        selling_company.BuyGood(lemon, 1000, 3.0m);
        TheEconomy.Instance.Register_Company(selling_company);
        //have the market buy the lemons
        TheEconomy.Instance.Queue_Trade(new Trade(MarketThatDemandsLemons, selling_company, lemon, 1000, 3.0m));
        TheEconomy.Instance.EndTradingPeriod();
        
        // Act
        var actual_fulfillment_rate = MarketThatDemandsLemons.MarketDemand[lemon].FulfilmentRate;
        // Assert
        Assert.AreEqual(1, actual_fulfillment_rate);
    }
    [Test]
    public void CalculateFulfillmentRates_returns_less_than_1_when_demand_is_not_met()
    {
        // Arrange
        var selling_company = ScriptableObject.CreateInstance<Company>();
        selling_company.Initialize("Test Company", CompanyLevelEnum.Beginner);
        //give the selling company some lemons
        selling_company.BuyGood(lemon, 500, 3.0m);
        TheEconomy.Instance.Register_Company(selling_company);

        //Make a market that demands lemons
        Market MarketThatDemandsLemons = Market.Factory.CreateMarket("Market That Demands Lemons", CompanyLevelEnum.Market, new LinearDemandStrategy());
        MarketThatDemandsLemons.InitializeDemand(lemon, 1000);
        TheEconomy.Instance.Register_Company(MarketThatDemandsLemons);

        //have the market buy some lemons
        TheEconomy.Instance.Queue_Trade(new Trade(MarketThatDemandsLemons, selling_company, lemon, 500, 3.0m));
        TheEconomy.Instance.EndTradingPeriod();
        //Act
        var actual_fulfillment_rate = MarketThatDemandsLemons.MarketDemand[lemon].FulfilmentRate;
        // Assert
        Assert.Less(actual_fulfillment_rate, 1);
    }
#endregion    
    [Test]
    public void Adjust_Demand_increases_demand_when_demand_is_60_percent_filled()
    {
        //Assert
        var expected_lemon_demand = 1400;
        var selling_company = ScriptableObject.CreateInstance<Company>();
        selling_company.Initialize("Test Company", CompanyLevelEnum.Beginner);
        TheEconomy.Instance.Register_Company(selling_company);
        //give the selling company some lemons
        selling_company.BuyGood(lemon, 900, 3.0m);
        //Act
        //have the market buy some lemons
        TheEconomy.Instance.Queue_Trade(new Trade(test_market, selling_company, lemon, 600, 3.0m));
        TheEconomy.Instance.EndTradingPeriod();
        var actual_lemon_demand = test_market.MarketDemand[lemon].CurrentDemand;
        //Assert
        Assert.AreEqual(expected_lemon_demand, actual_lemon_demand);
    }

    [Test]
    public void Adjust_demand_decreases_demand_when_demand_is_100_percent_filled()
    {
        //Assert
        var expected_lemon_demand = 900;
        var selling_company = ScriptableObject.CreateInstance<Company>();
        selling_company.Initialize("Test Company", CompanyLevelEnum.Beginner);
        TheEconomy.Instance.Register_Company(selling_company);
        //give the selling company some lemons
        selling_company.BuyGood(lemon, 1000, 3.0m);
        //Act
        //have the market buy some lemons
        TheEconomy.Instance.Queue_Trade(new Trade(test_market, selling_company, lemon, 1000, 3.0m));
        TheEconomy.Instance.EndTradingPeriod();
        var actual_lemon_demand = test_market.MarketDemand[lemon].CurrentDemand;
        //Assert
        Assert.AreEqual(expected_lemon_demand, actual_lemon_demand);
    }

    [Test]
    public void GetTotalSupply_returns_total_supply_of_good_in_inventory()
    {
        // Arrange
        test_market.BuyGood(lemon, 500,3.0m, 0);
        test_market.BuyGood(lemon, 500,3.0m, 1);
        test_market.BuyGood(lemon, 500,3.0m, 2);
        const int expected = 1500;
        // Act
        var actual = test_market.GetTotalSupply(0, lemon);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TearDown]
    public void TearDown()
    {
        ScriptableObject.DestroyImmediate(test_market);
        ScriptableObject.DestroyImmediate(lemon);
        ScriptableObject.DestroyImmediate(water);
        ScriptableObject.DestroyImmediate(sugar);
        GameObject.DestroyImmediate(TestEconomy);
    }
}