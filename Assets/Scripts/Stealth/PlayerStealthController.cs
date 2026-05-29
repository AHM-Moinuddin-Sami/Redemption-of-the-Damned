using UnityEngine;
using UnityEngine.InputSystem;

/*
 * PlayerStealthController
 * -----------------------
 * Handles the player's sneak toggle input.
 *
 * Current behavior:
 * - listens for a sneak toggle input action
 * - ignores input while the inventory UI is open
 * - toggles ActorStealth sneaking state
 * - writes a message to the message log
 *
 * Current gameplay effect:
 * - sneaking lowers movement noise
 * - sneaking reduces enemy detection range
 *
 * Important:
 * Toggling sneak does not consume a turn by default.
 * This keeps the control responsive and avoids making the player spend a turn
 * just to enter or leave sneaking mode.
 *
 * Later this can expand into:
 * - turn cost for changing stance
 * - stamina cost
 * - animation changes
 * - speed/action cost changes
 * - automatic stealth break when attacking
 */

[RequireComponent(typeof(ActorStealth))]
public class PlayerStealthController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference toggleSneakAction;

    [Header("Turn Cost")]
    [SerializeField] private bool toggleConsumesTurn = false;

    private ActorStealth actorStealth;
    private TurnManager turnManager;
    private bool isInitialized;

    private void Awake()
    {
        actorStealth = GetComponent<ActorStealth>();
    }

    private void OnEnable()
    {
        if (toggleSneakAction != null)
        {
            toggleSneakAction.action.performed += OnToggleSneakPerformed;
            toggleSneakAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (toggleSneakAction != null)
        {
            toggleSneakAction.action.performed -= OnToggleSneakPerformed;
            toggleSneakAction.action.Disable();
        }
    }

    public void Initialize(TurnManager newTurnManager)
    {
        turnManager = newTurnManager;
        isInitialized = true;
    }

    private void OnToggleSneakPerformed(InputAction.CallbackContext context)
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

        ToggleSneaking();
    }

    private void ToggleSneaking()
    {
        actorStealth.ToggleSneaking();

        if (actorStealth.IsSneaking)
        {
            GameMessageLog.Write("You begin sneaking.");
        }
        else
        {
            GameMessageLog.Write("You stop sneaking.");
        }

        if (toggleConsumesTurn && turnManager != null)
        {
            turnManager.PlayerTookAction();
        }
    }
}