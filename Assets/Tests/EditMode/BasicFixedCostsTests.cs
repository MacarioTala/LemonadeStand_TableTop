using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class BasicFixedCostsTests
{
    /// <summary>
    /// Note: All of these tests use the BasicFixedCost Strategy implementation
    /// It was too much trouble to refactor the tests to just test the BasicFixedCost Strategy
    /// If a kind soul wants to refactor the tests to JUST test the BasicFixedCost Strategy, please do so
    /// </summary>
    [Test]
    public void CalcFixedCostsForPeriodSumsAllValidCosts()
    {
        // Arrange
        var testFixedCostStrategy= new BasicFixedCostStrategy();

        var company1 = EconAgentBuilder.For<EconAgent>()
                        .Named("Company 1")
                        .AtLevel(AgentLevelEnum.Beginner)
                        .WithFixedCostStrategy(testFixedCostStrategy)
                        .Build();
        var rentTemplate = ScriptableObject.CreateInstance<FixedCostTemplate>();

        rentTemplate.Description = "Rent";
        rentTemplate.FixedCostType = FixedCostEnum.Rent;
        rentTemplate.Amount = 1000;
        rentTemplate.Frequency = 1;

        var rent = new FixedCostInstance(rentTemplate,1);
        company1.FixedCosts.Add(rent);

        var salaryTemplate = ScriptableObject.CreateInstance<FixedCostTemplate>();
        
        salaryTemplate.Description = "Salaries";
        salaryTemplate.FixedCostType = FixedCostEnum.Salaries;
        salaryTemplate.Amount = 5000;
        salaryTemplate.Frequency = 1;
        
        var salary = new FixedCostInstance(salaryTemplate,1);
        company1.FixedCosts.Add(salary);
        var expected = 6000m;
        // Act
        var actual = company1.CalculateFixedCostsForPeriod(2);
        // Assert
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void WhenCalculatingForPeriod2FixedCostsShouldIgnoreCostsWithFrequency2()
    {
        // Arrange
        var testFixedCostStrategy =  new BasicFixedCostStrategy();
        var company1 = EconAgentBuilder.For<EconAgent>()
                            .Named("Company 1")
                            .AtLevel(AgentLevelEnum.Beginner)
                            .WithFixedCostStrategy(testFixedCostStrategy)
                            .Build();
        var rentTemplate = ScriptableObject.CreateInstance<FixedCostTemplate>();

        rentTemplate.Description = "Rent";
        rentTemplate.FixedCostType = FixedCostEnum.Rent;
        rentTemplate.Amount = 1000;
        rentTemplate.Frequency = 2;
        
        var rent = new FixedCostInstance(rentTemplate,1);
        company1.FixedCosts.Add(rent);

        var salaryTemplate = ScriptableObject.CreateInstance<FixedCostTemplate>();

        salaryTemplate.Description = "Salaries";
        salaryTemplate.FixedCostType = FixedCostEnum.Salaries;
        salaryTemplate.Amount = 5000;
        salaryTemplate.Frequency = 1;

        var salaries = new FixedCostInstance(salaryTemplate,1);
        company1.FixedCosts.Add(salaries);
        var expected = 5000m;
        // Act
        var actual = company1.CalculateFixedCostsForPeriod(2);
        // Assert
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void WhenCalculatingForPeriod2FixedCostsShouldIgnoreCostsAcquiredInPeriod2()
    {
        // Arrange
        var testFixedCostStrategy = new BasicFixedCostStrategy();
        var company1 = EconAgentBuilder.For<EconAgent>()
                        .Named("Company 1")
                        .AtLevel(AgentLevelEnum.Beginner)
                        .WithFixedCostStrategy(testFixedCostStrategy)
                        .Build();

        var rentTemplate = ScriptableObject.CreateInstance<FixedCostTemplate>();
        rentTemplate.Description = "Rent";
        rentTemplate.FixedCostType = FixedCostEnum.Rent;
        rentTemplate.Amount = 1000;
        rentTemplate.Frequency = 1;

        var rent = new FixedCostInstance(rentTemplate,2);
        company1.FixedCosts.Add(rent);

        var salaryTemplate = ScriptableObject.CreateInstance<FixedCostTemplate>();
    
        salaryTemplate.Description = "Salaries";
        salaryTemplate.FixedCostType = FixedCostEnum.Salaries;
        salaryTemplate.Amount = 5000;
        salaryTemplate.Frequency = 1;
        
        var salaries = new FixedCostInstance(salaryTemplate,1);
    
        company1.FixedCosts.Add(salaries);
        var expected = 5000m;
        // Act
        var actual = company1.CalculateFixedCostsForPeriod(2);
        // Assert
        Assert.AreEqual(expected, actual);
    }
    [Test]
    public void FixedCostsWithInvalidFrequencyShouldThrowException()
    {
        // Arrange
        var company1 = EconAgent.Factory.Create("Company 1", AgentLevelEnum.Beginner,null, new BasicFixedCostStrategy());
        var rentTemplate = ScriptableObject.CreateInstance<FixedCostTemplate>();
        rentTemplate.Description = "Rent";
        rentTemplate.FixedCostType = FixedCostEnum.Rent;
        rentTemplate.Amount = 1000;
        rentTemplate.Frequency = 0;
        var rent = new FixedCostInstance(rentTemplate,1);
        company1.FixedCosts.Add(rent);
        // Act and Assert
        Assert.Throws<System.ArgumentException>(() => company1.CalculateFixedCostsForPeriod(2));
    }
    [Test]
    public void IfMultipleFixedCostsExistAcquiredInMultiplePeriodsOnlySumValidCosts()
    {
        //arrange
       var testFixedCostStrategy = new BasicFixedCostStrategy();
       var company1 = EconAgentBuilder.For<EconAgent>()
                        .Named("Company 1")
                        .AtLevel(AgentLevelEnum.Beginner)
                        .WithFixedCostStrategy(testFixedCostStrategy)
                        .Build();
        
        var rentTemplate = ScriptableObject.CreateInstance<FixedCostTemplate>();
        rentTemplate.Description = "Rent";
        rentTemplate.FixedCostType = FixedCostEnum.Rent;
        rentTemplate.Amount = 1000;
        rentTemplate.Frequency = 1;

        var rent = new FixedCostInstance(rentTemplate,1);

        company1.FixedCosts.Add(rent);
        var salaryTemplate = ScriptableObject.CreateInstance<FixedCostTemplate>();
        salaryTemplate.Description = "Salaries";
        salaryTemplate.FixedCostType = FixedCostEnum.Salaries;
        salaryTemplate.Amount = 5000;
        salaryTemplate.Frequency = 2;
        
        var salary = new FixedCostInstance(salaryTemplate,2);
        company1.FixedCosts.Add(salary);

        var roughNeighbourhoodTemplate = ScriptableObject.CreateInstance<FixedCostTemplate>();
        
        roughNeighbourhoodTemplate.Description = "Rough Neighbourhood";
        roughNeighbourhoodTemplate.FixedCostType = FixedCostEnum.Other;
        roughNeighbourhoodTemplate.Amount = 10000;
        roughNeighbourhoodTemplate.Frequency = 1;

        var roughNeighbourhood = new FixedCostInstance(roughNeighbourhoodTemplate,3);
        company1.FixedCosts.Add(roughNeighbourhood);

        var InsuranceTemplate = ScriptableObject.CreateInstance<FixedCostTemplate>();
        InsuranceTemplate.Description = "Insurance";
        InsuranceTemplate.FixedCostType = FixedCostEnum.Insurance;
        InsuranceTemplate.Amount = 5000;
        InsuranceTemplate.Frequency = 1;

        var Insurance = new FixedCostInstance(InsuranceTemplate,4);
        company1.FixedCosts.Add(Insurance);

        var UtilitiesTemplate = ScriptableObject.CreateInstance<FixedCostTemplate>();
        
        UtilitiesTemplate.Description = "Utilities";
        UtilitiesTemplate.FixedCostType = FixedCostEnum.Utilities;
        UtilitiesTemplate.Amount = 5000;
        UtilitiesTemplate.Frequency = 4;

        var Utilities = new FixedCostInstance(UtilitiesTemplate,1);
        company1.FixedCosts.Add(Utilities);

        var period = 4;
        var expected = 16000m;
        //act
        var actual = company1.CalculateFixedCostsForPeriod(period);
        //assert
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void FixedCostsForFreq2AcquiredOnP1IsNotIncludedInP4()
    {
        //arrange
       var company1 = EconAgent.Factory.Create("Company 1", AgentLevelEnum.Beginner,null, new BasicFixedCostStrategy());
        var rentTemplate = ScriptableObject.CreateInstance<FixedCostTemplate>();
        rentTemplate.Description = "Rent";
        rentTemplate.FixedCostType = FixedCostEnum.Rent;
        rentTemplate.Amount = 1000;
        rentTemplate.Frequency = 2;

        var rent = new FixedCostInstance(rentTemplate,1);
        company1.FixedCosts.Add(rent);
        var period = 4;
        var expected = 0m;
        //act
        var actual = company1.CalculateFixedCostsForPeriod(period);
        //assert
        Assert.AreEqual(expected, actual); 
    }
    [Test]
    public void IfFixedCostsAreEmptyReturnZero()
    {
        //arrange
        var company1 = EconAgent.Factory.Create("Company 1", AgentLevelEnum.Beginner,null, new BasicFixedCostStrategy());
        var period = 4;
        var expected = 0m;
        //act
        var actual = company1.CalculateFixedCostsForPeriod(period);
        //assert
        Assert.AreEqual(expected, actual);
    }
}