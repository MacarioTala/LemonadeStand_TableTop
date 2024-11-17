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
    Good lemon;
    Good water;
    Good sugar;

    readonly Price_band band1 = new(.5f, 1f);
    readonly Price_band band2 = new(1f, 3f);
    readonly ITradeLogger trade_logger = new MockLogger();
    readonly List<Good> test_goods = new();

    [SetUp]
    public void SetUp()
    {
        var economy_object = new GameObject();
        test_economy = economy_object.AddComponent<TheEconomy>();
        test_economy.Initialize(trade_logger);
        test_initial_market = (Market)test_economy.GetGlobalMarket(); 
        lemon = Good.CreateInstance("Lemon", band2, Rarity_enum.Common);
        water = Good.CreateInstance("Water", band1, Rarity_enum.Common);
        sugar = Good.CreateInstance("Sugar", band1, Rarity_enum.Common);
        test_goods.Add(lemon);
        test_goods.Add(water);
        test_goods.Add(sugar);
        //Make the market demand lemons
        test_initial_market.InitializeDemand(lemon, 1000);
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
        test_initial_market.BuyGood(lemon,initialLemons,3f);
        // Act
        test_initial_market.ConsumeGoods();
        var actual = test_initial_market.Get_inventory().Get_inventory_items().FirstOrDefault(x => x.good.good_name == "Lemon").quantity;
        // Assert
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void Markets_should_only_have_a_single_InventoryEntry_per_good()
    {
        throw new NotImplementedException();
    }
    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(lemon);
        UnityEngine.Object.DestroyImmediate(water);
        UnityEngine.Object.DestroyImmediate(sugar);
        UnityEngine.Object.DestroyImmediate(test_economy);
    }
}