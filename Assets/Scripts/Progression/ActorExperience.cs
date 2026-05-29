using System;
using UnityEngine;

/*
 * ActorExperience
 * ---------------
 * Stores experience points and level progression for an actor.
 *
 * This script is currently intended for the player.
 *
 * Current responsibilities:
 * - store current level
 * - store current XP toward the next level
 * - calculate XP required for the next level
 * - receive XP rewards
 * - handle multiple level-ups from one large XP reward
 * - apply simple level-up stat bonuses
 * - optionally heal the actor on level-up
 * - notify UI when XP or level changes
 *
 * Current XP formula:
 * XP To Next Level = Base XP + Growth Per Level * (Current Level - 1)
 *
 * Example:
 * Base XP: 20
 * Growth Per Level: 15
 *
 * Level 1 -> 2 requires 20 XP
 * Level 2 -> 3 requires 35 XP
 * Level 3 -> 4 requires 50 XP
 *
 * Current level-up bonuses:
 * - MaxHealth bonus every level
 * - AttackDamage bonus every few levels
 * - Armor bonus every few levels
 *
 * Important:
 * This is the foundation only.
 * Later this can become:
 * - attribute points
 * - skill points
 * - class-specific level tables
 * - subclass unlocks
 * - ability choices
 * - perk selection
 */

[RequireComponent(typeof(ActorStats))]
[RequireComponent(typeof(ActorHealth))]
public class ActorExperience : MonoBehaviour
{
    public event Action ExperienceChanged;
    public event Action<int> LeveledUp;

    [Header("Level")]
    [SerializeField] private int startingLevel = 1;

    [Header("XP Formula")]
    [SerializeField] private int baseExperienceToNextLevel = 20;
    [SerializeField] private int experienceGrowthPerLevel = 15;

    [Header("Level-Up Bonuses")]
    [SerializeField] private int maxHealthBonusPerLevel = 2;
    [SerializeField] private int attackDamageBonusEveryLevels = 2;
    [SerializeField] private int attackDamageBonusAmount = 1;
    [SerializeField] private int armorBonusEveryLevels = 3;
    [SerializeField] private int armorBonusAmount = 1;

    [Header("Healing")]
    [SerializeField] private bool restoreToFullHealthOnLevelUp = true;

    public int CurrentLevel { get; private set; }
    public int CurrentExperience { get; private set; }

    public int ExperienceToNextLevel
    {
        get
        {
            int levelOffset = Mathf.Max(0, CurrentLevel - 1);
            return Mathf.Max(1, baseExperienceToNextLevel + experienceGrowthPerLevel * levelOffset);
        }
    }

    private ActorStats actorStats;
    private ActorHealth actorHealth;

    private void Awake()
    {
        actorStats = GetComponent<ActorStats>();
        actorHealth = GetComponent<ActorHealth>();

        CurrentLevel = Mathf.Max(1, startingLevel);
        CurrentExperience = 0;
    }

    public void AddExperience(int amount)
    {
        int safeAmount = Mathf.Max(0, amount);

        if (safeAmount == 0)
        {
            return;
        }

        CurrentExperience += safeAmount;

        GameMessageLog.Write("You gain " + safeAmount + " XP.");

        ProcessLevelUps();
        NotifyExperienceChanged();
    }

    private void ProcessLevelUps()
    {
        while (CurrentExperience >= ExperienceToNextLevel)
        {
            CurrentExperience -= ExperienceToNextLevel;
            LevelUp();
        }
    }

    private void LevelUp()
    {
        CurrentLevel++;

        ApplyLevelUpBonuses();

        if (restoreToFullHealthOnLevelUp && actorHealth != null)
        {
            actorHealth.SetToFullHealth();
        }

        GameMessageLog.Write("You reach level " + CurrentLevel + ".");

        if (LeveledUp != null)
        {
            LeveledUp.Invoke(CurrentLevel);
        }
    }

    private void ApplyLevelUpBonuses()
    {
        if (actorStats == null)
        {
            return;
        }

        if (maxHealthBonusPerLevel != 0)
        {
            actorStats.AddPermanentModifier(StatType.MaxHealth, maxHealthBonusPerLevel);
        }

        if (ShouldApplyIntervalBonus(attackDamageBonusEveryLevels) && attackDamageBonusAmount != 0)
        {
            actorStats.AddPermanentModifier(StatType.AttackDamage, attackDamageBonusAmount);
        }

        if (ShouldApplyIntervalBonus(armorBonusEveryLevels) && armorBonusAmount != 0)
        {
            actorStats.AddPermanentModifier(StatType.Armor, armorBonusAmount);
        }
    }

    private bool ShouldApplyIntervalBonus(int interval)
    {
        if (interval <= 0)
        {
            return false;
        }

        return CurrentLevel % interval == 0;
    }

    private void NotifyExperienceChanged()
    {
        if (ExperienceChanged == null)
        {
            return;
        }

        ExperienceChanged.Invoke();
    }
}