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
 * 5. Field of view refreshes again.
 * 6. Control returns to the player.
 *
 * This version also respects game over state.
 * If GameUIState.IsGameOver is true, the player can no longer act and enemy
 * turn processing stops mattering.
 */

public class TurnManager : MonoBehaviour
{
    private readonly List<SimpleEnemyAI> enemies = new List<SimpleEnemyAI>();

    private MapData mapData;
    private ActorGridEntity playerActor;
    private ActorSurvival playerSurvival;
    private PlayerFieldOfView playerFieldOfView;
    private ActorItemUser playerItemUser;
    private bool isProcessingEnemyTurns;

    public bool CanPlayerAct
    {
        get
        {
            return !GameUIState.IsGameOver &&
                   !isProcessingEnemyTurns &&
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
        isProcessingEnemyTurns = false;

        playerItemUser = null;

        if (playerActor != null)
        {
            playerSurvival = playerActor.GetComponent<ActorSurvival>();
            playerFieldOfView = playerActor.GetComponent<PlayerFieldOfView>();
            playerItemUser = playerActor.GetComponent<ActorItemUser>();
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

        if (!CanPlayerAct)
        {
            return;
        }

        RefreshPlayerFieldOfView();

        if (!CanPlayerAct)
        {
            return;
        }

        ProcessEnemyTurns();
        RefreshPlayerFieldOfView();
        ProcessEndOfPlayerActionEffects();
    }

    private void ProcessPlayerActionEffects()
    {
        if (playerSurvival == null)
        {
            return;
        }

        playerSurvival.OnActionTaken();
    }

    private void ProcessEndOfPlayerActionEffects()
    {
        if (playerItemUser == null)
        {
            return;
        }

        playerItemUser.OnPlayerActionCompleted();
    }

    private void ProcessEnemyTurns()
    {
        if (GameUIState.IsGameOver)
        {
            return;
        }

        isProcessingEnemyTurns = true;

        for (int i = 0; i < enemies.Count; i++)
        {
            if (GameUIState.IsGameOver)
            {
                break;
            }

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