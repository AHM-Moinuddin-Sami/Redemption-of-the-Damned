using UnityEngine;
using UnityEngine.InputSystem;

/*
 * PlayerGridMover
 * ---------------
 * Handles tile-by-tile player movement and bump attacks.
 *
 * Current player behavior:
 * - receives movement input from the New Input System
 * - converts input into one cardinal grid direction
 * - if an actor is in the target cell, attacks that actor
 * - if the target cell is empty and walkable, moves there
 * - tells TurnManager when a valid move or attack was completed
 *
 * Main responsibilities:
 * - process movement input
 * - block movement input while inventory UI is open
 * - prevent input while enemy turns are processing
 * - trigger bump combat
 * - trigger grid movement
 * - notify TurnManager after successful movement or attack
 *
 * Important:
 * This script no longer automatically uses stairs.
 * Stairs and other map features are now handled by PlayerInteractionController.
 *
 * Later this can expand into:
 * - diagonal movement
 * - bumping doors open
 * - bump interaction options
 * - action energy costs
 */

[RequireComponent(typeof(ActorGridEntity))]
[RequireComponent(typeof(ActorCombat))]
public class PlayerGridMover : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;

    private MapData mapData;
    private TurnManager turnManager;
    private ActorGridEntity actorGridEntity;
    private ActorCombat actorCombat;
    private bool isInitialized;
    private PlayerFieldOfView playerFieldOfView;

    private void Awake()
    {
        actorGridEntity = GetComponent<ActorGridEntity>();
        actorCombat = GetComponent<ActorCombat>();
        playerFieldOfView = GetComponent<PlayerFieldOfView>();
    }

    private void OnEnable()
    {
        if (moveAction != null)
        {
            moveAction.action.performed += OnMovePerformed;
            moveAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (moveAction != null)
        {
            moveAction.action.performed -= OnMovePerformed;
            moveAction.action.Disable();
        }
    }

    public void Initialize(MapData newMapData, MapRenderer mapRenderer, Vector2Int startPosition, TurnManager newTurnManager)
    {
        mapData = newMapData;
        turnManager = newTurnManager;

        // ActorGridEntity handles map registration and position snapping.
        isInitialized = actorGridEntity.Initialize(mapData, mapRenderer, startPosition);
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
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

        Vector2 input = context.ReadValue<Vector2>();
        Vector2Int direction = GetCardinalDirection(input);

        if (direction == Vector2Int.zero)
        {
            return;
        }

        TryMoveOrAttack(direction);
    }

    private Vector2Int GetCardinalDirection(Vector2 input)
    {
        if (input.sqrMagnitude < 0.01f)
        {
            return Vector2Int.zero;
        }

        if (Mathf.Abs(input.x) >= Mathf.Abs(input.y))
        {
            if (input.x > 0f)
            {
                return Vector2Int.right;
            }

            return Vector2Int.left;
        }

        if (input.y > 0f)
        {
            return Vector2Int.up;
        }

        return Vector2Int.down;
    }

    private void TryMoveOrAttack(Vector2Int direction)
    {
        Vector2Int targetPosition = actorGridEntity.GridPosition + direction;
        ActorGridEntity blockingActor = mapData.GetActorAt(targetPosition);

        if (blockingActor != null)
        {
            bool attacked = actorCombat.Attack(blockingActor);

            if (attacked)
            {
                NotifyTurnManager();
            }

            return;
        }

        bool moved = actorGridEntity.TryMove(direction);

        if (moved)
        {
            RefreshFieldOfView();
            NotifyTurnManager();
        }
    }

    private void NotifyTurnManager()
    {
        if (turnManager == null)
        {
            return;
        }

        turnManager.PlayerTookAction();
    }

    private void RefreshFieldOfView()
    {
        if (playerFieldOfView == null)
        {
            return;
        }

        playerFieldOfView.RefreshVisibility();
    }
}