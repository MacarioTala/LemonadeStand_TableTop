
using NUnit.Framework;

[TestFixture]
public class GoodTests
{
    readonly Price_band price_band1 = new(.5f, 1f);

    [SetUp]
    public void SetUp()
    {
        
    }

    [Test]
    public void Creating_good_generates_price_based_on_price_band()
    {
        // Arrange
        var good_name = "lemon";
        var price_increment_rate = 0.1f;
        var price_increase_threshold = 10;
        var price_decrease_threshold = 5;
        var expected_price_min = 0.5f;
        var expected_price_max = 1f;
        // Act
        var lemon = Good.CreateInstance(good_name, price_increment_rate, price_increase_threshold, price_decrease_threshold, price_band1);
        // Assert
        Assert.AreEqual(good_name, lemon.good_name);
        Assert.AreEqual(price_increment_rate, lemon.price_increment_rate);
        Assert.AreEqual(price_increase_threshold, lemon.price_increase_threshold);
        Assert.AreEqual(price_decrease_threshold, lemon.price_decrease_threshold);
        Assert.IsTrue(lemon.Get_price() >= expected_price_min && lemon.Get_price() <= expected_price_max);
    }
    
}