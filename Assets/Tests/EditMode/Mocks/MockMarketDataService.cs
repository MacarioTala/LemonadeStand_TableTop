using System;
using System.Collections.Generic;
using System.Linq;

public class MockMarketDataService : iMarketDataService
{
    IEnumerable<PopulationHistory> PopulationHistory;
    private MarketFeatureSpriteDatabase spriteDB;
    

    public void SetPopulationHistory(IEnumerable<PopulationHistory> populationHistory)
    {
        PopulationHistory = populationHistory;
    }
    
    public List<PopulationHistory> GetPopulationHistory(Guid marketId)
    {
        return PopulationHistory.Where(x => x.MarketId == marketId).ToList();
    }

    public List<MarketFeature> GetMarketFeatures(Guid marketId)
    {
        // Returns a mocked list of MarketFeatures
        // These are currently designed around having a minimum
        // set of Market Features for Episode 1
        // Move this to an actual mocking framework in the future
        var featuresToReturn = new List<MarketFeature>();
        
        var CityHall = new MarketFeature
        (
            name : FeatureNameEnum.CityHall,
            size :(3, 3),
            priority:PriorityEnum.Mandatory,
            canBuildOnFeature:false,
            spriteDatabase: spriteDB
        );

        var school = new MarketFeature
        (
            name        : FeatureNameEnum.School,
            size        : (2, 4),
            priority    : PriorityEnum.Mandatory,
            canBuildOnFeature:false,
            spriteDatabase: spriteDB
        );

        var park = new MarketFeature
        (
            name        : FeatureNameEnum.Park,
            size        : (2, 10),
            priority    : PriorityEnum.Mandatory,
            canBuildOnFeature:false,
            spriteDatabase: spriteDB
        );

        var grocer = new MarketFeature
        (
            name        : FeatureNameEnum.Grocer,
            size        : (2, 4),
            priority    : PriorityEnum.Mandatory,
            canBuildOnFeature:false,
            spriteDatabase: spriteDB
        );

        var railroadFutureSite = new MarketFeature
        (
            name        : FeatureNameEnum.RailRoadFutureSite,
            size        : (3 , 5),
            priority    : PriorityEnum.Mandatory,
            canBuildOnFeature:false,
            spriteDatabase: spriteDB
        );
    
        var playerCharacterSpawnPoint = new MarketFeature
        (
            name        : FeatureNameEnum.LemonadeStand,
            size        : (2, 2),
            priority    : PriorityEnum.Mandatory,
            canBuildOnFeature:false,
            spriteDatabase: spriteDB
        );

        featuresToReturn.Add(CityHall);
        featuresToReturn.Add(school);
        featuresToReturn.Add(park);
        featuresToReturn.Add(grocer);
        featuresToReturn.Add(railroadFutureSite);
        featuresToReturn.Add(playerCharacterSpawnPoint);
        return featuresToReturn;
    }
}
