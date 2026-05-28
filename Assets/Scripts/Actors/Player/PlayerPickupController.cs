using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/*
 * PlayerPickupController
 * ----------------------
 * Handles picking up items from the player's current grid cell.
 *
 * Current behavior:
 * - listens for a pickup input action
 * - checks the player's current cell for ground items
 * - picks up every item on that tile
 * - adds each item to the player's ActorInventory
 * - removes each ground item from MapData
 * - destroys each picked-up ground item GameObject
 * - consumes a turn only if at least one item was picked up
 *
 * Main responsibilities:
 * - receive pickup input
 * - prevent pickup while enemy turns are processing
 * - prevent pickup while inventory UI is open
 * - transfer ground item instances into inventory
 * - clear picked-up ground items from the map
 * - notify TurnManager after successful pickup
 *
 * Important:
 * Picking up all items at once is a temporary convenience.
 * Later, when item pile UI exists, the player can choose exactly which item from
 * a pile to pick up.
 */

[RequireComponent(typeof(ActorGridEntity))]
[RequireComponent(typeof(ActorInventory))]
public class PlayerPickupController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference pickupAction;

    private MapData mapData;
    private TurnManager turnManager;
    private ActorGridEntity actorGridEntity;
    private ActorInventory actorInventory;
    private bool isInitialized;

    private void Awake()
    {
        actorGridEntity = GetComponent<ActorGridEntity>();
        actorInventory = GetComponent<ActorInventory>();
    }

    private void OnEnable()
    {
        if (pickupAction != null)
        {
            pickupAction.action.performed += OnPickupPerformed;
            pickupAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (pickupAction != null)
        {
            pickupAction.action.performed -= OnPickupPerformed;
            pickupAction.action.Disable();
        }
    }

    public void Initialize(MapData newMapData, TurnManager newTurnManager)
    {
        mapData = newMapData;
        turnManager = newTurnManager;
        isInitialized = true;
    }

    private void OnPickupPerformed(InputAction.CallbackContext context)
    {
        if (!isInitialized)
        {
            return;
        }

        if (GameUIState.IsInventoryOpen)
        {
            return;
        }

        if (turnManager != null && !turnManager.CanPlayerAct)
        {
            return;
        }

        TryPickupItems();
    }

    private void TryPickupItems()
    {
        List<ItemGridEntity> itemsOnGround = mapData.GetItemsAt(actorGridEntity.GridPosition);

        if (itemsOnGround.Count == 0)
        {
            GameMessageLog.Write("There is nothing here to pick up.");
            return;
        }

        int pickedUpCount = 0;

        for (int i = 0; i < itemsOnGround.Count; i++)
        {
            ItemGridEntity itemOnGround = itemsOnGround[i];

            if (itemOnGround == null)
            {
                continue;
            }

            PickUpSingleItem(itemOnGround);
            pickedUpCount++;
        }

        if (pickedUpCount <= 0)
        {
            return;
        }

        if (pickedUpCount > 1)
        {
            GameMessageLog.Write("You pick up " + pickedUpCount + " items.");
        }

        if (turnManager != null)
        {
            turnManager.PlayerTookAction();
        }
    }

    private void PickUpSingleItem(ItemGridEntity itemOnGround)
    {
        ItemInstance itemInstance = itemOnGround.CreateItemInstance();

        actorInventory.AddItem(itemInstance);

        // Clear map registration before destroying the ground item object.
        itemOnGround.ClearFromMap();
        Destroy(itemOnGround.gameObject);
    }
}