using UnityEngine;

/*
 * ActorCombat
 * -----------
 * Handles basic direct attacks between actors.
 *
 * This script now sends player-facing combat messages to GameMessageLog instead
 * of relying on raw Debug.Log output.
 *
 * Current responsibilities:
 * - read attack damage from ActorStats
 * - receive a target actor
 * - find the target's ActorHealth component
 * - apply damage to the target
 * - show a clean combat message in the message log
 *
 * Important:
 * Armor reduction is handled by ActorHealth.
 */

[RequireComponent(typeof(ActorGridEntity))]
public class ActorCombat : MonoBehaviour
{
    [Header("Fallback Combat")]
    [SerializeField] private int fallbackAttackDamage = 3;

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

        GameMessageLog.Write(GetDisplayName() + " attacks " + target.DisplayName + " for " + damage + " damage.");

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

    private string GetDisplayName()
    {
        if (actorGridEntity == null)
        {
            return gameObject.name;
        }

        return actorGridEntity.DisplayName;
    }
}