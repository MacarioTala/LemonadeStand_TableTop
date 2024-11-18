
using NUnit.Framework;

[TestFixture]
public class GoodTests
{
    readonly Price_band price_band1 = new(.5m, 1.0m);

    [SetUp]
    public void SetUp()
    {
        
    }

    [Test]
    public void Creating_good_generates_price_based_on_price_band()
    {
        // Arrange
        var good_name = "lemon";
        var uncommon = Rarity_enum.Uncommon;
        var expected_price_min = 0.5m;
        var expected_price_max = 1.0m;
        var expected_rarity = Rarity_enum.Uncommon;

        // Act
        var lemon = Good.CreateInstance(good_name, price_band1,uncommon);
        // Assert
        Assert.AreEqual(good_name, lemon.good_name);
        Assert.AreEqual(expected_rarity, lemon.Get_rarity());
        Assert.IsTrue(lemon.GetPrice() >= expected_price_min && lemon.GetPrice() <= expected_price_max);
    }
    
    [Test]
    public void Creating_good_generates_price_thresholds_based_on_rarity()
    {
        // Arrange
        var good_name = "lemon";
        var uncommon = Rarity_enum.Uncommon;
        var expected_rarity = Rarity_enum.Uncommon;
        var expected_price_increase_threshold = 250;
        var expected_price_decrease_threshold = 50;

        // Act
        var lemon = Good.CreateInstance(good_name, price_band1,uncommon);
        // Assert
        Assert.AreEqual(good_name, lemon.good_name);
        Assert.AreEqual(expected_price_increase_threshold, lemon.price_increase_threshold);
        Assert.AreEqual(expected_price_decrease_threshold, lemon.price_decrease_threshold);
        Assert.AreEqual(expected_rarity, lemon.Get_rarity());
    }

    [Test]
    public void Creating_good_generates_price_increment_rates_based_on_rarity()
    {
        // Arrange
        var good_name = "lemon";
        var uncommon = Rarity_enum.Uncommon;
        var expected_rarity = Rarity_enum.Uncommon;
        var expected_price_increment_rate = .15f;

        // Act
        var lemon = Good.CreateInstance(good_name, price_band1,uncommon);
        // Assert
        Assert.AreEqual(good_name, lemon.good_name);
        Assert.AreEqual(expected_price_increment_rate, lemon.Get_price_increment_rate());
        Assert.AreEqual(expected_rarity, lemon.Get_rarity());
    }
}