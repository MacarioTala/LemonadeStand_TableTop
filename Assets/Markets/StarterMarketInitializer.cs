using System;
using System.Collections.Generic;

public class StarterMarketInitializer : iMarketInitializer
{
    public void InitializeMarket(Market market)
    {
        if (market == null) throw new ArgumentNullException(nameof(market));
        CreateStarterDemand(market);
        SeedWithInitialGoods(market);
        SetInitialCash(market);
    }
    private void SeedWithInitialGoods (Market market)
    {
        var _inventory = market.GetInventory();
        var Lemonade = Good.CreateInstance("Lemonade", new Price_band(8.0m, 13.0m), Rarity_enum.Uncommon);
        var Lemon = Good.CreateInstance("Lemon", new Price_band(1.0m, 3.0m), Rarity_enum.Common);
        var Sugar = Good.CreateInstance("Sugar", new Price_band(1.0m, 2.0m), Rarity_enum.Common);
        var Water = Good.CreateInstance("Water", new Price_band(.5m, 1.0m), Rarity_enum.Common);
        _inventory.AddGood(new InventoryEntry(Lemon, 10000, 2.0m, 0));
        _inventory.AddGood(new InventoryEntry(Sugar, 10000, 1.5m, 0));
        _inventory.AddGood(new InventoryEntry(Water, 10000, .75m, 0));
        var LemonadeRecipe = new Recipe(RecipeName: "Basic Lemonade",
                                        product: Lemonade,
                                        ingredients: new List<Ingredient> { new(Lemon, 9),
                                                                           new(Sugar, 2),
                                                                           new(Water, 7) });
        market.AddRecipe(LemonadeRecipe);
    }

    private void SetInitialCash(Market market)
    {
        var company_level = market.company_level;
        switch(company_level)
        {
            case CompanyLevelEnum.Beginner:
                market.SetCash(10000);
                break;
            case CompanyLevelEnum.Intermediate:
                market.SetCash(5000);
                break;
            case CompanyLevelEnum.Advanced:
                market.SetCash(1000);
                break;  
            case CompanyLevelEnum.Market:
                market.SetCash(1000000000000);
                break;
        }
    }
    private void CreateStarterDemand (Market market)
    {
        //Initialize demand data
        //If no demand data is passed, demand defaults to 1000 units of Lemonade
        //This is a placeholder and will be replaced with a more sophisticated system
        var lemonade = Good.CreateInstance("Lemonade", new Price_band(8.0m, 13.0m), Rarity_enum.Uncommon);
        lemonade.IsProducedGood = true;
        market.InitializeDemand(lemonade, 1000);
    }
}
