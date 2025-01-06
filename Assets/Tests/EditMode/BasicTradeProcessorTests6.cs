//This partial class tests the ProcessMarketOrder method of the BasicTradeProcessor class
using System.Collections.Generic;
using NUnit.Framework;
using static TestHelpers;

public partial class BasicTradeProcessorTests
{
[TestCase(TestName = "ProcessMarketOrder returns LemonadeStandResultObject.Success() when called with a valid order")]
public void PMO_SucceedsWhenCalledWithValidOrderAndCounterpartyOrder()
{
    //Note: fills happen as part of FulfillDemand, not ProcessMarketOrder
    //this is as of 2024-01-04
    //Arrange
    TestMarket.InitializeDemandForSpecificGood(Lemonade, 1000);

    Company1.GetInventory().AddGood(new InventoryEntry(Lemonade, 1000, 3m,0));
    var Company1SellsLemonadeToAnyone = new Order(null, Company1, Lemonade, 1000, 3.5m);
    var MarketCounterPartyOrder = new Order(TestMarket, Company1, Lemonade, 1000, 3.5m)
    {SubmittingCompany = TestMarket};

    var validContext = CreateActionContext( order: Company1SellsLemonadeToAnyone, 
                                            market: TestMarket, 
                                            period: Period
                                            ,new List<Order>{MarketCounterPartyOrder});
        validContext.PrimaryOrder = Company1SellsLemonadeToAnyone;
        validContext.PrimaryOrder.Buyer = TestMarket;

    var expectedResult = LemonadeStandResultObject.Success();
    //Act
    var actualResult = TestMarket.ProcessMarketOrder(validContext);
    //Assert
    Assert.AreEqual(expectedResult, actualResult);
}
}