 using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class BankruptcyTests
{
    TheEconomy testEconomy;
    [SetUp]
    public void Setup()
    {
        var economyObject = new GameObject();
        testEconomy = economyObject.AddComponent<TheEconomy>();
        testEconomy.Initialize(new MockLogger());
    }

    [Test]
    public void CompanyGoesBankruptWhenItCannotPayFixedCosts()
    {
        //Entities currently go bankrupt as soon as they hit zero cash
        //Change this behaviour when we implement TheEconomy.StartTradingPeriod()

        //Arrange
        const int tradingCycles=2;
        var testMarket = Market.Factory.CreateStarterMarket("Test Market", CompanyLevelEnum.Market, new LinearDemandStrategy());
        var fixedCostStrategy = new BasicFixedCostStrategy();
        var company1 = Company.Factory.Create("Company 1", CompanyLevelEnum.Beginner, null, fixedCostStrategy);
        company1.SetCash(1000);

        testEconomy.RegisterCompany(testMarket);
        testMarket.RegisterCompany(company1);
        
        var rent = new FixedCost
        {
            Description = "Rent",
            FixedCostType = FixedCostEnum.Rent,
            Amount = 1000,
            Frequency = 1,
            PeriodAcquired = 0
        };
        company1.FixedCosts.Add(rent);
        var expected = true;
        //Act
        for (int i = 0; i < tradingCycles; i++)
        {
            testEconomy.EndTradingPeriod();
        }
        var actual = company1.IsBankrupt();
        //Assert
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void ACompanyGoingBankruptShouldTriggerAnEvent()
    {throw new System.NotImplementedException();}
    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(testEconomy.gameObject);
    }
}
