using System.Collections.Generic;
using UnityEngine;

/*
 * ItemDefinition
 * --------------
 * Stores the authored template data for an item type.
 *
 * This is a ScriptableObject asset. It represents the base data for an item,
 * not a specific dropped copy.
 *
 * Current responsibilities:
 * - store display name
 * - store description text
 * - store icon sprite
 * - store broad item category
 * - store stack settings
 * - store equipment slot information
 * - store basic flat stat modifiers
 * - store consumable effects
 *
 * The description text is now used by item inspection/tooltips.
 *
 * Important:
 * This does not store random affixes, rarity, rolled damage ranges, or unique
 * item logic yet. Those belong to ItemInstance and future item generation.
 */

[CreateAssetMenu(menuName = "Roguelike/Items/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    [Header("Display")]
    [SerializeField] private string displayName = "New Item";
    [TextArea(2, 5)]
    [SerializeField] private string description = "";
    [SerializeField] private Sprite iconSprite;

    [Header("Classification")]
    [SerializeField] private ItemCategory category = ItemCategory.Junk;

    [Header("Stacking")]
    [SerializeField] private bool stackable = false;
    [SerializeField] private int maxStackSize = 1;

    [Header("Equipment")]
    [SerializeField] private EquipmentSlotType equipmentSlot = EquipmentSlotType.None;
    [SerializeField] private bool twoHanded = false;

    [Header("Stat Modifiers")]
    [SerializeField] private List<StatModifier> statModifiers = new List<StatModifier>();

    [Header("Consumable Effects")]
    [SerializeField] private List<ConsumableEffect> consumableEffects = new List<ConsumableEffect>();

    public string DisplayName
    {
        get
        {
            return displayName;
        }
    }

    public string Description
    {
        get
        {
            return description;
        }
    }

    public Sprite IconSprite
    {
        get
        {
            return iconSprite;
        }
    }

    public ItemCategory Category
    {
        get
        {
            return category;
        }
    }

    public bool Stackable
    {
        get
        {
            return stackable;
        }
    }

    public int MaxStackSize
    {
        get
        {
            return Mathf.Max(1, maxStackSize);
        }
    }

    public EquipmentSlotType EquipmentSlot
    {
        get
        {
            return equipmentSlot;
        }
    }

    public bool TwoHanded
    {
        get
        {
            return twoHanded;
        }
    }

    public IReadOnlyList<StatModifier> StatModifiers
    {
        get
        {
            return statModifiers;
        }
    }

    public IReadOnlyList<ConsumableEffect> ConsumableEffects
    {
        get
        {
            return consumableEffects;
        }
    }

    public bool IsEquipment
    {
        get
        {
            return category == ItemCategory.Weapon ||
                   category == ItemCategory.Armor ||
                   category == ItemCategory.Shield;
        }
    }

    public bool IsConsumable
    {
        get
        {
            return category == ItemCategory.Consumable ||
                   category == ItemCategory.Food;
        }
    }

    private void OnValidate()
    {
        if (IsEquipment || category == ItemCategory.Quest)
        {
            stackable = false;
            maxStackSize = 1;
        }

        if (!stackable)
        {
            maxStackSize = 1;
        }

        if (maxStackSize < 1)
        {
            maxStackSize = 1;
        }

        if (!IsEquipment)
        {
            equipmentSlot = EquipmentSlotType.None;
            twoHanded = false;
        }

        if (category == ItemCategory.Shield)
        {
            equipmentSlot = EquipmentSlotType.OffHand;
            twoHanded = false;
        }

        if (!IsConsumable && consumableEffects.Count > 0)
        {
            consumableEffects.Clear();
        }
    }
}