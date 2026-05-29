using UnityEngine;

/*
 * ActorItemDropper
 * ----------------
 * Handles dropping inventory items onto the ground at the actor's current grid cell.
 *
 * This version preserves rolled item data.
 * If the player drops a Rare sword, the ground item keeps the same rarity and
 * affixes instead of becoming a plain base sword.
 */

[RequireComponent(typeof(ActorGridEntity))]
[RequireComponent(typeof(ActorInventory))]
public class ActorItemDropper : MonoBehaviour
{
    [Header("Noise")]
    [SerializeField] private int dropNoiseRange = 5;

    private MapData mapData;
    private MapRenderer mapRenderer;
    private ItemGridEntity groundItemPrefab;
    private Transform itemParent;

    private ActorGridEntity actorGridEntity;
    private ActorInventory actorInventory;
    private bool isInitialized;

    private void Awake()
    {
        actorGridEntity = GetComponent<ActorGridEntity>();
        actorInventory = GetComponent<ActorInventory>();
    }

    public void Initialize(
        MapData newMapData,
        MapRenderer newMapRenderer,
        ItemGridEntity newGroundItemPrefab,
        Transform newItemParent)
    {
        mapData = newMapData;
        mapRenderer = newMapRenderer;
        groundItemPrefab = newGroundItemPrefab;
        itemParent = newItemParent;

        isInitialized = true;
    }

    public bool TryDropItem(ItemInstance itemInstance)
    {
        if (!isInitialized)
        {
            Debug.LogWarning(gameObject.name + " cannot drop items because ActorItemDropper is not initialized.");
            return false;
        }

        if (itemInstance == null || itemInstance.Definition == null)
        {
            return false;
        }

        if (groundItemPrefab == null)
        {
            Debug.LogWarning("Cannot drop item because ground item prefab is missing.");
            return false;
        }

        Vector2Int dropPosition = actorGridEntity.GridPosition;
        ItemGridEntity droppedItem = Instantiate(groundItemPrefab, itemParent);

        bool placed = droppedItem.Initialize(
            mapData,
            mapRenderer,
            dropPosition,
            itemInstance
        );

        if (!placed)
        {
            Destroy(droppedItem.gameObject);
            GameMessageLog.Write("There is no room to drop " + itemInstance.GetDisplayName() + ".");
            return false;
        }

        bool removedFromInventory = actorInventory.RemoveItem(itemInstance);

        if (!removedFromInventory)
        {
            droppedItem.ClearFromMap();
            Destroy(droppedItem.gameObject);

            Debug.LogWarning("Dropped item was placed, but inventory removal failed.");
            return false;
        }

        GameMessageLog.Write(gameObject.name + " drops " + ItemTextFormatter.FormatItemName(itemInstance) + ".");
        EmitDropNoise();

        return true;
    }

    private void EmitDropNoise()
    {
        GameNoiseSystem.EmitNoise(
            actorGridEntity.GridPosition,
            dropNoiseRange,
            actorGridEntity,
            NoiseCategory.Item
        );
    }
}