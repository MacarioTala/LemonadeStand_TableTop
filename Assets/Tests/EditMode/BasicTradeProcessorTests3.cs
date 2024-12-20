using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

public partial class BasicTradeProcessorTests
{
    #region ExecuteBestTradesForGood Tests

    public static IEnumerable<TestCaseData> TestCases_OneBuyerOneSeller{
        get
        {
            yield return new TestCaseData(10,10m,10,10,10,10,10m,10m)
             .SetName("One Buyer, One Seller, Perfect Match" );
            yield return new TestCaseData(10,10m,10,5,5,5,10m,10m)
             .SetName("One Buyer, One Seller, Demand Exceeds Supply");
            yield return new TestCaseData(10,10m,5,10,5,5,10m,10m)
             .SetName("One Buyer, One Seller, Supply Exceeds Demand");
        }
    }
    [Test,TestCaseSource(nameof(TestCases_OneBuyerOneSeller))]
    public void EBTFG_OneBuyerOneSeller(int sellerInventoryQuantity,
                                        decimal sellerAcquirePrice,
                                        int buyerOrderQuantity, 
                                        int sellerOrderQuantity,
                                        int expectedBuyerFill,
                                        int expectedSellerFill,
                                        decimal buyerBid,
                                        decimal sellerAsk
                                        )
    {
        // Arrange
        Company2.GetInventory().AddGood(new InventoryEntry(Lemonade, sellerInventoryQuantity, sellerAcquirePrice, Period));
        var company1BuysRLFromCompany2ByCompany1 = new Order(Company1, Company2, Lemonade, buyerOrderQuantity, buyerBid);
        var company2SellsRLToCompany1ByCompany2 = new Order(Company1, Company2, Lemonade,sellerOrderQuantity, sellerAsk);
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
        var OrdersSentToMarket = TestTradeProcessor.GetOrders();
        // Act
        TestTradeProcessor.ExecuteBestTradesForGood(Lemonade,TestMarket,OrdersSentToMarket);
        var actualCompany1Fills = company1BuysRLFromCompany2ByCompany1.FilledQuantity;
        var actualCompany2Fills = company2SellsRLToCompany1ByCompany2.FilledQuantity;
        // Assert
        Assert.AreEqual(expectedBuyerFill, actualCompany1Fills, "Company 1 did not fill the order");
        Assert.AreEqual(expectedSellerFill, actualCompany2Fills, "Company 2 did not fill the order");
    }
    [Test]
    public void EBTFG_OneBuyerMultipleSellersPerfectMatchAllFill()
    {
        // Arrange
        var Company3 = Company.Factory.Create("Company 3", CompanyLevelEnum.Beginner);
        Company2.GetInventory().AddGood(new InventoryEntry(RadioactiveLemonade, 15, 10m, Period));
        Company3.GetInventory().AddGood(new InventoryEntry(RadioactiveLemonade, 10, 10m, Period));
        
        TestMarket.RegisterCompany(Company3);
        var company1BuysRLFromAnyoneByCompany1 = new Order(Company1, null, RadioactiveLemonade, 20, 10m);
        var company2SellsRLToAnyone1ByCompany2 = new Order(null, Company2, RadioactiveLemonade, 11, 10m);
        var company3SellsRLToAnyone2ByCompany3 = new Order(null, Company3, RadioactiveLemonade, 9, 10m);
        var Company1Context = new ActionContext
        {
            TradeToSubmit = company1BuysRLFromAnyoneByCompany1,
            MarketToSubmitTo = TestMarket,
            Period = Period
        };
        Company1.QueueOrder(Company1Context);
        var Company2Context = new ActionContext
        {
            TradeToSubmit = company2SellsRLToAnyone1ByCompany2,
            MarketToSubmitTo = TestMarket,
            Period = Period
        };
        Company2.QueueOrder(Company2Context);
        var Company3Context = new ActionContext
        {
            TradeToSubmit = company3SellsRLToAnyone2ByCompany3,
            MarketToSubmitTo = TestMarket,
            Period = Period
        };
        Company3.QueueOrder(Company3Context);
        var expectedCompany1Fills = 20;
        var expectedCompany2Fills = 11;
        var expectedCompany3Fills = 9;
        var OrdersSentToMarket = TestTradeProcessor.GetOrders();
        // Act
        TestTradeProcessor.ExecuteBestTradesForGood(RadioactiveLemonade,TestMarket,OrdersSentToMarket);
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
    public void EBTFG_MultipleBuyersOneSellerWhaleFillsFirst()
    {
        // Arrange
        var Company3 = Company.Factory.Create("Company 3", CompanyLevelEnum.Beginner);
        Company3.GetInventory().AddGood(new InventoryEntry(RadioactiveLemonade, 30, 10m, Period));
        TestMarket.RegisterCompany(Company3);
        var company1BuysRLFromAnyoneByCompany1 = new Order(Company1, null, RadioactiveLemonade, 20, 10m);
        var company2BuysRLFromAnyoneByCompany2 = new Order(Company2, null, RadioactiveLemonade, 15, 10m);
        var company3SellsRLToAnyoneByCompany3 = new Order(null, Company3, RadioactiveLemonade, 30, 10m);
        var Company1Context = new ActionContext
        {
            TradeToSubmit = company1BuysRLFromAnyoneByCompany1,
            MarketToSubmitTo = TestMarket,
            Period = Period
        };
        Company1.QueueOrder(Company1Context);
        var Company2Context = new ActionContext
        {
            TradeToSubmit = company2BuysRLFromAnyoneByCompany2,
            MarketToSubmitTo = TestMarket,
            Period = Period
        };
        Company2.QueueOrder(Company2Context);
        var Company3Context = new ActionContext
        {
            TradeToSubmit = company3SellsRLToAnyoneByCompany3,
            MarketToSubmitTo = TestMarket,
            Period = Period
        };
        Company3.QueueOrder(Company3Context);
        var expectedCompany1Fills = 20;
        var expectedCompany2Fills = 10;
        var expectedCompany3Fills = 30;
        var OrdersSentToMarket = TestTradeProcessor.GetOrders();
        // Act
        TestTradeProcessor.ExecuteBestTradesForGood(RadioactiveLemonade,TestMarket,OrdersSentToMarket);
        var actualCompany1Fills = company1BuysRLFromAnyoneByCompany1.FilledQuantity;
        var actualCompany2Fills = company2BuysRLFromAnyoneByCompany2.FilledQuantity;
        var actualCompany3Fills = company3SellsRLToAnyoneByCompany3.FilledQuantity;
        // Assert
        Assert.AreEqual(expectedCompany1Fills, actualCompany1Fills, "Company 1 did not fill the order correctly");
        Assert.AreEqual(expectedCompany2Fills, actualCompany2Fills, "Company 2 did not fill the order correctly");
        Assert.AreEqual(expectedCompany3Fills, actualCompany3Fills, "Company 3 did not fill the order correctly");
    }

    [Test]
    public void EBTFG_NoBuyersNoFills()
    {
        // Arrange
        Company1.GetInventory().AddGood(new InventoryEntry(RadioactiveLemonade, 10, 10m, Period));
        Company2.GetInventory().AddGood(new InventoryEntry(RadioactiveLemonade, 10, 10m, Period));
        var company1SellsRLToAnyone= new Order(null, Company1, RadioactiveLemonade, 10, 10m);
        var company2SellsRLToAnyone= new Order(null, Company2, RadioactiveLemonade, 10, 10m);
        var Company1Context = new ActionContext
        {
            TradeToSubmit = company1SellsRLToAnyone,
            MarketToSubmitTo = TestMarket,
            Period = Period
        };
        Company1.QueueOrder(Company1Context);
        var Company2Context = new ActionContext
        {
            TradeToSubmit = company2SellsRLToAnyone,
            MarketToSubmitTo = TestMarket,
            Period = Period
        };
        Company2.QueueOrder(Company2Context);
        var expectedCompany1Fills = 0;
        var expectedCompany2Fills = 0;
        var OrdersSentToMarket = TestTradeProcessor.GetOrders();
        // Act
        TestTradeProcessor.ExecuteBestTradesForGood(RadioactiveLemonade,TestMarket,OrdersSentToMarket);
        var actualCompany1Fills = company1SellsRLToAnyone.FilledQuantity;
        var actualCompany2Fills = company2SellsRLToAnyone.FilledQuantity;
        // Assert
        Assert.AreEqual(expectedCompany1Fills, actualCompany1Fills, "Company 1 did not fill the order correctly");
        Assert.AreEqual(expectedCompany2Fills, actualCompany2Fills, "Company 2 did not fill the order correctly");
    }
    [Test]
    public void EBTFG_NoSellersNoFills()
    {
        // Arrange
        var company1BuysRLFromAnyone= new Order(Company1, null, RadioactiveLemonade, 10, 10m);
        var company2BuysRLFromAnyone= new Order(Company2, null, RadioactiveLemonade, 10, 10m);
        var Company1Context = new ActionContext
        {
            TradeToSubmit = company1BuysRLFromAnyone,
            MarketToSubmitTo = TestMarket,
            Period = Period
        };
        Company1.QueueOrder(Company1Context);
        var Company2Context = new ActionContext
        {
            TradeToSubmit = company2BuysRLFromAnyone,
            MarketToSubmitTo = TestMarket,
            Period = Period
        };
        Company2.QueueOrder(Company2Context);
        var expectedCompany1Fills = 0;
        var expectedCompany2Fills = 0;
        var OrdersSentToMarket = TestTradeProcessor.GetOrders();
        // Act
        TestTradeProcessor.ExecuteBestTradesForGood(RadioactiveLemonade,TestMarket,OrdersSentToMarket);
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
        var Company3 = Company.Factory.Create("Company 3", CompanyLevelEnum.Beginner);
        TestMarket.RegisterCompany(Company3);
        Company3.GetInventory().AddGood(new InventoryEntry(RadioactiveLemonade, 20, 10m, Period));
        Company2.GetInventory().AddGood(new InventoryEntry(RadioactiveLemonade, 20, 10m, Period));
        
        var company1BuysRLFromAnyone= new Order(Company1, null, RadioactiveLemonade, 20, 10m);
        var company2SellsRLToAnyone= new Order(null, Company2, RadioactiveLemonade, 20, 10m);
        var company3SellsRLToAnyone= new Order(null, Company3, RadioactiveLemonade, 20, 9m);
        var Company1Context = new ActionContext
        {
            TradeToSubmit = company1BuysRLFromAnyone,
            MarketToSubmitTo = TestMarket,
            Period = Period
        };
        Company1.QueueOrder(Company1Context);
        var Company2Context = new ActionContext
        {
            TradeToSubmit = company2SellsRLToAnyone,
            MarketToSubmitTo = TestMarket,
            Period = Period
        };
        Company2.QueueOrder(Company2Context);
        var Company3Context = new ActionContext
        {
            TradeToSubmit = company3SellsRLToAnyone,
            MarketToSubmitTo = TestMarket,
            Period = Period
        };
        Company3.QueueOrder(Company3Context);
        var expectedCompany1Fills = 20;
        var expectedCompany2Fills = 0;
        var expectedCompany3Fills = 20;
        var OrdersSentToMarket = TestTradeProcessor.GetOrders();
        // Act
        TestTradeProcessor.ExecuteBestTradesForGood(RadioactiveLemonade,TestMarket,OrdersSentToMarket);
        var actualCompany1Fills = company1BuysRLFromAnyone.FilledQuantity;
        var actualCompany2Fills = company2SellsRLToAnyone.FilledQuantity;
        var actualCompany3Fills = company3SellsRLToAnyone.FilledQuantity;
        // Assert
        Assert.AreEqual(expectedCompany1Fills, actualCompany1Fills, "Company 1 did not fill the order correctly");
        Assert.AreEqual(expectedCompany2Fills, actualCompany2Fills, "Company 2 did not fill the order correctly");
        Assert.AreEqual(expectedCompany3Fills, actualCompany3Fills, "Company 3 did not fill the order correctly");
    }

    [Test]
    public void EBTFG_OneSellerMultipleBuyersHighestBuyQuantityFills()
    {
        //Arrange
        var Company3 = Company.Factory.Create("Company 3", CompanyLevelEnum.Beginner);
        TestMarket.RegisterCompany(Company3);
        Company3.GetInventory().AddGood(new InventoryEntry(RadioactiveLemonade, 20, 10m, Period));

        var Company1BuysRLFromAnyone = new Order(Company1, null, RadioactiveLemonade, 20, 10m);
        var Company2BuysRLFromAnyone = new Order(Company2, null, RadioactiveLemonade, 15, 10m);
        var Company3SellsRLToAnyone = new Order(null, Company3, RadioactiveLemonade, 20, 10m);

        Company1.QueueOrder( new ActionContext
        {
            TradeToSubmit = Company1BuysRLFromAnyone,
            MarketToSubmitTo = TestMarket,
            Period = Period
        });
        Company2.QueueOrder( new ActionContext
        {
            TradeToSubmit = Company2BuysRLFromAnyone,
            MarketToSubmitTo = TestMarket,
            Period = Period
        });
        Company3.QueueOrder( new ActionContext
        {
            TradeToSubmit = Company3SellsRLToAnyone,
            MarketToSubmitTo = TestMarket,
            Period = Period
        });
        var expectedCompany1Fills = 20;
        var expectedCompany2Fills = 0;
        var expectedCompany3Fills = 20;
        var OrdersSentToMarket = TestTradeProcessor.GetOrders();
        //Act
        TestTradeProcessor.ExecuteBestTradesForGood(RadioactiveLemonade,TestMarket,OrdersSentToMarket);
        var actualCompany1Fills = Company1BuysRLFromAnyone.FilledQuantity;
        var actualCompany2Fills = Company2BuysRLFromAnyone.FilledQuantity;
        var actualCompany3Fills = Company3SellsRLToAnyone.FilledQuantity;
        //Assert
        Assert.AreEqual(expectedCompany1Fills, actualCompany1Fills, "Company 1 did not fill the order correctly");
        Assert.AreEqual(expectedCompany2Fills, actualCompany2Fills, "Company 2 did not fill the order correctly");
        Assert.AreEqual(expectedCompany3Fills, actualCompany3Fills, "Company 3 did not fill the order correctly");
    }
#endregion
}