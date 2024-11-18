using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class GameOfLifeTest
{
    private TheEconomy LemonadeEconomy;
    private int TotalNumberOfCycles;
    private int TradesPerCycle;

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
        TradesPerCycle = 10;

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
            foreach (var good in TestGoods)
            {
                company.BuyGood(good, random.Next(0, 1000), good.GetPrice());
            }
        }
            
    }

    private void InitializeFirstMarket()
    {
        //Get a reference to the company's market
        //Give it some goods
        //Let it demand Lemonade, Lemons, etc
        LemonadeMarket = (Market)LemonadeEconomy.GetGlobalMarket();
       
       foreach (var good in TestGoods)
        {
            LemonadeMarket.InitializeDemand(good, Random.Range(100, 1000));
            LemonadeMarket.BuyGood(good,10000,.5f);
        }
    }
    [Test]
    public void SimulateRandom()
    {
        for (int cycle = 0; cycle < TotalNumberOfCycles; cycle++)
        {
            Debug.Log("Starting cycle:" + cycle);
            for (int i = 0; i < TradesPerCycle; i++) 
            {
                PerformRandomAction(cycle);
            }
            try{
                TheEconomy.Instance.ExecuteDailyTrades();
                }
            catch (System.Exception e)
            {
                Debug.Log("Error in cycle:" + cycle + " " + e.Message);
            }

            Debug.Log("Cycle:" + cycle + " completed");
        }

       //GenerateSummary();
    }

    private void PerformRandomAction(int cycle)
    {
        var potentialSellers = new List<iCompany>(Companies){LemonadeMarket};
        var potentialBuyers = new List<iCompany>(Companies){LemonadeMarket};

        var buyer = potentialBuyers[Random.Range(0, potentialBuyers.Count)];
        var seller = potentialSellers[Random.Range(0, potentialSellers.Count)];

        if(buyer==seller) return;
        
        var goodToBuy = SelectRandomGood();
        if (goodToBuy == null) return;
        
        var quantity = Random.Range(1, 10);
        var price = goodToBuy.GetPrice();
        var trade = new Trade(buyer, seller, goodToBuy, quantity, price);
        TheEconomy.Instance.Queue_Trade(trade);
        Debug.Log("Trade queued in cycle: "+cycle+ " " + trade);
    }

    private Good SelectRandomGood()
    {
        return TestGoods[Random.Range(0, TestGoods.Count)];
    }
    
}
