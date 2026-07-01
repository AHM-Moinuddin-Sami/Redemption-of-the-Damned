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
 * - mark an item as Unique
 * - store unique item flavour text
 * - store a unique ID so a unique item can be limited to one copy per run
 *
 * Unique item rule:
 * A unique item is hand-authored. It does not roll random affixes.
 * Its special identity comes from its authored name, description, flavour text,
 * and stat modifiers.
 *
 * Important:
 * This does not yet support special triggered effects like:
 * - chance to poison
 * - heal on kill
 * - fire aura
 * - teleport on hit
 *
 * Those should be added later through a separate item effect system.
 */

[CreateAssetMenu(menuName = "Roguelike/Items/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    [Header("Display")]
    [SerializeField] private string displayName = "New Item";

    [TextArea(2, 5)]
    [SerializeField] private string description = "";

    [SerializeField] private Sprite iconSprite;

    [Header("Unique Item")]
    [SerializeField] private bool uniqueItem = false;

    [Tooltip("Used to prevent the same unique item from being generated multiple times in one run. If empty, the asset name is used.")]
    [SerializeField] private string uniqueId = "";

    [TextArea(2, 5)]
    [SerializeField] private string uniqueFlavorText = "";

    [Header("Classification")]
    [SerializeField] private ItemCategory category = ItemCategory.Junk;

    [Header("Use Settings")]
    [SerializeField] private bool canBeUsedDirectly = false;
    [SerializeField] private bool consumeOnUse = true;

    [Header("Cooldowns And Charges")]
    [SerializeField] private int useCooldownTurns = 0;
    [SerializeField] private int maxCharges = 0;
    [SerializeField] private bool consumeWhenChargesEmpty = false;

    [Header("Stacking")]
    [SerializeField] private bool stackable = false;
    [SerializeField] private int maxStackSize = 1;

    [Header("Equipment")]
    [SerializeField] private EquipmentSlotType equipmentSlot = EquipmentSlotType.None;
    [SerializeField] private bool twoHanded = false;

    [Header("Stat Modifiers")]
    [SerializeField] private List<StatModifier> statModifiers = new List<StatModifier>();

    [Header("Special Effects")]
    [SerializeField] private List<ItemSpecialEffectDefinition> specialEffects = new List<ItemSpecialEffectDefinition>();

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

    public bool IsUnique
    {
        get
        {
            return uniqueItem;
        }
    }

    public string UniqueId
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(uniqueId))
            {
                return uniqueId;
            }

            return name;
        }
    }

    public string UniqueFlavorText
    {
        get
        {
            return uniqueFlavorText;
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

    public IReadOnlyList<ItemSpecialEffectDefinition> SpecialEffects
    {
        get
        {
            return specialEffects;
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
                   category == ItemCategory.Shield ||
                   category == ItemCategory.Accessory;
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

    public bool CanBeUsedDirectly
    {
        get
        {
            return IsConsumable ||
                   canBeUsedDirectly ||
                   HasSpecialEffectTrigger(ItemSpecialEffectTrigger.OnUse);
        }
    }

    public bool ConsumeOnUse
    {
        get
        {
            return consumeOnUse;
        }
    }

    public int UseCooldownTurns
    {
        get
        {
            return Mathf.Max(0, useCooldownTurns);
        }
    }

    public int MaxCharges
    {
        get
        {
            return Mathf.Max(0, maxCharges);
        }
    }

    public bool UsesCharges
    {
        get
        {
            return MaxCharges > 0;
        }
    }

    public bool ConsumeWhenChargesEmpty
    {
        get
        {
            return consumeWhenChargesEmpty;
        }
    }

    public bool HasSpecialEffectTrigger(ItemSpecialEffectTrigger trigger)
    {
        if (specialEffects == null)
        {
            return false;
        }

        for (int i = 0; i < specialEffects.Count; i++)
        {
            if (specialEffects[i] == null)
            {
                continue;
            }

            if (specialEffects[i].Trigger == trigger)
            {
                return true;
            }
        }

        return false;
    }

    private void OnValidate()
    {
        if (uniqueItem)
        {
            stackable = false;
            maxStackSize = 1;
        }

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

        if (category == ItemCategory.Accessory && equipmentSlot == EquipmentSlotType.None)
        {
            equipmentSlot = EquipmentSlotType.Trinket;
        }

        if (!IsConsumable && consumableEffects.Count > 0)
        {
            consumableEffects.Clear();
        }

        if (!CanBeUsedDirectly)
        {
            consumeOnUse = false;
            useCooldownTurns = 0;
            maxCharges = 0;
            consumeWhenChargesEmpty = false;
        }

        if (consumeOnUse)
        {
            consumeWhenChargesEmpty = false;
        }

        if (maxCharges <= 0)
        {
            consumeWhenChargesEmpty = false;
        }
    }
}