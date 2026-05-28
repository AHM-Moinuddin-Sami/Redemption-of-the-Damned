using UnityEngine;
using UnityEngine.InputSystem;

/*
 * PlayerWaitController
 * --------------------
 * Handles the player's wait/skip-turn action.
 *
 * Waiting is important for a traditional roguelike because it allows the player
 * to intentionally spend one turn without moving, attacking, picking up items,
 * or interacting with anything.
 *
 * Current behavior:
 * - listens for a Wait input action
 * - ignores input while inventory UI is open
 * - ignores input while enemies are taking turns
 * - writes a message to the message log
 * - tells TurnManager that the player took a valid action
 *
 * Waiting currently causes:
 * - hunger/thirst to update
 * - enemies to act
 * - field of view to refresh
 * - object visibility to update through the existing FOV system
 *
 * Important:
 * This does not heal or rest by itself.
 * It only spends one normal turn.
 *
 * Later this can expand into:
 * - hold-to-wait multiple turns
 * - rest until healed
 * - interrupt rest when enemy appears
 * - wait until enemy acts
 * - wait until hunger/thirst warning
 */

public class PlayerWaitController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference waitAction;

    private TurnManager turnManager;
    private bool isInitialized;

    private void OnEnable()
    {
        if (waitAction != null)
        {
            waitAction.action.performed += OnWaitPerformed;
            waitAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (waitAction != null)
        {
            waitAction.action.performed -= OnWaitPerformed;
            waitAction.action.Disable();
        }
    }

    public void Initialize(TurnManager newTurnManager)
    {
        turnManager = newTurnManager;
        isInitialized = true;
    }

    private void OnWaitPerformed(InputAction.CallbackContext context)
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

        WaitOneTurn();
    }

    private void WaitOneTurn()
    {
        GameMessageLog.Write("You wait.");

        if (turnManager != null)
        {
            turnManager.PlayerTookAction();
        }
    }
}