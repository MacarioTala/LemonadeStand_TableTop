using System.Linq;
using NUnit.Framework;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEngine;

[TestFixture]
public class LinearDemandStrategyTests
{
    TheEconomy TestEconomy;
    Good Lemonade;
    
    [SetUp]
    public void SetUp()
    {
        Lemonade = Good.CreateInstance("Lemonade", new Price_band(.5m, 2m), Rarity_enum.Uncommon);

        var EconomyObject = new GameObject();
        TestEconomy = EconomyObject.AddComponent<TheEconomy>();
    }
    [Test]
    public void InitializeDemandForSpecificGoodReplacesDemandForExistingGood()
    {
        //Arrange
        var marketToTest = Market.Factory.CreateStarterMarket("Starter Market",
                                                              CompanyLevelEnum.Market,
                                                              new LinearDemandStrategy() );
        var expected = 500;
        var expectedDemandLinesForLemonade = 1;
        //Act
        marketToTest.InitializeDemandForSpecificGood(Lemonade, expected);
        var actual = marketToTest.GetMarketDemand()[Lemonade].CurrentDemand;
        var demandLinesForLemonade = marketToTest.GetMarketDemand().Where(x=>x.Key.Equals(Lemonade)).Count();
        //Assert
        Assert.AreEqual(expected, actual);
        Assert.AreEqual(expectedDemandLinesForLemonade, demandLinesForLemonade);
    }
    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(TestEconomy.gameObject);
    }
}