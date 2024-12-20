using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

public partial class BasicTradeProcessorTests
{
    #region CounterParty Tests
    [Test]
    public void FindCounterPartiesForOrderReturnsSellOrderIfBuyOrderIsPrimary_OneCounterParty()
    {
        // Arrange
        Company2.GetInventory().AddGood(new InventoryEntry(RadioactiveLemonade, 10, 10m, Period));
        var company1BuysRLFromCompany2ByCompany1 = new Order(Company1, Company2, RadioactiveLemonade, 10, 10m);
        var company2SellsRLToCompany1ByCompany2 = new Order(Company1, Company2, RadioactiveLemonade, 10, 10m);
        var Company1Context = new ActionContext
        {
            TradeToSubmit = company1BuysRLFromCompany2ByCompany1,
            MarketToSubmitTo = TestMarket,
            Period = Period
        };
        Company1.QueueOrder(Company1Context);
        var Company2Context = new ActionContext
        {
            TradeToSubmit = company2SellsRLToCompany1ByCompany2,
            MarketToSubmitTo = TestMarket,
            Period = Period
        };
        Company2.QueueOrder(Company2Context);
        var expected = company2SellsRLToCompany1ByCompany2;
        // Act
        var actual=(TestTradeProcessor.FindCounterPartiesForOrder(TestMarket).ExtraData as List<Order>)?.FirstOrDefault();
        // Assert
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void FindCounterPartiesForOrderReturnsSellOrderIfBuyOrderIsPrimary_TwoCounterParties()
    {
        // Arrange
        var Company3 = Company.Factory.Create("Company 3", CompanyLevelEnum.Beginner);
        Company2.GetInventory().AddGood(new InventoryEntry(RadioactiveLemonade, 10, 10m, Period));
        Company3.GetInventory().AddGood(new InventoryEntry(RadioactiveLemonade, 10, 10m, Period));
        
        var company1BuysRLFromAnyoneByCompany1 = new Order(Company1, null, RadioactiveLemonade, 20, 10m);
        var company2SellsRLToCompany1ByCompany2 = new Order(Company1, Company2, RadioactiveLemonade, 11, 10m);
        var company3SellsRLToCompany1ByCompany3 = new Order(Company1, Company3, RadioactiveLemonade, 9, 10m);
        var Company1Context = new ActionContext
        {
            TradeToSubmit = company1BuysRLFromAnyoneByCompany1,
            MarketToSubmitTo = TestMarket,
            Period = Period
        };
        Company1.QueueOrder(Company1Context);
        var Company2Context = new ActionContext
        {
            TradeToSubmit = company2SellsRLToCompany1ByCompany2,
            MarketToSubmitTo = TestMarket,
            Period = Period
        };
        Company2.QueueOrder(Company2Context);
        var Company3Context = new ActionContext
        {
            TradeToSubmit = company3SellsRLToCompany1ByCompany3,
            MarketToSubmitTo = TestMarket,
            Period = Period
        };
        Company3.QueueOrder(Company3Context);
        var expected = 2;
        // Act
        var actual=(TestTradeProcessor.FindCounterPartiesForOrder(TestMarket).ExtraData as List<Order>)?.Count;
        // Assert
        Assert.AreEqual(expected, actual);
    }
    [Test]
    public void FindCounterPartiesForOrderReturnsEmptySetWhenOnlySellOrdersExist()
    {
        // Arrange
        Company1.GetInventory().AddGood(new InventoryEntry(RadioactiveLemonade, 10, 10m, Period));
        Company2.GetInventory().AddGood(new InventoryEntry(RadioactiveLemonade, 10, 10m, Period));
        var company1SellRLToAny= new Order(null, Company1, RadioactiveLemonade, 10, 10m);
        var company2SellRLToAny= new Order(null, Company2, RadioactiveLemonade, 10, 10m);
        var Company1Context = new ActionContext
        {
            TradeToSubmit = company1SellRLToAny,
            MarketToSubmitTo = TestMarket,
            Period = Period
        };
        Company1.QueueOrder(Company1Context);
        var Company2Context = new ActionContext
        {
            TradeToSubmit = company2SellRLToAny,
            MarketToSubmitTo = TestMarket,
            Period = Period
        };
        Company2.QueueOrder(Company2Context);
        var expected = 0;
        // Act
        var actual=(TestTradeProcessor.FindCounterPartiesForOrder(TestMarket)
                        .ExtraData as List<Order>)?.Count??0;
        // Assert
        Assert.AreEqual(expected, actual);
    }
    public void FindCounterPartiesForOrderReturnsEmptyWhenOnlyBuyersExist()
    {
        //Arrange
        var company1BuysRLFromAny= new Order(Company1, null, RadioactiveLemonade, 10, 10m);
        var company2BuysRLFromAny= new Order(Company2, null, RadioactiveLemonade, 10, 10m);
        var Company1Context = new ActionContext
        {
            TradeToSubmit = company1BuysRLFromAny,
            MarketToSubmitTo = TestMarket,
            Period = Period
        };
        Company1.QueueOrder(Company1Context);
        var Company2Context = new ActionContext
        {
            TradeToSubmit = company2BuysRLFromAny,
            MarketToSubmitTo = TestMarket,
            Period = Period
        };
        Company2.QueueOrder(Company2Context);
        var expected = 0;
        // Act
        var actual = (TestTradeProcessor.FindCounterPartiesForOrder(TestMarket)
                        .ExtraData as List<Order>)?.Count??0;
        // Assert
        Assert.AreEqual(expected, actual);
    }
    //You are here. Add condition where only Sell orders exist.
    //Add condition where only Buy orders exist.
    //If only buy or sell orders exist, the counterparty is the market.
#endregion
}