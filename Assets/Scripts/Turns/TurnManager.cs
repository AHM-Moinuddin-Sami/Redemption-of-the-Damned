using System.Collections.Generic;
using UnityEngine;

/*
 * TurnManager
 * -----------
 * Controls the basic turn flow for the roguelike prototype.
 *
 * Current turn order:
 * 1. Player performs one valid action.
 * 2. Player survival updates.
 * 3. TurnManager processes all enemy turns.
 * 4. Control returns to the player.
 *
 * A valid player action is currently:
 * - moving one tile
 * - attacking an enemy by bumping into it
 * - picking up an item
 * - equipping an item
 * - using an item
 *
 * Invalid actions, such as walking into a wall, do not consume a turn.
 *
 * Main responsibilities:
 * - know when the player is allowed to act
 * - receive a signal after the player performs a valid action
 * - update player survival after valid actions
 * - tell each enemy AI to take one turn
 * - skip dead, destroyed, or disabled enemies
 *
 * Important:
 * This is not a speed/energy scheduler yet.
 * Later, this can evolve into a proper roguelike time system where actions
 * have different costs, fast enemies act more often, and slow actions delay
 * the actor's next turn.
 */

public class TurnManager : MonoBehaviour
{
    private readonly List<SimpleEnemyAI> enemies = new List<SimpleEnemyAI>();

    private MapData mapData;
    private ActorGridEntity playerActor;
    private ActorSurvival playerSurvival;
    private bool isProcessingEnemyTurns;

    public bool CanPlayerAct
    {
        get
        {
            return !isProcessingEnemyTurns && playerActor != null && playerActor.gameObject.activeInHierarchy;
        }
    }

    public void Initialize(MapData newMapData, ActorGridEntity newPlayerActor, IReadOnlyList<SimpleEnemyAI> newEnemies)
    {
        mapData = newMapData;
        playerActor = newPlayerActor;
        playerSurvival = null;

        if (playerActor != null)
        {
            playerSurvival = playerActor.GetComponent<ActorSurvival>();
        }

        enemies.Clear();

        for (int i = 0; i < newEnemies.Count; i++)
        {
            if (newEnemies[i] != null)
            {
                enemies.Add(newEnemies[i]);
            }
        }
    }

    public void PlayerTookAction()
    {
        if (!CanPlayerAct)
        {
            return;
        }

        ProcessPlayerActionEffects();

        if (!CanPlayerAct)
        {
            return;
        }

        ProcessEnemyTurns();
    }

    private void ProcessPlayerActionEffects()
    {
        if (playerSurvival == null)
        {
            return;
        }

        playerSurvival.OnActionTaken();
    }

    private void ProcessEnemyTurns()
    {
        isProcessingEnemyTurns = true;

        for (int i = 0; i < enemies.Count; i++)
        {
            SimpleEnemyAI enemy = enemies[i];

            // Destroyed Unity objects compare as null.
            if (enemy == null)
            {
                continue;
            }

            if (!enemy.isActiveAndEnabled)
            {
                continue;
            }

            if (playerActor == null || !playerActor.gameObject.activeInHierarchy)
            {
                break;
            }

            enemy.TakeTurn();
        }

        isProcessingEnemyTurns = false;
    }
}