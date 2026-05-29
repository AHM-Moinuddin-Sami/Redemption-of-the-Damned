using System.Collections.Generic;
using UnityEngine;

/*
 * ItemInstance
 * ------------
 * Represents one actual item copy during a run.
 *
 * ItemDefinition is the authored template.
 * ItemInstance is the runtime item.
 *
 * Current responsibilities:
 * - store item definition
 * - store stack quantity
 * - store rarity
 * - store rolled affixes
 * - preserve unique item identity
 * - provide display/debug names
 * - provide all stat modifiers from base item + generated affixes
 * - support stack quantity changes
 *
 * Unique item rule:
 * If the ItemDefinition is marked unique, the ItemInstance becomes Unique and
 * clears all random rolled affixes.
 */

public class ItemInstance
{
    private readonly List<ItemAffixDefinition> rolledAffixes = new List<ItemAffixDefinition>();

    public ItemDefinition Definition { get; private set; }
    public int Quantity { get; private set; }
    public ItemRarity Rarity { get; private set; }

    public IReadOnlyList<ItemAffixDefinition> RolledAffixes
    {
        get
        {
            return rolledAffixes;
        }
    }

    public bool IsUnique
    {
        get
        {
            return Definition != null && Definition.IsUnique;
        }
    }

    public bool IsStackable
    {
        get
        {
            return Definition != null && Definition.Stackable;
        }
    }

    public bool IsEquipment
    {
        get
        {
            return Definition != null && Definition.IsEquipment;
        }
    }

    public ItemCategory Category
    {
        get
        {
            if (Definition == null)
            {
                return ItemCategory.Junk;
            }

            return Definition.Category;
        }
    }

    public ItemInstance(ItemDefinition definition, int quantity)
    {
        Definition = definition;
        Quantity = Mathf.Max(1, quantity);
        Rarity = ItemRarity.Normal;

        ApplyDefinitionRules();
    }

    public ItemInstance(
        ItemDefinition definition,
        int quantity,
        ItemRarity rarity,
        IReadOnlyList<ItemAffixDefinition> newRolledAffixes)
    {
        Definition = definition;
        Quantity = Mathf.Max(1, quantity);
        Rarity = rarity;

        if (newRolledAffixes != null)
        {
            for (int i = 0; i < newRolledAffixes.Count; i++)
            {
                if (newRolledAffixes[i] == null)
                {
                    continue;
                }

                rolledAffixes.Add(newRolledAffixes[i]);
            }
        }

        ApplyDefinitionRules();
    }

    public string GetDisplayName()
    {
        if (Definition == null)
        {
            return "Unknown Item";
        }

        string baseName = Definition.DisplayName;

        if (Quantity > 1)
        {
            baseName += " x" + Quantity;
        }

        if (Rarity == ItemRarity.Normal)
        {
            return baseName;
        }

        if (Rarity == ItemRarity.Unique)
        {
            return baseName;
        }

        return Rarity + " " + baseName;
    }

    public string GetDebugDescription()
    {
        string text = GetDisplayName() + " [" + Category + "]";

        if (Rarity == ItemRarity.Unique)
        {
            text += " Unique";
        }

        if (rolledAffixes.Count > 0)
        {
            text += " Affixes:";

            for (int i = 0; i < rolledAffixes.Count; i++)
            {
                text += " " + rolledAffixes[i].DisplayName;

                if (i < rolledAffixes.Count - 1)
                {
                    text += ",";
                }
            }
        }

        return text;
    }

    public bool CanStackWith(ItemDefinition otherDefinition)
    {
        if (Definition == null || otherDefinition == null)
        {
            return false;
        }

        if (!IsStackable)
        {
            return false;
        }

        if (Rarity != ItemRarity.Normal || rolledAffixes.Count > 0)
        {
            return false;
        }

        if (Definition != otherDefinition)
        {
            return false;
        }

        return Quantity < Definition.MaxStackSize;
    }

    public int AddQuantity(int amount)
    {
        if (!IsStackable)
        {
            return amount;
        }

        int safeAmount = Mathf.Max(0, amount);
        int space = Definition.MaxStackSize - Quantity;
        int amountToAdd = Mathf.Min(space, safeAmount);

        Quantity += amountToAdd;

        return safeAmount - amountToAdd;
    }

    public int RemoveQuantity(int amount)
    {
        int safeAmount = Mathf.Max(0, amount);
        int removedAmount = Mathf.Min(Quantity, safeAmount);

        Quantity -= removedAmount;

        return removedAmount;
    }

    public List<StatModifier> GetAllStatModifiers()
    {
        List<StatModifier> allModifiers = new List<StatModifier>();

        if (Definition != null)
        {
            IReadOnlyList<StatModifier> baseModifiers = Definition.StatModifiers;

            for (int i = 0; i < baseModifiers.Count; i++)
            {
                if (baseModifiers[i] == null)
                {
                    continue;
                }

                allModifiers.Add(baseModifiers[i]);
            }
        }

        for (int i = 0; i < rolledAffixes.Count; i++)
        {
            if (rolledAffixes[i] == null)
            {
                continue;
            }

            IReadOnlyList<StatModifier> affixModifiers = rolledAffixes[i].StatModifiers;

            for (int j = 0; j < affixModifiers.Count; j++)
            {
                if (affixModifiers[j] == null)
                {
                    continue;
                }

                allModifiers.Add(affixModifiers[j]);
            }
        }

        return allModifiers;
    }

    private void ApplyDefinitionRules()
    {
        if (Definition == null)
        {
            Rarity = ItemRarity.Normal;
            rolledAffixes.Clear();
            return;
        }

        if (Definition.IsUnique)
        {
            Rarity = ItemRarity.Unique;
            rolledAffixes.Clear();
            Quantity = 1;
            return;
        }

        if (!IsEquipment || IsStackable)
        {
            Rarity = ItemRarity.Normal;
            rolledAffixes.Clear();
        }
    }
}