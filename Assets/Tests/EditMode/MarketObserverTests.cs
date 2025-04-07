using System.Collections.Generic;
using NUnit.Framework;
/// <summary>
/// This class includes tests for FullfilmentInfoExtensions
/// </summary>
[TestFixture]
public class MarketObserverTests
{
    Good Lemon;
    Good Lemonade;
    Good Water;
    Good Sugar;
    
    [SetUp]
    public void Setup()
    {
        Lemon = Good.CreateInstance("Lemon", new PriceBand(0.5m, 1.0m), RarityEnum.Common);
        Lemonade = Good.CreateInstance("Lemonade", new PriceBand(5.0m, 10m), RarityEnum.Uncommon);
        Water = Good.CreateInstance("Water", new PriceBand(0.1m, 0.5m), RarityEnum.Common);
        Sugar = Good.CreateInstance("Sugar", new PriceBand(0.2m, 0.8m), RarityEnum.Common);
    }

    [TestCase(TestName = "CalculateFulfillmentRates returns empty list when both demand and supply are empty")]
    public void CFR_EmptyDemandAndSupply_ReturnsEmptyList()
    {
        // Arrange
        var marketDemand = new Dictionary<Good, DemandData>();
        var marketSupply = new Dictionary<Good, int>();
        var expectedFulfillmentInfoList = new List<FulfillmentInfo>();
        // Act
        var result = MarketObserver.CalculateFulfillmentRates(marketDemand, marketSupply);
        // Assert   
        Assert.AreEqual(expectedFulfillmentInfoList.Count, result.Count);
    }

    [TestCase(TestName = "CalculateFulfillmentRates returns 1 when demand and supply are equal")]
    public void CFR_EqualDemandAndSupply_ReturnsFulfillmentInfoWith1()
    {
        // Arrange
        var marketDemand = new Dictionary<Good, DemandData>
        {
            { Lemon, new DemandData { CurrentDemand = 10 } },
            { Lemonade, new DemandData { CurrentDemand = 5 } }
        };
        var marketSupply = new Dictionary<Good, int>
        {
            { Lemon, 10 },
            { Lemonade, 5 }
        };
        var expectedLemonFulfillmentRate = 100f;
        var expectedLemonadeFulfillmentRate = 100f;
        // Act
        var result = MarketObserver.CalculateFulfillmentRates(marketDemand, marketSupply);
        var lemonFulfillmentRate = result.Find(x => x.Good == Lemon).FulfillmentRate;
        var lemonadeFulfillmentRate = result.Find(x => x.Good == Lemonade).FulfillmentRate;
        // Assert   
        Assert.AreEqual(expectedLemonFulfillmentRate, lemonFulfillmentRate);
        Assert.AreEqual(expectedLemonadeFulfillmentRate, lemonadeFulfillmentRate);
    }

    [TestCase(TestName = "CalculateFulfillmentRates returns 0 when demand is 0 and supply is greater than 0")]
    public void CFR_ZeroDemandAndPositiveSupply_ReturnsFulfillmentInfoWith0()
    {
        // Arrange
        var marketDemand = new Dictionary<Good, DemandData>
        {
            { Lemon, new DemandData { CurrentDemand = 0 } }
        };
        var marketSupply = new Dictionary<Good, int>
        {
            { Lemon, 10 }
        };
        var expectedLemonFulfillmentRate = 0.0f;
        // Act
        var result = MarketObserver.CalculateFulfillmentRates(marketDemand, marketSupply);
        var lemonFulfillmentRate = result.Find(x => x.Good == Lemon).FulfillmentRate;
        
        // Assert   
        Assert.AreEqual(expectedLemonFulfillmentRate, lemonFulfillmentRate);
    }

    [TestCase(TestName = "CalculateFulfillmentRates returns 0 when demand is greater than 0 and supply is 0")]
    public void CFR_PositiveDemandAndZeroSupply_ReturnsFulfillmentInfoWith0()
    {
        // Arrange
        var marketDemand = new Dictionary<Good, DemandData>
        {
            { Lemon, new DemandData { CurrentDemand = 10 } }
        };
        var marketSupply = new Dictionary<Good, int>
        {
            { Lemon, 0 }
        };
        var expectedLemonFulfillmentRate = 0.0f;
        // Act
        var result = MarketObserver.CalculateFulfillmentRates(marketDemand, marketSupply);
        var lemonFulfillmentRate = result.Find(x => x.Good == Lemon).FulfillmentRate;
        
        // Assert   
        Assert.AreEqual(expectedLemonFulfillmentRate, lemonFulfillmentRate);
    }
    [TestCase(TestName = "CalculateFulfillmentRates returns 50 when demand is 2 and supply is 1")]
    public void CFR_PositiveDemandAndSupply_ReturnsFulfillmentInfoWith50()
    {
        // Arrange
        var marketDemand = new Dictionary<Good, DemandData>
        {
            { Lemon, new DemandData { CurrentDemand = 2 } }
        };
        var marketSupply = new Dictionary<Good, int>
        {
            { Lemon, 1 }
        };
        var expectedLemonFulfillmentRate = 50f;
        // Act
        var result = MarketObserver.CalculateFulfillmentRates(marketDemand, marketSupply);
        var lemonFulfillmentRate = result.Find(x => x.Good == Lemon).FulfillmentRate;
        
        // Assert   
        Assert.AreEqual(expectedLemonFulfillmentRate, lemonFulfillmentRate);
    }
    [TestCase(TestName = "CalculateFulfillmentRates returns 0 when demand is 0 and supply is 0")]
    public void CFR_ZeroDemandAndSupply_ReturnsFulfillmentInfoWith0()
    {
        // Arrange
        var marketDemand = new Dictionary<Good, DemandData>
        {
            { Lemon, new DemandData { CurrentDemand = 0 } }
        };
        var marketSupply = new Dictionary<Good, int>
        {
            { Lemon, 0 }
        };
        var expectedLemonFulfillmentRate = 0.0f;
        // Act
        var result = MarketObserver.CalculateFulfillmentRates(marketDemand, marketSupply);
        var lemonFulfillmentRate = result.Find(x => x.Good == Lemon).FulfillmentRate;
        
        // Assert   
        Assert.AreEqual(expectedLemonFulfillmentRate, lemonFulfillmentRate);
    }
    [TestCase(TestName = "SupplyShortageRate returns 50 when demand is 2 and supply is 1(standalone)")]
    public void SupplyShortageRate_PositiveDemandAndSupply_Returns50()
    {
        // Arrange
        var fulfillmentInfo = new FulfillmentInfo(Lemon, 2, 1);
        var expectedSupplyShortageRate = 50f;
        // Act
        var result = fulfillmentInfo.SupplyShortageRate;
        // Assert   
        Assert.AreEqual(expectedSupplyShortageRate, result);
    }
    [TestCase(TestName = "SupplyExcessRate returns 100 when demand is 1 and supply is 2(standalone)")]
    public void SupplyExcessRate_Demand1Supply2_returns100()
    {
        // Arrange
        var fulfillmentInfo = new FulfillmentInfo(good: Lemon, demand: 1,supply: 2);
        var expectedSupplyExcessRate = 100f;
        // Act
        var result = fulfillmentInfo.SupplyExcessRate;
        // Assert   
        Assert.AreEqual(expectedSupplyExcessRate, result);
    }
    [TestCase(TestName = "CalculateFulfillmentRates returns supplyshortage rate of 50 when demand is 2 and supply is 1")]
    public void CFR_SupplyShortageRate_PositiveDemandAndSupply_Returns50()
    {
        // Arrange
        var marketDemand = new Dictionary<Good, DemandData>
        {
            { Lemon, new DemandData { CurrentDemand = 2 } }
        };
        var marketSupply = new Dictionary<Good, int>
        {
            { Lemon, 1 }
        };
        var expectedLemonFulfillmentRate = 50f;
        // Act
        var result = MarketObserver.CalculateFulfillmentRates(marketDemand, marketSupply);
        var lemonFulfillmentRate = result.Find(x => x.Good == Lemon).SupplyShortageRate;
        
        // Assert   
        Assert.AreEqual(expectedLemonFulfillmentRate, lemonFulfillmentRate);
    }
    [TestCase(TestName = "CalculateFulfillmentRates returns SupplyExcessRate of 100 when demand is 1 and supply is 2")]
    public void CFR_SupplyExcessRate_Demand1Supply3_Returns100()
    {
        // Arrange
        var marketDemand = new Dictionary<Good, DemandData>
        {
            { Lemon, new DemandData { CurrentDemand = 1 } }
        };
        var marketSupply = new Dictionary<Good, int>
        {
            { Lemon, 2 }
        };
        var expectedLemonSupplyExcessRate = 100f;
        // Act
        var result = MarketObserver.CalculateFulfillmentRates(marketDemand, marketSupply);
        var actualLemonExcessRate = result.Find(x => x.Good == Lemon).SupplyExcessRate;
        
        // Assert   
        Assert.AreEqual(expectedLemonSupplyExcessRate, actualLemonExcessRate);
    }
#region FulfillmentInfoExtension tests
    [TestCase(TestName = "FulfillmentInfoExtension returns 0 when demand is 0 and supply is greater than 0")]
    public void FIE_ZeroDemandAndPositiveSupply_ReturnsFulfillmentInfoWith0()
    {
        // Arrange
        var fulfillmentInfo = new FulfillmentInfo(Lemon, 0, 10);
        var expectedFulfillmentRate = 0.0f;
        // Act
        var result = fulfillmentInfo.FulfillmentRate;
        // Assert   
        Assert.AreEqual(expectedFulfillmentRate, result);
    }
    
    
#endregion
}