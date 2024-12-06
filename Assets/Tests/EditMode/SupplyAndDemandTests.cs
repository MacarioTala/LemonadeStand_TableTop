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
        test_market = Market.Factory.CreateStarterMarket("The First Market"
                                                        , CompanyLevelEnum.Market
                                                        , new LinearDemandStrategy());             
        //Make the market demand lemons and lemonade
        test_market.InitializeDemandForSpecificGood(lemonade,1000);
        test_market.InitializeDemandForSpecificGood(lemon,1000);
    }
#region UpdatePrices tests
    [Test]
    public void UpdatePrices_increases_price_by_price_increment_rate_when_demand_threshold_is_reached()
    {
        // Arrange
        var testMarket = Market.Factory.CreateStarterMarket("Market To Test", CompanyLevelEnum.Market, new LinearDemandStrategy());
        var testPeriod = 0;
        testMarket.BuyGood(lemonade, 500,3.0m, testPeriod);
        var currentLemonadePrice = lemonade.GetPrice();
        var priceIncrementRate = lemonade.Get_price_increment_rate();
        var expectedLemonPrice = Math.Round(currentLemonadePrice * (1 + priceIncrementRate), 2);
        // Act
        testMarket.UpdatePrices();   
        // Only one entry per good in market inventories
        var actualLemonade = testMarket.GetInventory().GetInventoryEntriesByGood(lemonade.good_name).FirstOrDefault();
        var actualLemonadePrice = Math.Round(actualLemonade.good.GetPrice(),2);
        // Assert
        Assert.AreEqual(expectedLemonPrice, actualLemonadePrice);
    }

    [Test]
    public void If_market_buying_in_a_period_exceeds_demand_threshold_increase_prices()
    {
        // Arrange
        var marketToTest = Market.Factory.CreateStarterMarket("Market To Test", CompanyLevelEnum.Market, new LinearDemandStrategy());
        var test_period = 0;
        var currentLemonadePrice = lemonade.GetPrice();
        var price_increment_rate = lemonade.Get_price_increment_rate();
        var expectedLemonadePrice = Math.Round(currentLemonadePrice * (1 + price_increment_rate), 2);
        marketToTest.BuyGood(lemonade, 500,3.0m, test_period);
        marketToTest.BuyGood(lemonade, 500,3.0m, test_period);
        marketToTest.BuyGood(lemonade, 500,3.0m, test_period);
        // Act
        marketToTest.UpdatePrices();
        var actualLemonade = marketToTest.GetInventory().GetInventoryEntriesByGood(lemonade.good_name).FirstOrDefault();
        var actualLemonadePrice = Math.Round(actualLemonade.good.GetPrice(),2);
        // Assert
        Assert.AreEqual(expectedLemonadePrice, actualLemonadePrice);
    }
    #endregion
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
        var selling_company = Company.Factory.Create("Test Company", CompanyLevelEnum.Beginner);
        // Set up a market with demand for lemons
        Market MarketThatDemandsLemons = Market.Factory.CreateStarterMarket("Market That Demands Lemons", CompanyLevelEnum.Market, new LinearDemandStrategy());
        MarketThatDemandsLemons.InitializeDemandForSpecificGood(lemon, 1000);
        TheEconomy.Instance.RegisterCompany(MarketThatDemandsLemons);

        //give the selling company some lemons
        selling_company.BuyGood(lemon, 1000, 3.0m);
        TheEconomy.Instance.RegisterCompany(selling_company);
        //have the market buy the lemons
        TheEconomy.Instance.Queue_Trade(new Trade(MarketThatDemandsLemons, selling_company, lemon, 1000, 3.0m));
        TheEconomy.Instance.EndTradingPeriod();
        
        // Act
        var actual_fulfillment_rate = MarketThatDemandsLemons.GetMarketDemand()[lemon].FulfilmentRate;
        // Assert
        Assert.AreEqual(1, actual_fulfillment_rate);
    }
    [Test]
    public void CalculateFulfillmentRates_returns_less_than_1_when_demand_is_not_met()
    {
        // Arrange
        var selling_company = Company.Factory.Create("Test Company", CompanyLevelEnum.Beginner);
        //give the selling company some lemons
        selling_company.BuyGood(lemon, 500, 3.0m);
        TheEconomy.Instance.RegisterCompany(selling_company);

        //Make a market that demands lemons
        Market MarketThatDemandsLemons = Market.Factory.CreateStarterMarket("Market That Demands Lemons", CompanyLevelEnum.Market, new LinearDemandStrategy());
        MarketThatDemandsLemons.InitializeDemandForSpecificGood(lemon, 1000);
        TheEconomy.Instance.RegisterCompany(MarketThatDemandsLemons);

        //have the market buy some lemons
        TheEconomy.Instance.Queue_Trade(new Trade(MarketThatDemandsLemons, selling_company, lemon, 500, 3.0m));
        TheEconomy.Instance.EndTradingPeriod();
        //Act
        var actual_fulfillment_rate = MarketThatDemandsLemons.GetMarketDemand()[lemon].FulfilmentRate;
        // Assert
        Assert.Less(actual_fulfillment_rate, 1);
    }
#endregion    
    [Test]
    public void Adjust_Demand_increases_demand_when_demand_is_60_percent_filled()
    {
        //Assert
        //Make a market that demands lemons
        Market marketThatDemandsLemons = Market.Factory.CreateStarterMarket("Market That Demands Lemons", CompanyLevelEnum.Market, new LinearDemandStrategy());
        marketThatDemandsLemons.InitializeDemandForSpecificGood(lemon, 1000);
        TheEconomy.Instance.RegisterCompany(marketThatDemandsLemons);

        var expectedLemonDemand = 1400;
        var sellingCompany = Company.Factory.Create("Test Company", CompanyLevelEnum.Beginner);
        TheEconomy.Instance.RegisterCompany(sellingCompany);
        //give the selling company some lemons
        sellingCompany.BuyGood(lemon, 900, 3.0m);
        //Act
        //have the market buy some lemons
        TheEconomy.Instance.Queue_Trade(new Trade(marketThatDemandsLemons, sellingCompany, lemon, 600, 3.0m));
        TheEconomy.Instance.EndTradingPeriod();
        var actualLemonDemand = marketThatDemandsLemons.GetMarketDemand()[lemon].CurrentDemand;
        //Assert
        Assert.AreEqual(expectedLemonDemand, actualLemonDemand);
    }

    [Test]
    public void Adjust_demand_decreases_demand_when_demand_is_100_percent_filled()
    {
        //Assert
        Market marketToTest = (Market)TheEconomy.Instance.GetGlobalMarket();
        var initialLemonadeDemand = marketToTest.GetMarketDemand()[lemonade].CurrentDemand;
        var currentLinearDemandAdjustment = 0.9;
        var expectedLemonadeDemand = initialLemonadeDemand * currentLinearDemandAdjustment;
        var selling_company = Company.Factory.Create("Test Company", CompanyLevelEnum.Beginner);
        TheEconomy.Instance.RegisterCompany(selling_company);
        //give the selling company some lemonade
        selling_company.BuyGood(lemonade, 1000, 3.0m);
        //Act
        //have the market buy some lemonade
        TheEconomy.Instance.Queue_Trade(new Trade ( buyer: marketToTest,
                                                    seller: selling_company, 
                                                    good: lemonade, 
                                                    quantity: 1000, 
                                                    price: 3.0m));
        TheEconomy.Instance.EndTradingPeriod();
        var actualLemonadeDemand = marketToTest.GetMarketDemand()[lemonade].CurrentDemand;
        //Assert
        Assert.AreEqual(expectedLemonadeDemand, actualLemonadeDemand);
    }

    [Test]
    public void GetTotalSupply_returns_total_supply_of_good_in_inventory()
    {
        // Arrange
        var test_market = Market.Factory.CreateMarket("Test Market", CompanyLevelEnum.Market, new LinearDemandStrategy());
        test_market.SetCash(1000000);
        test_market.BuyGood(lemon, 500,3.0m, 0);
        test_market.BuyGood(lemon, 500,3.0m, 1);
        test_market.BuyGood(lemon, 500,3.0m, 2);
        const int expected = 1500;
        // Act
        var actual = test_market.GetTotalSupply(lemon);

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