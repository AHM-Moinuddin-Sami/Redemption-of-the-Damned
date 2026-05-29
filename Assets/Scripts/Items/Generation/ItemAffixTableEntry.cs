using System;
using UnityEngine;

/*
 * ItemAffixTableEntry
 * -------------------
 * Stores one weighted affix entry inside an ItemAffixTable.
 *
 * Higher weight means the affix is more likely to roll.
 */

[Serializable]
public class ItemAffixTableEntry
{
    [SerializeField] private ItemAffixDefinition affixDefinition;
    [SerializeField] private int weight = 1;

    public ItemAffixDefinition AffixDefinition
    {
        get
        {
            return affixDefinition;
        }
    }

    public int Weight
    {
        get
        {
            return Mathf.Max(0, weight);
        }
    }
}