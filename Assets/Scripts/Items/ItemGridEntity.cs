using UnityEngine;

/*
 * ItemGridEntity
 * --------------
 * Represents an item lying on the dungeon floor.
 *
 * This script connects a visible item GameObject to the gameplay grid.
 * It registers itself into MapData so the current cell knows an item exists there.
 *
 * Current responsibilities:
 * - store the item definition
 * - store item quantity
 * - allow spawned items to receive an ItemDefinition from a loot table
 * - register the item into MapData
 * - snap the item visual to the center of its cell
 * - create an ItemInstance when picked up
 * - provide inspect text for the player look command
 * - clear itself from MapData when destroyed
 *
 * Important:
 * Items do not block movement.
 * Actors can stand on the same cell as items.
 */

[RequireComponent(typeof(SpriteRenderer))]
public class ItemGridEntity : MonoBehaviour
{
    [Header("Item")]
    [SerializeField] private ItemDefinition itemDefinition;
    [SerializeField] private int quantity = 1;

    public ItemDefinition ItemDefinition
    {
        get
        {
            return itemDefinition;
        }
    }

    public int Quantity
    {
        get
        {
            return quantity;
        }
    }

    public Vector2Int GridPosition { get; private set; }

    private MapData mapData;
    private MapRenderer mapRenderer;
    private SpriteRenderer spriteRenderer;
    private bool isInitialized;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        RefreshVisual();
    }

    private void OnDestroy()
    {
        ClearFromMap();
    }

    public bool Initialize(MapData newMapData, MapRenderer newMapRenderer, Vector2Int startPosition)
    {
        mapData = newMapData;
        mapRenderer = newMapRenderer;

        if (itemDefinition == null)
        {
            Debug.LogError(gameObject.name + " has no ItemDefinition assigned.");
            return false;
        }

        if (!mapData.TryPlaceItem(this, startPosition))
        {
            Debug.LogError(itemDefinition.DisplayName + " could not be placed at " + startPosition);
            return false;
        }

        GridPosition = startPosition;
        isInitialized = true;

        RefreshVisual();
        SnapToGridPosition();

        return true;
    }

    public bool Initialize(
        MapData newMapData,
        MapRenderer newMapRenderer,
        Vector2Int startPosition,
        ItemDefinition newItemDefinition,
        int newQuantity)
    {
        itemDefinition = newItemDefinition;
        quantity = Mathf.Max(1, newQuantity);

        return Initialize(newMapData, newMapRenderer, startPosition);
    }

    public ItemInstance CreateItemInstance()
    {
        return new ItemInstance(itemDefinition, Mathf.Max(1, quantity));
    }

    public string GetInspectText()
    {
        return ItemDescriptionBuilder.Build(itemDefinition, quantity);
    }

    public void ClearFromMap()
    {
        if (!isInitialized)
        {
            return;
        }

        if (mapData != null)
        {
            mapData.TryRemoveItem(this, GridPosition);
        }

        isInitialized = false;
    }

    private void RefreshVisual()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        if (itemDefinition == null)
        {
            return;
        }

        if (itemDefinition.IconSprite != null)
        {
            spriteRenderer.sprite = itemDefinition.IconSprite;
        }
    }

    private void SnapToGridPosition()
    {
        Vector3 worldPosition = mapRenderer.GetCellCenterWorld(GridPosition);

        worldPosition.z = transform.position.z;

        transform.position = worldPosition;
    }
}