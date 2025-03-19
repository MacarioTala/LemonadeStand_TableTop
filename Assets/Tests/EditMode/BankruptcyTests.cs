 using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class BankruptcyTests
{
    private readonly TheEconomy testEconomy=TheEconomy.Instance;
    Market TestMarket;
    Company Company1;

    readonly iFixedCostStrategy TestFixedCostStrategy = new BasicFixedCostStrategy();
    readonly iDemandStrategy TestDemandStrategy = ScriptableObject.CreateInstance<LinearDemandStrategy>();
    readonly iMarketDataService TestMarketDataService = new MockMarketDataService();
    [SetUp]
    public void Setup()
    {
        TheEconomy.SetupForTests(new MockLogger());
        var existingMarket = TheEconomy.Instance.GetMarketByName("The First Market");
        testEconomy.RemoveMarket(existingMarket);

        TestMarket= Market.Factory.CreateStarterMarket("Test Market", CompanyLevelEnum.Market, TestDemandStrategy);
        TestMarket.SetMarketDataService(TestMarketDataService);
        testEconomy.RegisterCompany(TestMarket);

        Company1 = Company.Factory.Create("Company 1", CompanyLevelEnum.Beginner, null, TestFixedCostStrategy);
        TestMarket.RegisterCompany(Company1);
    }

    [Test]
    public void CompanyWithoutCashGoesBankrupt()
    {
        //Arrange
        Company1.SetCash(0);
        var expected = true;
        //Act
        testEconomy.EndTradingPeriod();
        var actual = Company1.IsBankrupt();
        //Assert
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void CompanyGoesBankruptWhenItCannotPayFixedCosts()
    {
        //Entities currently go bankrupt as soon as they hit zero cash
        //Change this behaviour when we implement TheEconomy.StartTradingPeriod()

        //Arrange
        const int tradingCycles=2;
        Company1.SetCash(1000);
 
        var rent = new FixedCost
        {
            Description = "Rent",
            FixedCostType = FixedCostEnum.Rent,
            Amount = 1000,
            Frequency = 1,
            PeriodAcquired = 0
        };
        Company1.FixedCosts.Add(rent);
        var expected = true;
        //Act
        for (int i = 0; i < tradingCycles; i++)
        {
            testEconomy.EndTradingPeriod();
        }
        var actual = Company1.IsBankrupt();
        //Assert
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void ACompanyGoingBankruptShouldTriggerAnEvent()
    {throw new System.NotImplementedException();}

    [TearDown]
    public void TearDown()
    {
        if(testEconomy != null)
        {
            Object.DestroyImmediate(testEconomy.gameObject);
        }
        //Cleanup
        if (Company1 != null)
        {
            TestMarket.RemoveCompany(Company1);
            Company1 = null;
        }
        if (TestMarket != null)
        {
            testEconomy.RemoveMarket(TestMarket);
            TestMarket = null;
        }
    }
}
