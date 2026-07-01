using UnityEngine;
using UnityEngine.InputSystem;

/*
 * PlayerEquippedActiveItemController
 * ----------------------------------
 * Handles using equipped active items through a gameplay hotkey.
 *
 * Current behavior:
 * - Press V to use the first ready equipped active item.
 * - The item must be equipped.
 * - If the item succeeds, a player turn is consumed.
 *
 * This is intentionally simple.
 * Later, this can become:
 * - separate hotkeys for ring/neck/trinket
 * - active ability bar
 * - radial item menu
 * - controller-friendly active item selection
 */

[RequireComponent(typeof(ActorItemUser))]
public class PlayerEquippedActiveItemController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private Key useEquippedActiveItemKey = Key.V;

    private ActorItemUser actorItemUser;
    private TurnManager turnManager;
    private bool isInitialized;

    private void Awake()
    {
        actorItemUser = GetComponent<ActorItemUser>();
    }

    public void Initialize(TurnManager newTurnManager)
    {
        turnManager = newTurnManager;
        isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized)
        {
            return;
        }

        if (GameUIState.IsGameplayInputBlocked)
        {
            return;
        }

        if (Keyboard.current == null)
        {
            return;
        }

        if (!Keyboard.current[useEquippedActiveItemKey].wasPressedThisFrame)
        {
            return;
        }

        TryUseEquippedActiveItem();
    }

    private void TryUseEquippedActiveItem()
    {
        if (actorItemUser == null)
        {
            return;
        }

        bool used = actorItemUser.TryUseFirstReadyEquippedActiveItem();

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