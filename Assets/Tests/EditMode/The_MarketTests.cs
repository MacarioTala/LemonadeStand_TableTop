using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class The_MarketTests
{
    private The_Market test_market;
    Good lemon;
    Good water;
    Good sugar;

    Price_band band1 = new Price_band(.5f, 1f);
    Price_band band2 = new Price_band(1f, 3f);
    Price_band band3 = new Price_band(3f, 5f);
    Price_band band4 = new Price_band(5f, 10f);

    List<Good> test_goods = new();

    [SetUp]
    public void SetUp()
    {
        var market_object = new GameObject();
        test_market = market_object.AddComponent<The_Market>();
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
        var expected = 1;
        // Act
        test_market.Register_Company(company);
        var actual = test_market.companies.Count;
        
        // Assert
        Assert.AreEqual(expected, test_market.companies.Count);
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
        Assert.Throws<CompanyException>(() => test_market.Register_Company(company2));
    }

    [Test]
    public void Create_initial_goods_creates_1to1000_goods_if_rarity_is_common()
    {
        // Arrange
        const int expected_floor = 1;
        const int expected_ceiling = 1000;
        // Act
        test_market.Create_initial_goods(test_goods);
        var actual_quantity = test_market.goods_in_market.Where(x => x.good.good_name == "Lemon").First().quantity;
        // Assert
        Assert.IsTrue(actual_quantity >= expected_floor && actual_quantity <= expected_ceiling); 
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(test_market.gameObject);
    }
}
