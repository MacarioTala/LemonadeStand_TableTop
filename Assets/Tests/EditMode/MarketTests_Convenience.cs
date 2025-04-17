using System.Linq;
using NUnit.Framework;
using UnityEngine;
using static TestHelpers;

public partial class MarketTests
{
    [Test]
    public void GetOrdersSentToMarketByCompanyReturnsOnlyCompanyAOrdersWhenPassed()
    {
        //Arrange
        var lemonsCompanyWillSellToMarket = 500;
        var currentPeriod = 0;
        var basicDemandStrategy = ScriptableObject.CreateInstance<LinearDemandStrategy>();
        var convenienceMarket = Market.Factory.CreateMarket("Convenience Market", CompanyLevelEnum.Market)
            .WithDemandStrategy(basicDemandStrategy)
            .WithTradeProcessor(new BasicTradeProcessor())
            .WithTransactionManager(new BasicTransactionManager())
            ;
        var companyA = Company.Factory.Create("CompanyA", CompanyLevelEnum.Beginner);
        var companyB = Company.Factory.Create("CompanyB", CompanyLevelEnum.Beginner);
        var lemonSaleA = new Order(TestMarket, Company1, lemon, lemonsCompanyWillSellToMarket, 3.0m);
        var lemonSaleB = new Order(TestMarket, Company2, lemon, lemonsCompanyWillSellToMarket, 3.0m);

        convenienceMarket.RegisterCompany(companyA);
        convenienceMarket.RegisterCompany(companyB);

        var expected = lemonSaleA;
        
        //Act
        companyA.QueueOrder(CreateActionContext(lemonSaleA, convenienceMarket,currentPeriod));
        companyB.QueueOrder(CreateActionContext(lemonSaleB, convenienceMarket,currentPeriod));
        var actual = convenienceMarket.GetOrdersSentToMarketByCompany(companyA).FirstOrDefault();
        var actualOrdersReturned = convenienceMarket.GetOrdersSentToMarketByCompany(companyA).Count();

        //Assert
        Assert.AreEqual(expected, actual);
        Assert.AreEqual(1, actualOrdersReturned);
    }
}