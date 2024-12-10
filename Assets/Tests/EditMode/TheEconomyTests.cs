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
        test_initial_market.InitializeDemandForSpecificGood(lemon, 1000);
    }
#region  Initialization tests
  [Test]
    public void Make_sure_the_first_market_exists_in_TheEconomy_with_proper_params()
    {
        // Arrange
        var expected_name = "The First Market";
        // Act
        var actual = test_economy.GetGlobalMarket().Name;
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
        company.Name = "Test Company";
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
        company.Name = "Test Company";
        test_economy.RegisterCompany(company);
        var company2 = ScriptableObject.CreateInstance<Company>();
        company2.Name = "Test Company";
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

    [TearDown]
    public void TearDown()
    {
        test_economy.ClearEconomy();
        UnityEngine.Object.DestroyImmediate(test_economy.gameObject);
        UnityEngine.Object.DestroyImmediate(test_initial_market);
    }


}
