using UnityEngine;

/*
 * ActorGridEntity
 * ---------------
 * Represents an actor's position on the gameplay grid.
 *
 * This script connects an actor GameObject to MapData. It stores the actor's
 * grid position, registers the actor into the current MapData, moves the actor
 * through the grid, and snaps the actor's world position to the Tilemap cell.
 *
 * Current responsibilities:
 * - store actor display name
 * - store current grid position
 * - register actor into MapData
 * - clear actor from MapData
 * - move actor one grid cell at a time
 * - snap actor visual position to the center of a cell
 * - support reinitializing the same actor onto a new floor
 *
 * Important:
 * Reinitialization matters because the player now persists between floors.
 * When the player descends, we do not destroy and respawn the player anymore.
 * Instead, the same player object is cleared from the old MapData and placed
 * into the new MapData at the new floor's spawn position.
 *
 * Later this can expand into:
 * - actor facing direction
 * - movement animation hooks
 * - actor size larger than 1 tile
 * - forced movement
 * - teleportation
 * - knockback
 */

public class ActorGridEntity : MonoBehaviour
{
    [Header("Actor Info")]
    [SerializeField] private string displayName = "Actor";

    public string DisplayName
    {
        get
        {
            return displayName;
        }
    }

    public Vector2Int GridPosition { get; private set; }

    private MapData mapData;
    private MapRenderer mapRenderer;
    private bool isInitialized;

    private void OnDestroy()
    {
        ClearFromMap();
    }

    public bool Initialize(MapData newMapData, MapRenderer newMapRenderer, Vector2Int startPosition)
    {
        // If this actor was already registered into a previous map, clear it first.
        // This is required for persistent actors such as the player when changing floors.
        ClearFromMap();

        mapData = newMapData;
        mapRenderer = newMapRenderer;

        if (mapData == null)
        {
            Debug.LogError(displayName + " cannot initialize because MapData is missing.");
            return false;
        }

        if (mapRenderer == null)
        {
            Debug.LogError(displayName + " cannot initialize because MapRenderer is missing.");
            return false;
        }

        if (!mapData.TryPlaceActor(this, startPosition))
        {
            Debug.LogError(displayName + " could not be placed at " + startPosition);
            return false;
        }

        GridPosition = startPosition;
        isInitialized = true;

        SnapToGridPosition();

        return true;
    }

    public bool TryMove(Vector2Int direction)
    {
        if (!isInitialized)
        {
            return false;
        }

        if (direction == Vector2Int.zero)
        {
            return false;
        }

        Vector2Int targetPosition = GridPosition + direction;

        if (!mapData.TryMoveActor(this, GridPosition, targetPosition))
        {
            return false;
        }

        GridPosition = targetPosition;

        SnapToGridPosition();

        return true;
    }

    public void ClearFromMap()
    {
        if (!isInitialized)
        {
            return;
        }

        if (mapData != null)
        {
            mapData.TryRemoveActor(this, GridPosition);
        }

        isInitialized = false;
    }

    private void SnapToGridPosition()
    {
        Vector3 worldPosition = mapRenderer.GetCellCenterWorld(GridPosition);

        // Preserve the actor's Z value so sorting/depth stays controlled by prefab setup.
        worldPosition.z = transform.position.z;

        transform.position = worldPosition;
    }

    public void SetDisplayName(string newDisplayName)
    {
        if (string.IsNullOrWhiteSpace(newDisplayName))
        {
            return;
        }

        displayName = newDisplayName;
    }
}