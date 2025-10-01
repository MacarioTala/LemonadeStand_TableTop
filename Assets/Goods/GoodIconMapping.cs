using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GoodIconMapping", menuName = "LemonadeStand/Good Icon Mapping")]
public class GoodIconMapping : ScriptableObject
{
    [System.Serializable]
    public struct GoodSpritePair
    {
        public string goodName;
        public Sprite sprite;
    }
    
    public GoodSpritePair[] mappings;
    private Dictionary<string, Sprite> mappingDictionary;
    
    private void OnEnable()
    {
        InitializeDictionary();
    }
    
    private void InitializeDictionary()
    {
        mappingDictionary = new Dictionary<string, Sprite>();
        foreach (var pair in mappings)
        {
            if (!string.IsNullOrEmpty(pair.goodName) && pair.sprite != null)
            {
                mappingDictionary[pair.goodName] = pair.sprite;
            }
        }
    }
    
    public Sprite GetSpriteForGood(string goodName)
    {
        if (mappingDictionary == null)
        {
            InitializeDictionary();
        }
        
        if (mappingDictionary.TryGetValue(goodName, out Sprite sprite))
        {
            return sprite;
        }
        
        return null;
    }
    
    public bool HasMapping(string goodName)
    {
        if (mappingDictionary == null)
        {
            InitializeDictionary();
        }
        
        return mappingDictionary.ContainsKey(goodName);
    }
}
