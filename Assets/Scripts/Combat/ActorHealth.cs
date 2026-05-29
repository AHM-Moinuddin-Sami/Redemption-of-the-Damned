using System;
using UnityEngine;

/*
 * ActorHealth
 * -----------
 * Stores and manages health for an actor.
 *
 * This version supports:
 * - max health from ActorStats
 * - incoming damage
 * - armor reduction
 * - healing
 * - death detection
 * - visibility-aware messages
 * - a Died event so GameBootstrap can detect player death
 *
 * Current armor formula:
 * Final Damage = Incoming Damage - Armor
 * Minimum Final Damage = 1
 *
 * Important:
 * Game over is not handled directly inside this script.
 * This script only announces death through the Died event. GameBootstrap decides
 * whether that death belongs to the player and then shows the game over UI.
 */

public class ActorHealth : MonoBehaviour
{
    public event Action<ActorHealth> Died;

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
            return hasDied || CurrentHealth <= 0;
        }
    }

    private ActorStats actorStats;
    private ActorGridEntity actorGridEntity;
    private bool hasDied;

    private void Awake()
    {
        actorStats = GetComponent<ActorStats>();
        actorGridEntity = GetComponent<ActorGridEntity>();

        CurrentHealth = MaxHealth;
        hasDied = false;
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

        WriteDamageMessage(safeIncomingDamage, finalDamage);

        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
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
            WriteMessageIfKnown(GetDisplayName() + " is already at full health.");
            return false;
        }

        int oldHealth = CurrentHealth;

        CurrentHealth += safeHealAmount;

        if (CurrentHealth > MaxHealth)
        {
            CurrentHealth = MaxHealth;
        }

        int actualHealedAmount = CurrentHealth - oldHealth;

        WriteHealMessage(actualHealedAmount);

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

    private void WriteDamageMessage(int incomingDamage, int finalDamage)
    {
        if (incomingDamage <= 0)
        {
            return;
        }

        bool isPlayer = PlayerAwarenessContext.IsPlayer(actorGridEntity);
        bool isVisible = PlayerAwarenessContext.CanSeeActor(actorGridEntity);

        if (!isPlayer && !isVisible)
        {
            return;
        }

        int reducedAmount = incomingDamage - finalDamage;

        if (reducedAmount > 0)
        {
            if (isPlayer)
            {
                GameMessageLog.Write("Your armor reduces damage by " + reducedAmount + ".");
            }
            else
            {
                GameMessageLog.Write(GetDisplayName() + "'s armor reduces damage by " + reducedAmount + ".");
            }
        }

        if (isPlayer)
        {
            GameMessageLog.Write("You take " + finalDamage + " damage. HP: " + CurrentHealth + "/" + MaxHealth + ".");
            return;
        }

        GameMessageLog.Write(GetDisplayName() + " takes " + finalDamage + " damage. HP: " + CurrentHealth + "/" + MaxHealth + ".");
    }

    private void WriteHealMessage(int healedAmount)
    {
        bool isPlayer = PlayerAwarenessContext.IsPlayer(actorGridEntity);

        if (isPlayer)
        {
            GameMessageLog.Write("You heal " + healedAmount + " HP. HP: " + CurrentHealth + "/" + MaxHealth + ".");
            return;
        }

        if (PlayerAwarenessContext.CanSeeActor(actorGridEntity))
        {
            GameMessageLog.Write(GetDisplayName() + " heals " + healedAmount + " HP.");
        }
    }

    private void WriteMessageIfKnown(string message)
    {
        if (PlayerAwarenessContext.IsPlayer(actorGridEntity) || PlayerAwarenessContext.CanSeeActor(actorGridEntity))
        {
            GameMessageLog.Write(message);
        }
    }

    private void Die()
    {
        if (hasDied)
        {
            return;
        }

        hasDied = true;

        bool isPlayer = PlayerAwarenessContext.IsPlayer(actorGridEntity);
        bool isVisible = PlayerAwarenessContext.CanSeeActor(actorGridEntity);

        if (isPlayer)
        {
            GameMessageLog.Write("You die.");
        }
        else if (isVisible)
        {
            GameMessageLog.Write(GetDisplayName() + " dies.");
        }

        if (Died != null)
        {
            Died.Invoke(this);
        }

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

    public void SetToFullHealth()
    {
        CurrentHealth = MaxHealth;
    }
}