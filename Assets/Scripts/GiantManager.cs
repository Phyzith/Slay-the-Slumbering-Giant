using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI; 

public class GiantManager : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private GameObject player;
    

    public Tilemap tilemap;
    private Vector3Int giantCell;
    public Vector3 giantPos;
    private CustomHex giantHex;

    static readonly Vector3Int[] EvenColDirs =
    {
        new Vector3Int( 0,  1, 0),
        new Vector3Int( 1,  0, 0),
        new Vector3Int( 1, -1, 0),
        new Vector3Int( 0, -1, 0),
        new Vector3Int(-1, -1, 0),
        new Vector3Int(-1,  0, 0),
    };

    static readonly Vector3Int[] OddColDirs =
    {
        new Vector3Int( 0,  1, 0),
        new Vector3Int( 1,  1, 0),
        new Vector3Int( 1,  0, 0),
        new Vector3Int( 0, -1, 0),
        new Vector3Int(-1,  0, 0),
        new Vector3Int(-1,  1, 0),
    };

    void Start()
    {
        giantCell = new Vector3Int(Random.Range(-4, 4), Random.Range(-4, 4), 0);
        giantPos = tilemap.GetCellCenterWorld(giantCell);
        giantHex = tilemap.GetTile<CustomHex>(giantCell);

        printGiantLocation();
        MoveGiant();
        MoveGiant();
        MoveGiant();
    }

    void Update()
    {
        
    }

    void SetGiantLocation(Vector3Int cell)
    {
        giantCell = cell;
        giantPos = tilemap.GetCellCenterWorld(cell);
        giantHex = tilemap.GetTile<CustomHex>(cell);
    }

    public void MoveGiant()
    {
        bool isEvenCell = Mathf.Abs(giantCell.x) % 2 == 0;
        int moveDirection = Random.Range(0, 6); // returns 1-6 where the number is the direction the giant will move.
        Vector3Int move;

        if (isEvenCell) {move = EvenColDirs[moveDirection];}
        else {move = OddColDirs[moveDirection];}

        if (tilemap.GetTile<CustomHex>(giantCell + move).name != "VoidHex") 
        {
            SetGiantLocation(giantCell + move);
        }

        printGiantLocation();
    }

    public string getGiantCompassDirection()
    {
        string[] Compass6 = { "N", "NE", "SE", "S", "SW", "NW" };

        Vector3 toGiant = giantPos - player.transform.position;

        if (toGiant.sqrMagnitude < 0.001f)
            return "Here"; // the giant is in your tile

        float angle = Mathf.Atan2(toGiant.x, toGiant.y) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        int index = Mathf.RoundToInt(angle / 60f) % 6;
        return Compass6[index];
    }

    void printGiantLocation() {Debug.Log($"Giant Location: Cell {giantCell}, Pos {giantPos}, Hex {giantHex}, Direction {getGiantCompassDirection()}");}
}
