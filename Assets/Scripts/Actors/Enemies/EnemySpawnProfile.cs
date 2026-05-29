using System.Collections.Generic;
using UnityEngine;

/*
 * EnemySpawnProfile
 * -----------------
 * Chooses an EnemySpawnTable based on dungeon floor depth.
 *
 * This is a higher-level enemy spawning asset.
 *
 * EnemySpawnTable:
 * - chooses the actual enemy type
 *
 * EnemySpawnProfile:
 * - chooses which table to use for the current floor
 *
 * Example setup:
 *
 * Entry 1:
 * - Floor 1-2
 * - EarlyDungeonEnemyTable
 *
 * Entry 2:
 * - Floor 3-5
 * - MidDungeonEnemyTable
 *
 * Entry 3:
 * - Floor 6+
 * - DeepDungeonEnemyTable
 *
 * Current responsibilities:
 * - store a fallback/default enemy spawn table
 * - store depth-specific table entries
 * - find all entries matching the current floor
 * - choose one matching table using weights
 * - roll an EnemyDefinition from the selected table
 *
 * Important:
 * This does not know about rooms, biomes, factions, or quests yet.
 * It only handles floor-depth enemy pools.
 *
 * Later this can expand into:
 * - biome-specific tables
 * - faction-specific tables
 * - boss tables
 * - unique enemy spawn rules
 * - floor danger ratings
 * - encounter budget systems
 */

[CreateAssetMenu(menuName = "Roguelike/Enemies/Enemy Spawn Profile")]
public class EnemySpawnProfile : ScriptableObject
{
    [Header("Fallback")]
    [SerializeField] private EnemySpawnTable fallbackEnemySpawnTable;

    [Header("Depth Entries")]
    [SerializeField] private List<EnemySpawnProfileEntry> entries = new List<EnemySpawnProfileEntry>();

    public EnemyDefinition RollEnemyDefinition(int floorNumber, System.Random random)
    {
        EnemySpawnTable table = RollSpawnTable(floorNumber, random);

        if (table == null)
        {
            return null;
        }

        return table.Roll(random);
    }

    public EnemySpawnTable RollSpawnTable(int floorNumber, System.Random random)
    {
        if (random == null)
        {
            random = new System.Random();
        }

        List<EnemySpawnProfileEntry> matchingEntries = GetMatchingEntries(floorNumber);

        if (matchingEntries.Count == 0)
        {
            return fallbackEnemySpawnTable;
        }

        EnemySpawnProfileEntry selectedEntry = RollMatchingEntry(matchingEntries, random);

        if (selectedEntry == null)
        {
            return fallbackEnemySpawnTable;
        }

        if (selectedEntry.EnemySpawnTable == null)
        {
            return fallbackEnemySpawnTable;
        }

        return selectedEntry.EnemySpawnTable;
    }

    private List<EnemySpawnProfileEntry> GetMatchingEntries(int floorNumber)
    {
        List<EnemySpawnProfileEntry> matchingEntries = new List<EnemySpawnProfileEntry>();

        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i] == null)
            {
                continue;
            }

            if (entries[i].EnemySpawnTable == null)
            {
                continue;
            }

            if (!entries[i].MatchesFloor(floorNumber))
            {
                continue;
            }

            if (entries[i].Weight <= 0)
            {
                continue;
            }

            matchingEntries.Add(entries[i]);
        }

        return matchingEntries;
    }

    private EnemySpawnProfileEntry RollMatchingEntry(
        List<EnemySpawnProfileEntry> matchingEntries,
        System.Random random)
    {
        int totalWeight = 0;

        for (int i = 0; i < matchingEntries.Count; i++)
        {
            totalWeight += matchingEntries[i].Weight;
        }

        if (totalWeight <= 0)
        {
            return null;
        }

        int roll = random.Next(0, totalWeight);
        int runningTotal = 0;

        for (int i = 0; i < matchingEntries.Count; i++)
        {
            runningTotal += matchingEntries[i].Weight;

            if (roll < runningTotal)
            {
                return matchingEntries[i];
            }
        }

        return null;
    }
}