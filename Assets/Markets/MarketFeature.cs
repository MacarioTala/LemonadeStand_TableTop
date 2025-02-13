using System;
using System.Collections.Generic;
using UnityEngine;

public class MarketFeature
{
    public Guid Id{get; private set;}=Guid.NewGuid();
    public FeatureNameEnum FeatureName;
    [SerializeField]
    public Sprite featureSprite;
    public (int x, int y) FeatureSize;
    
    private MarketFeatureSpriteDatabase spriteDatabase;

    public PriorityEnum Priority;
    public bool CanBuildOnFeature;

    public List<int> Location= new();

    public int GetSpace()
    {
        return FeatureSize.x * FeatureSize.y;
    }

    public MarketFeature(FeatureNameEnum name, (int x, int y) size, PriorityEnum priority, bool canBuildOnFeature, MarketFeatureSpriteDatabase spriteDatabase)
    {
        FeatureName = name;
        FeatureSize = size;
        Priority = priority;
        CanBuildOnFeature = canBuildOnFeature;
        this.spriteDatabase = spriteDatabase?? throw new System.ArgumentNullException(nameof(spriteDatabase));
        featureSprite= GetRandomSprite();
    }

    public Sprite GetRandomSprite()
    {
        if (spriteDatabase == null)
        {
            Debug.LogWarning($"MarketFeature '{FeatureName}' has no sprite database");
            return null;
        }
        var spriteVariant = spriteDatabase?.GetVariant(FeatureName);
        return spriteVariant?.GetRandomSprite();
    }
}
