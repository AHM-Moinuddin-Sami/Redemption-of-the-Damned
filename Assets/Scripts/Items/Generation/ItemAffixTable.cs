using System.Collections.Generic;
using UnityEngine;

/*
 * ItemAffixTable
 * --------------
 * Rolls random affixes for generated equipment items.
 *
 * Current responsibilities:
 * - store weighted affix entries
 * - filter affixes by item definition and floor number
 * - prevent the same affix from rolling twice on the same item
 * - return a list of rolled affixes
 *
 * Important:
 * This is a simple affix table.
 * Later, you can split this into:
 * - weapon prefix table
 * - weapon suffix table
 * - armor prefix table
 * - shield suffix table
 * - biome-specific affix tables
 * - unique item restrictions
 */

[CreateAssetMenu(menuName = "Roguelike/Items/Generation/Item Affix Table")]
public class ItemAffixTable : ScriptableObject
{
    [Header("Affixes")]
    [SerializeField] private List<ItemAffixTableEntry> entries = new List<ItemAffixTableEntry>();

    public List<ItemAffixDefinition> RollAffixes(
        ItemDefinition itemDefinition,
        int floorNumber,
        int affixCount,
        System.Random random)
    {
        List<ItemAffixDefinition> rolledAffixes = new List<ItemAffixDefinition>();

        if (itemDefinition == null)
        {
            return rolledAffixes;
        }

        int safeAffixCount = Mathf.Max(0, affixCount);

        for (int i = 0; i < safeAffixCount; i++)
        {
            ItemAffixDefinition rolledAffix = RollSingleAffix(
                itemDefinition,
                floorNumber,
                rolledAffixes,
                random
            );

            if (rolledAffix == null)
            {
                break;
            }

            rolledAffixes.Add(rolledAffix);
        }

        return rolledAffixes;
    }

    private ItemAffixDefinition RollSingleAffix(
        ItemDefinition itemDefinition,
        int floorNumber,
        List<ItemAffixDefinition> alreadyRolledAffixes,
        System.Random random)
    {
        List<ItemAffixTableEntry> validEntries = GetValidEntries(
            itemDefinition,
            floorNumber,
            alreadyRolledAffixes
        );

        int totalWeight = GetTotalWeight(validEntries);

        if (totalWeight <= 0)
        {
            return null;
        }

        int roll = random.Next(0, totalWeight);
        int runningTotal = 0;

        for (int i = 0; i < validEntries.Count; i++)
        {
            runningTotal += validEntries[i].Weight;

            if (roll < runningTotal)
            {
                return validEntries[i].AffixDefinition;
            }
        }

        return null;
    }

    private List<ItemAffixTableEntry> GetValidEntries(
        ItemDefinition itemDefinition,
        int floorNumber,
        List<ItemAffixDefinition> alreadyRolledAffixes)
    {
        List<ItemAffixTableEntry> validEntries = new List<ItemAffixTableEntry>();

        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i] == null || entries[i].AffixDefinition == null)
            {
                continue;
            }

            if (entries[i].Weight <= 0)
            {
                continue;
            }

            if (alreadyRolledAffixes.Contains(entries[i].AffixDefinition))
            {
                continue;
            }

            if (!entries[i].AffixDefinition.CanApplyTo(itemDefinition, floorNumber))
            {
                continue;
            }

            validEntries.Add(entries[i]);
        }

        return validEntries;
    }

    private int GetTotalWeight(List<ItemAffixTableEntry> validEntries)
    {
        int totalWeight = 0;

        for (int i = 0; i < validEntries.Count; i++)
        {
            totalWeight += validEntries[i].Weight;
        }

        return totalWeight;
    }
}