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
 * 3. Field of view refreshes.
 * 4. Enemies take turns.
 * 5. Field of view refreshes again so enemy visibility updates.
 * 6. Control returns to the player.
 *
 * A valid player action is currently:
 * - moving one tile
 * - attacking an enemy by bumping into it
 * - picking up items
 * - equipping an item
 * - using an item
 * - dropping an item
 * - opening/closing a door
 *
 * Invalid actions, such as walking into a wall, do not consume a turn.
 *
 * Important:
 * This is not a speed/energy scheduler yet.
 * Later, this can evolve into a proper roguelike time system where actions have
 * different costs and actors act according to speed.
 */

public class TurnManager : MonoBehaviour
{
    private readonly List<SimpleEnemyAI> enemies = new List<SimpleEnemyAI>();

    private MapData mapData;
    private ActorGridEntity playerActor;
    private ActorSurvival playerSurvival;
    private PlayerFieldOfView playerFieldOfView;
    private bool isProcessingEnemyTurns;

    public bool CanPlayerAct
    {
        get
        {
            return !isProcessingEnemyTurns &&
                   playerActor != null &&
                   playerActor.gameObject.activeInHierarchy;
        }
    }

    public void Initialize(MapData newMapData, ActorGridEntity newPlayerActor, IReadOnlyList<SimpleEnemyAI> newEnemies)
    {
        mapData = newMapData;
        playerActor = newPlayerActor;
        playerSurvival = null;
        playerFieldOfView = null;

        if (playerActor != null)
        {
            playerSurvival = playerActor.GetComponent<ActorSurvival>();
            playerFieldOfView = playerActor.GetComponent<PlayerFieldOfView>();
        }

        enemies.Clear();

        for (int i = 0; i < newEnemies.Count; i++)
        {
            if (newEnemies[i] != null)
            {
                enemies.Add(newEnemies[i]);
            }
        }

        RefreshPlayerFieldOfView();
    }

    public void PlayerTookAction()
    {
        if (!CanPlayerAct)
        {
            return;
        }

        ProcessPlayerActionEffects();
        RefreshPlayerFieldOfView();

        if (!CanPlayerAct)
        {
            return;
        }

        ProcessEnemyTurns();
        RefreshPlayerFieldOfView();
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

    private void RefreshPlayerFieldOfView()
    {
        if (playerFieldOfView == null)
        {
            return;
        }

        playerFieldOfView.RefreshVisibility();
    }
}