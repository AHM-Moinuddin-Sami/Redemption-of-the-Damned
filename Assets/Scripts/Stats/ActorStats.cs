using System.Collections.Generic;
using UnityEngine;

/*
 * ActorStats
 * ----------
 * Stores and recalculates an actor's current gameplay stats.
 *
 * This version supports:
 * - base stats
 * - permanent modifiers from class/background
 * - permanent modifiers from level-ups
 * - equipment modifiers
 *
 * Stat calculation order:
 * 1. Start with base stats.
 * 2. Apply permanent modifiers.
 * 3. Apply equipment modifiers.
 *
 * Example:
 * Base Attack Damage: 3
 * Fighter class: +2 AttackDamage
 * Level-up bonus: +1 AttackDamage
 * Rusty Sword: +3 AttackDamage
 * Final Attack Damage: 9
 *
 * Current modifier model:
 * - only flat modifiers are supported
 *
 * Important:
 * This is still intentionally simple.
 * Later this can expand into Path of Exile-style modifier layers:
 * - flat
 * - increased
 * - more
 * - local item modifiers
 * - global modifiers
 * - conditional modifiers
 */

public class ActorStats : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private int baseMaxHealth = 20;
    [SerializeField] private int baseAttackDamage = 3;
    [SerializeField] private int baseArmor = 0;

    private readonly List<StatModifier> permanentModifiers = new List<StatModifier>();
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

    public void ClearPermanentModifiers()
    {
        permanentModifiers.Clear();
        RecalculateWithoutEquipment();
    }

    public void AddPermanentModifier(StatType statType, int value)
    {
        permanentModifiers.Add(new StatModifier(statType, value));
        RecalculateFromCurrentEquipment();
    }

    public void AddPermanentModifiers(IReadOnlyList<StatModifier> modifiers)
    {
        if (modifiers == null)
        {
            return;
        }

        for (int i = 0; i < modifiers.Count; i++)
        {
            if (modifiers[i] == null)
            {
                continue;
            }

            permanentModifiers.Add(modifiers[i]);
        }

        RecalculateFromCurrentEquipment();
    }

    public void RecalculateFromEquipment(ActorEquipment actorEquipment)
    {
        // Start from base + permanent values every time so old equipment bonuses
        // do not stack forever.
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

        ApplyPermanentModifiers();
    }

    private void RecalculateFromCurrentEquipment()
    {
        ActorEquipment actorEquipment = GetComponent<ActorEquipment>();
        RecalculateFromEquipment(actorEquipment);
    }

    private void ApplyPermanentModifiers()
    {
        for (int i = 0; i < permanentModifiers.Count; i++)
        {
            ApplyModifier(permanentModifiers[i]);
        }
    }

    private void ApplyItemModifiers(ItemInstance itemInstance)
    {
        if (itemInstance == null || itemInstance.Definition == null)
        {
            return;
        }

        List<StatModifier> modifiers = itemInstance.GetAllStatModifiers();

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

    public void SetBaseStats(int maxHealth, int attackDamage, int armor)
    {
        baseMaxHealth = Mathf.Max(1, maxHealth);
        baseAttackDamage = Mathf.Max(0, attackDamage);
        baseArmor = Mathf.Max(0, armor);

        RecalculateFromCurrentEquipment();
    }
}