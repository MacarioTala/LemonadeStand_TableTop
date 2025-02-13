using UnityEngine;

public class Maptile
{
    public string tileName;
    public Sprite tileSprite;
    public (int x, int y) tileSize;

    public TileTypeEnum tileType;
    public bool isPassable;
}
