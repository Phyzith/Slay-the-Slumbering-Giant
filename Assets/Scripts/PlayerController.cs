using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveTime = 1f;
    public int playerMovement = 2;

    [Header("Hex Settings")]
    public int pathMovementCost = 0;
    public int plainsMovementCost = 1;
    public int forestMovementCost = 2;
    public int seaMovementCost = 2;

    [Header("Other Settings")]
    public Tilemap targetTilemap;
    public Camera mainCamera;
    private Vector3 currentVelocity = Vector3.zero;
    private Vector3 targetPosition = Vector3.zero;
    private Vector3Int currentCellPos;

    void Start()
    {
        if (mainCamera == null) {mainCamera = Camera.main;}
        targetPosition = transform.position;
        currentCellPos = targetTilemap.WorldToCell(transform.position);
    }

    void Update()
    {
        // all functions that trigger upon a mouseclick
        if (Input.GetMouseButtonDown(0)) {
            Vector3Int selectedHexPos = getSelectedHexCellPos();
            Vector3 selectedHexWorldCenter = targetTilemap.GetCellCenterWorld(selectedHexPos);
            CustomHex selectedHex = targetTilemap.GetTile<CustomHex>(selectedHexPos);

            if (selectedHex != null && isHexAdjacent(selectedHexPos) && selectedHex.isTraversable && playerMovement >= selectedHex.movementCost) 
            {
                targetPosition = new Vector3(selectedHexWorldCenter.x, selectedHexWorldCenter.y, transform.position.z);
                playerMovement -= selectedHex.movementCost;

                currentCellPos = selectedHexPos; // new
            }
        }

        MovePlayer(targetPosition);
    }

    Vector3Int getSelectedHexCellPos() 
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Mathf.Abs(mainCamera.transform.position.z);
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);

        Vector3Int cellPos = targetTilemap.WorldToCell(mouseWorldPos);

        TileBase clickedTile = targetTilemap.GetTile(cellPos);
          Debug.Log($"Tile type: {clickedTile.GetType()}");

        if (clickedTile != null)
        {
            Vector3 cellWorldCenter = targetTilemap.GetCellCenterWorld(cellPos);
            Debug.Log($"Selected Tile: {clickedTile.name} | Grid Coords: {cellPos} | World Center: {cellWorldCenter}");
            return cellPos;
        }
        else
        {
            Debug.Log($"Clicked empty cell at Grid Coords: {cellPos}");
        }

        return new Vector3Int(0,0,0);
    }

    bool isHexAdjacent(Vector3Int cellPos) {
        if (cellPos == currentCellPos) return false;

        Vector3 from = targetTilemap.GetCellCenterWorld(currentCellPos);
        Vector3 to = targetTilemap.GetCellCenterWorld(cellPos);

        // (0,1) is always a direct neighbor on a flat-top grid, so use it as the reference distance
        float neighborDist = Vector3.Distance(from, targetTilemap.GetCellCenterWorld(currentCellPos + Vector3Int.up));

        return Vector3.Distance(from, to) < neighborDist * 1.1f;
    }

    void MovePlayer(Vector3 hexPos) 
    {
        transform.position = Vector3.SmoothDamp(transform.position, hexPos, ref currentVelocity, moveTime);
    }
}
