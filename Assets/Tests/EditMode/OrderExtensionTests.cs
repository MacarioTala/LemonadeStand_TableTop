using NUnit.Framework;

[TestFixture]
public class OrderExtensionsTests
{
    private EconAgent buyer;
    private EconAgent seller;
    private Good lemonade;

    [SetUp]
    public void SetUp()
    {
        // Arrange
        buyer = EconAgentBuilder.For<EconAgent>()
            .Named("Buyer Company")
            .AtLevel(AgentLevelEnum.Beginner)
            .WithInitialCash(1000)
            .WithFixedCostStrategy(new BasicFixedCostStrategy())
            .Build();

        seller = EconAgentBuilder.For<EconAgent>()
            .Named("Seller Company")
            .AtLevel(AgentLevelEnum.Beginner)
            .WithInitialCash(1000)
            .WithFixedCostStrategy(new BasicFixedCostStrategy())
            .Build();

        lemonade = new GoodBuilder()
            .Named("Lemonade")
            .Costing(3)
            .Build();
    }

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(lemonade);

        buyer = null;
        seller = null;
        lemonade = null;
    }

    [Test]
    public void ToOrderSnapshot_ClonesOrderWithCorrectValues()
    {
        // Arrange
        var order = new Order(buyer, seller, lemonade, 10, 3)
        {
            SubmittingCompany = seller
        };

        // Act
        var snapshot = order.ToOrderSnapshot();

        // Assert
        Assert.NotNull(snapshot);
        Assert.AreEqual(order.Id, snapshot.OrderId);
        Assert.AreEqual(buyer.Name, snapshot.BuyerName);
        Assert.AreEqual(seller.Name, snapshot.SellerName);
        Assert.AreEqual(lemonade.GoodName, snapshot.GoodName);
        Assert.AreEqual(order.Quantity, snapshot.Quantity);
        Assert.AreEqual(order.Price, snapshot.Price);
    }
}