using System.Collections.Generic;
using UnityEngine;

/*
 * SimpleEnemyAI
 * -------------
 * Gives a basic enemy one turn of behavior whenever TurnManager asks it to act.
 *
 * This version supports:
 * - line-of-sight player detection
 * - chasing the player while visible
 * - remembering the player's last known position
 * - giving up after losing sight for several turns
 * - wandering while unaware
 *
 * Current enemy behavior:
 * 1. If adjacent to the player, attack.
 * 2. If the player is within detection range and line of sight, become aware.
 * 3. If aware, chase the player or last known player position.
 * 4. If unaware, randomly wander near the enemy's starting position.
 *
 * Main responsibilities:
 * - detect the player fairly using range and line of sight
 * - path toward the player or last known player position
 * - wander when idle
 * - avoid walking into walls, closed doors, blocking features, and other actors
 *
 * Important:
 * Closed doors block enemy sight and pathing.
 * Enemies do not open doors yet.
 *
 * Later this can expand into:
 * - proper patrol routes
 * - enemies opening doors
 * - sound-based investigation
 * - group alerting
 * - ranged attacks
 * - fleeing
 * - faction behavior
 * - stealth/sneaking
 */

[RequireComponent(typeof(ActorGridEntity))]
[RequireComponent(typeof(ActorCombat))]
public class SimpleEnemyAI : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private int detectionRange = 10;
    [SerializeField] private int chaseTurnsAfterLosingSight = 4;

    [Header("Wandering")]
    [SerializeField] private bool canWanderWhileUnaware = true;
    [SerializeField] private int wanderChancePercent = 45;
    [SerializeField] private int maxWanderDistanceFromHome = 6;
    [SerializeField] private int wanderDirectionAttempts = 4;

    [Header("Debug")]
    [SerializeField] private bool printAwarenessDebug;

    private readonly Vector2Int[] cardinalDirections =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    private MapData mapData;
    private ActorGridEntity actorGridEntity;
    private ActorCombat actorCombat;
    private ActorGridEntity playerActor;

    private Vector2Int homePosition;
    private Vector2Int lastKnownPlayerPosition;

    private int turnsSincePlayerSeen;
    private bool hasLastKnownPlayerPosition;
    private bool isAware;
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

        homePosition = actorGridEntity.GridPosition;
        lastKnownPlayerPosition = Vector2Int.zero;

        hasLastKnownPlayerPosition = false;
        isAware = false;
        turnsSincePlayerSeen = 0;

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

        if (TryAttackAdjacentPlayer())
        {
            return;
        }

        UpdateAwareness();

        if (isAware)
        {
            ActWhileAware();
            return;
        }

        ActWhileUnaware();
    }

    private bool TryAttackAdjacentPlayer()
    {
        int distanceToPlayer = GetManhattanDistance(actorGridEntity.GridPosition, playerActor.GridPosition);

        if (distanceToPlayer != 1)
        {
            return false;
        }

        actorCombat.Attack(playerActor);

        // If the enemy is close enough to attack, it definitely knows where the player is.
        RememberPlayerPosition();

        isAware = true;
        turnsSincePlayerSeen = 0;

        return true;
    }

    private void UpdateAwareness()
    {
        if (CanCurrentlySeePlayer())
        {
            if (!isAware && printAwarenessDebug)
            {
                Debug.Log(actorGridEntity.DisplayName + " sees the player.");
            }

            isAware = true;
            turnsSincePlayerSeen = 0;

            RememberPlayerPosition();
            return;
        }

        if (!isAware)
        {
            return;
        }

        turnsSincePlayerSeen++;

        if (turnsSincePlayerSeen > chaseTurnsAfterLosingSight)
        {
            if (printAwarenessDebug)
            {
                Debug.Log(actorGridEntity.DisplayName + " lost the player.");
            }

            isAware = false;
            hasLastKnownPlayerPosition = false;
        }
    }

    private bool CanCurrentlySeePlayer()
    {
        int distanceToPlayer = GetManhattanDistance(actorGridEntity.GridPosition, playerActor.GridPosition);

        if (distanceToPlayer > detectionRange)
        {
            return false;
        }

        return GridLineOfSight.HasLineOfSight(
            mapData,
            actorGridEntity.GridPosition,
            playerActor.GridPosition
        );
    }

    private void RememberPlayerPosition()
    {
        lastKnownPlayerPosition = playerActor.GridPosition;
        hasLastKnownPlayerPosition = true;
    }

    private void ActWhileAware()
    {
        if (CanCurrentlySeePlayer())
        {
            TryPathToward(playerActor.GridPosition, true);
            return;
        }

        if (!hasLastKnownPlayerPosition)
        {
            return;
        }

        if (actorGridEntity.GridPosition == lastKnownPlayerPosition)
        {
            isAware = false;
            hasLastKnownPlayerPosition = false;
            return;
        }

        TryPathToward(lastKnownPlayerPosition, false);
    }

    private void ActWhileUnaware()
    {
        if (!canWanderWhileUnaware)
        {
            return;
        }

        int safeWanderChance = Mathf.Clamp(wanderChancePercent, 0, 100);
        int roll = Random.Range(0, 100);

        if (roll >= safeWanderChance)
        {
            return;
        }

        TryWander();
    }

    private void TryWander()
    {
        int safeAttempts = Mathf.Max(1, wanderDirectionAttempts);

        for (int i = 0; i < safeAttempts; i++)
        {
            Vector2Int direction = GetRandomCardinalDirection();
            Vector2Int targetPosition = actorGridEntity.GridPosition + direction;

            if (!CanWanderTo(targetPosition))
            {
                continue;
            }

            actorGridEntity.TryMove(direction);
            return;
        }
    }

    private bool CanWanderTo(Vector2Int targetPosition)
    {
        if (mapData == null)
        {
            return false;
        }

        if (!mapData.IsWalkable(targetPosition))
        {
            return false;
        }

        // Keep idle wandering near the enemy's spawn point.
        // This prevents enemies from randomly drifting across the whole dungeon.
        int distanceFromHome = GetManhattanDistance(homePosition, targetPosition);

        if (distanceFromHome > maxWanderDistanceFromHome)
        {
            return false;
        }

        return true;
    }

    private void TryPathToward(Vector2Int targetPosition, bool targetIsPlayer)
    {
        List<Vector2Int> path;

        bool foundPath = GridPathfinder.TryFindPath(
            mapData,
            actorGridEntity.GridPosition,
            targetPosition,
            detectionRange + chaseTurnsAfterLosingSight,
            out path
        );

        if (!foundPath)
        {
            return;
        }

        if (path.Count < 2)
        {
            return;
        }

        Vector2Int nextStep = path[1];

        if (targetIsPlayer)
        {
            ActorGridEntity actorAtNextStep = mapData.GetActorAt(nextStep);

            if (actorAtNextStep == playerActor)
            {
                actorCombat.Attack(playerActor);
                return;
            }
        }

        TryMoveToNextStep(nextStep);
    }

    private void TryMoveToNextStep(Vector2Int nextStep)
    {
        ActorGridEntity actorAtNextStep = mapData.GetActorAt(nextStep);

        if (actorAtNextStep != null)
        {
            return;
        }

        Vector2Int direction = nextStep - actorGridEntity.GridPosition;

        actorGridEntity.TryMove(direction);
    }

    private Vector2Int GetRandomCardinalDirection()
    {
        int index = Random.Range(0, cardinalDirections.Length);
        return cardinalDirections[index];
    }

    private int GetManhattanDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}