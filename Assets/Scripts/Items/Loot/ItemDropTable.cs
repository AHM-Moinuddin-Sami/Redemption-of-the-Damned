using System;
using System.Collections.Generic;
using UnityEngine;

/*
 * ItemDropTable
 * -------------
 * Stores a weighted list of possible item definitions that can be rolled when
 * the game wants to spawn loot.
 *
 * This version supports both:
 * - weighted item selection
 * - random quantity ranges
 *
 * Example:
 * Copper Coin:
 * - Weight: 20
 * - Quantity: 3 to 15
 *
 * Rusty Sword:
 * - Weight: 5
 * - Quantity: 1 to 1
 *
 * Current responsibilities:
 * - store weighted item entries
 * - store minimum and maximum quantity per entry
 * - calculate total table weight
 * - roll one item entry using System.Random
 * - return an ItemDropResult containing item definition and quantity
 *
 * Important:
 * This table still does not create a full Path of Exile-style generated item.
 * It only chooses a base item and quantity.
 *
 * Later this can expand into:
 * - rarity weights
 * - affix rolling
 * - item level
 * - biome restrictions
 * - faction-specific drops
 * - unique item chances
 * - magic/rare item generation
 */

[CreateAssetMenu(menuName = "Roguelike/Items/Item Drop Table")]
public class ItemDropTable : ScriptableObject
{
    [Serializable]
    private class ItemDropTableEntry
    {
        [Header("Item")]
        [SerializeField] private ItemDefinition itemDefinition;

        [Header("Weight")]
        [SerializeField] private int weight = 1;

        [Header("Quantity")]
        [SerializeField] private int minQuantity = 1;
        [SerializeField] private int maxQuantity = 1;

        public ItemDefinition ItemDefinition
        {
            get
            {
                return itemDefinition;
            }
        }

        public int Weight
        {
            get
            {
                return Mathf.Max(0, weight);
            }
        }

        public int MinQuantity
        {
            get
            {
                return Mathf.Max(1, minQuantity);
            }
        }

        public int MaxQuantity
        {
            get
            {
                return Mathf.Max(MinQuantity, maxQuantity);
            }
        }

        public int RollQuantity(System.Random random)
        {
            if (random == null)
            {
                return 1;
            }

            if (itemDefinition == null)
            {
                return 1;
            }

            // Non-stackable items should not spawn as quantity stacks.
            // Example: Rusty Sword x5 would be wrong for this system.
            if (!itemDefinition.Stackable)
            {
                return 1;
            }

            return random.Next(MinQuantity, MaxQuantity + 1);
        }
    }

    [Header("Drops")]
    [SerializeField] private List<ItemDropTableEntry> entries = new List<ItemDropTableEntry>();

    public ItemDropResult Roll(System.Random random)
    {
        if (random == null)
        {
            Debug.LogWarning(name + " cannot roll because random is null.");
            return null;
        }

        ItemDropTableEntry selectedEntry = RollEntry(random);

        if (selectedEntry == null)
        {
            return null;
        }

        int rolledQuantity = selectedEntry.RollQuantity(random);

        return new ItemDropResult(selectedEntry.ItemDefinition, rolledQuantity);
    }

    private ItemDropTableEntry RollEntry(System.Random random)
    {
        int totalWeight = GetTotalWeight();

        if (totalWeight <= 0)
        {
            Debug.LogWarning(name + " has no valid weighted item entries.");
            return null;
        }

        int roll = random.Next(0, totalWeight);
        int runningWeight = 0;

        for (int i = 0; i < entries.Count; i++)
        {
            if (!IsValidEntry(entries[i]))
            {
                continue;
            }

            runningWeight += entries[i].Weight;

            if (roll < runningWeight)
            {
                return entries[i];
            }
        }

        return null;
    }

    private int GetTotalWeight()
    {
        int totalWeight = 0;

        for (int i = 0; i < entries.Count; i++)
        {
            if (!IsValidEntry(entries[i]))
            {
                continue;
            }

            totalWeight += entries[i].Weight;
        }

        return totalWeight;
    }

    private bool IsValidEntry(ItemDropTableEntry entry)
    {
        if (entry == null)
        {
            return false;
        }

        if (entry.ItemDefinition == null)
        {
            return false;
        }

        if (entry.Weight <= 0)
        {
            return false;
        }

        return true;
    }
}