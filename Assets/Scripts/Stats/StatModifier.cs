using System;
using UnityEngine;

/*
 * StatModifier
 * ------------
 * Represents one simple flat stat modifier.
 *
 * Current behavior:
 * - stores which stat is modified
 * - stores the flat value added to that stat
 *
 * Example:
 * Rusty Sword:
 * - Stat Type: AttackDamage
 * - Value: 3
 *
 * Old Shield:
 * - Stat Type: Armor
 * - Value: 2
 *
 * Important:
 * This is intentionally simple right now.
 * Later, this can become a more advanced Path of Exile-style modifier system
 * with:
 * - flat modifiers
 * - increased modifiers
 * - more modifiers
 * - local weapon modifiers
 * - global modifiers
 * - conditional modifiers
 * - affix tiers
 */

[Serializable]
public class StatModifier
{
    [SerializeField] private StatType statType;
    [SerializeField] private int value;

    public StatType StatType
    {
        get
        {
            return statType;
        }
    }

    public int Value
    {
        get
        {
            return value;
        }
    }
}