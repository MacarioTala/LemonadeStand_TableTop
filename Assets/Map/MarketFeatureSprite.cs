
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// A MarketFeatureSprite contains a list of 
/// sprite variants for each MarketFeature.
/// The MarketFeature will have only 
/// one Sprite set as the featureSprite.
/// SpriteVariants are purely cosmetic
/// and have no in-game effect.
/// </summary>
[CreateAssetMenu(fileName = "MarketFeatureSprite", menuName = "Art/Feature Sprite Variant")]
public class MarketFeatureSprite : ScriptableObject
{
    public FeatureNameEnum FeatureName;
    public List<Sprite> SpriteVariants;

    public Sprite GetRandomSprite()
    {
        if (SpriteVariants == null || SpriteVariants.Count == 0)
        {
            Debug.LogWarning($"MarketFeatureSprite '{FeatureName}' has no sprite variants");
            return null;
        }
        int randomIndex = Random.Range(0, SpriteVariants.Count);
        return SpriteVariants[randomIndex];
    }
    public override string ToString()
    {
        return FeatureName.ToString();
    }
    public override bool Equals(object other)
    {
        if (other == null || GetType() != other.GetType())
        {
            return false;
        }
        return FeatureName == ((MarketFeatureSprite)other).FeatureName;
    }

    public override int GetHashCode()
    {
        return FeatureName.GetHashCode();
    }
}
