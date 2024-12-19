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
        var period = 0;
        var radioactiveLemonade = Good.CreateInstance("Radioactive Lemonade", new Price_band(10, 30), Rarity_enum.Uncommon);
        var Company1 = Company.Factory.Create("Company 1", CompanyLevelEnum.Beginner);
        var Company2 = Company.Factory.Create("Company 2", CompanyLevelEnum.Beginner);
        Company2.GetInventory().AddGood(new InventoryEntry(radioactiveLemonade, 10, 10m, period));
        var tradeProcessor = new BasicTradeProcessor();
        var testMarket = Market.Factory.CreateMarket("Test Market", CompanyLevelEnum.Market, new LinearDemandStrategy());
        testMarket.SetTradeProcessor(tradeProcessor);
        testMarket.RegisterCompany(Company1);
        testMarket.RegisterCompany(Company2);
        var company1BuysRLFromCompany2ByCompany1 = new Order(Company1, Company2, radioactiveLemonade, 10, 10m);
        var company2SellsRLToCompany1ByCompany2 = new Order(Company1, Company2, radioactiveLemonade, 10, 10m);
        var Company1Context = new ActionContext
        {
            TradeToSubmit = company1BuysRLFromCompany2ByCompany1,
            MarketToSubmitTo = testMarket,
            Period = period
        };
        Company1.QueueOrder(Company1Context);
        var Company2Context = new ActionContext
        {
            TradeToSubmit = company2SellsRLToCompany1ByCompany2,
            MarketToSubmitTo = testMarket,
            Period = period
        };
        Company2.QueueOrder(Company2Context);
        var expected = company2SellsRLToCompany1ByCompany2;
        // Act
        var actual=(tradeProcessor.FindCounterPartiesForOrder(testMarket).ExtraData as List<Order>)?.FirstOrDefault();
        // Assert
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void FindCounterPartiesForOrderReturnsSellOrderIfBuyOrderIsPrimary_TwoCounterParties()
    {
        // Arrange
        var period = 0;
        var radioactiveLemonade = Good.CreateInstance("Radioactive Lemonade", new Price_band(10, 30), Rarity_enum.Uncommon);
        var Company1 = Company.Factory.Create("Company 1", CompanyLevelEnum.Beginner);
        var Company2 = Company.Factory.Create("Company 2", CompanyLevelEnum.Beginner);
        var Company3 = Company.Factory.Create("Company 3", CompanyLevelEnum.Beginner);
        Company2.GetInventory().AddGood(new InventoryEntry(radioactiveLemonade, 10, 10m, period));
        Company3.GetInventory().AddGood(new InventoryEntry(radioactiveLemonade, 10, 10m, period));
        var tradeProcessor = new BasicTradeProcessor();
        var testMarket = Market.Factory.CreateMarket("Test Market", CompanyLevelEnum.Market, new LinearDemandStrategy());
        testMarket.SetTradeProcessor(tradeProcessor);
        testMarket.RegisterCompany(Company1);
        testMarket.RegisterCompany(Company2);
        var company1BuysRLFromAnyoneByCompany1 = new Order(Company1, null, radioactiveLemonade, 20, 10m);
        var company2SellsRLToCompany1ByCompany2 = new Order(Company1, Company2, radioactiveLemonade, 11, 10m);
        var company3SellsRLToCompany1ByCompany3 = new Order(Company1, Company3, radioactiveLemonade, 9, 10m);
        var Company1Context = new ActionContext
        {
            TradeToSubmit = company1BuysRLFromAnyoneByCompany1,
            MarketToSubmitTo = testMarket,
            Period = period
        };
        Company1.QueueOrder(Company1Context);
        var Company2Context = new ActionContext
        {
            TradeToSubmit = company2SellsRLToCompany1ByCompany2,
            MarketToSubmitTo = testMarket,
            Period = period
        };
        Company2.QueueOrder(Company2Context);
        var Company3Context = new ActionContext
        {
            TradeToSubmit = company3SellsRLToCompany1ByCompany3,
            MarketToSubmitTo = testMarket,
            Period = period
        };
        Company3.QueueOrder(Company3Context);
        var expected = 2;
        // Act
        var actual=(tradeProcessor.FindCounterPartiesForOrder(testMarket).ExtraData as List<Order>)?.Count;
        // Assert
        Assert.AreEqual(expected, actual);
    }
    [Test]
    public void FindCounterPartiesForOrderReturnsEmptySetWhenOnlySellOrdersExist()
    {
        // Arrange
        var period = 0;
        var radioactiveLemonade = Good.CreateInstance("Radioactive Lemonade", new Price_band(10, 30), Rarity_enum.Uncommon);
        var Company1 = Company.Factory.Create("Company 1", CompanyLevelEnum.Beginner);
        var Company2 = Company.Factory.Create("Company 2", CompanyLevelEnum.Beginner);
        var testMarket = Market.Factory.CreateMarket("Test Market", CompanyLevelEnum.Market, new LinearDemandStrategy());
        var tradeProcessor = new BasicTradeProcessor();
        testMarket.SetTradeProcessor(tradeProcessor);
        testMarket.RegisterCompany(Company1);
        testMarket.RegisterCompany(Company2);
        Company1.GetInventory().AddGood(new InventoryEntry(radioactiveLemonade, 10, 10m, period));
        Company2.GetInventory().AddGood(new InventoryEntry(radioactiveLemonade, 10, 10m, period));
        var company1SellRLToAny= new Order(null, Company1, radioactiveLemonade, 10, 10m);
        var company2SellRLToAny= new Order(null, Company2, radioactiveLemonade, 10, 10m);
        var Company1Context = new ActionContext
        {
            TradeToSubmit = company1SellRLToAny,
            MarketToSubmitTo = testMarket,
            Period = period
        };
        Company1.QueueOrder(Company1Context);
        var Company2Context = new ActionContext
        {
            TradeToSubmit = company2SellRLToAny,
            MarketToSubmitTo = testMarket,
            Period = period
        };
        Company2.QueueOrder(Company2Context);
        var expected = 0;
        // Act
        var actual=(tradeProcessor.FindCounterPartiesForOrder(testMarket)
                        .ExtraData as List<Order>)?.Count??0;
        // Assert
        Assert.AreEqual(expected, actual);
    }
    public void FindCounterPartiesForOrderReturnsEmptyWhenOnlyBuyersExist()
    {
        //Arrange
        var period = 0;
        var radioactiveLemonade = Good.CreateInstance("Radioactive Lemonade", new Price_band(10, 30), Rarity_enum.Uncommon);
        var Company1 = Company.Factory.Create("Company 1", CompanyLevelEnum.Beginner);
        var Company2 = Company.Factory.Create("Company 2", CompanyLevelEnum.Beginner);
        var testMarket = Market.Factory.CreateMarket("Test Market", CompanyLevelEnum.Market, new LinearDemandStrategy());
        var tradeProcessor = new BasicTradeProcessor();
        testMarket.SetTradeProcessor(tradeProcessor);
        testMarket.RegisterCompany(Company1);
        testMarket.RegisterCompany(Company2);
        var company1BuysRLFromAny= new Order(Company1, null, radioactiveLemonade, 10, 10m);
        var company2BuysRLFromAny= new Order(Company2, null, radioactiveLemonade, 10, 10m);
        var Company1Context = new ActionContext
        {
            TradeToSubmit = company1BuysRLFromAny,
            MarketToSubmitTo = testMarket,
            Period = period
        };
        Company1.QueueOrder(Company1Context);
        var Company2Context = new ActionContext
        {
            TradeToSubmit = company2BuysRLFromAny,
            MarketToSubmitTo = testMarket,
            Period = period
        };
        Company2.QueueOrder(Company2Context);
        var expected = 0;
        // Act
        var actual = (tradeProcessor.FindCounterPartiesForOrder(testMarket)
                        .ExtraData as List<Order>)?.Count??0;
        // Assert
        Assert.AreEqual(expected, actual);
    }
    //You are here. Add condition where only Sell orders exist.
    //Add condition where only Buy orders exist.
    //If only buy or sell orders exist, the counterparty is the market.
#endregion
}