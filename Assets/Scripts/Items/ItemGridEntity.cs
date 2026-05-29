using UnityEngine;

/*
 * ItemGridEntity
 * --------------
 * Represents an item lying on the dungeon floor.
 *
 * This version stores a full ItemInstance instead of only an ItemDefinition.
 * That matters because rolled items need to keep their rarity and affixes while
 * lying on the ground.
 *
 * Current responsibilities:
 * - store the runtime ItemInstance
 * - register the ground item into MapData
 * - snap the item visual to the center of its cell
 * - return the stored ItemInstance when picked up
 * - provide inspect text
 * - clear itself from MapData when destroyed
 */

[RequireComponent(typeof(SpriteRenderer))]
public class ItemGridEntity : MonoBehaviour
{
    [Header("Fallback Item")]
    [SerializeField] private ItemDefinition itemDefinition;
    [SerializeField] private int quantity = 1;

    public ItemInstance ItemInstance
    {
        get
        {
            return itemInstance;
        }
    }

    public ItemDefinition ItemDefinition
    {
        get
        {
            if (itemInstance != null)
            {
                return itemInstance.Definition;
            }

            return itemDefinition;
        }
    }

    public int Quantity
    {
        get
        {
            if (itemInstance != null)
            {
                return itemInstance.Quantity;
            }

            return Mathf.Max(1, quantity);
        }
    }

    public Vector2Int GridPosition { get; private set; }

    private ItemInstance itemInstance;
    private MapData mapData;
    private MapRenderer mapRenderer;
    private SpriteRenderer spriteRenderer;
    private bool isInitialized;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (itemInstance == null && itemDefinition != null)
        {
            itemInstance = new ItemInstance(itemDefinition, quantity);
        }

        RefreshVisual();
    }

    private void OnDestroy()
    {
        ClearFromMap();
    }

    public bool Initialize(MapData newMapData, MapRenderer newMapRenderer, Vector2Int startPosition)
    {
        if (itemInstance == null && itemDefinition != null)
        {
            itemInstance = new ItemInstance(itemDefinition, quantity);
        }

        return Initialize(newMapData, newMapRenderer, startPosition, itemInstance);
    }

    public bool Initialize(
        MapData newMapData,
        MapRenderer newMapRenderer,
        Vector2Int startPosition,
        ItemDefinition newItemDefinition,
        int newQuantity)
    {
        itemInstance = new ItemInstance(newItemDefinition, Mathf.Max(1, newQuantity));
        itemDefinition = newItemDefinition;
        quantity = Mathf.Max(1, newQuantity);

        return Initialize(newMapData, newMapRenderer, startPosition, itemInstance);
    }

    public bool Initialize(
        MapData newMapData,
        MapRenderer newMapRenderer,
        Vector2Int startPosition,
        ItemInstance newItemInstance)
    {
        mapData = newMapData;
        mapRenderer = newMapRenderer;
        itemInstance = newItemInstance;

        if (itemInstance == null || itemInstance.Definition == null)
        {
            Debug.LogError(gameObject.name + " has no valid ItemInstance assigned.");
            return false;
        }

        itemDefinition = itemInstance.Definition;
        quantity = itemInstance.Quantity;

        if (!mapData.TryPlaceItem(this, startPosition))
        {
            Debug.LogError(itemInstance.GetDisplayName() + " could not be placed at " + startPosition);
            return false;
        }

        GridPosition = startPosition;
        isInitialized = true;

        RefreshVisual();
        SnapToGridPosition();

        return true;
    }

    public ItemInstance CreateItemInstance()
    {
        return itemInstance;
    }

    public string GetInspectText()
    {
        return ItemDescriptionBuilder.Build(itemInstance);
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

        if (ItemDefinition == null)
        {
            return;
        }

        if (ItemDefinition.IconSprite != null)
        {
            spriteRenderer.sprite = ItemDefinition.IconSprite;
        }
    }

    private void SnapToGridPosition()
    {
        Vector3 worldPosition = mapRenderer.GetCellCenterWorld(GridPosition);

        worldPosition.z = transform.position.z;

        transform.position = worldPosition;
    }

    public string GetDisplayName()
    {
        if (itemInstance != null)
        {
            return itemInstance.GetDisplayName();
        }

        if (itemDefinition == null)
        {
            return "Unknown Item";
        }

        if (quantity > 1)
        {
            return itemDefinition.DisplayName + " x" + quantity;
        }

        return itemDefinition.DisplayName;
    }

    public string GetFormattedDisplayName()
    {
        if (itemInstance != null)
        {
            return ItemTextFormatter.FormatItemName(itemInstance);
        }

        return ItemTextFormatter.FormatItemName(itemDefinition, quantity);
    }
}