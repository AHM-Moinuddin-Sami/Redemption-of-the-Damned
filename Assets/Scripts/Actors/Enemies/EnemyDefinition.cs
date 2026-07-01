using System.Collections.Generic;
using UnityEngine;

/*
 * EnemyDefinition
 * ---------------
 * Stores authored data for one enemy type.
 *
 * This is a ScriptableObject asset. It lets you create enemies like:
 * - Rat
 * - Bandit
 * - Skeleton
 * - Cultist
 * - Slime
 *
 * Current responsibilities:
 * - store enemy display data
 * - store enemy combat stats
 * - store XP reward
 * - store basic AI settings
 *
 * Important:
 * This definition does not spawn itself.
 * GameBootstrap rolls an EnemySpawnTable, spawns the generic enemy prefab,
 * then EnemyDefinitionApplier copies this data onto that spawned enemy.
 *
 * Later this can expand into:
 * - factions
 * - loot tables
 * - abilities
 * - resistances
 * - status immunities
 * - special AI profiles
 * - biome restrictions
 */

[CreateAssetMenu(menuName = "Roguelike/Enemies/Enemy Definition")]
public class EnemyDefinition : ScriptableObject
{
    [Header("Display")]
    [SerializeField] private string displayName = "Enemy";
    [SerializeField] private Sprite sprite;

    [Header("Stats")]
    [SerializeField] private int maxHealth = 6;
    [SerializeField] private int attackDamage = 2;
    [SerializeField] private int armor = 0;

    [Header("Progression")]
    [SerializeField] private int experienceReward = 5;

    [Header("Detection")]
    [SerializeField] private int detectionRange = 10;
    [SerializeField] private int chaseTurnsAfterLosingSight = 4;

    [Header("Sound")]
    [SerializeField] private bool canHearNoise = true;
    [SerializeField] private int investigationTurnsAfterHearingNoise = 6;

    [Header("Doors")]
    [SerializeField] private bool canOpenDoorsWhileAware = true;

    [Header("Wandering")]
    [SerializeField] private bool canWanderWhileUnaware = true;
    [SerializeField] private int wanderChancePercent = 45;
    [SerializeField] private int maxWanderDistanceFromHome = 6;
    [SerializeField] private int wanderDirectionAttempts = 4;

    [Header("Death Loot")]
    [SerializeField] private bool canDropLoot = true;

    [Range(0, 100)]
    [SerializeField] private int deathDropChancePercent = 35;

    [SerializeField] private int minDeathDrops = 0;
    [SerializeField] private int maxDeathDrops = 1;

    [SerializeField] private ItemDropTable deathLootTable;
    [SerializeField] private bool useFloorLootProfileIfNoDeathTable = false;

    [Header("Guaranteed Death Drops")]
    [SerializeField] private List<EnemyGuaranteedDropEntry> guaranteedDeathDrops = new List<EnemyGuaranteedDropEntry>();

    public string DisplayName
    {
        get
        {
            return displayName;
        }
    }

    public Sprite Sprite
    {
        get
        {
            return sprite;
        }
    }

    public int MaxHealth
    {
        get
        {
            return Mathf.Max(1, maxHealth);
        }
    }

    public int AttackDamage
    {
        get
        {
            return Mathf.Max(0, attackDamage);
        }
    }

    public int Armor
    {
        get
        {
            return Mathf.Max(0, armor);
        }
    }

    public int ExperienceReward
    {
        get
        {
            return Mathf.Max(0, experienceReward);
        }
    }

    public int DetectionRange
    {
        get
        {
            return Mathf.Max(0, detectionRange);
        }
    }

    public int ChaseTurnsAfterLosingSight
    {
        get
        {
            return Mathf.Max(0, chaseTurnsAfterLosingSight);
        }
    }

    public bool CanHearNoise
    {
        get
        {
            return canHearNoise;
        }
    }

    public int InvestigationTurnsAfterHearingNoise
    {
        get
        {
            return Mathf.Max(0, investigationTurnsAfterHearingNoise);
        }
    }

    public bool CanOpenDoorsWhileAware
    {
        get
        {
            return canOpenDoorsWhileAware;
        }
    }

    public bool CanWanderWhileUnaware
    {
        get
        {
            return canWanderWhileUnaware;
        }
    }

    public int WanderChancePercent
    {
        get
        {
            return Mathf.Clamp(wanderChancePercent, 0, 100);
        }
    }

    public int MaxWanderDistanceFromHome
    {
        get
        {
            return Mathf.Max(0, maxWanderDistanceFromHome);
        }
    }

    public int WanderDirectionAttempts
    {
        get
        {
            return Mathf.Max(1, wanderDirectionAttempts);
        }
    }

    public bool CanDropLoot
    {
        get
        {
            return canDropLoot;
        }
    }

    public int DeathDropChancePercent
    {
        get
        {
            return Mathf.Clamp(deathDropChancePercent, 0, 100);
        }
    }

    public int MinDeathDrops
    {
        get
        {
            return Mathf.Max(0, minDeathDrops);
        }
    }

    public int MaxDeathDrops
    {
        get
        {
            return Mathf.Max(MinDeathDrops, maxDeathDrops);
        }
    }

    public ItemDropTable DeathLootTable
    {
        get
        {
            return deathLootTable;
        }
    }

    public bool UseFloorLootProfileIfNoDeathTable
    {
        get
        {
            return useFloorLootProfileIfNoDeathTable;
        }
    }
    public IReadOnlyList<EnemyGuaranteedDropEntry> GuaranteedDeathDrops
    {
        get
        {
            return guaranteedDeathDrops;
        }
    }
}