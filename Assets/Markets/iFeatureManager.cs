using System;
using System.Collections.Generic;
using System.Linq;

public interface iFeatureManager
{
    public LemonadeStandResultObject AddFeature(MarketFeature feature, Market market);
    public LemonadeStandResultObject RemoveFeature(MarketFeature feature, Market market);
    public static List<MarketFeature> GetFeatures(Market market)
    {
        return market.MarketFeatures;   
    }

    public static int GetFreeSpace(Market market)
    {
        var features = GetFeatures(market);
        var totalSpace = market.GetWidth() * market.GetHeight();
        var usedSpace = 0;

        foreach(var feature in features)
        {
            usedSpace += feature.GetSpace();
        }

        return totalSpace - usedSpace;
    }
    
    private int GetMandatoryFeatureSpace(List<MarketFeature> features)
    {
        var mandatoryFeatures = features.Where(f => f.Priority == PriorityEnum.Mandatory);
        var mandatorySpace = 0;

        foreach(var feature in mandatoryFeatures)
        {
            mandatorySpace += feature.GetSpace();
        }

        return mandatorySpace;
    }

    private int GetStreetTilesRequired(List<MarketFeature> features)
    {
        throw new NotImplementedException();
    }

    public LemonadeStandResultObject PlaceInitialFeatures(Market market)
    {
        var features = market.GetMarketFeatures();
        var mandatorySpace = GetMandatoryFeatureSpace(features);
        var streetTilesRequired = GetStreetTilesRequired(features);

        var marketWidth = market.GetWidth();
        var marketHeight = market.GetHeight();

        var totalSpace = marketWidth * marketHeight;

        throw new NotImplementedException();
    }

    
}