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
 * Fighter class:
 * - Stat Type: MaxHealth
 * - Value: 5
 *
 * Level-up bonus:
 * - Stat Type: MaxHealth
 * - Value: 2
 *
 * Important:
 * This version includes constructors so runtime systems, like level-up logic,
 * can create stat modifiers through code.
 *
 * Later, this can become a more advanced modifier system with:
 * - flat modifiers
 * - increased modifiers
 * - more modifiers
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

    public StatModifier()
    {
    }

    public StatModifier(StatType newStatType, int newValue)
    {
        statType = newStatType;
        value = newValue;
    }
}