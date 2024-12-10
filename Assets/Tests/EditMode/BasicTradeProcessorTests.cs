using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class BasicTradeProcessorTests
{
    TheEconomy TestEconomy;
    [SetUp]
    public void SetUp()
    {
        var economyObject = new GameObject();
        TestEconomy = economyObject.AddComponent<TheEconomy>();
        
    }
    [Test]
    public void ProcessCompanyOrdersIgnoresOrdersWhereSellerIsMarket()
    {
        throw new System.NotImplementedException();
    }
    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(TestEconomy.gameObject);
    }
}