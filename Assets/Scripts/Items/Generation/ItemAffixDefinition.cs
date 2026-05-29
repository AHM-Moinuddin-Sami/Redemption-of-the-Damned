using System.Collections.Generic;
using UnityEngine;

/*
 * ItemAffixDefinition
 * -------------------
 * Defines one possible generated item affix.
 *
 * Example affixes:
 * - Sharp: AttackDamage +1
 * - Brutal: AttackDamage +2
 * - Reinforced: Armor +1
 * - Sturdy: MaxHealth +3
 *
 * Current responsibilities:
 * - store affix display name
 * - store minimum floor/depth requirement
 * - optionally restrict which item categories can roll this affix
 * - store flat stat modifiers granted by the affix
 *
 * Important:
 * This is intentionally simple.
 * Later this can expand into:
 * - prefix/suffix separation
 * - affix tiers
 * - local weapon damage
 * - percentage modifiers
 * - rarity restrictions
 * - item level requirements
 */

[CreateAssetMenu(menuName = "Roguelike/Items/Generation/Item Affix Definition")]
public class ItemAffixDefinition : ScriptableObject
{
    [Header("Display")]
    [SerializeField] private string displayName = "New Affix";

    [Header("Requirements")]
    [SerializeField] private int minimumFloor = 1;
    [SerializeField] private List<ItemCategory> allowedCategories = new List<ItemCategory>();

    [Header("Stat Modifiers")]
    [SerializeField] private List<StatModifier> statModifiers = new List<StatModifier>();

    public string DisplayName
    {
        get
        {
            return displayName;
        }
    }

    public IReadOnlyList<StatModifier> StatModifiers
    {
        get
        {
            return statModifiers;
        }
    }

    public bool CanApplyTo(ItemDefinition itemDefinition, int floorNumber)
    {
        if (itemDefinition == null)
        {
            return false;
        }

        if (!itemDefinition.IsEquipment)
        {
            return false;
        }

        if (floorNumber < Mathf.Max(1, minimumFloor))
        {
            return false;
        }

        // Empty category list means the affix can apply to any equipment item.
        if (allowedCategories == null || allowedCategories.Count == 0)
        {
            return true;
        }

        return allowedCategories.Contains(itemDefinition.Category);
    }
}