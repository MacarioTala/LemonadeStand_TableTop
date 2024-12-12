 using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class FillsAndPartialFillsTests
{
    TheEconomy testEconomy;
    [SetUp]
    public void Setup()
    {
        var economyObject = new GameObject();
        testEconomy = economyObject.AddComponent<TheEconomy>();
        testEconomy.Initialize(new MockLogger());
    }

    [Test]
    public void TestThatMarketPartiallyFillsOrderIfBuyingCompanyDoesntWantEntireQuantity()
    { 
        throw new System.NotImplementedException(); 
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(testEconomy.gameObject);
    }
}
