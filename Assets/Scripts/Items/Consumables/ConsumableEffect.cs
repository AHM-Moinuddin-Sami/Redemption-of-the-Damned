using System;
using UnityEngine;

/*
 * ConsumableEffect
 * ----------------
 * Represents one effect applied by a consumable item.
 *
 * Example:
 * Healing Potion:
 * - Effect Type: HealHealth
 * - Value: 8
 *
 * Bread:
 * - Effect Type: HealHealth
 * - Value: 2
 *
 * Current responsibilities:
 * - store the effect type
 * - store the effect value
 *
 * Important:
 * This class only stores data. It does not apply the effect by itself.
 * ActorItemUser reads this data and applies the effect to the actor.
 *
 * Later this can support:
 * - duration
 * - chance
 * - scaling
 * - stat modifiers
 * - status effects
 * - hunger/thirst restoration
 */

[Serializable]
public class ConsumableEffect
{
    [SerializeField] private ConsumableEffectType effectType;
    [SerializeField] private int value = 1;

    public ConsumableEffectType EffectType
    {
        get
        {
            return effectType;
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