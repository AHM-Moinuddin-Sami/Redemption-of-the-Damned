using UnityEngine;
using UnityEngine.InputSystem;

/*
 * PlayerItemUseController
 * -----------------------
 * Handles temporary keyboard-based consumable use for the player.
 *
 * This script now sends no-consumable feedback to GameMessageLog.
 */

[RequireComponent(typeof(ActorInventory))]
[RequireComponent(typeof(ActorItemUser))]
public class PlayerItemUseController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference useFirstConsumableAction;

    private ActorInventory actorInventory;
    private ActorItemUser actorItemUser;
    private TurnManager turnManager;
    private bool isInitialized;

    private void Awake()
    {
        actorInventory = GetComponent<ActorInventory>();
        actorItemUser = GetComponent<ActorItemUser>();
    }

    private void OnEnable()
    {
        if (useFirstConsumableAction != null)
        {
            useFirstConsumableAction.action.performed += OnUseFirstConsumablePerformed;
            useFirstConsumableAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (useFirstConsumableAction != null)
        {
            useFirstConsumableAction.action.performed -= OnUseFirstConsumablePerformed;
            useFirstConsumableAction.action.Disable();
        }
    }

    public void Initialize(TurnManager newTurnManager)
    {
        turnManager = newTurnManager;
        isInitialized = true;
    }

    private void OnUseFirstConsumablePerformed(InputAction.CallbackContext context)
    {
        if (!isInitialized)
        {
            return;
        }

        if (turnManager != null && !turnManager.CanPlayerAct)
        {
            return;
        }

        TryUseFirstConsumable();
    }

    private void TryUseFirstConsumable()
    {
        ItemInstance consumableItem = actorInventory.GetFirstConsumableItem();

        if (consumableItem == null)
        {
            GameMessageLog.Write("There is no consumable item in the inventory.");
            return;
        }

        bool used = actorItemUser.TryUseItem(consumableItem);

        if (!used)
        {
            return;
        }

        if (turnManager != null)
        {
            turnManager.PlayerTookAction();
        }
    }
}