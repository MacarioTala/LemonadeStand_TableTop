using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class The_MarketTests
{
    private The_Market test_market;

    private Company TheGlobalMarket_for_testing;
    Good lemon;
    Good water;
    Good sugar;

    readonly Price_band band1 = new(.5f, 1f);
    readonly Price_band band2 = new(1f, 3f);
    readonly Price_band band3 = new(3f, 5f);
    readonly Price_band band4 = new(5f, 10f);
    readonly ITradeLogger trade_logger = null;
    readonly List<Good> test_goods = new();

    [SetUp]
    public void SetUp()
    {
        var market_object = new GameObject();
        test_market = market_object.AddComponent<The_Market>();
        test_market.Initialize(trade_logger);
        TheGlobalMarket_for_testing = test_market.GetGlobalMarket(); 
        lemon = Good.CreateInstance("Lemon", band2, Rarity_enum.Common);
        water = Good.CreateInstance("Water", band1, Rarity_enum.Common);
        sugar = Good.CreateInstance("Sugar", band1, Rarity_enum.Common);
        test_goods.Add(lemon);
        test_goods.Add(water);
        test_goods.Add(sugar);
    }

    [Test]
    public void Register_Company_adds_company_to_companies_list_if_no_companies_are_registered()
    {
        // Arrange
        var company = ScriptableObject.CreateInstance<Company>();
        company.company_name = "Test Company";
        var expected = test_market.companies.Count + 1;
        // Act
        test_market.Register_Company(company);
        var actual = test_market.companies.Count;
        
        // Assert
        Assert.AreEqual(expected, actual);
    }
    [Test]
    public void Register_Company_does_not_add_company_to_companies_list_if_company_already_registered()
    {
        // Arrange
        var company = ScriptableObject.CreateInstance<Company>();
        company.company_name = "Test Company";
        test_market.Register_Company(company);
        var company2 = ScriptableObject.CreateInstance<Company>();
        company2.company_name = "Test Company";
        // Act
        // Assert
        Assert.Throws<TheMarket_CompanyException>(() => test_market.Register_Company(company2));
    }

    [Test]
    public void Create_initial_goods_creates_1to1000_goods_if_rarity_is_common()
    {
        // Arrange
        const int expected_floor = 1;
        const int expected_ceiling = 1000;
        // Act
        test_market.Create_initial_goods(test_goods);
        var actual_good = TheGlobalMarket_for_testing.Get_inventory().Get_inventory_items().FirstOrDefault(x => x.good.good_name == "Lemon");
        var actual_quantity = actual_good.quantity;
        // Assert
        Assert.IsTrue(actual_quantity >= expected_floor && actual_quantity <= expected_ceiling); 
    }

    [Test]
    public void Make_sure_the_Market_company_exists_in_The_Market_with_proper_params()
    {
        // Arrange
        var expected_name = "The Global Market";
        // Act
        var actual = test_market.GetGlobalMarket().company_name;
        // Assert
        Assert.AreEqual(expected_name, actual);
    }

    [Test]
    public void When_market_is_initialized_goods_are_created_in_the_Global_Market()
    {
        // Arrange
        var expected = "Lemon";
        // Act
        test_market.Create_initial_goods(test_goods);
        var actual = TheGlobalMarket_for_testing.Get_inventory().Get_inventory_items().FirstOrDefault(x => x.good.good_name == "Lemon").good.good_name;
        // Assert
        Assert.AreEqual(expected, actual);
    }
    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(test_market.gameObject);
    }
}
