using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "NewCustomTile", menuName = "2D/Tiles/Custom Tile")]
public class CustomHex : Tile
{
    [Header("Hex Properties")]
    public int movementCost = 1;
    public bool isTraversable = true;
}
