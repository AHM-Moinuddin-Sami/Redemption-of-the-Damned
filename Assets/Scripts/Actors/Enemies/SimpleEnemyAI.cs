using System.Collections.Generic;
using UnityEngine;

/*
 * SimpleEnemyAI
 * -------------
 * Gives a basic enemy one turn of behavior whenever the TurnManager asks it to act.
 *
 * Current enemy behavior:
 * - If the player is adjacent, attack the player.
 * - If the player is within detection range, move one tile toward the player.
 * - If blocked, try the secondary direction.
 * - If still blocked, do nothing.
 *
 * This is intentionally simple. It is not proper pathfinding yet.
 *
 * Main responsibilities:
 * - store a reference to MapData
 * - store a reference to the player actor
 * - decide whether to attack, move, or wait
 * - use ActorGridEntity for movement
 * - use ActorCombat for attacking
 *
 * Later this can expand into:
 * - pathfinding
 * - field of view checks
 * - faction behavior
 * - fleeing
 * - ranged attacks
 * - ability usage
 * - patrol/wander behavior
 * - different AI profiles per enemy type
 */

[RequireComponent(typeof(ActorGridEntity))]
[RequireComponent(typeof(ActorCombat))]
public class SimpleEnemyAI : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private int detectionRange = 10;

    private MapData mapData;
    private ActorGridEntity actorGridEntity;
    private ActorCombat actorCombat;
    private ActorGridEntity playerActor;
    private bool isInitialized;

    private void Awake()
    {
        actorGridEntity = GetComponent<ActorGridEntity>();
        actorCombat = GetComponent<ActorCombat>();
    }

    public void Initialize(MapData newMapData, ActorGridEntity newPlayerActor)
    {
        mapData = newMapData;
        playerActor = newPlayerActor;
        isInitialized = true;
    }

    public void TakeTurn()
    {
        if (!isInitialized)
        {
            return;
        }

        if (playerActor == null || !playerActor.gameObject.activeInHierarchy)
        {
            return;
        }

        int distanceToPlayer = GetManhattanDistance(actorGridEntity.GridPosition, playerActor.GridPosition);

        if (distanceToPlayer == 1)
        {
            actorCombat.Attack(playerActor);
            return;
        }

        if (distanceToPlayer > detectionRange)
        {
            return;
        }

        TryMoveTowardPlayer();
    }

    private void TryMoveTowardPlayer()
    {
        List<Vector2Int> preferredDirections = GetPreferredDirectionsToPlayer();

        for (int i = 0; i < preferredDirections.Count; i++)
        {
            Vector2Int direction = preferredDirections[i];
            Vector2Int targetPosition = actorGridEntity.GridPosition + direction;

            ActorGridEntity blockingActor = mapData.GetActorAt(targetPosition);

            // If the target tile is the player, attack instead of moving.
            if (blockingActor == playerActor)
            {
                actorCombat.Attack(playerActor);
                return;
            }

            // Do not move into other enemies or blocked actor cells.
            if (blockingActor != null)
            {
                continue;
            }

            if (actorGridEntity.TryMove(direction))
            {
                return;
            }
        }
    }

    private List<Vector2Int> GetPreferredDirectionsToPlayer()
    {
        List<Vector2Int> directions = new List<Vector2Int>();

        Vector2Int offset = playerActor.GridPosition - actorGridEntity.GridPosition;

        Vector2Int horizontalDirection = Vector2Int.zero;
        Vector2Int verticalDirection = Vector2Int.zero;

        if (offset.x > 0)
        {
            horizontalDirection = Vector2Int.right;
        }
        else if (offset.x < 0)
        {
            horizontalDirection = Vector2Int.left;
        }

        if (offset.y > 0)
        {
            verticalDirection = Vector2Int.up;
        }
        else if (offset.y < 0)
        {
            verticalDirection = Vector2Int.down;
        }

        // Try the strongest axis first.
        // Example: if the player is much farther horizontally, move horizontally first.
        if (Mathf.Abs(offset.x) >= Mathf.Abs(offset.y))
        {
            AddDirectionIfValid(directions, horizontalDirection);
            AddDirectionIfValid(directions, verticalDirection);
        }
        else
        {
            AddDirectionIfValid(directions, verticalDirection);
            AddDirectionIfValid(directions, horizontalDirection);
        }

        return directions;
    }

    private void AddDirectionIfValid(List<Vector2Int> directions, Vector2Int direction)
    {
        if (direction == Vector2Int.zero)
        {
            return;
        }

        directions.Add(direction);
    }

    private int GetManhattanDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}