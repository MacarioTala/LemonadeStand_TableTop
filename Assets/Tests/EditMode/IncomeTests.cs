using NUnit.Framework;
using UnityEditor.VersionControl;

[TestFixture]
public class IncomeTests
{
    [Test]
    [TestCase(0,4,100,TestName = "For an income of 100 and  Frequency 4 in Period 0, income should be paid")]
    [TestCase(1,4,0,TestName = "For an income of 100 and Frequency 4 in Period 1, income should not be paid")]
    [TestCase(3,4,0,TestName = "For an income of 100 and Frequency 4 in Period 3, income should not be paid")]
    [TestCase(4,4,100,TestName = "For an income of 100 and Frequency 4 in Period 4, income should be paid")]
    [TestCase(8,4,100,TestName = "For an income of 100 and Frequency 4 in Period 8, income should be paid")]

    [TestCase(0, 1, 100, TestName = "Frequency 1 pays at period 0")]
    [TestCase(1, 1, 100, TestName = "Frequency 1 pays every period (1)")]
    [TestCase(2, 1, 100, TestName = "Frequency 1 pays every period (2)")]

    [TestCase(0, 2, 100, TestName = "Frequency 2 pays at period 0")]
    [TestCase(1, 2, 0,   TestName = "Frequency 2 does not pay at period 1")]
    [TestCase(2, 2, 100, TestName = "Frequency 2 pays at period 2")]
    [TestCase(3, 2, 0,   TestName = "Frequency 2 does not pay at period 3")]

    [TestCase(0, 3, 100, TestName = "Freq 3 → period 0 pays (start)")]
    [TestCase(1, 3, 0,   TestName = "Freq 3 → period 1 does not pay (odd)")]
    [TestCase(2, 3, 0,   TestName = "Freq 3 → period 2 does not pay (even)")]
    [TestCase(3, 3, 100, TestName = "Freq 3 → period 3 pays (next cycle)")]
    
    public void IncomeIsPaidOnCorrectFrequency( int period,
                                                int frequency,
                                                int expected)
    {
        //Arrange
        var testIncome = new Income(){IncomeInCents=100,FrequencyInPeriods=frequency};
        var population = EconAgentBuilder.For<PopulationAgent>()
                .Named("TestPopForIncome")
                .WithIncome(testIncome)
                .Build();
        
        //Act
        var actual = population.GetIncomeInCentsForPeriod(period);

        //Assert
        Asset.Equals(expected,actual);
    }
}