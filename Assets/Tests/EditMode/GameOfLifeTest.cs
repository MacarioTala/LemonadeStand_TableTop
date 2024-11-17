using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class GameOfLifeTest
{
    private TheEconomy LemonadeEconomy;
    private int TotalNumberOfCycles;

    private Market LemonadeMarket;

    private readonly List<Company> Companies = new();

    private Good Lemon;
    private Good Water;
    private Good Sugar;
    private Good Lemonade;

    private readonly List<Good> TestGoods= new();

    [SetUp]
    public void SetUp()
    {
        // Create the economy
        var economy_object = new GameObject("LemonadeEconomy");
        LemonadeEconomy = economy_object.AddComponent<TheEconomy>();
        LemonadeEconomy.Initialize(new MockLogger());

        // Fill it with goods
        MakeGoods();

        // get the first market ready
        InitializeFirstMarket();

        // create companies and initialize them with cash and inventories
        CreateTestCompanies();

        TotalNumberOfCycles = 10;

    }

    private void MakeGoods()
    {
        Lemon = Good.CreateInstance("Lemon", new Price_band(1f, 3f), Rarity_enum.Common);
        Water = Good.CreateInstance("Water", new Price_band(1f, 1f), Rarity_enum.Common);
        Sugar = Good.CreateInstance("Sugar", new Price_band(1f, 2f), Rarity_enum.Common);
        Lemonade = Good.CreateInstance("Lemonade", new Price_band(4f, 5f), Rarity_enum.Uncommon);
        TestGoods.Add(Lemon);
        TestGoods.Add(Water);
        TestGoods.Add(Sugar);
        TestGoods.Add(Lemonade);
    }

    private void CreateTestCompanies()
    {
        //make a variable number of companies and give them varying amounts of goods and cash
        var random = new System.Random ();
        var numberOfCompanies = random.Next(1, 10);
        for (int i = 0; i < numberOfCompanies; i++)
        {
            var company = ScriptableObject.CreateInstance<Company>();
            company.Initialize("Company" + i, CompanyLevelEnum.Beginner);
            TheEconomy.Instance.Register_Company(company);
            Companies.Add(company);
        }
        foreach (var company in Companies)
        {

        }
            
    }

    private void InitializeFirstMarket()
    {
        //Get a reference to the company's market
        //Give it some goods
        //Let it demand Lemonade, Lemons, etc
        LemonadeMarket = (Market)LemonadeEconomy.GetGlobalMarket();
        LemonadeMarket.InitializeDemand(Lemonade, 1000);
        LemonadeMarket.InitializeDemand(Lemon, 1000);
        LemonadeMarket.InitializeDemand(Water, 1000);
        LemonadeMarket.InitializeDemand(Sugar, 1000);
        LemonadeMarket.BuyGood(Lemon, 10000, .5f);
        LemonadeMarket.BuyGood(Water, 10000, .5f);
        LemonadeMarket.BuyGood(Sugar, 10000, .5f);
    }

}
