using NUnit.Framework;

public partial class BasicTradeProcessorTests
{
    #region ExecuteBestTradesForGood Tests
    [Test]
    public void EBTFG_BuyOrderThatPerfectlyMatchesSellOrderFills_TwoOrders()
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
        var expectedCompany1Fills = 10;
        var expectedCompany2Fills = 10;
        var OrdersSentToMarket = tradeProcessor.GetOrders();
        // Act
        tradeProcessor.ExecuteBestTradesForGood(radioactiveLemonade,testMarket,OrdersSentToMarket);
        var actualCompany1Fills = company1BuysRLFromCompany2ByCompany1.FilledQuantity;
        var actualCompany2Fills = company2SellsRLToCompany1ByCompany2.FilledQuantity;
        // Assert
        Assert.AreEqual(expectedCompany1Fills, actualCompany1Fills, "Company 1 did not fill the order");
        Assert.AreEqual(expectedCompany2Fills, actualCompany2Fills, "Company 2 did not fill the order");
    }
    [Test]
    public void EBTFG_WhenMultipleSellersFillBuyOrderAllFillsRecordedProperly()
    {
        // Arrange
        var period = 0;
        var radioactiveLemonade = Good.CreateInstance("Radioactive Lemonade", new Price_band(10, 30), Rarity_enum.Uncommon);
        var Company1 = Company.Factory.Create("Company 1", CompanyLevelEnum.Beginner);
        var Company2 = Company.Factory.Create("Company 2", CompanyLevelEnum.Beginner);
        var Company3 = Company.Factory.Create("Company 3", CompanyLevelEnum.Beginner);
        Company2.GetInventory().AddGood(new InventoryEntry(radioactiveLemonade, 15, 10m, period));
        Company3.GetInventory().AddGood(new InventoryEntry(radioactiveLemonade, 10, 10m, period));
        var tradeProcessor = new BasicTradeProcessor();
        var testMarket = Market.Factory.CreateMarket("Test Market", CompanyLevelEnum.Market, new LinearDemandStrategy());
        testMarket.SetTradeProcessor(tradeProcessor);
        testMarket.RegisterCompany(Company1);
        testMarket.RegisterCompany(Company2);
        testMarket.RegisterCompany(Company3);
        var company1BuysRLFromAnyoneByCompany1 = new Order(Company1, null, radioactiveLemonade, 20, 10m);
        var company2SellsRLToAnyone1ByCompany2 = new Order(null, Company2, radioactiveLemonade, 11, 10m);
        var company3SellsRLToAnyone2ByCompany3 = new Order(null, Company3, radioactiveLemonade, 9, 10m);
        var Company1Context = new ActionContext
        {
            TradeToSubmit = company1BuysRLFromAnyoneByCompany1,
            MarketToSubmitTo = testMarket,
            Period = period
        };
        Company1.QueueOrder(Company1Context);
        var Company2Context = new ActionContext
        {
            TradeToSubmit = company2SellsRLToAnyone1ByCompany2,
            MarketToSubmitTo = testMarket,
            Period = period
        };
        Company2.QueueOrder(Company2Context);
        var Company3Context = new ActionContext
        {
            TradeToSubmit = company3SellsRLToAnyone2ByCompany3,
            MarketToSubmitTo = testMarket,
            Period = period
        };
        Company3.QueueOrder(Company3Context);
        var expectedCompany1Fills = 20;
        var expectedCompany2Fills = 11;
        var expectedCompany3Fills = 9;
        var OrdersSentToMarket = tradeProcessor.GetOrders();
        // Act
        tradeProcessor.ExecuteBestTradesForGood(radioactiveLemonade,testMarket,OrdersSentToMarket);
        var actualCompany1Fills = company1BuysRLFromAnyoneByCompany1.FilledQuantity;
        var actualCompany2Fills = company2SellsRLToAnyone1ByCompany2.FilledQuantity;
        var actualCompany3Fills = company3SellsRLToAnyone2ByCompany3.FilledQuantity;
        // Assert
        Assert.AreEqual(expectedCompany1Fills, actualCompany1Fills, "Company 1 did not fill the order");
        Assert.AreEqual(expectedCompany2Fills, actualCompany2Fills, "Company 2 did not fill the order");
        Assert.AreEqual(expectedCompany3Fills, actualCompany3Fills, "Company 3 did not fill the order");
        Assert.IsTrue(company1BuysRLFromAnyoneByCompany1.IsFullyFilled);
        Assert.IsTrue(company2SellsRLToAnyone1ByCompany2.IsFullyFilled);
        Assert.IsTrue(company3SellsRLToAnyone2ByCompany3.IsFullyFilled);
    }
    [Test]
    public void EBTFG_WhenMultipleBuyersWhaleIsFilledFirst()
    {
        // Arrange
        var period = 0;
        var radioactiveLemonade = Good.CreateInstance("Radioactive Lemonade", new Price_band(10, 30), Rarity_enum.Uncommon);
        var Company1 = Company.Factory.Create("Company 1", CompanyLevelEnum.Beginner);
        var Company2 = Company.Factory.Create("Company 2", CompanyLevelEnum.Beginner);
        var Company3 = Company.Factory.Create("Company 3", CompanyLevelEnum.Beginner);
        Company3.GetInventory().AddGood(new InventoryEntry(radioactiveLemonade, 30, 10m, period));
        var tradeProcessor = new BasicTradeProcessor();
        var testMarket = Market.Factory.CreateMarket("Test Market", CompanyLevelEnum.Market, new LinearDemandStrategy());
        testMarket.SetTradeProcessor(tradeProcessor);
        testMarket.RegisterCompany(Company1);
        testMarket.RegisterCompany(Company2);
        testMarket.RegisterCompany(Company3);
        var company1BuysRLFromAnyoneByCompany1 = new Order(Company1, null, radioactiveLemonade, 20, 10m);
        var company2BuysRLFromAnyoneByCompany2 = new Order(Company2, null, radioactiveLemonade, 15, 10m);
        var company3SellsRLToAnyoneByCompany3 = new Order(null, Company3, radioactiveLemonade, 30, 10m);
        var Company1Context = new ActionContext
        {
            TradeToSubmit = company1BuysRLFromAnyoneByCompany1,
            MarketToSubmitTo = testMarket,
            Period = period
        };
        Company1.QueueOrder(Company1Context);
        var Company2Context = new ActionContext
        {
            TradeToSubmit = company2BuysRLFromAnyoneByCompany2,
            MarketToSubmitTo = testMarket,
            Period = period
        };
        Company2.QueueOrder(Company2Context);
        var Company3Context = new ActionContext
        {
            TradeToSubmit = company3SellsRLToAnyoneByCompany3,
            MarketToSubmitTo = testMarket,
            Period = period
        };
        Company3.QueueOrder(Company3Context);
        var expectedCompany1Fills = 20;
        var expectedCompany2Fills = 10;
        var expectedCompany3Fills = 30;
        var OrdersSentToMarket = tradeProcessor.GetOrders();
        // Act
        tradeProcessor.ExecuteBestTradesForGood(radioactiveLemonade,testMarket,OrdersSentToMarket);
        var actualCompany1Fills = company1BuysRLFromAnyoneByCompany1.FilledQuantity;
        var actualCompany2Fills = company2BuysRLFromAnyoneByCompany2.FilledQuantity;
        var actualCompany3Fills = company3SellsRLToAnyoneByCompany3.FilledQuantity;
        // Assert
        Assert.AreEqual(expectedCompany1Fills, actualCompany1Fills, "Company 1 did not fill the order correctly");
        Assert.AreEqual(expectedCompany2Fills, actualCompany2Fills, "Company 2 did not fill the order correctly");
        Assert.AreEqual(expectedCompany3Fills, actualCompany3Fills, "Company 3 did not fill the order correctly");
    }

    [Test]
    public void EBTFG_WhenNoBuyersNothingIsFilled()
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
        var company1SellsRLToAnyone= new Order(null, Company1, radioactiveLemonade, 10, 10m);
        var company2SellsRLToAnyone= new Order(null, Company2, radioactiveLemonade, 10, 10m);
        var Company1Context = new ActionContext
        {
            TradeToSubmit = company1SellsRLToAnyone,
            MarketToSubmitTo = testMarket,
            Period = period
        };
        Company1.QueueOrder(Company1Context);
        var Company2Context = new ActionContext
        {
            TradeToSubmit = company2SellsRLToAnyone,
            MarketToSubmitTo = testMarket,
            Period = period
        };
        Company2.QueueOrder(Company2Context);
        var expectedCompany1Fills = 0;
        var expectedCompany2Fills = 0;
        var OrdersSentToMarket = tradeProcessor.GetOrders();
        // Act
        tradeProcessor.ExecuteBestTradesForGood(radioactiveLemonade,testMarket,OrdersSentToMarket);
        var actualCompany1Fills = company1SellsRLToAnyone.FilledQuantity;
        var actualCompany2Fills = company2SellsRLToAnyone.FilledQuantity;
        // Assert
        Assert.AreEqual(expectedCompany1Fills, actualCompany1Fills, "Company 1 did not fill the order correctly");
        Assert.AreEqual(expectedCompany2Fills, actualCompany2Fills, "Company 2 did not fill the order correctly");
    }
    [Test]
    public void EBTFG_WhenNoSellersNothingIsFilled()
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
        var company1BuysRLFromAnyone= new Order(Company1, null, radioactiveLemonade, 10, 10m);
        var company2BuysRLFromAnyone= new Order(Company2, null, radioactiveLemonade, 10, 10m);
        var Company1Context = new ActionContext
        {
            TradeToSubmit = company1BuysRLFromAnyone,
            MarketToSubmitTo = testMarket,
            Period = period
        };
        Company1.QueueOrder(Company1Context);
        var Company2Context = new ActionContext
        {
            TradeToSubmit = company2BuysRLFromAnyone,
            MarketToSubmitTo = testMarket,
            Period = period
        };
        Company2.QueueOrder(Company2Context);
        var expectedCompany1Fills = 0;
        var expectedCompany2Fills = 0;
        var OrdersSentToMarket = tradeProcessor.GetOrders();
        // Act
        tradeProcessor.ExecuteBestTradesForGood(radioactiveLemonade,testMarket,OrdersSentToMarket);
        var actualCompany1Fills = company1BuysRLFromAnyone.FilledQuantity;
        var actualCompany2Fills = company2BuysRLFromAnyone.FilledQuantity;
        // Assert
        Assert.AreEqual(expectedCompany1Fills, actualCompany1Fills, "Company 1 did not fill the order correctly");
        Assert.AreEqual(expectedCompany2Fills, actualCompany2Fills, "Company 2 did not fill the order correctly");
    }
    [Test]
    public void EBTFG_OneBuyerMultSellersLowestSalePriceFills()
    {
        // Arrange
        var period = 0;
        var radioactiveLemonade = Good.CreateInstance("Radioactive Lemonade", new Price_band(10, 30), Rarity_enum.Uncommon);
        var Company1 = Company.Factory.Create("Company 1", CompanyLevelEnum.Beginner);
        var Company2 = Company.Factory.Create("Company 2", CompanyLevelEnum.Beginner);
        var Company3 = Company.Factory.Create("Company 3", CompanyLevelEnum.Beginner);
        Company3.GetInventory().AddGood(new InventoryEntry(radioactiveLemonade, 20, 10m, period));
        Company2.GetInventory().AddGood(new InventoryEntry(radioactiveLemonade, 20, 10m, period));
        var testMarket = Market.Factory.CreateMarket("Test Market", CompanyLevelEnum.Market, new LinearDemandStrategy());
        var tradeProcessor = new BasicTradeProcessor();
        testMarket.SetTradeProcessor(tradeProcessor);
        testMarket.RegisterCompany(Company1);
        testMarket.RegisterCompany(Company2);
        testMarket.RegisterCompany(Company3);
        var company1BuysRLFromAnyone= new Order(Company1, null, radioactiveLemonade, 20, 10m);
        var company2SellsRLToAnyone= new Order(null, Company2, radioactiveLemonade, 20, 10m);
        var company3SellsRLToAnyone= new Order(null, Company3, radioactiveLemonade, 20, 9m);
        var Company1Context = new ActionContext
        {
            TradeToSubmit = company1BuysRLFromAnyone,
            MarketToSubmitTo = testMarket,
            Period = period
        };
        Company1.QueueOrder(Company1Context);
        var Company2Context = new ActionContext
        {
            TradeToSubmit = company2SellsRLToAnyone,
            MarketToSubmitTo = testMarket,
            Period = period
        };
        Company2.QueueOrder(Company2Context);
        var Company3Context = new ActionContext
        {
            TradeToSubmit = company3SellsRLToAnyone,
            MarketToSubmitTo = testMarket,
            Period = period
        };
        Company3.QueueOrder(Company3Context);
        var expectedCompany1Fills = 20;
        var expectedCompany2Fills = 0;
        var expectedCompany3Fills = 20;
        var OrdersSentToMarket = tradeProcessor.GetOrders();
        // Act
        tradeProcessor.ExecuteBestTradesForGood(radioactiveLemonade,testMarket,OrdersSentToMarket);
        var actualCompany1Fills = company1BuysRLFromAnyone.FilledQuantity;
        var actualCompany2Fills = company2SellsRLToAnyone.FilledQuantity;
        var actualCompany3Fills = company3SellsRLToAnyone.FilledQuantity;
        // Assert
        Assert.AreEqual(expectedCompany1Fills, actualCompany1Fills, "Company 1 did not fill the order correctly");
        Assert.AreEqual(expectedCompany2Fills, actualCompany2Fills, "Company 2 did not fill the order correctly");
        Assert.AreEqual(expectedCompany3Fills, actualCompany3Fills, "Company 3 did not fill the order correctly");
    }
#endregion
}