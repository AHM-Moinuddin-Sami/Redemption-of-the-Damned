using System.Collections.Generic;
using UnityEngine;

/*
 * ItemLootProfile
 * ---------------
 * Chooses an ItemDropTable based on dungeon floor depth.
 *
 * This is a higher-level loot asset.
 *
 * ItemDropTable:
 * - chooses the actual dropped item and quantity
 *
 * ItemLootProfile:
 * - chooses which ItemDropTable to use for the current floor
 *
 * Example setup:
 *
 * Entry 1:
 * - Floor 1-2
 * - EarlyDungeonLootTable
 *
 * Entry 2:
 * - Floor 3-5
 * - MidDungeonLootTable
 *
 * Entry 3:
 * - Floor 6+
 * - DeepDungeonLootTable
 *
 * Current responsibilities:
 * - store a fallback/default item drop table
 * - store floor-depth-specific loot table entries
 * - find all entries matching the current floor
 * - choose one matching table using weights
 * - roll an ItemDropResult from the selected table
 *
 * Important:
 * This does not know about rooms, enemies, biomes, quests, or rarity tiers yet.
 * It only handles floor-depth loot pools.
 *
 * Later this can expand into:
 * - biome-specific loot
 * - faction-specific loot
 * - chest-specific loot
 * - boss loot
 * - guaranteed food/water rules
 * - rarity scaling by floor
 * - magic item affixes
 * - unique item restrictions
 */

[CreateAssetMenu(menuName = "Roguelike/Items/Item Loot Profile")]
public class ItemLootProfile : ScriptableObject
{
    [Header("Fallback")]
    [SerializeField] private ItemDropTable fallbackItemDropTable;

    [Header("Depth Entries")]
    [SerializeField] private List<ItemLootProfileEntry> entries = new List<ItemLootProfileEntry>();

    public ItemDropResult RollDropResult(int floorNumber, System.Random random)
    {
        ItemDropTable table = RollDropTable(floorNumber, random);

        if (table == null)
        {
            return null;
        }

        return table.Roll(random);
    }

    public ItemDropTable RollDropTable(int floorNumber, System.Random random)
    {
        if (random == null)
        {
            random = new System.Random();
        }

        List<ItemLootProfileEntry> matchingEntries = GetMatchingEntries(floorNumber);

        if (matchingEntries.Count == 0)
        {
            return fallbackItemDropTable;
        }

        ItemLootProfileEntry selectedEntry = RollMatchingEntry(matchingEntries, random);

        if (selectedEntry == null)
        {
            return fallbackItemDropTable;
        }

        if (selectedEntry.ItemDropTable == null)
        {
            return fallbackItemDropTable;
        }

        return selectedEntry.ItemDropTable;
    }

    private List<ItemLootProfileEntry> GetMatchingEntries(int floorNumber)
    {
        List<ItemLootProfileEntry> matchingEntries = new List<ItemLootProfileEntry>();

        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i] == null)
            {
                continue;
            }

            if (entries[i].ItemDropTable == null)
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

    private ItemLootProfileEntry RollMatchingEntry(
        List<ItemLootProfileEntry> matchingEntries,
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