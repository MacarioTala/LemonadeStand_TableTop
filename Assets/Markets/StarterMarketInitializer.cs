using System;
using UnityEngine;

public class StarterMarketInitializer : iMarketInitializer
{
    public void InitializeMarket(Market market)
    {
        if (market == null) throw new ArgumentNullException(nameof(market));
        SeedWithInitialGoods(market);
        CreateStarterDemand(market);
        SetInitialCash(market);
    }
    private void SeedWithInitialGoods (Market market)
    {
        var _inventory = market.GetInventory();
        var Lemonade = Good.CreateInstance("Lemonade", new PriceBand(8, 13), RarityEnum.Uncommon);
        var Lemon = Good.CreateInstance("Lemon", new PriceBand(1, 3), RarityEnum.Common);
        var Sugar = Good.CreateInstance("Sugar", new PriceBand(1, 2), RarityEnum.Common);
        var Water = Good.CreateInstance("Water", new PriceBand(1, 1), RarityEnum.Common);
        _inventory.AddInventoryEntry(new InventoryEntry(Lemon, 100, 2, 0));
        _inventory.AddInventoryEntry(new InventoryEntry(Sugar, 100, 1, 0));
        _inventory.AddInventoryEntry(new InventoryEntry(Water, 100, 1, 0));
        var LemonadeRecipe = ScriptableObject.CreateInstance<Recipe>();
        LemonadeRecipe.Initialize("Basic Lemonade",
                                  Lemonade,
                                  new(){ new(Lemon, 9),
                                         new(Sugar, 2),
                                         new(Water, 7)
                                  }
        );
        market.AddRecipe(LemonadeRecipe);
    }

    private void SetInitialCash(Market market)
    {
        var company_level = market.company_level;
        switch(company_level)
        {
            case AgentLevelEnum.Beginner:
                market.SetCash(1000);
                break;
            case AgentLevelEnum.Intermediate:
                market.SetCash(500);
                break;
            case AgentLevelEnum.Advanced:
                market.SetCash(100);
                break;  
            case AgentLevelEnum.Market:
                market.SetCash(100000);
                break;
        }
    }
    private void CreateStarterDemand (Market market)
    {
        //Initialize demand data
        //If no demand data is passed, demand defaults to 1000 units of Lemonade
        //This is a placeholder and will be replaced with a more sophisticated system
        var lemonade = Good.CreateInstance("Lemonade", new PriceBand(8, 13), RarityEnum.Uncommon);
        lemonade.IsProducedGood = true;
    }
}
