using System;
using UnityEngine;

/*
 * ItemSpecialEffectDefinition
 * ---------------------------
 * Defines one authored special effect on an item.
 *
 * This is serializable, so effects can be added directly inside ItemDefinition.
 *
 * Example active charm:
 * - Trigger: OnUse
 * - Effect Type: HealSelf
 * - Value: 3
 * - Chance Percent: 100
 * - Activation Message: The charm warms in your hand.
 *
 * Example unique sword:
 * - Trigger: OnHit
 * - Effect Type: BonusDamage
 * - Value: 2
 *
 * Current responsibilities:
 * - store trigger timing
 * - store effect type
 * - store effect value
 * - store activation chance
 * - store custom description text
 * - store optional activation message
 */

[Serializable]
public class ItemSpecialEffectDefinition
{
    [Header("Trigger")]
    [SerializeField] private ItemSpecialEffectTrigger trigger = ItemSpecialEffectTrigger.OnUse;

    [Header("Effect")]
    [SerializeField] private ItemSpecialEffectType effectType = ItemSpecialEffectType.HealSelf;
    [SerializeField] private int value = 1;

    [Header("Chance")]
    [Range(0, 100)]
    [SerializeField] private int chancePercent = 100;

    [Header("Display")]
    [TextArea(2, 4)]
    [SerializeField] private string customDescription = "";

    [TextArea(1, 3)]
    [SerializeField] private string activationMessage = "";

    public ItemSpecialEffectTrigger Trigger
    {
        get
        {
            return trigger;
        }
    }

    public ItemSpecialEffectType EffectType
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

    public int ChancePercent
    {
        get
        {
            return Mathf.Clamp(chancePercent, 0, 100);
        }
    }

    public string ActivationMessage
    {
        get
        {
            return activationMessage;
        }
    }

    public bool PassesChance()
    {
        if (ChancePercent <= 0)
        {
            return false;
        }

        if (ChancePercent >= 100)
        {
            return true;
        }

        int roll = UnityEngine.Random.Range(0, 100);
        return roll < ChancePercent;
    }

    public string GetDescription()
    {
        if (!string.IsNullOrWhiteSpace(customDescription))
        {
            return customDescription;
        }

        string chanceText = "";

        if (ChancePercent < 100)
        {
            chanceText = " (" + ChancePercent + "% chance)";
        }

        return GetTriggerText() + ": " + GetEffectText() + chanceText;
    }

    private string GetTriggerText()
    {
        if (trigger == ItemSpecialEffectTrigger.OnUse)
        {
            return "On use";
        }

        if (trigger == ItemSpecialEffectTrigger.OnHit)
        {
            return "On hit";
        }

        if (trigger == ItemSpecialEffectTrigger.OnKill)
        {
            return "On kill";
        }

        return trigger.ToString();
    }

    private string GetEffectText()
    {
        if (effectType == ItemSpecialEffectType.BonusDamage)
        {
            return "+" + value + " damage";
        }

        if (effectType == ItemSpecialEffectType.HealSelf)
        {
            return "heal " + value + " HP";
        }

        if (effectType == ItemSpecialEffectType.RestoreHunger)
        {
            return "restore " + value + " hunger";
        }

        if (effectType == ItemSpecialEffectType.RestoreThirst)
        {
            return "restore " + value + " thirst";
        }

        if (effectType == ItemSpecialEffectType.MessageOnly)
        {
            return "special effect";
        }

        return effectType.ToString();
    }
}