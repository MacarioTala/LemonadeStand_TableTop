using NUnit.Framework;
using UnityEditor.Graphs;

[TestFixture]
public class DemographicPropertyBagTests
{
    [Test]
    public void AsPercentageReturnsNoMantissa()
    {
        //Arrange
        var decimalValue = 98.7m;
        const float expected = 99f;
        var name = "propertyX";
        var bag = new DemographicPropertyBag();
        bag.AddProperty(DemographicProperty.Percentage(name, decimalValue));

        //Act
        var actual = bag.GetProperty(name).AsPercentage();

        //Assert
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void AsMoneyReturnsTwoDecimalPoints()
    {
        //Arrange
        var decimalValue = 98.77777777777m;
        const decimal expected = 98.78m;
        var name = "propertyX";
        var bag = new DemographicPropertyBag();
        bag.AddProperty(DemographicProperty.Money(name, decimalValue));

        //Act
        var actual = bag.GetProperty(name).AsMoney();

        //Assert
        Assert.AreEqual(expected, actual);
    }
}