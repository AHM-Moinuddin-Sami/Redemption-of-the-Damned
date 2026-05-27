using System.Collections.Generic;
using UnityEngine;

/*
 * ActorStats
 * ----------
 * Stores and recalculates an actor's current gameplay stats.
 *
 * This script separates base stats from final calculated stats.
 *
 * Base stats:
 * - values the actor has naturally
 *
 * Current stats:
 * - base stats plus equipment modifiers
 *
 * Current responsibilities:
 * - store base max health
 * - store base attack damage
 * - store base armor
 * - recalculate final stats from equipped items
 * - provide stat values to other systems
 *
 * Current modifier model:
 * - only flat modifiers are supported
 * - equipment adds directly to current stat values
 *
 * Example:
 * Base Attack Damage: 3
 * Rusty Sword: +4 Attack Damage
 * Final Attack Damage: 7
 *
 * Later this can expand into:
 * - class/background stat bonuses
 * - level-up bonuses
 * - temporary buffs/debuffs
 * - hunger/thirst penalties
 * - sanity effects
 * - Path of Exile-style flat/increased/more modifier layers
 */

public class ActorStats : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private int baseMaxHealth = 20;
    [SerializeField] private int baseAttackDamage = 3;
    [SerializeField] private int baseArmor = 0;

    private readonly Dictionary<StatType, int> currentStats = new Dictionary<StatType, int>();

    private void Awake()
    {
        RecalculateWithoutEquipment();
    }

    public int GetStat(StatType statType)
    {
        if (!currentStats.ContainsKey(statType))
        {
            return 0;
        }

        return currentStats[statType];
    }

    public void RecalculateFromEquipment(ActorEquipment actorEquipment)
    {
        // Start from base values every time so old equipment bonuses do not stack forever.
        RecalculateWithoutEquipment();

        if (actorEquipment == null)
        {
            return;
        }

        IReadOnlyList<ItemInstance> equippedItems = actorEquipment.GetEquippedItems();

        for (int i = 0; i < equippedItems.Count; i++)
        {
            ApplyItemModifiers(equippedItems[i]);
        }

        PrintStatsDebug();
    }

    public void RecalculateWithoutEquipment()
    {
        currentStats.Clear();

        currentStats[StatType.MaxHealth] = baseMaxHealth;
        currentStats[StatType.AttackDamage] = baseAttackDamage;
        currentStats[StatType.Armor] = baseArmor;
    }

    private void ApplyItemModifiers(ItemInstance itemInstance)
    {
        if (itemInstance == null || itemInstance.Definition == null)
        {
            return;
        }

        IReadOnlyList<StatModifier> modifiers = itemInstance.Definition.StatModifiers;

        for (int i = 0; i < modifiers.Count; i++)
        {
            ApplyModifier(modifiers[i]);
        }
    }

    private void ApplyModifier(StatModifier modifier)
    {
        if (modifier == null)
        {
            return;
        }

        if (!currentStats.ContainsKey(modifier.StatType))
        {
            currentStats[modifier.StatType] = 0;
        }

        currentStats[modifier.StatType] += modifier.Value;
    }

    public void PrintStatsDebug()
    {
        string statsText = gameObject.name + " stats:";
        statsText += "\n- Max Health: " + GetStat(StatType.MaxHealth);
        statsText += "\n- Attack Damage: " + GetStat(StatType.AttackDamage);
        statsText += "\n- Armor: " + GetStat(StatType.Armor);

        Debug.Log(statsText);
    }
}