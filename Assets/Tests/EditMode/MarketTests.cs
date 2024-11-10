using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;

[TestFixture]
public class MarketTests
{
    public Market test_market;
    Good lemon;
    Good water;
    Good sugar;
    readonly Price_band band1 = new(.5f, 1f);
    readonly Price_band band2 = new(1f, 3f);
    readonly Price_band band3 = new(3f, 5f);
    readonly Price_band band4 = new(5f, 10f);
    readonly List<Good> test_goods = new();
    readonly ITradeLogger trade_logger = new MockLogger();

    GameObject TestEconomy;

    [SetUp]
    public void Setup()
    {
        test_market = ScriptableObject.CreateInstance<Market>();
        test_market.Initialize("Test Market", CompanyLevelEnum.Global);
        lemon = Good.CreateInstance("Lemon", band2, Rarity_enum.Common);
        water = Good.CreateInstance("Water", band1, Rarity_enum.Common);
        sugar = Good.CreateInstance("Sugar", band1, Rarity_enum.Common);
        test_goods.Add(lemon);
        test_goods.Add(water);
        test_goods.Add(sugar);
        TestEconomy = new GameObject("TestEconomy");
        var economyComponent=TestEconomy.AddComponent<TheEconomy>();
        if (TheEconomy.Instance == null)
        {
            economyComponent.Initialize(trade_logger);
        }
        Assert.IsNotNull(TheEconomy.Instance, "TheEconomy singleton instance was not initialized.");
    }

    [Test]
    public void UpdatePrices_increases_price_when_demand_threshold_is_reached()
    {
        // Arrange
        var test_period = 0;
        test_market.BuyGood(lemon, 500,3f, test_period);
        var current_lemon_price = lemon.Get_price();
        var price_increment_rate = lemon.Get_price_increment_rate();
        var expected_lemon_price = Math.Round(current_lemon_price * (1 + price_increment_rate), 2);
        // Act
        test_market.UpdatePrices();        
        var actual_lemon_price = Math.Round(test_market.Get_inventory().Get_inventory_items().Find(item => item.good.good_name == lemon.good_name).acquisition_price,2);
        // Assert
        Assert.AreEqual(expected_lemon_price, actual_lemon_price);
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