using NUnit.Framework;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
[TestFixture]
public class CompanyTests
{
    private TestHelpers testHelpers;
    [SetUp]
    public void Setup()
    {
        testHelpers = new TestHelpers();
    }

    [Test]
    public void Set_initial_cash_sets_cash_to_10000_for_beginner()
    {
        //arrange
        var company = Company.Factory.Create("Test Company", CompanyLevelEnum.Beginner);
        const float expected_cash = 10000;
        //Act
        //Assert
        Assert.AreEqual(expected_cash, company.Get_cash());
    }
    [Test]
    public void Buy_good_when_buyer_has_enough_cash_removes_cash_from_buyer()
    {
        //arrange
        var company = Company.Factory.Create("Test Company", CompanyLevelEnum.Beginner);
        var good = Good.CreateInstance("Lemon", new Price_band(1, 3), Rarity_enum.Common);
        const decimal trade_price = 3.0m;
        var expected_cash = 10000 - 3;
        //Act
        company.BuyGood(good, 1, trade_price);
        var actual_cash = company.Get_cash();
        //Assert
        Assert.AreEqual(expected_cash, actual_cash);

    }

    [Test]
    public void Buy_good_when_buyer_has_enough_cash_adds_good_to_inventory()
    {
        //arrange
        const int PeriodIsIrrelevant = 0;
        var company = Company.Factory.Create("Test Company", CompanyLevelEnum.Beginner);
        var good = Good.CreateInstance("Lemon", new Price_band(1, 3), Rarity_enum.Common);
        const decimal trade_price = 3.0m;
        var expected_inventory_entry = new InventoryEntry(good, 1, trade_price, PeriodIsIrrelevant);
        
        //Act
        company.BuyGood(good, 1, trade_price);
        var actual_inventory_entry = company.GetInventory().GetInventoryEntries().Where(x => x.good.good_name == "Lemon" 
                                                                    && x.quantity == 1 
                                                                    && x.acquisition_price == trade_price
                                                                    ).FirstOrDefault();
        //Assert
        var expected_inventory = testHelpers.ListToString(new List<InventoryEntry> { expected_inventory_entry });
        var actual_inventory = testHelpers.ListToString(company.GetInventory().GetInventoryEntries());
        Assert.AreEqual(expected_inventory, actual_inventory);
    }

    [Test]
    public void Buy_good_when_buyer_doesnt_have_enough_cash_throws_exception()
    {
        //arrange
        var company = Company.Factory.Create("Test Company", CompanyLevelEnum.Beginner);
        var good = Good.CreateInstance("Lemon", new Price_band(1, 3), Rarity_enum.Common);
        const decimal trade_price = 3.0m;
        const int trade_quantity = 10000;
        //Act
        //Assert
        Assert.Throws<Company_InsufficientFundsException>(() => company.BuyGood(good, trade_quantity, trade_price));
    }
    [Test]
    public void Sell_good_when_seller_has_good_in_inventory_adds_cash_to_seller()
    {
        //arrange
        var company = Company.Factory.Create("Test Company", CompanyLevelEnum.Beginner);
        var good = Good.CreateInstance("Lemon", new Price_band(1, 3), Rarity_enum.Common);
        company.BuyGood(good, 1,good.GetPrice());
        var initial_cash = company.Get_cash();
        var good_price = 3.0m;
        var expected_cash = initial_cash + good_price;
        
        //Act
        company.SellGood(good, 1, good_price);
        var actual_cash = company.Get_cash();
        //Assert
        Assert.AreEqual(expected_cash, actual_cash);
    }

    [Test]
    public void Sell_good_throws_exception_when_not_enough_quantity()
    {
        //arrange
        var company = Company.Factory.Create("Test Company", CompanyLevelEnum.Beginner);
        var good = Good.CreateInstance("Lemon", new Price_band(1, 3), Rarity_enum.Common);
        company.BuyGood(good, 1,good.GetPrice());
        var good_price = 3.0m;
        //Act
        //Assert
        Assert.Throws<Company_InventoryException>(() => company.SellGood(good, 2, good_price));
    }

    [Test]
    public void QueueTradethrowsContextExceptionWhenActionContextIncomplete()
    {
        //Assert
        var company1 = Company.Factory.Create("Test Company", CompanyLevelEnum.Beginner);
        var lemon = Good.CreateInstance("Lemon", new Price_band(1, 3), Rarity_enum.Common);
        var context = new ActionContext{BidToSubmit = 2.0m, AskToSubmit = 3.0m, GoodToSubmit = lemon};
        System.Exception actual=null;
        var expected = new ContextException("Seller, Good, Quantity, or Price not set in context");
        //Act
        try
        {
            company1.QueueTrade(context);
        }
        catch (System.Exception e)
        {
            actual = e;
        }
        //Assert
        Assert.AreEqual(expected.Message, actual.Message);
        
    }
}