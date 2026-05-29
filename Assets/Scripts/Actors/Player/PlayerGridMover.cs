using UnityEngine;
using UnityEngine.InputSystem;

/*
 * PlayerGridMover
 * ---------------
 * Handles tile-by-tile player movement and bump attacks.
 *
 * This version emits movement noise based on the player's stealth state.
 *
 * Current player behavior:
 * - receives movement input from the New Input System
 * - converts input into one cardinal grid direction
 * - if an actor is in the target cell, attacks that actor
 * - if the target cell is empty and walkable, moves there
 * - refreshes FOV after moving
 * - emits footstep noise after moving
 * - lowers footstep noise while sneaking
 * - tells TurnManager when a valid move or attack was completed
 *
 * Important:
 * Sneaking does not currently slow movement.
 * It only reduces movement noise and enemy detection range through ActorStealth.
 */

[RequireComponent(typeof(ActorGridEntity))]
[RequireComponent(typeof(ActorCombat))]
public class PlayerGridMover : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;

    [Header("Noise")]
    [SerializeField] private int movementNoiseRange = 3;

    private MapData mapData;
    private TurnManager turnManager;
    private ActorGridEntity actorGridEntity;
    private ActorCombat actorCombat;
    private PlayerFieldOfView playerFieldOfView;
    private ActorStealth actorStealth;
    private bool isInitialized;

    private void Awake()
    {
        actorGridEntity = GetComponent<ActorGridEntity>();
        actorCombat = GetComponent<ActorCombat>();
        playerFieldOfView = GetComponent<PlayerFieldOfView>();
        actorStealth = GetComponent<ActorStealth>();
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
            EmitMovementNoise();
            RefreshFieldOfView();
            NotifyTurnManager();
        }
    }

    private void EmitMovementNoise()
    {
        int finalNoiseRange = movementNoiseRange;

        if (actorStealth != null)
        {
            finalNoiseRange = actorStealth.GetModifiedMovementNoiseRange(movementNoiseRange);
        }

        GameNoiseSystem.EmitNoise(
            actorGridEntity.GridPosition,
            finalNoiseRange,
            actorGridEntity,
            NoiseCategory.Movement
        );
    }

    private void RefreshFieldOfView()
    {
        if (playerFieldOfView == null)
        {
            return;
        }

        playerFieldOfView.RefreshVisibility();
    }

    private void NotifyTurnManager()
    {
        if (turnManager == null)
        {
            return;
        }

        turnManager.PlayerTookAction();
    }
}