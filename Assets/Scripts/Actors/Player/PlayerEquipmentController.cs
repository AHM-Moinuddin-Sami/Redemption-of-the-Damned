using UnityEngine;
using UnityEngine.InputSystem;

/*
 * PlayerEquipmentController
 * -------------------------
 * Handles temporary keyboard-based equipment testing for the player.
 *
 * This script now sends no-equipment feedback to GameMessageLog.
 */

[RequireComponent(typeof(ActorInventory))]
[RequireComponent(typeof(ActorEquipment))]
public class PlayerEquipmentController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference equipFirstEquipmentAction;

    private ActorInventory actorInventory;
    private ActorEquipment actorEquipment;
    private TurnManager turnManager;
    private bool isInitialized;

    private void Awake()
    {
        actorInventory = GetComponent<ActorInventory>();
        actorEquipment = GetComponent<ActorEquipment>();
    }

    private void OnEnable()
    {
        if (equipFirstEquipmentAction != null)
        {
            equipFirstEquipmentAction.action.performed += OnEquipFirstEquipmentPerformed;
            equipFirstEquipmentAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (equipFirstEquipmentAction != null)
        {
            equipFirstEquipmentAction.action.performed -= OnEquipFirstEquipmentPerformed;
            equipFirstEquipmentAction.action.Disable();
        }
    }

    public void Initialize(TurnManager newTurnManager)
    {
        turnManager = newTurnManager;
        isInitialized = true;
    }

    private void OnEquipFirstEquipmentPerformed(InputAction.CallbackContext context)
    {
        if (!isInitialized)
        {
            return;
        }

        if (turnManager != null && !turnManager.CanPlayerAct)
        {
            return;
        }

        TryEquipFirstEquipmentItem();
    }

    private void TryEquipFirstEquipmentItem()
    {
        ItemInstance equipmentItem = actorInventory.GetFirstEquipmentItem();

        if (equipmentItem == null)
        {
            GameMessageLog.Write("There is no equipment item in the inventory.");
            return;
        }

        bool equipped = actorEquipment.TryEquip(equipmentItem, actorInventory);

        if (!equipped)
        {
            return;
        }

        if (turnManager != null)
        {
            turnManager.PlayerTookAction();
        }
    }
}