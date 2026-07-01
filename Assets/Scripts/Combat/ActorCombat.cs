using UnityEngine;

/*
 * ActorCombat
 * -----------
 * Handles basic direct attacks between actors.
 *
 * This version:
 * - reads attack damage from ActorStats
 * - allows equipped item special effects to modify outgoing damage
 * - triggers equipped item effects on hit
 * - triggers equipped item effects on kill
 * - writes visibility-aware attack messages
 * - emits combat noise so nearby enemies can investigate
 *
 * Current item special effect integration:
 * - OnHit BonusDamage is applied before the target receives damage.
 * - OnHit effects are triggered after a successful damage attempt.
 * - OnKill effects are triggered if the target dies from the attack.
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
    private ActorItemSpecialEffectHandler specialEffectHandler;

    private void Awake()
    {
        actorStats = GetComponent<ActorStats>();
        actorGridEntity = GetComponent<ActorGridEntity>();
        specialEffectHandler = GetComponent<ActorItemSpecialEffectHandler>();
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
        damage = ApplyOutgoingDamageEffects(target, damage);

        WriteAttackMessage(target, damage);
        EmitAttackNoise();

        bool damaged = targetHealth.TakeDamage(damage);

        if (!damaged)
        {
            return false;
        }

        TriggerOnHitEffects(target);

        if (targetHealth.IsDead)
        {
            TriggerOnKillEffects(target);
        }

        return true;
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

    private int ApplyOutgoingDamageEffects(ActorGridEntity target, int baseDamage)
    {
        if (specialEffectHandler == null)
        {
            return baseDamage;
        }

        return specialEffectHandler.ModifyOutgoingAttackDamage(target, baseDamage);
    }

    private void TriggerOnHitEffects(ActorGridEntity target)
    {
        if (specialEffectHandler == null)
        {
            return;
        }

        specialEffectHandler.OnHitTarget(target);
    }

    private void TriggerOnKillEffects(ActorGridEntity target)
    {
        if (specialEffectHandler == null)
        {
            return;
        }

        specialEffectHandler.OnKilledTarget(target);
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