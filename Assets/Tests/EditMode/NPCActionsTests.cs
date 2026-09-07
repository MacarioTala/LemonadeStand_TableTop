using System.Collections.Generic;
using NUnit.Framework;
using static TestHelpers;

[TestFixture]
public class NPCActionsTests
{
    Market TestMarket;
    EconAgent Company1;
    EconAgent Company2;

    [SetUp]
    public void SetUp()
    {


        //Setup Market
        TestMarket = Market.Factory.CreateMarket("Test Market")
            .EnsureDefaults();

        //Setup companies
        Company1 = EconAgentBuilder.For<EconAgent>()
                        .Named("Company 1")
                        .AtLevel(AgentLevelEnum.Beginner)
                        .WithInitialCash(100)
                        .Build();

        Company2 = EconAgentBuilder.For<EconAgent>()
            .Named("Company2")
            .AtLevel(AgentLevelEnum.Beginner) 
            .WithInitialCash(100)
            .Build();
    }
    [TearDown]
    public void TearDown()
    {
        //Cleanup
        Company1.GetInventory().Clear();
        Company2.GetInventory().Clear();
    }

    [Test]
    public void CompaniesCanSubmitDifferentPricesForTheSameGood()
    {
        //Arrange
        var lemonade = new GoodBuilder()
            .Named("Lemonade")
            .Costing(5)
            .WhichIsProducedGood()
            .Build();
        
        Company1.GetInventory().AddInventoryEntry(new(lemonade,1,5,0));
        Company2.GetInventory().AddInventoryEntry(new(lemonade,1,6,0));
        var lemonadeSaleA = new Order(null, Company1, lemonade, 1, 6);
        var lemonadeSaleB = new Order(null, Company2, lemonade, 1, 7);
        var expectedGoodsAvailable = new List<AvailableGood>
        {
            new (Company1,lemonade,1,6),
            new (Company2,lemonade,1,7)
        };

        //Act
        Company1.QueueOrder(CreateActionContext(lemonadeSaleA,TestMarket,0));
        Company2.QueueOrder(CreateActionContext(lemonadeSaleB,TestMarket,0));
        var actualGoodsAvailable = TestMarket.GetGoodsAvailableInPeriod(TestMarket);

        //Assert
        Assert.AreEqual(expectedGoodsAvailable,actualGoodsAvailable);
        
    }

    [Test]
    public void GetGoodsAvailableInPeriodIncludesMarketInventory()
    {
        //Arrange
        var lemonade = new GoodBuilder()
            .Named("Lemonade")
            .Costing(5)
            .WhichIsProducedGood()
            .Build();
        
        var lemon = new GoodBuilder()
            .Named("Lemon")
            .Costing(1)
            .Build();
        
        TestMarket.GetInventory().AddInventoryEntry(new(lemon,20,1,0));
        Company1.GetInventory().AddInventoryEntry(new(lemonade,1,5,0));
        Company2.GetInventory().AddInventoryEntry(new(lemonade,1,6,0));
        var lemonadeSaleA = new Order(null, Company1, lemonade, 1, 6);
        var lemonadeSaleB = new Order(null, Company2, lemonade, 1, 7);
        var lemonSaleA = new Order(null, TestMarket,lemon,20,2);
        var expectedGoodsAvailable = new List<AvailableGood>
        {
            new (Company1,lemonade,1,6),
            new (Company2,lemonade,1,7),
            new (TestMarket,lemon,20,2)
        };

        //Act
        Company1.QueueOrder(CreateActionContext(lemonadeSaleA,TestMarket,0));
        Company2.QueueOrder(CreateActionContext(lemonadeSaleB,TestMarket,0));
        var marketcontext = CreateActionContext(lemonSaleA,TestMarket,0);
        marketcontext.SubmittingCompany = TestMarket;
        TestMarket.QueueOrder(marketcontext);
        var actualGoodsAvailable = TestMarket.GetGoodsAvailableInPeriod(TestMarket);

        //Assert
        Assert.AreEqual(expectedGoodsAvailable,actualGoodsAvailable);
    }

    [Test]
    public void CompaniesCannotSeeOtherCompaniesOrdersIfThoseOrdersAreOnlyProducedGoods()
    {
        //Arrange
        var lemonade = new GoodBuilder()
            .Named("Lemonade")
            .Costing(5)
            .WhichIsProducedGood()
            .Build();
        
        var lemon = new GoodBuilder()
            .Named("Lemon")
            .Costing(1)
            .Build();
        
        TestMarket.GetInventory().AddInventoryEntry(new(lemon,20,1,0));
        Company1.GetInventory().AddInventoryEntry(new(lemonade,1,5,0));
        Company2.GetInventory().AddInventoryEntry(new(lemonade,1,6,0));
        var lemonadeSaleA = new Order(null, Company1, lemonade, 1, 6);
        var lemonadeSaleB = new Order(null, Company2, lemonade, 1, 7);
        var lemonSaleA = new Order(null, TestMarket,lemon,20,2);
        var expectedGoodsAvailable = new List<AvailableGood>
        {
            new (TestMarket,lemon,20,2)
        };

        //Act
        Company1.QueueOrder(CreateActionContext(lemonadeSaleA,TestMarket,0));
        Company2.QueueOrder(CreateActionContext(lemonadeSaleB,TestMarket,0));
        var marketcontext = CreateActionContext(lemonSaleA,TestMarket,0);
        marketcontext.SubmittingCompany = TestMarket;
        TestMarket.QueueOrder(marketcontext);
        var actualGoodsAvailable = TestMarket.GetGoodsAvailableInPeriod(Company1);

        //Assert
        Assert.AreEqual(expectedGoodsAvailable,actualGoodsAvailable);
    }

    [Test]
    public void CompaniesCanSeeIfOtherCompaniesAreSellingRawGoods()
    {
        //Arrange
        var lemonade = new GoodBuilder()
            .Named("Lemonade")
            .Costing(5)
            .WhichIsProducedGood()
            .Build();
        
        var lemon = new GoodBuilder()
            .Named("Lemon")
            .Costing(1)
            .Build();
        
        TestMarket.GetInventory().AddInventoryEntry(new(lemon,20,1,0));
        Company1.GetInventory().AddInventoryEntry(new(lemonade,1,5,0));
        Company2.GetInventory().AddInventoryEntry(new(lemon,1,1,0));
        var lemonadeSaleA = new Order(null, Company1, lemonade, 1, 6);
        var lemonSaleB = new Order(null, Company2, lemon, 1, 1);
        var lemonSaleA = new Order(null, TestMarket,lemon,20,2);
        var expectedGoodsAvailable = new List<AvailableGood>
        {
            new (Company2,lemon,1,1),
            new (TestMarket,lemon,20,2)
        };

        //Act
        Company1.QueueOrder(CreateActionContext(lemonadeSaleA,TestMarket,0));
        Company2.QueueOrder(CreateActionContext(lemonSaleB,TestMarket,0));
        var marketcontext = CreateActionContext(lemonSaleA,TestMarket,0);
        marketcontext.SubmittingCompany = TestMarket;
        TestMarket.QueueOrder(marketcontext);
        var actualGoodsAvailable = TestMarket.GetGoodsAvailableInPeriod(Company1);

        //Assert
        Assert.AreEqual(expectedGoodsAvailable,actualGoodsAvailable);
        
    }
    [Test]
    public void IfACompanyHasBothProducedAndRawGoodsInOrdersOnlyRawGoodsAreVisible()
    {
        //Arrange
        var lemonade = new GoodBuilder()
            .Named("Lemonade")
            .Costing(5)
            .WhichIsProducedGood()
            .Build();
        
        var lemon = new GoodBuilder()
            .Named("Lemon")
            .Costing(1)
            .Build();
        
        TestMarket.GetInventory().AddInventoryEntry(new(lemon,20,1,0));
        Company1.GetInventory().AddInventoryEntry(new(lemonade,1,5,0));
        Company2.GetInventory().AddInventoryEntry(new(lemon,1,1,0));
        Company2.GetInventory().AddInventoryEntry(new(lemon,1,1,0));

        var lemonadeSaleA = new Order(null, Company1, lemonade, 1, 6);
        var lemonadeSaleB = new Order(null, Company2, lemonade, 1, 7);
        var lemonSaleB = new Order(null, Company2, lemon, 1, 1);
        var lemonSaleA = new Order(null, TestMarket,lemon,20,2);
        var expectedGoodsAvailable = new List<AvailableGood>
        {
            new (Company2,lemon,1,1),
            new (TestMarket,lemon,20,2)
        };

        //Act
        Company1.QueueOrder(CreateActionContext(lemonadeSaleA,TestMarket,0));
        Company2.QueueOrder(CreateActionContext(lemonSaleB,TestMarket,0));
        Company2.QueueOrder(CreateActionContext(lemonadeSaleB,TestMarket,0));
        var marketcontext = CreateActionContext(lemonSaleA,TestMarket,0);
        marketcontext.SubmittingCompany = TestMarket;
        TestMarket.QueueOrder(marketcontext);
        var actualGoodsAvailable = TestMarket.GetGoodsAvailableInPeriod(Company1);

        //Assert
        Assert.AreEqual(expectedGoodsAvailable,actualGoodsAvailable);
        
    }
}