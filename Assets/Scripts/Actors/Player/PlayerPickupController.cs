using UnityEngine;
using UnityEngine.InputSystem;

/*
 * PlayerPickupController
 * ----------------------
 * Handles picking up items from the player's current grid cell.
 *
 * This script now sends empty-tile feedback to GameMessageLog.
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

        if (turnManager != null && !turnManager.CanPlayerAct)
        {
            return;
        }

        TryPickupItem();
    }

    private void TryPickupItem()
    {
        ItemGridEntity itemOnGround = mapData.GetTopItemAt(actorGridEntity.GridPosition);

        if (itemOnGround == null)
        {
            GameMessageLog.Write("There is nothing here to pick up.");
            return;
        }

        ItemInstance itemInstance = itemOnGround.CreateItemInstance();

        actorInventory.AddItem(itemInstance);

        itemOnGround.ClearFromMap();
        Destroy(itemOnGround.gameObject);

        if (turnManager != null)
        {
            turnManager.PlayerTookAction();
        }
    }
}