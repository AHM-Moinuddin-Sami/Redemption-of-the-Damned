using UnityEngine;

/*
 * MapFeatureEntity
 * ----------------
 * Represents a non-actor object that exists on the gameplay grid.
 *
 * Features are things like:
 * - stairs
 * - doors
 * - chests
 * - traps
 * - shrines
 * - interactable dungeon objects
 *
 * This is different from ActorGridEntity.
 * Actors are creatures that take turns and occupy actor slots.
 * Features are map objects that may or may not block movement.
 *
 * Main responsibilities:
 * - store the feature's grid position
 * - register the feature into MapData
 * - snap the feature visual to the center of its grid cell
 * - clear the feature from MapData when destroyed
 *
 * Current usage:
 * - StairsDownFeature uses this base class to exist on the map.
 *
 * Later this can support:
 * - doors that block movement
 * - chests that do not block movement
 * - traps that trigger when stepped on
 * - shrines that can be interacted with
 */

public class MapFeatureEntity : MonoBehaviour
{
    [Header("Feature Info")]
    [SerializeField] private string displayName = "Feature";
    [SerializeField] private bool blocksMovement = false;

    public string DisplayName
    {
        get
        {
            return displayName;
        }
    }

    public bool BlocksMovement
    {
        get
        {
            return blocksMovement;
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

        // Register this feature into the map's feature occupancy data.
        if (!mapData.TryPlaceFeature(this, startPosition))
        {
            Debug.LogError(displayName + " could not be placed at " + startPosition);
            return false;
        }

        GridPosition = startPosition;
        isInitialized = true;

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
            mapData.TryRemoveFeature(this, GridPosition);
        }

        isInitialized = false;
    }

    private void SnapToGridPosition()
    {
        Vector3 worldPosition = mapRenderer.GetCellCenterWorld(GridPosition);

        // Preserve Z position so feature sorting/depth stays controlled by prefab setup.
        worldPosition.z = transform.position.z;

        transform.position = worldPosition;
    }
}