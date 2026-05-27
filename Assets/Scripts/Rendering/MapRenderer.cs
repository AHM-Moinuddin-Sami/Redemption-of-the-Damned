using UnityEngine;
using UnityEngine.Tilemaps;

/*
 * MapRenderer
 * -----------
 * Converts MapData gameplay cells into visible Unity Tilemap tiles.
 *
 * Main responsibilities:
 * - clear old Tilemap visuals
 * - paint floor tiles
 * - paint wall tiles
 * - provide world-space cell centers for spawned actors
 *
 * Important:
 * MapData is the gameplay source of truth.
 * Tilemaps are only visual output.
 *
 * The GetCellCenterWorld method is useful because actors should stand in the
 * center of a tile cell, not on the corner of the cell.
 */

public class MapRenderer : MonoBehaviour
{
    [Header("Tilemaps")]
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap wallTilemap;

    [Header("Tiles")]
    [SerializeField] private TileBase floorTile;
    [SerializeField] private TileBase wallTile;

    public void Render(MapData mapData)
    {
        groundTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();

        for (int x = 0; x < mapData.Width; x++)
        {
            for (int y = 0; y < mapData.Height; y++)
            {
                Vector2Int gridPosition = new Vector2Int(x, y);
                Vector3Int tilePosition = new Vector3Int(x, y, 0);

                MapCell cell = mapData.GetCell(gridPosition);

                if (cell.Terrain == CellTerrain.Floor)
                {
                    groundTilemap.SetTile(tilePosition, floorTile);
                }
                else if (cell.Terrain == CellTerrain.Wall)
                {
                    wallTilemap.SetTile(tilePosition, wallTile);
                }
            }
        }
    }

    public Vector3 GetCellCenterWorld(Vector2Int gridPosition)
    {
        // Converts gameplay grid position to the center of a Unity Tilemap cell.
        Vector3Int tilePosition = new Vector3Int(gridPosition.x, gridPosition.y, 0);
        return groundTilemap.GetCellCenterWorld(tilePosition);
    }
}