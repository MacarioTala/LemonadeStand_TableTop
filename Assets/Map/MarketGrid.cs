using System;
using System.Collections.Generic;
using UnityEngine;

public class MarketGrid : MonoBehaviour
{
    [SerializeField] private Market market;
    [SerializeField] private float tileSize;
    [SerializeField] private GameObject tilePrefab;

    private readonly Dictionary<Guid, MarketFeature> featureDictionary = new();

    private GameObject[,] grid;

    int GridWidth;
    int GridHeight;


    public void InitializeGrid()
    {
        GridWidth = market.GetWidth();
        GridHeight = market.GetHeight();
    }

    public void RegisterFeature(MarketFeature feature)
    {
        if(!featureDictionary.ContainsKey(feature.Id))
        {
            featureDictionary.Add(feature.Id, feature);
        }
        
    }

    public MarketFeature GetFeatureById(Guid id)
    {
        return featureDictionary.TryGetValue(id, out var feature) ? feature : null;
    }
}
