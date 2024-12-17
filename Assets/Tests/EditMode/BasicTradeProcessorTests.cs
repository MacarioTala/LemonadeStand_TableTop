using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using UnityEngine;

[TestFixture]
public class BasicTradeProcessorTests
{
    TheEconomy TestEconomy;
    [SetUp]
    public void SetUp()
    {
        var economyObject = new GameObject();
        TestEconomy = economyObject.AddComponent<TheEconomy>();
        TestEconomy.Initialize(new MockLogger());
        
    }
    [Test]
    public void ProcessCompanyOrdersIgnoresOrdersWhereSellerIsMarket()
    {
        // Arrange
        var period = 0;
        var demandStrategy = new LinearDemandStrategy();
        var testMarket = Market.Factory.CreateMarket("Test Market", CompanyLevelEnum.Market,demandStrategy);
        var testCompany = Company.Factory.Create("Test Company", CompanyLevelEnum.Beginner);
        testMarket.RegisterCompany(testCompany);
        var lemonade = Good.CreateInstance("Lemonade", new Price_band(1, 3), Rarity_enum.Uncommon);
        testCompany.GetInventory().AddGood(new InventoryEntry(lemonade, 100, 1m, period));
        var testOrder = new Order(testMarket, testCompany, lemonade, 100, 10m);
        var testContext = new ActionContext{
                    TradeToSubmit = testOrder,
                    MarketToSubmitTo = testMarket,
                    Period = period
                                            };
        var expected=0;
        // Act
        testCompany.QueueOrder(testContext);
        testMarket.ProcessCompanyOrders();
        var actual = testOrder.FilledQuantity;
        // Assert
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void GetOrdersGetsUpdatedFillsForOrdersBetweenCompanies()
    {
        // Arrange
        var period = 0;
        var lemonade = Good.CreateInstance("Lemonade", new Price_band(1, 3), Rarity_enum.Uncommon);
        var radioactiveLemonade = Good.CreateInstance("Radioactive Lemonade", new Price_band(10,20), Rarity_enum.Very_Rare);

        var demandStrategy = new LinearDemandStrategy();
        var testMarket = Market.Factory.CreateMarket("Test Market", CompanyLevelEnum.Market,demandStrategy);
        testMarket.SetCash(1000000);
        
        var testCompany = Company.Factory.Create("Test Company", CompanyLevelEnum.Beginner);
        testMarket.RegisterCompany(testCompany);
        
        var testCompany2 = Company.Factory.Create("Test Company 2", CompanyLevelEnum.Beginner);
        testMarket.RegisterCompany(testCompany2);

        testCompany.GetInventory().AddGood(new InventoryEntry(lemonade, 100, 1m, period));
        testCompany2.GetInventory().AddGood(new InventoryEntry(radioactiveLemonade, 10, 10m, period));

        var testOrder = new Order(testCompany2,testCompany, lemonade, 100, 10m);
        var testOrder2 = new Order(testCompany,testCompany2, radioactiveLemonade, 10, 10m);
        var testContext = new ActionContext{
                    TradeToSubmit = testOrder,
                    MarketToSubmitTo = testMarket,
                    Period = period
                                            };
        var testContext2 = new ActionContext{
                    TradeToSubmit = testOrder2,
                    MarketToSubmitTo = testMarket,
                    Period = period
                                            };
        var expectedLemonadeFill=100;
        var expectedRadioactiveLemonadeFill=10;
        // Act
        testCompany.QueueOrder(testContext);
        testCompany2.QueueOrder(testContext2);
        testMarket.ProcessCompanyOrders();
        
        var actualLemonadeFill = testOrder.FilledQuantity;
        var actualRadioactiveLemonadeFill = testOrder2.FilledQuantity;
        // Assert
        Assert.AreEqual(expectedLemonadeFill, actualLemonadeFill);
        Assert.AreEqual(expectedRadioactiveLemonadeFill, actualRadioactiveLemonadeFill);
    }
#region GeneratePrimaryOrder Tests
    [Test]
    public void IfOnlyTwoOrdersExistBuyOrderIsPrimary_MatchedOrders()
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
        var company2BuysRLFromCompany1ByCompany2 = new Order(Company1, Company2, radioactiveLemonade, 10, 10m);
        var Company1Context = new ActionContext
        {
            TradeToSubmit = company1BuysRLFromCompany2ByCompany1,
            MarketToSubmitTo = testMarket,
            Period = period
        };
        Company1.QueueOrder(Company1Context);
        var Company2Context = new ActionContext
        {
            TradeToSubmit = company2BuysRLFromCompany1ByCompany2,
            MarketToSubmitTo = testMarket,
            Period = period
        };
        Company2.QueueOrder(Company2Context);
        
        var expected = company1BuysRLFromCompany2ByCompany1;
        // Act
        var actual=tradeProcessor.GeneratePrimaryOrder(testMarket);
        // Assert
        Assert.AreEqual(expected, actual);
    }

     [Test]
    public void IfOnlySellOrderExistsItIsPrimary()
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
        var Company1Context = new ActionContext
        {
            TradeToSubmit = company1BuysRLFromCompany2ByCompany1,
            MarketToSubmitTo = testMarket,
            Period = period
        };
        Company1.QueueOrder(Company1Context);
   
        var expected = company1BuysRLFromCompany2ByCompany1;
        // Act
        var actual=tradeProcessor.GeneratePrimaryOrder(testMarket);
        // Assert
        Assert.AreEqual(expected, actual);
    }

     [Test]
    public void BuyOrderWithGreatestQuantityIsPrimary()
    {
        // Arrange
        var period = 0;
        var radioactiveLemonade = Good.CreateInstance("Radioactive Lemonade", new Price_band(10, 30), Rarity_enum.Uncommon);
        var Company1 = Company.Factory.Create("Company 1", CompanyLevelEnum.Beginner);
        var Company2 = Company.Factory.Create("Company 2", CompanyLevelEnum.Beginner);
        var Company3 = Company.Factory.Create("Company 3", CompanyLevelEnum.Beginner);
        Company2.GetInventory().AddGood(new InventoryEntry(radioactiveLemonade, 10, 10m, period));
        var tradeProcessor = new BasicTradeProcessor();
        var testMarket = Market.Factory.CreateMarket("Test Market", CompanyLevelEnum.Market, new LinearDemandStrategy());
        testMarket.SetTradeProcessor(tradeProcessor);
        testMarket.RegisterCompany(Company1);
        testMarket.RegisterCompany(Company2);
        testMarket.RegisterCompany(Company3);
        var company1BuysRLFromCompany2ByCompany1 = new Order(Company1, Company2, radioactiveLemonade, 8, 10m);
        var company2BuysRLFromCompany1ByCompany2 = new Order(Company1, Company2, radioactiveLemonade, 15, 10m);
        var company3BuysRLFromCompany2ByCompany3 = new Order(Company3, Company2, radioactiveLemonade, 7, 10m);
        var Company1Context = new ActionContext
        {
            TradeToSubmit = company1BuysRLFromCompany2ByCompany1,
            MarketToSubmitTo = testMarket,
            Period = period
        };
        Company1.QueueOrder(Company1Context);
        var Company2Context = new ActionContext
        {
            TradeToSubmit = company2BuysRLFromCompany1ByCompany2,
            MarketToSubmitTo = testMarket,
            Period = period
        };
        Company2.QueueOrder(Company2Context);
        var Company3Context = new ActionContext
        {
            TradeToSubmit = company3BuysRLFromCompany2ByCompany3,
            MarketToSubmitTo = testMarket,
            Period = period
        };
        Company3.QueueOrder(Company3Context);
        var expected = company1BuysRLFromCompany2ByCompany1;

        // Act
        var actual=tradeProcessor.GeneratePrimaryOrder(testMarket);
        // Assert
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void SellOrderWithGreatestQuantityIsPrimaryIfNoBuyOrdersExist()
    {
        // Arrange
        var period = 0;
        var radioactiveLemonade = Good.CreateInstance("Radioactive Lemonade", new Price_band(10, 30), Rarity_enum.Uncommon);
        var Company1 = Company.Factory.Create("Company 1", CompanyLevelEnum.Beginner);
        var Company2 = Company.Factory.Create("Company 2", CompanyLevelEnum.Beginner);
        var Company3 = Company.Factory.Create("Company 3", CompanyLevelEnum.Beginner);
        Company2.GetInventory().AddGood(new InventoryEntry(radioactiveLemonade, 10, 10m, period));
        var tradeProcessor = new BasicTradeProcessor();
        var testMarket = Market.Factory.CreateMarket("Test Market", CompanyLevelEnum.Market, new LinearDemandStrategy());
        testMarket.SetTradeProcessor(tradeProcessor);
        testMarket.RegisterCompany(Company1);
        testMarket.RegisterCompany(Company2);
        testMarket.RegisterCompany(Company3);
        var company2SellsRLToCompany1ByCompany2 = new Order(Company1, Company2, radioactiveLemonade, 8, 10m);
        var company3SellsRLToCompany1ByCompany3 = new Order(Company1, Company3, radioactiveLemonade, 15, 10m);
        var company2SellsToCompany1Context = new ActionContext
        {
            TradeToSubmit = company2SellsRLToCompany1ByCompany2,
            MarketToSubmitTo = testMarket,
            Period = period
        };
        Company2.QueueOrder(company2SellsToCompany1Context);
        var company3SellsToCompany1Context = new ActionContext
        {
            TradeToSubmit = company3SellsRLToCompany1ByCompany3,
            MarketToSubmitTo = testMarket,
            Period = period
        };
        Company3.QueueOrder(company3SellsToCompany1Context);
        var expected = company3SellsRLToCompany1ByCompany3;
        // Act
        var actual=tradeProcessor.GeneratePrimaryOrder(testMarket);
        // Assert
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void BuyOrdersAreStillPrimaryEvenWithNoSeller()
    {
        // Arrange
        var period = 0;
        var radioactiveLemonade = Good.CreateInstance("Radioactive Lemonade", new Price_band(10, 30), Rarity_enum.Uncommon);
        var Company1 = Company.Factory.Create("Company 1", CompanyLevelEnum.Beginner);
        var testMarket = Market.Factory.CreateMarket("Test Market", CompanyLevelEnum.Market, new LinearDemandStrategy());
        testMarket.RegisterCompany(Company1);
        var tradeProcessor = new BasicTradeProcessor();
        testMarket.SetTradeProcessor(tradeProcessor);
        var company1BuysRLFromMarket = new Order(Company1, null, radioactiveLemonade, 10, 10m);
        var Company1Context = new ActionContext
        {
            TradeToSubmit = company1BuysRLFromMarket,
            MarketToSubmitTo = testMarket,
            Period = period
        };
        Company1.QueueOrder(Company1Context);
        var expected = company1BuysRLFromMarket;
        // Act
        var actual=tradeProcessor.GeneratePrimaryOrder(testMarket);
        // Assert
        Assert.AreEqual(expected, actual);
    }
    [Test]
    public void SellOrdersArePrimaryEvenWithNoBuyerIfTheyreAlone()
    {
        // Arrange
        var period = 0;
        var radioactiveLemonade = Good.CreateInstance("Radioactive Lemonade", new Price_band(10, 30), Rarity_enum.Uncommon);
        var Company1 = Company.Factory.Create("Company 1", CompanyLevelEnum.Beginner);
        var testMarket = Market.Factory.CreateMarket("Test Market", CompanyLevelEnum.Market, new LinearDemandStrategy());
        testMarket.RegisterCompany(Company1);
        var tradeProcessor = new BasicTradeProcessor();
        testMarket.SetTradeProcessor(tradeProcessor);
        var company1SellsRLFromMarket = new Order(null, Company1, radioactiveLemonade, 10, 10m);
        var Company1Context = new ActionContext
        {
            TradeToSubmit = company1SellsRLFromMarket,
            MarketToSubmitTo = testMarket,
            Period = period
        };
        Company1.QueueOrder(Company1Context);
        var expected = company1SellsRLFromMarket;
        // Act
        var actual=tradeProcessor.GeneratePrimaryOrder(testMarket);
        // Assert
        Assert.AreEqual(expected, actual);
    }
#endregion
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
        var actual=(tradeProcessor.FindCounterPartiesForOrder(Company1Context).ExtraData as List<Order>)?.FirstOrDefault();
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
        var actual=(tradeProcessor.FindCounterPartiesForOrder(Company1Context).ExtraData as List<Order>)?.Count;
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
        var actual=(tradeProcessor.FindCounterPartiesForOrder(Company1Context)
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
        var actual = (tradeProcessor.FindCounterPartiesForOrder(Company1Context)
                        .ExtraData as List<Order>)?.Count??0;
        // Assert
        Assert.AreEqual(expected, actual);
    }
    //You are here. Add condition where only Sell orders exist.
    //Add condition where only Buy orders exist.
    //If only buy or sell orders exist, the counterparty is the market.
#endregion
    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(TestEconomy.gameObject);
    }
}