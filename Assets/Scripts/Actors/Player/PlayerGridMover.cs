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
 * - if the player steps onto stairs, triggers the next floor
 * - otherwise tells TurnManager when a valid action was completed
 *
 * Main responsibilities:
 * - process player movement input
 * - prevent input while enemy turns are processing
 * - trigger bump combat
 * - trigger player movement
 * - check for features after movement
 * - notify TurnManager after a successful normal move or attack
 *
 * Important:
 * Invalid movement into walls or empty space does not consume a turn.
 * Stepping onto stairs changes floor immediately and does not process enemy turns.
 *
 * Later this can expand into:
 * - requiring an interact key for stairs
 * - interacting with doors
 * - picking up items
 * - waiting/skipping a turn
 * - diagonal movement
 * - ability targeting
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

    private void Awake()
    {
        actorGridEntity = GetComponent<ActorGridEntity>();
        actorCombat = GetComponent<ActorCombat>();
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

        if (!moved)
        {
            return;
        }

        // After a successful move, check whether the player stepped onto a feature.
        // Stairs currently trigger immediately.
        if (TryUseFeatureAtCurrentPosition())
        {
            return;
        }

        NotifyTurnManager();
    }

    private bool TryUseFeatureAtCurrentPosition()
    {
        MapFeatureEntity feature = mapData.GetFeatureAt(actorGridEntity.GridPosition);

        if (feature == null)
        {
            return false;
        }

        StairsDownFeature stairsDownFeature = feature.GetComponent<StairsDownFeature>();

        if (stairsDownFeature == null)
        {
            return false;
        }

        stairsDownFeature.Use();
        return true;
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