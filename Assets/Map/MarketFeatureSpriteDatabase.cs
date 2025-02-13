using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "MarketFeatureSpriteDatabase", menuName = "Art/Feature Sprite Database")]
public class MarketFeatureSpriteDatabase : ScriptableObject
{
    public List<MarketFeatureSprite> MarketFeatureSprites;
    private Dictionary<FeatureNameEnum, List<MarketFeatureSprite>> spriteDictionary; //for faster lookup

    private void Awake()
    {
        InitializeSpriteDictionary();
    }
    
    private void InitializeSpriteDictionary()
    {
        if (MarketFeatureSprites == null)
        {
            Debug.LogWarning("MarketFeatureSprites is null. Make sure it's assigned in the inspector.");
            return;
        }
        
        spriteDictionary=MarketFeatureSprites
            .GroupBy(x => x.FeatureName)
            .ToDictionary(x => x.Key, x => x.ToList());
    }
    public MarketFeatureSprite GetVariant(FeatureNameEnum featureName)
    {
        if(spriteDictionary.TryGetValue(featureName, out var variants) && variants.Count > 0)
        {
            int randomIndex = Random.Range(0, variants.Count);
            return variants[randomIndex];
        }
        else
        {
            Debug.LogWarning($"MarketFeatureSpriteDatabase does not contain feature '{featureName.ToString()}'");
            return null;
        }
        
    }
}