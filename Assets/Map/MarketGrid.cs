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
        GenerateGrid();
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
    

    private void GenerateGrid()
    {
      grid = new GameObject[GridWidth, GridHeight];

        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                throw new NotImplementedException();
            }
        }

}
}
