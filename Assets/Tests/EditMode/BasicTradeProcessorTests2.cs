using NUnit.Framework;

[TestFixture]
public partial class BasicTradeProcessorTests
{
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
    [Test]
    public void FullyFilledOrdersCannotBePrimary()
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
        var company1BuysRLFromAnyByCompany1 = new Order(Company1, null, radioactiveLemonade, 10, 10m);
        var company2SellsRLToCompany1ByCompany2 = new Order(Company1, Company2, radioactiveLemonade, 10, 10m);
        company1BuysRLFromAnyByCompany1.FilledQuantity = 10;
        var Company1Context = new ActionContext
        {
            TradeToSubmit = company1BuysRLFromAnyByCompany1,
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
        var actual=tradeProcessor.GeneratePrimaryOrder(testMarket);
        // Assert
        Assert.AreEqual(expected, actual);
    }
#endregion
}