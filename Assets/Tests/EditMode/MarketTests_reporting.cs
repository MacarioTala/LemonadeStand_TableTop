using NUnit.Framework;
using static TestHelpers;

public partial class MarketTests
{
    [Test]
    public void MarketLemonSellOrderVisibleToGetIngredientBidAskSpreadForPeriod()
    {
        //Arrange
        var lemonSaleA = new Order(null, TestMarket,lemon,20,2);
        TestMarket.GetInventory().AddInventoryEntry(new(lemon,20,1,0));
        var marketcontext = CreateActionContext(lemonSaleA,TestMarket,0);
        marketcontext.SubmittingCompany = TestMarket;
        TestMarket.QueueOrder(marketcontext);
        const int expectedRowCount = 1;
        var expectedGood = lemon;
        const decimal expectedBid = 0;
        const decimal expectedAsk = 2m;

        //Act
        var actualRow = TestMarket.GetIngredientBidAskSpreadForPeriod(TestMarket,0);
        var actualRowCount = actualRow.Count;
        var actualGood = actualRow[0].Good;
        var actualBid = actualRow[0].Bid;
        var actualAsk = actualRow[0].Ask;

        //Assert
        Assert.AreEqual(expectedRowCount,actualRowCount);
        Assert.AreEqual(expectedGood,actualGood);
        Assert.AreEqual(expectedBid,actualBid);
        Assert.AreEqual(expectedAsk,actualAsk);

        //TearDown
        TestMarket.GetInventory().Clear();
    }

    [Test]
    public void SelfSubmittedLemonSellOrderInvisibleToGetIngredientBidAskSpreadForPeriod()
    {
        //Arrange
        var lemonSaleA = new Order(null, Company1,lemon,20,2);
        Company1.GetInventory().AddInventoryEntry(new(lemon,20,1,0));
        var lemonSaleContext = CreateActionContext(lemonSaleA,TestMarket,0);
        
        Company1.QueueOrder(lemonSaleContext);

        //Act
        var actualRows = TestMarket.GetIngredientBidAskSpreadForPeriod(Company1,0);
        
        //Assert
        Assert.IsEmpty(actualRows);
        
        //TearDown
        Company1.GetInventory().Clear();
    }

    [Test]
    public void GetIngredientBidAskSpreadForPeriodReturnsPriceForCorrectPeriod()
    {
        //Arrange
        TestMarket.GetInventory().AddInventoryEntry(new(lemon,15,1,0));
        var Period1lemonSale = new Order(null, TestMarket,lemon,10,2);
        var marketcontext1 = CreateActionContext(Period1lemonSale,TestMarket,0);
        marketcontext1.SubmittingCompany = TestMarket;
        TestMarket.QueueOrder(marketcontext1);

        var Period2lemonSale = new Order(null, TestMarket,lemon,5,1);
        var marketcontext2 = CreateActionContext(Period2lemonSale,TestMarket,1);
        marketcontext2.SubmittingCompany = TestMarket;
        TestMarket.QueueOrder(marketcontext2);

        const decimal expectedAsk = 1m;
        
        //Act
        var actualRow = TestMarket.GetIngredientBidAskSpreadForPeriod(TestMarket,1);
        var actualAsk = actualRow[0].Ask;
        
        //Assert
        Assert.AreEqual(expectedAsk,actualAsk);
        
        //TearDown
        TestMarket.GetInventory().Clear();
    }
}