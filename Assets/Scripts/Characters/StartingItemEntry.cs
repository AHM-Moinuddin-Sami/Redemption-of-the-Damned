using System;
using UnityEngine;

/*
 * StartingItemEntry
 * -----------------
 * Defines one item granted to a character at the start of a run.
 *
 * This is used by character classes and backgrounds.
 *
 * Example:
 * Warrior class:
 * - Rusty Sword x1
 * - Bread x2
 *
 * Nomad background:
 * - Water Flask x2
 *
 * Current responsibilities:
 * - store an ItemDefinition reference
 * - store starting quantity
 * - create an ItemInstance when the run begins
 *
 * Important:
 * This is only for starting items.
 * It is not used for dungeon loot tables.
 */

[Serializable]
public class StartingItemEntry
{
    [SerializeField] private ItemDefinition itemDefinition;
    [SerializeField] private int quantity = 1;

    public ItemDefinition ItemDefinition
    {
        get
        {
            return itemDefinition;
        }
    }

    public int Quantity
    {
        get
        {
            return Mathf.Max(1, quantity);
        }
    }

    public ItemInstance CreateItemInstance()
    {
        if (itemDefinition == null)
        {
            return null;
        }

        return new ItemInstance(itemDefinition, Quantity);
    }
}