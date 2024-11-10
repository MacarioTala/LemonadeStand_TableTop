using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class TheEconomyTests
{
    private TheEconomy test_market;

    private Company TheGlobalMarket_for_testing;
    Good lemon;
    Good water;
    Good sugar;

    readonly Price_band band1 = new(.5f, 1f);
    readonly Price_band band2 = new(1f, 3f);
    readonly Price_band band3 = new(3f, 5f);
    readonly Price_band band4 = new(5f, 10f);
    readonly ITradeLogger trade_logger = new MockLogger();
    readonly List<Good> test_goods = new();

    [SetUp]
    public void SetUp()
    {
        var market_object = new GameObject();
        test_market = market_object.AddComponent<TheEconomy>();
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
    public void Make_sure_the_first_market_exists_in_TheEconomy_with_proper_params()
    {
        // Arrange
        var expected_name = "The First Market";
        // Act
        var actual = test_market.GetGlobalMarket().company_name;
        // Assert
        Assert.AreEqual(expected_name, actual);
    }

    [Test]
    public void When_TheEconomy_is_initialized_goods_are_created_in_InitialMarket()
    {
        // Arrange
        var expected = "Lemon";
        // Act
        test_market.Create_initial_goods(test_goods);
        var actual = TheGlobalMarket_for_testing.Get_inventory().Get_inventory_items().FirstOrDefault(x => x.good.good_name == "Lemon").good.good_name;
        // Assert
        Assert.AreEqual(expected, actual);
    }
#region TradeTests
    [Test]
    public void A_Company_buying_a_good_from_another_company_via_queue_can_be_initiated_by_queue_trade()
    {
        // Arrange
        var company1 = ScriptableObject.CreateInstance<Company>();
        company1.Initialize("Company1", CompanyLevelEnum.Beginner);
        var company2 = ScriptableObject.CreateInstance<Company>();
        company2.Initialize("Company2", CompanyLevelEnum.Beginner);
        test_market.Register_Company(company1);
        test_market.Register_Company(company2);
        company1.BuyGood(lemon, 10, 3f);
        company1.BuyGood(water, 10, 1f);
        company1.BuyGood(sugar, 10, 1f);
        company2.BuyGood(lemon, 10, 1f);
        company2.BuyGood(water, 10, 3f);
        company2.BuyGood(sugar, 10, 1f);
        var expected_company1_cash = company1.Get_cash() - 2;
        var expected_company2_cash = company2.Get_cash() + 2;
        var expected_company1_2flemon_quantity = 1;
        var expected_company2_lemon_quantity = 10 - 1;

        // Act
        var trade = new Trade(company1, company2, lemon, 1, 2f);
        test_market.Queue_Trade(trade);
        test_market.ExecuteDailyTrades();
        var actual_company1_cash = company1.Get_cash();
        var actual_company2_cash = company2.Get_cash();
        var actual_company1_2flemon_quantity = company1.Get_inventory().Get_inventory_items().FirstOrDefault(x => x.good.good_name == "Lemon"&& x.acquisition_price==2f).quantity;
        var actual_company2_lemon_quantity = company2.Get_inventory().Get_inventory_items().FirstOrDefault(x => x.good.good_name == "Lemon").quantity;
        // Assert
        Assert.AreEqual(expected_company1_cash, actual_company1_cash);
        Assert.AreEqual(expected_company2_cash, actual_company2_cash);
        Assert.AreEqual(expected_company1_2flemon_quantity, actual_company1_2flemon_quantity);
        Assert.AreEqual(expected_company2_lemon_quantity, actual_company2_lemon_quantity);
    }



#endregion

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(test_market.gameObject);
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