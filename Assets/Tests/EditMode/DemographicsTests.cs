using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class DemographicsTests
{
    [Test]
    public void PopulationShouldComeFromPopulationCompany()
    {
        // Arrange
        var testDemandStrategy = ScriptableObject.CreateInstance<LinearDemandStrategy>();
        var demographicManager = new BasicDemographicManager();

        const int expectedPopulation = 1000;
        var testPopulationCompany = CompanyBuilder.For<PopulationCompany>()
            .WithPopulation(expectedPopulation)
            .Named("Test Company")
            .Build();
        
        var testMarket = Market.Factory.CreateStarterMarket("Test Market",CompanyLevelEnum.Market,testDemandStrategy)
        .WithDemographicManager(demographicManager);
        
        testMarket.RegisterMarketParticipant(testPopulationCompany);

        // Act
        var actualPopulation = testMarket.GetPopulation();
        // Assert
        Assert.AreEqual(expectedPopulation, actualPopulation);
    }
}