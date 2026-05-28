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
 *
 * This version supports both movement blocking and sight blocking.
 * This matters because a closed door should block movement and vision, while an
 * open door should block neither.
 *
 * Main responsibilities:
 * - store the feature's display name
 * - store whether the feature blocks movement
 * - store whether the feature blocks sight
 * - register the feature into MapData
 * - snap the feature visual to the grid
 * - clear itself from MapData when destroyed
 */

public class MapFeatureEntity : MonoBehaviour
{
    [Header("Feature Info")]
    [SerializeField] private string displayName = "Feature";
    [SerializeField] private bool blocksMovement = false;
    [SerializeField] private bool blocksSight = false;

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

    public bool BlocksSight
    {
        get
        {
            return blocksSight;
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

    public void SetBlocksMovement(bool value)
    {
        blocksMovement = value;
    }

    public void SetBlocksSight(bool value)
    {
        blocksSight = value;
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

        worldPosition.z = transform.position.z;

        transform.position = worldPosition;
    }
}