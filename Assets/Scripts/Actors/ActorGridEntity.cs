using UnityEngine;

/*
 * ActorGridEntity
 * ---------------
 * Represents an actor that occupies one cell on the gameplay grid.
 *
 * This script is used by both the player and enemies. It stores the actor's
 * current grid position and registers the actor inside MapData so the map knows
 * which cells are occupied.
 *
 * Main responsibilities:
 * - store the actor's grid position
 * - register the actor into MapData
 * - move the actor between grid cells
 * - keep the actor visually snapped to the center of its current tile
 * - clear the actor from MapData when destroyed
 *
 * Important:
 * This script does not handle input.
 * Player input belongs in PlayerGridMover.
 *
 * This script also does not handle AI.
 * Enemy AI will later tell ActorGridEntity where to move.
 *
 * OnDestroy matters because when an enemy dies, the GameObject is destroyed.
 * If we do not clear the actor from MapData, the cell would stay permanently
 * occupied even though the enemy is gone.
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
        mapData = newMapData;
        mapRenderer = newMapRenderer;

        // Register this actor into the map occupancy data.
        // If this fails, the actor was spawned on an invalid or occupied cell.
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

        Vector2Int targetPosition = GridPosition + direction;

        // MapData handles terrain blocking and actor occupancy blocking.
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

        // Preserve the actor's Z value so sorting/depth is not changed.
        worldPosition.z = transform.position.z;

        transform.position = worldPosition;
    }
}