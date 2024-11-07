using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class The_MarketTests
{
    private The_Market test_market;
    [SetUp]
    public void SetUp()
    {
        var market_object = new GameObject();
        test_market = market_object.AddComponent<The_Market>();
    }

    [Test]
    public void Register_Company_adds_company_to_companies_list_if_no_companies_are_registered()
    {
        // Arrange
        var company = ScriptableObject.CreateInstance<Company>();
        company.company_name = "Test Company";
        var expected = 1;
        // Act
        test_market.Register_Company(company);
        var actual = test_market.companies.Count;
        
        // Assert
        Assert.AreEqual(expected, test_market.companies.Count);
    }
    [Test]
    public void Register_Company_does_not_add_company_to_companies_list_if_company_already_registered()
    {
        // Arrange
        var company = ScriptableObject.CreateInstance<Company>();
        company.company_name = "Test Company";
        test_market.Register_Company(company);
        var company2 = ScriptableObject.CreateInstance<Company>();
        company2.company_name = "Test Company";
        // Act
        // Assert
        Assert.Throws<CompanyException>(() => test_market.Register_Company(company2));
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(test_market.gameObject);
    }
}
