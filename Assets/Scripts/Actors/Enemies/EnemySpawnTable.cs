using System.Collections.Generic;
using UnityEngine;

/*
 * EnemySpawnTable
 * ---------------
 * Rolls a random enemy definition from a weighted list.
 *
 * This lets each dungeon floor choose from multiple enemy types.
 *
 * Current responsibilities:
 * - store weighted enemy entries
 * - roll one EnemyDefinition using System.Random
 *
 * Important:
 * This is a simple global table for now.
 * Later, this can become biome-aware, floor-depth-aware, faction-aware, or
 * dungeon-theme-aware.
 */

[CreateAssetMenu(menuName = "Roguelike/Enemies/Enemy Spawn Table")]
public class EnemySpawnTable : ScriptableObject
{
    [Header("Entries")]
    [SerializeField] private List<EnemySpawnTableEntry> entries = new List<EnemySpawnTableEntry>();

    public EnemyDefinition Roll(System.Random random)
    {
        if (random == null)
        {
            random = new System.Random();
        }

        int totalWeight = GetTotalWeight();

        if (totalWeight <= 0)
        {
            return null;
        }

        int roll = random.Next(0, totalWeight);
        int runningTotal = 0;

        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i] == null || entries[i].EnemyDefinition == null)
            {
                continue;
            }

            int weight = entries[i].Weight;

            if (weight <= 0)
            {
                continue;
            }

            runningTotal += weight;

            if (roll < runningTotal)
            {
                return entries[i].EnemyDefinition;
            }
        }

        return null;
    }

    private int GetTotalWeight()
    {
        int totalWeight = 0;

        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i] == null || entries[i].EnemyDefinition == null)
            {
                continue;
            }

            totalWeight += entries[i].Weight;
        }

        return totalWeight;
    }
}