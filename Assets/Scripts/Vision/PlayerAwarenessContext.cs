using UnityEngine;

/*
 * PlayerAwarenessContext
 * ----------------------
 * Stores the current player's awareness references for systems that need to know
 * what the player can see or hear.
 *
 * This is a lightweight prototype service.
 *
 * Current responsibilities:
 * - store the current player ActorGridEntity
 * - store the current PlayerFieldOfView
 * - answer whether a grid cell is currently visible
 * - answer whether a grid cell is close enough to be heard
 * - answer whether an actor is the player
 *
 * Why this exists:
 * Combat, death, and door messages should not always reveal exact information.
 * If an enemy opens a door outside the player's sight, the message should be
 * vague or silent instead of always saying exactly what happened.
 *
 * Important:
 * This class does not calculate field of view.
 * PlayerFieldOfView still owns the actual visibility data.
 *
 * Later this can expand into:
 * - noise levels
 * - stealth checks
 * - perception stats
 * - blindness/deafness effects
 * - faction-specific knowledge
 * - message categories
 */

public static class PlayerAwarenessContext
{
    private static ActorGridEntity playerActor;
    private static PlayerFieldOfView playerFieldOfView;

    public static void SetPlayer(ActorGridEntity newPlayerActor, PlayerFieldOfView newPlayerFieldOfView)
    {
        playerActor = newPlayerActor;
        playerFieldOfView = newPlayerFieldOfView;
    }

    public static void Clear()
    {
        playerActor = null;
        playerFieldOfView = null;
    }

    public static bool IsPlayer(ActorGridEntity actor)
    {
        return actor != null && actor == playerActor;
    }

    public static bool CanSee(Vector2Int position)
    {
        if (playerFieldOfView == null)
        {
            return false;
        }

        return playerFieldOfView.IsCellVisible(position);
    }

    public static bool CanSeeActor(ActorGridEntity actor)
    {
        if (actor == null)
        {
            return false;
        }

        if (IsPlayer(actor))
        {
            return true;
        }

        return CanSee(actor.GridPosition);
    }

    public static bool CanHear(Vector2Int position, int hearingRange)
    {
        if (playerActor == null)
        {
            return false;
        }

        int safeRange = Mathf.Max(0, hearingRange);
        int distance = Mathf.Abs(playerActor.GridPosition.x - position.x) +
                       Mathf.Abs(playerActor.GridPosition.y - position.y);

        return distance <= safeRange;
    }
}