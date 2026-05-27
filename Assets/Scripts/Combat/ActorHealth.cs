using UnityEngine;

/*
 * ActorHealth
 * -----------
 * Stores and manages health for an actor.
 *
 * This script handles:
 * - current health
 * - max health from ActorStats
 * - incoming damage
 * - armor reduction
 * - healing
 * - death
 * - clean message log output
 *
 * Current armor formula:
 * Final Damage = Incoming Damage - Armor
 * Minimum Final Damage = 1
 *
 * Important:
 * This is still a simple combat health system. Later, this can support damage
 * types, resistances, poison, bleeding, armor penetration, regeneration, and
 * death drops.
 */

public class ActorHealth : MonoBehaviour
{
    [Header("Fallback Health")]
    [SerializeField] private int fallbackMaxHealth = 10;

    [Header("Damage Settings")]
    [SerializeField] private int minimumDamage = 1;

    public int CurrentHealth { get; private set; }

    public int MaxHealth
    {
        get
        {
            if (actorStats == null)
            {
                return fallbackMaxHealth;
            }

            int statMaxHealth = actorStats.GetStat(StatType.MaxHealth);

            if (statMaxHealth < 1)
            {
                return fallbackMaxHealth;
            }

            return statMaxHealth;
        }
    }

    public bool IsDead
    {
        get
        {
            return CurrentHealth <= 0;
        }
    }

    private ActorStats actorStats;
    private ActorGridEntity actorGridEntity;

    private void Awake()
    {
        actorStats = GetComponent<ActorStats>();
        actorGridEntity = GetComponent<ActorGridEntity>();

        CurrentHealth = MaxHealth;
    }

    public bool TakeDamage(int incomingDamage)
    {
        if (IsDead)
        {
            return false;
        }

        int safeIncomingDamage = Mathf.Max(0, incomingDamage);
        int finalDamage = CalculateFinalDamage(safeIncomingDamage);

        CurrentHealth -= finalDamage;

        if (safeIncomingDamage > 0)
        {
            GameMessageLog.Write(GetDisplayName() + " takes " + finalDamage + " damage. HP: " + CurrentHealth + "/" + MaxHealth + ".");
        }

        if (CurrentHealth <= 0)
        {
            Die();
        }

        return true;
    }

    public bool Heal(int healAmount)
    {
        if (IsDead)
        {
            return false;
        }

        int safeHealAmount = Mathf.Max(0, healAmount);

        if (safeHealAmount == 0)
        {
            return false;
        }

        if (CurrentHealth >= MaxHealth)
        {
            GameMessageLog.Write(GetDisplayName() + " is already at full health.");
            return false;
        }

        int oldHealth = CurrentHealth;

        CurrentHealth += safeHealAmount;

        if (CurrentHealth > MaxHealth)
        {
            CurrentHealth = MaxHealth;
        }

        int actualHealedAmount = CurrentHealth - oldHealth;

        GameMessageLog.Write(GetDisplayName() + " heals " + actualHealedAmount + " HP. HP: " + CurrentHealth + "/" + MaxHealth + ".");

        return actualHealedAmount > 0;
    }

    private int CalculateFinalDamage(int incomingDamage)
    {
        if (incomingDamage == 0)
        {
            return 0;
        }

        int armor = GetArmor();
        int finalDamage = incomingDamage - armor;

        if (finalDamage < minimumDamage)
        {
            finalDamage = minimumDamage;
        }

        if (armor > 0 && finalDamage < incomingDamage)
        {
            int reducedAmount = incomingDamage - finalDamage;
            GameMessageLog.Write(GetDisplayName() + "'s armor reduces damage by " + reducedAmount + ".");
        }

        return finalDamage;
    }

    private int GetArmor()
    {
        if (actorStats == null)
        {
            return 0;
        }

        int armor = actorStats.GetStat(StatType.Armor);

        if (armor < 0)
        {
            return 0;
        }

        return armor;
    }

    private void Die()
    {
        GameMessageLog.Write(GetDisplayName() + " dies.");

        ActorGridEntity gridEntity = GetComponent<ActorGridEntity>();

        if (gridEntity != null)
        {
            gridEntity.ClearFromMap();
        }

        gameObject.SetActive(false);
        Destroy(gameObject);
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