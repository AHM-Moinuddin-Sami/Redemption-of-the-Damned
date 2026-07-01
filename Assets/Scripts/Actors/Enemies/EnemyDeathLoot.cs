using System.Collections.Generic;
using UnityEngine;

/*
 * EnemyDeathLoot
 * --------------
 * Stores the death-loot settings for one spawned enemy.
 *
 * This component belongs on the generic enemy prefab.
 * EnemyDefinitionApplier copies EnemyDefinition loot settings into this script.
 *
 * Current responsibilities:
 * - roll guaranteed/chance-based specific drops
 * - decide whether random loot should drop
 * - roll how many random items should drop
 * - choose which loot table/profile to roll random drops from
 *
 * Drop layers:
 * 1. Guaranteed Death Drops
 *    - specific authored item entries
 *    - each has its own drop chance
 *
 * 2. Random Death Loot
 *    - rolls from Death Loot Table
 *    - optionally falls back to floor loot profile
 *
 * Example:
 * Bandit:
 * - Guaranteed drops: 20% Bread, 15% Healing Potion
 * - Random table: bandit weapon/armor loot table
 *
 * Boss:
 * - Guaranteed drops: 100% unique reward
 * - Random table: boss reward table
 */

public class EnemyDeathLoot : MonoBehaviour
{
    [Header("Random Drop Rules")]
    [SerializeField] private bool canDropLoot = true;

    [Range(0, 100)]
    [SerializeField] private int dropChancePercent = 35;

    [SerializeField] private int minDrops = 0;
    [SerializeField] private int maxDrops = 1;

    [Header("Random Loot Source")]
    [SerializeField] private ItemDropTable deathLootTable;
    [SerializeField] private bool useFloorLootProfileIfNoDeathTable = false;

    private readonly List<EnemyGuaranteedDropEntry> guaranteedDeathDrops = new List<EnemyGuaranteedDropEntry>();

    public void ApplyDefinition(EnemyDefinition enemyDefinition)
    {
        if (enemyDefinition == null)
        {
            return;
        }

        canDropLoot = enemyDefinition.CanDropLoot;
        dropChancePercent = enemyDefinition.DeathDropChancePercent;
        minDrops = enemyDefinition.MinDeathDrops;
        maxDrops = enemyDefinition.MaxDeathDrops;
        deathLootTable = enemyDefinition.DeathLootTable;
        useFloorLootProfileIfNoDeathTable = enemyDefinition.UseFloorLootProfileIfNoDeathTable;

        guaranteedDeathDrops.Clear();

        IReadOnlyList<EnemyGuaranteedDropEntry> definitionDrops = enemyDefinition.GuaranteedDeathDrops;

        for (int i = 0; i < definitionDrops.Count; i++)
        {
            if (definitionDrops[i] == null)
            {
                continue;
            }

            guaranteedDeathDrops.Add(definitionDrops[i]);
        }
    }

    public List<ItemDropResult> RollGuaranteedDropResults(System.Random random)
    {
        List<ItemDropResult> results = new List<ItemDropResult>();

        if (random == null)
        {
            random = new System.Random();
        }

        for (int i = 0; i < guaranteedDeathDrops.Count; i++)
        {
            if (guaranteedDeathDrops[i] == null)
            {
                continue;
            }

            ItemDropResult result = guaranteedDeathDrops[i].Roll(random);

            if (result == null)
            {
                continue;
            }

            results.Add(result);
        }

        return results;
    }

    public int RollRandomDropCount(System.Random random)
    {
        if (!canDropLoot)
        {
            return 0;
        }

        if (random == null)
        {
            random = new System.Random();
        }

        int safeDropChance = Mathf.Clamp(dropChancePercent, 0, 100);

        if (safeDropChance <= 0)
        {
            return 0;
        }

        int chanceRoll = random.Next(0, 100);

        if (chanceRoll >= safeDropChance)
        {
            return 0;
        }

        int safeMinDrops = Mathf.Max(0, minDrops);
        int safeMaxDrops = Mathf.Max(safeMinDrops, maxDrops);

        return random.Next(safeMinDrops, safeMaxDrops + 1);
    }

    public ItemDropResult RollRandomDropResult(
        int floorNumber,
        System.Random random,
        ItemLootProfile floorLootProfile,
        ItemDropTable fallbackFloorLootTable)
    {
        if (deathLootTable != null)
        {
            return deathLootTable.Roll(random);
        }

        if (!useFloorLootProfileIfNoDeathTable)
        {
            return null;
        }

        if (floorLootProfile != null)
        {
            return floorLootProfile.RollDropResult(floorNumber, random);
        }

        if (fallbackFloorLootTable != null)
        {
            return fallbackFloorLootTable.Roll(random);
        }

        return null;
    }
}