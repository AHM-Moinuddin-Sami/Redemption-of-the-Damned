using UnityEngine;
using UnityEngine.Tilemaps;

/*
 * FieldOfViewRenderer
 * -------------------
 * Renders fog of war using two overlay Tilemaps.
 *
 * This script does not calculate visibility by itself. It only receives visible
 * and explored arrays from PlayerFieldOfView, then paints fog tiles over the map.
 *
 * Current fog states:
 * - Visible: no fog tile is drawn.
 * - Explored but not visible: dim fog tile is drawn.
 * - Never explored: dark fog tile is drawn.
 *
 * Required scene setup:
 * - one Tilemap for unexplored fog
 * - one Tilemap for explored/dim fog
 * - both fog Tilemaps should render above actors/items/features
 *
 * Important:
 * This approach is simple and works well for a prototype. It hides the world by
 * drawing dark overlay tiles above the map.
 *
 * Later this can expand into:
 * - smooth lighting
 * - color-tinted fog by biome
 * - separate actor visibility
 * - stealth/sneaking visibility
 * - light radius from torches
 */

public class FieldOfViewRenderer : MonoBehaviour
{
    [Header("Tilemaps")]
    [SerializeField] private Tilemap unexploredFogTilemap;
    [SerializeField] private Tilemap exploredFogTilemap;

    [Header("Tiles")]
    [SerializeField] private TileBase fogTile;

    [Header("Fog Colors")]
    [SerializeField] private Color unexploredFogColor = new Color(0f, 0f, 0f, 1f);
    [SerializeField] private Color exploredFogColor = new Color(0f, 0f, 0f, 0.55f);

    private void Awake()
    {
        ApplyTilemapColors();
    }

    public void Render(MapData mapData, bool[,] visibleCells, bool[,] exploredCells)
    {
        if (mapData == null || visibleCells == null || exploredCells == null)
        {
            return;
        }

        if (unexploredFogTilemap == null || exploredFogTilemap == null || fogTile == null)
        {
            return;
        }

        ApplyTilemapColors();

        unexploredFogTilemap.ClearAllTiles();
        exploredFogTilemap.ClearAllTiles();

        for (int x = 0; x < mapData.Width; x++)
        {
            for (int y = 0; y < mapData.Height; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);

                if (visibleCells[x, y])
                {
                    // Visible cells have no fog overlay.
                    continue;
                }

                if (exploredCells[x, y])
                {
                    exploredFogTilemap.SetTile(tilePosition, fogTile);
                    continue;
                }

                unexploredFogTilemap.SetTile(tilePosition, fogTile);
            }
        }
    }

    public void Clear()
    {
        if (unexploredFogTilemap != null)
        {
            unexploredFogTilemap.ClearAllTiles();
        }

        if (exploredFogTilemap != null)
        {
            exploredFogTilemap.ClearAllTiles();
        }
    }

    private void ApplyTilemapColors()
    {
        if (unexploredFogTilemap != null)
        {
            unexploredFogTilemap.color = unexploredFogColor;
        }

        if (exploredFogTilemap != null)
        {
            exploredFogTilemap.color = exploredFogColor;
        }
    }
}