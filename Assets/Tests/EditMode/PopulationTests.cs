using System.Linq;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class PopulationTests
{
    Good lemon;
    [SetUp]
    public void SetUp()
    {
        lemon = ScriptableObject.CreateInstance<Good>();
        lemon.GoodName = "Lemon";
    }

    [Test]
    public void PopulationCompany_Consume_RemovesItemsFromInventory()
    {
        // Arrange
        var inventory = new Inventory();
        const int initialLemonCount = 10;
        inventory.AddGood(new InventoryEntry(lemon, initialLemonCount, 1m,0));

        var population = CompanyBuilder.For<PopulationCompany>()
            .Named("Test Company")
            .AtLevel(CompanyLevelEnum.Beginner)
            .WithPopulation(100)
            .WithInventory(inventory)
            .WithEnnui(.99f)
            .Build();
        
        const int expectedLemonCount = 9;

        // Act
        population.Consume();
        var inventoryEntries = population.GetInventory().GetInventoryEntries();
        var lemonEntry = inventoryEntries.FirstOrDefault(entry => entry.good == lemon);
        var actualLemonCount = lemonEntry?.quantity ?? 0;

        // Assert
        Assert.AreEqual(expectedLemonCount, actualLemonCount);
    }

    [Test]
    public void PopulationCompany_ConsumeScalesWithPopulation()
    {
        // Arrange
        var inventory1 = new Inventory();
        const int initialLemonCount = 10;
        inventory1.AddGood(new InventoryEntry(lemon, initialLemonCount, 1m,0));

        var inventory2 = new Inventory();
        inventory2.AddGood(new InventoryEntry(lemon, initialLemonCount, 1m,0));

        var population1 = CompanyBuilder.For<PopulationCompany>()
            .Named("Test Company")
            .AtLevel(CompanyLevelEnum.Beginner)
            .WithPopulation(100)
            .WithInventory(inventory1)
            .WithEnnui(.99f)
            .Build();
        
        var population2 = CompanyBuilder.For<PopulationCompany>()
            .Named("Test Company")
            .AtLevel(CompanyLevelEnum.Beginner)
            .WithPopulation(200)
            .WithInventory(inventory2)
            .WithEnnui(.99f)
            .Build();

        const int expectedPopulation1LemonCount = 9;
        const int expectedPopulation2LemonCount = 8;

        // Act
        population1.Consume();
        population2.Consume();

        var inventoryEntries1 = population1.GetInventory().GetInventoryEntries();
        var lemonEntryFor1 = inventoryEntries1.FirstOrDefault(entry => entry.good == lemon);
        var actualLemonCount1 = lemonEntryFor1?.quantity ?? 0;

        var inventoryEntries2 = population2.GetInventory().GetInventoryEntries();
        var lemonEntryFor2 = inventoryEntries2.FirstOrDefault(entry => entry.good == lemon);
        var actualLemonCount2 = lemonEntryFor2?.quantity ?? 0;

        // Assert
        Assert.AreEqual(expectedPopulation1LemonCount, actualLemonCount1);
        Assert.AreEqual(expectedPopulation2LemonCount, actualLemonCount2);
    }


}