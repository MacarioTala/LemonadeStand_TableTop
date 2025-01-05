using System.Linq;
using NUnit.Framework;

//This partial class tests the RecordTrade method of the BasicTradeProcessor
public partial class BasicTradeProcessorTests
{
    //For RecordTrade
    [Test]
    public void RT_OneBuyerOneSeller()
    {
        //Arrange
        Company2.GetInventory().AddGood(new InventoryEntry(Lemonade, 5,9m,Period));
        var Company1BuysLemonadeFromAny = new Order(Company1,null,Lemonade,5,10m);
        var Company1Context = new ActionContext
        {
            TradeToSubmit = Company1BuysLemonadeFromAny,
            MarketToSubmitTo = TestMarket,
            Period = Period
        };
        var Company2SellsLemonadeToAny = new Order(null,Company2,Lemonade,5,10m);
        var Company2Context = new ActionContext
        {
            TradeToSubmit = Company2SellsLemonadeToAny,
            MarketToSubmitTo = TestMarket,
            Period = Period
        };
        Company1.QueueOrder(Company1Context);
        Company2.QueueOrder(Company2Context);
        var OrdersSentToMarket = TestTradeProcessor.GetOrders();
        //Act
        TestTradeProcessor.ExecuteBestTradesForGood(Lemonade,TestMarket,OrdersSentToMarket);
        //Assert
        var transactionsRecorded = TestMarket.GetMarketTradesInPeriod(Period);
        var recordedTransaction = transactionsRecorded?.FirstOrDefault();
        Assert.IsNotNull(recordedTransaction);
        Assert.IsTrue(transactionsRecorded.Count == 2);
        Assert.AreEqual(Company1,recordedTransaction.RecordedTrade.Buyer);
        Assert.AreEqual(Company2,recordedTransaction.RecordedTrade.Seller);
        Assert.AreEqual(Lemonade,recordedTransaction.RecordedTrade.Good);
        Assert.AreEqual(5,recordedTransaction.RecordedTrade.FilledQuantity);
        Assert.AreEqual(10m,recordedTransaction.RecordedTrade.Price);

    }
    
     
    [Test,Description("One Buyer, Multiple Sellers, both sellers fill")]
    public void RT_OneBuyerMultSellerBothSellersFill()
    {
        //Arrange
        var Company3 = Company.Factory.Create("Company 3", CompanyLevelEnum.Beginner);
        TestMarket.RegisterCompany(Company3);
        Company2.GetInventory().AddGood(new InventoryEntry(Lemonade, 3,9m,Period));
        Company3.GetInventory().AddGood(new InventoryEntry(Lemonade, 2,9m,Period));
        var Company1BuysLemonadeFromAny = new Order(Company1,null,Lemonade,5,10m);
        var Company1Context = new ActionContext
        {
            TradeToSubmit = Company1BuysLemonadeFromAny,
            MarketToSubmitTo = TestMarket,
            Period = Period
        };
        var Company2SellsLemonadeToAny = new Order(null,Company2,Lemonade,3,10m);
        var Company2Context = new ActionContext
        {
            TradeToSubmit = Company2SellsLemonadeToAny,
            MarketToSubmitTo = TestMarket,
            Period = Period
        };
        var Company3SellsLemonadeToAny = new Order(null,Company3,Lemonade,2,10m);
        var Company3Context = new ActionContext
        {
            TradeToSubmit = Company3SellsLemonadeToAny,
            MarketToSubmitTo = TestMarket,
            Period = Period
        };
        Company1.QueueOrder(Company1Context);
        Company2.QueueOrder(Company2Context);
        Company3.QueueOrder(Company3Context);
        var OrdersSentToMarket = TestTradeProcessor.GetOrders();
        //Act
        TestTradeProcessor.ExecuteBestTradesForGood(Lemonade,TestMarket,OrdersSentToMarket);
        //Assert
        var transactionsInMarket = TestMarket.GetMarketTradesInPeriod(Period); 
        var recordedTransaction = transactionsInMarket.FirstOrDefault();
        Assert.IsNotNull(recordedTransaction);
        Assert.IsTrue(transactionsInMarket.Count == 3);
        Assert.AreEqual(Company1,recordedTransaction.RecordedTrade.Buyer);
        Assert.AreEqual(null,recordedTransaction.RecordedTrade.Seller);
        Assert.AreEqual(Lemonade,recordedTransaction.RecordedTrade.Good);
        Assert.AreEqual(5,recordedTransaction.RecordedTrade.FilledQuantity);
        Assert.AreEqual(10m,recordedTransaction.RecordedTrade.Price);
        Assert.IsTrue(recordedTransaction.CounterPartyTrades.Select(x=>x.Seller).Contains(Company2));
        Assert.IsTrue(recordedTransaction.CounterPartyTrades.Select(x=>x.Seller).Contains(Company3));
    }

    [Test,Description("One Buyer, Multiple Sellers, Low Price seller fills")]
    public void RT_OneBuyerMultSellerOneSellerFills()
    {
        //Arrange
        var Company3 = Company.Factory.Create("Company 3", CompanyLevelEnum.Beginner);
        TestMarket.RegisterCompany(Company3);
        Company2.GetInventory().AddGood(new InventoryEntry(Lemonade, 5,9m,Period));
        Company3.GetInventory().AddGood(new InventoryEntry(Lemonade, 2,10m,Period));
        var Company1BuysLemonadeFromAny = new Order(Company1,null,Lemonade,5,10m);
        var Company1Context = new ActionContext
        {
            TradeToSubmit = Company1BuysLemonadeFromAny,
            MarketToSubmitTo = TestMarket,
            Period = Period
        };
        var Company2SellsLemonadeToAny = new Order(null,Company2,Lemonade,5,10m);
        var Company2Context = new ActionContext
        {
            TradeToSubmit = Company2SellsLemonadeToAny,
            MarketToSubmitTo = TestMarket,
            Period = Period
        };
        var Company3SellsLemonadeToAny = new Order(null,Company3,Lemonade,2,10m);
        var Company3Context = new ActionContext
        {
            TradeToSubmit = Company3SellsLemonadeToAny,
            MarketToSubmitTo = TestMarket,
            Period = Period
        };
        Company1.QueueOrder(Company1Context);
        Company2.QueueOrder(Company2Context);
        Company3.QueueOrder(Company3Context);
        var OrdersSentToMarket = TestTradeProcessor.GetOrders();
        //Act
        TestTradeProcessor.ExecuteBestTradesForGood(Lemonade,TestMarket,OrdersSentToMarket);
        //Assert
        var transactionsInMarket = TestMarket.GetMarketTradesInPeriod(Period);
        var recordedTransaction = transactionsInMarket?.FirstOrDefault();
        Assert.IsNotNull(recordedTransaction);
        Assert.IsTrue(transactionsInMarket.Count == 2); //Note: Company 3's order might be processed
                                                        //downstream, during market trade processing
        Assert.AreEqual(Company1,recordedTransaction.RecordedTrade.Buyer);
        Assert.AreEqual(Company2,recordedTransaction.RecordedTrade.Seller);
        Assert.AreEqual(Lemonade,recordedTransaction.RecordedTrade.Good);
        Assert.AreEqual(5,recordedTransaction.RecordedTrade.FilledQuantity);
        Assert.AreEqual(10m,recordedTransaction.RecordedTrade.Price);
        Assert.IsTrue(recordedTransaction.CounterPartyTrades.Select(x=>x.Seller).Contains(Company2),"Company 2 should be a counterparty");
        Assert.IsFalse(recordedTransaction.CounterPartyTrades.Select(x=>x.Seller).Contains(Company3),"Company 3 should not be a counterparty");
    }

    

}