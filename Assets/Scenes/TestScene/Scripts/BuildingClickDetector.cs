using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingClickDetector : MonoBehaviour
{
    // Start is called before the first frame update
    public Tilemap BuildingTileMap;
    public GameObject NeighbourhoodGrid;
    public GameObject StandPOVView;
    private readonly Dictionary<Vector2Int, FeatureNameEnum> featureMap = new();

    private void Start()
    {
        registerFeature(new Vector2Int(-4,6), new Vector2Int(0,2), FeatureNameEnum.Grocer);
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Camera.main == null)
            {
                Debug.LogError("Main camera not found");
                return;
            }

            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int coordinate = BuildingTileMap.WorldToCell(mouseWorldPos);
            
            TileBase clickedTile = BuildingTileMap.GetTile(coordinate);
            if (clickedTile != null)
            {
                var tileKey = new Vector2Int(coordinate.x, coordinate.y);
                if (featureMap.ContainsKey(tileKey))
                {
                    if(featureMap.TryGetValue(tileKey, out var featureName))
                    {
                        Debug.Log($"Clicked on tile at {coordinate} with feature {featureName}");
                        switchViewToFeature(featureName);
                    }
                }
                else
                {
                    Debug.Log($"Clicked on tile at {coordinate}");
                }
            }
            
        }
    }

    private void registerFeature(Vector2Int topLeft, Vector2Int bottomRight, FeatureNameEnum featureName)
    {
        for (int x = topLeft.x; x <= bottomRight.x; x++)
        {
            for (int y = topLeft.y; y >= bottomRight.y; y--)
            {
                featureMap.Add(new Vector2Int(x, y), featureName);
            }
        }
    }

    private void switchViewToFeature(FeatureNameEnum featureName)
    {
        NeighbourhoodGrid.SetActive(false);
        StandPOVView.SetActive(true);
    }
}
