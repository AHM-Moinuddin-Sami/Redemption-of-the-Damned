using System.Collections.Generic;
using UnityEngine;

/*
 * SimpleEnemyAI
 * -------------
 * Gives a basic enemy one turn of behavior whenever TurnManager asks it to act.
 *
 * This version supports:
 * - line-of-sight player detection
 * - reduced detection range against sneaking targets
 * - chasing the player while visible
 * - remembering the player's last known position
 * - investigating heard noises
 * - giving up after losing sight or reaching the investigation point
 * - wandering while unaware
 * - opening closed doors while chasing/investigating
 *
 * Current stealth behavior:
 * - if the player is sneaking, this enemy's detection range is reduced
 * - line of sight is still required
 * - sound can still reveal the player's approximate position
 *
 * Important:
 * Sneaking is not invisibility.
 * If the player is very close and in line of sight, the enemy can still detect them.
 */

[RequireComponent(typeof(ActorGridEntity))]
[RequireComponent(typeof(ActorCombat))]
public class SimpleEnemyAI : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private int detectionRange = 10;
    [SerializeField] private int chaseTurnsAfterLosingSight = 4;

    [Header("Sound")]
    [SerializeField] private bool canHearNoise = true;
    [SerializeField] private int investigationTurnsAfterHearingNoise = 6;

    [Header("Doors")]
    [SerializeField] private bool canOpenDoorsWhileAware = true;

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
    private ActorStealth playerStealth;

    private Vector2Int homePosition;
    private Vector2Int lastKnownTargetPosition;

    private int turnsSincePlayerSeen;
    private int turnsSinceNoiseHeard;

    private bool hasLastKnownTargetPosition;
    private bool isAware;
    private bool isInvestigatingNoise;
    private bool isInitialized;

    private void Awake()
    {
        actorGridEntity = GetComponent<ActorGridEntity>();
        actorCombat = GetComponent<ActorCombat>();
    }

    private void OnEnable()
    {
        GameNoiseSystem.NoiseEmitted += OnNoiseEmitted;
    }

    private void OnDisable()
    {
        GameNoiseSystem.NoiseEmitted -= OnNoiseEmitted;
    }

    public void Initialize(MapData newMapData, ActorGridEntity newPlayerActor)
    {
        mapData = newMapData;
        playerActor = newPlayerActor;
        playerStealth = null;

        if (playerActor != null)
        {
            playerStealth = playerActor.GetComponent<ActorStealth>();
        }

        homePosition = actorGridEntity.GridPosition;
        lastKnownTargetPosition = Vector2Int.zero;

        turnsSincePlayerSeen = 0;
        turnsSinceNoiseHeard = 0;

        hasLastKnownTargetPosition = false;
        isAware = false;
        isInvestigatingNoise = false;

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

    private void OnNoiseEmitted(GameNoiseEvent noiseEvent)
    {
        if (!isInitialized)
        {
            return;
        }

        if (!canHearNoise)
        {
            return;
        }

        if (noiseEvent == null)
        {
            return;
        }

        if (noiseEvent.SourceActor == actorGridEntity)
        {
            return;
        }

        int distanceToNoise = GetManhattanDistance(actorGridEntity.GridPosition, noiseEvent.Position);

        if (distanceToNoise > noiseEvent.NoiseRange)
        {
            return;
        }

        if (CanCurrentlySeePlayer())
        {
            return;
        }

        HearNoise(noiseEvent);
    }

    private void HearNoise(GameNoiseEvent noiseEvent)
    {
        isAware = true;
        isInvestigatingNoise = true;
        turnsSinceNoiseHeard = 0;

        lastKnownTargetPosition = noiseEvent.Position;
        hasLastKnownTargetPosition = true;

        if (printAwarenessDebug)
        {
            Debug.Log(actorGridEntity.DisplayName + " heard " + noiseEvent.Category + " at " + noiseEvent.Position + ".");
        }
    }

    private bool TryAttackAdjacentPlayer()
    {
        int distanceToPlayer = GetManhattanDistance(actorGridEntity.GridPosition, playerActor.GridPosition);

        if (distanceToPlayer != 1)
        {
            return false;
        }

        actorCombat.Attack(playerActor);

        RememberPlayerPosition();

        isAware = true;
        isInvestigatingNoise = false;
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
            isInvestigatingNoise = false;
            turnsSincePlayerSeen = 0;

            RememberPlayerPosition();
            return;
        }

        if (!isAware)
        {
            return;
        }

        if (isInvestigatingNoise)
        {
            UpdateNoiseInvestigationTimer();
            return;
        }

        UpdateLostSightTimer();
    }

    private void UpdateLostSightTimer()
    {
        turnsSincePlayerSeen++;

        if (turnsSincePlayerSeen > chaseTurnsAfterLosingSight)
        {
            if (printAwarenessDebug)
            {
                Debug.Log(actorGridEntity.DisplayName + " lost the player.");
            }

            ClearAwareness();
        }
    }

    private void UpdateNoiseInvestigationTimer()
    {
        turnsSinceNoiseHeard++;

        if (turnsSinceNoiseHeard > investigationTurnsAfterHearingNoise)
        {
            if (printAwarenessDebug)
            {
                Debug.Log(actorGridEntity.DisplayName + " stopped investigating noise.");
            }

            ClearAwareness();
        }
    }

    private bool CanCurrentlySeePlayer()
    {
        int distanceToPlayer = GetManhattanDistance(actorGridEntity.GridPosition, playerActor.GridPosition);
        int effectiveDetectionRange = GetEffectiveDetectionRangeAgainstPlayer();

        if (distanceToPlayer > effectiveDetectionRange)
        {
            return false;
        }

        return GridLineOfSight.HasLineOfSight(
            mapData,
            actorGridEntity.GridPosition,
            playerActor.GridPosition
        );
    }

    private int GetEffectiveDetectionRangeAgainstPlayer()
    {
        if (playerStealth == null)
        {
            return detectionRange;
        }

        return playerStealth.GetDetectionRangeAgainstActor(detectionRange);
    }

    private void RememberPlayerPosition()
    {
        lastKnownTargetPosition = playerActor.GridPosition;
        hasLastKnownTargetPosition = true;
    }

    private void ActWhileAware()
    {
        if (CanCurrentlySeePlayer())
        {
            TryPathToward(playerActor.GridPosition, true);
            return;
        }

        if (!hasLastKnownTargetPosition)
        {
            ClearAwareness();
            return;
        }

        if (actorGridEntity.GridPosition == lastKnownTargetPosition)
        {
            ClearAwareness();
            return;
        }

        TryPathToward(lastKnownTargetPosition, false);
    }

    private void ClearAwareness()
    {
        isAware = false;
        isInvestigatingNoise = false;
        hasLastKnownTargetPosition = false;

        turnsSincePlayerSeen = 0;
        turnsSinceNoiseHeard = 0;
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
            detectionRange + chaseTurnsAfterLosingSight + investigationTurnsAfterHearingNoise,
            canOpenDoorsWhileAware,
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

        if (TryOpenDoorAtNextStep(nextStep))
        {
            return;
        }

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

    private bool TryOpenDoorAtNextStep(Vector2Int nextStep)
    {
        if (!canOpenDoorsWhileAware)
        {
            return false;
        }

        DoorFeature door = mapData.GetDoorAt(nextStep);

        if (door == null)
        {
            return false;
        }

        if (door.IsOpen)
        {
            return false;
        }

        return door.TryOpen("A door opens.", "You hear a door open.", actorGridEntity);
    }

    public void ApplyEnemyDefinition(EnemyDefinition enemyDefinition)
    {
        if (enemyDefinition == null)
        {
            return;
        }

        detectionRange = enemyDefinition.DetectionRange;
        chaseTurnsAfterLosingSight = enemyDefinition.ChaseTurnsAfterLosingSight;

        canHearNoise = enemyDefinition.CanHearNoise;
        investigationTurnsAfterHearingNoise = enemyDefinition.InvestigationTurnsAfterHearingNoise;

        canOpenDoorsWhileAware = enemyDefinition.CanOpenDoorsWhileAware;

        canWanderWhileUnaware = enemyDefinition.CanWanderWhileUnaware;
        wanderChancePercent = enemyDefinition.WanderChancePercent;
        maxWanderDistanceFromHome = enemyDefinition.MaxWanderDistanceFromHome;
        wanderDirectionAttempts = enemyDefinition.WanderDirectionAttempts;
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