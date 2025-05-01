using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class BuilderTests
{
#region Markets
    [Test]
    public void MarketBuilderWithDemandStrategyShouldSetDemandStrategy()
    {
        // Arrange
        var testDemandStrategy = ScriptableObject.CreateInstance<LinearDemandStrategy>();
        var market = Market.Create()
                    .WithDemandStrategy(testDemandStrategy);

        // Act
        market.WithDemandStrategy(testDemandStrategy);

        // Assert
        Assert.AreEqual(testDemandStrategy, market.DemandStrategy);
    }
#endregion

#region Companies
    [Test]
    public void CompanyNamedShouldSetName()
    {
        // Arrange
        const string expectedName = "Test Company";
        var testCompany = CompanyBuilder.For<Company>()
                    .Named(expectedName)
                    .Build();
        // Act
        var actual = testCompany.Name;
        // Assert
        Assert.AreEqual(expectedName, actual);
    }
    [Test]
    public void UsingTheBuilderToBuildPopulationCompanyShouldReturnPopulationCompany()
    {
        //Arrange
        var testCompany = CompanyBuilder.For<PopulationCompany>()
                    .Build();
        var expected = typeof(PopulationCompany);
        //Act
        var actual = testCompany.GetType();
        //Assert
        Assert.AreEqual(expected, actual);
        Assert.IsTrue(testCompany is PopulationCompany);
    }
#endregion
}