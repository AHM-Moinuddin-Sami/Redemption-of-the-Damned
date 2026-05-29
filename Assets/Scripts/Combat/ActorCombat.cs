using UnityEngine;

/*
 * ActorCombat
 * -----------
 * Handles basic direct attacks between actors.
 *
 * This version:
 * - reads attack damage from ActorStats
 * - writes visibility-aware attack messages
 * - emits combat noise so nearby enemies can investigate
 *
 * Current message examples:
 * - You attack Rat for 6 damage.
 * - Rat attacks you for 2 damage.
 * - Something attacks you for 2 damage.
 *
 * Current noise behavior:
 * - every successful attack emits combat noise from the attacker's position
 *
 * Important:
 * Armor reduction and death messages are handled by ActorHealth.
 */

[RequireComponent(typeof(ActorGridEntity))]
public class ActorCombat : MonoBehaviour
{
    [Header("Fallback Combat")]
    [SerializeField] private int fallbackAttackDamage = 3;

    [Header("Noise")]
    [SerializeField] private int attackNoiseRange = 10;

    private ActorStats actorStats;
    private ActorGridEntity actorGridEntity;

    private void Awake()
    {
        actorStats = GetComponent<ActorStats>();
        actorGridEntity = GetComponent<ActorGridEntity>();
    }

    public bool Attack(ActorGridEntity target)
    {
        if (target == null)
        {
            return false;
        }

        ActorHealth targetHealth = target.GetComponent<ActorHealth>();

        if (targetHealth == null)
        {
            Debug.LogWarning(target.DisplayName + " has no ActorHealth component.");
            return false;
        }

        if (targetHealth.IsDead)
        {
            return false;
        }

        int damage = GetAttackDamage();

        WriteAttackMessage(target, damage);
        EmitAttackNoise();

        return targetHealth.TakeDamage(damage);
    }

    private int GetAttackDamage()
    {
        if (actorStats == null)
        {
            return fallbackAttackDamage;
        }

        int attackDamage = actorStats.GetStat(StatType.AttackDamage);

        if (attackDamage < 1)
        {
            return 1;
        }

        return attackDamage;
    }

    private void EmitAttackNoise()
    {
        if (actorGridEntity == null)
        {
            return;
        }

        GameNoiseSystem.EmitNoise(
            actorGridEntity.GridPosition,
            attackNoiseRange,
            actorGridEntity,
            NoiseCategory.Combat
        );
    }

    private void WriteAttackMessage(ActorGridEntity target, int damage)
    {
        bool attackerIsPlayer = PlayerAwarenessContext.IsPlayer(actorGridEntity);
        bool targetIsPlayer = PlayerAwarenessContext.IsPlayer(target);
        bool attackerVisible = PlayerAwarenessContext.CanSeeActor(actorGridEntity);
        bool targetVisible = PlayerAwarenessContext.CanSeeActor(target);

        if (attackerIsPlayer)
        {
            GameMessageLog.Write("You attack " + target.DisplayName + " for " + damage + " damage.");
            return;
        }

        if (targetIsPlayer)
        {
            if (attackerVisible)
            {
                GameMessageLog.Write(actorGridEntity.DisplayName + " attacks you for " + damage + " damage.");
                return;
            }

            GameMessageLog.Write("Something attacks you for " + damage + " damage.");
            return;
        }

        if (attackerVisible || targetVisible)
        {
            GameMessageLog.Write(actorGridEntity.DisplayName + " attacks " + target.DisplayName + " for " + damage + " damage.");
        }
    }
}