using UnityEngine;

/*
 * ActorItemDropper
 * ----------------
 * Handles dropping inventory items onto the ground at the actor's current grid cell.
 *
 * This script belongs on an actor that has:
 * - ActorGridEntity
 * - ActorInventory
 *
 * Current behavior:
 * - receives an ItemInstance from UI or another controller
 * - creates a ground item at the actor's current grid position
 * - removes the item from inventory only if the ground item was placed successfully
 * - drops the entire selected stack for stackable items
 *
 * Example:
 * Inventory has Bread x3.
 * Player drops Bread.
 * Bread x3 appears on the ground.
 * Bread x3 is removed from inventory.
 *
 * Important:
 * This does not split stacks yet.
 * It also does not choose nearby empty cells if the current tile cannot accept the item.
 *
 * Later this can expand into:
 * - drop one item from a stack
 * - drop custom quantity
 * - drop to adjacent tile if current tile is blocked
 * - throw item
 * - container transfer
 * - item pile merging
 */

[RequireComponent(typeof(ActorGridEntity))]
[RequireComponent(typeof(ActorInventory))]
public class ActorItemDropper : MonoBehaviour
{
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
            itemInstance.Definition,
            itemInstance.Quantity
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

        GameMessageLog.Write(gameObject.name + " drops " + itemInstance.GetDisplayName() + ".");

        return true;
    }
}