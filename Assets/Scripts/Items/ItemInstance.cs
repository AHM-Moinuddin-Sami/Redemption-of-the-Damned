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
 * - store current cooldown for active items
 * - store current charges for active items
 * - provide display/debug names
 * - provide all stat modifiers from base item + generated affixes
 * - support stack quantity changes
 *
 * Cooldown rule:
 * When an item is used, cooldown starts immediately.
 * The first cooldown tick is skipped so the item does not lose one cooldown turn
 * on the same turn it was activated.
 *
 * Charge rule:
 * MaxCharges <= 0 means the item does not use charges.
 */

public class ItemInstance
{
    private readonly List<ItemAffixDefinition> rolledAffixes = new List<ItemAffixDefinition>();

    public ItemDefinition Definition { get; private set; }
    public int Quantity { get; private set; }
    public ItemRarity Rarity { get; private set; }

    public int CurrentCooldownTurns { get; private set; }
    public int CurrentCharges { get; private set; }

    private bool cooldownStartedThisTurn;

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

    public bool IsOnCooldown
    {
        get
        {
            return CurrentCooldownTurns > 0;
        }
    }

    public bool UsesCharges
    {
        get
        {
            return Definition != null && Definition.UsesCharges;
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

        InitializeUseState();
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

        InitializeUseState();
        ApplyDefinitionRules();
    }

    public bool CanUse(out string failureMessage)
    {
        failureMessage = "";

        if (Definition == null)
        {
            failureMessage = "Unknown item cannot be used.";
            return false;
        }

        if (!Definition.CanBeUsedDirectly)
        {
            failureMessage = GetDisplayName() + " cannot be used.";
            return false;
        }

        if (IsOnCooldown)
        {
            failureMessage = GetDisplayName() + " is on cooldown for " + CurrentCooldownTurns + " more turns.";
            return false;
        }

        if (UsesCharges && CurrentCharges <= 0)
        {
            failureMessage = GetDisplayName() + " has no charges left.";
            return false;
        }

        return true;
    }

    public void SpendUse()
    {
        if (Definition == null)
        {
            return;
        }

        if (UsesCharges && CurrentCharges > 0)
        {
            CurrentCharges--;
        }

        StartCooldown(Definition.UseCooldownTurns);
    }

    public bool ShouldBeRemovedBecauseChargesEmpty()
    {
        if (Definition == null)
        {
            return false;
        }

        if (!Definition.UsesCharges)
        {
            return false;
        }

        if (!Definition.ConsumeWhenChargesEmpty)
        {
            return false;
        }

        return CurrentCharges <= 0;
    }

    public void TickUseCooldown()
    {
        if (CurrentCooldownTurns <= 0)
        {
            cooldownStartedThisTurn = false;
            return;
        }

        if (cooldownStartedThisTurn)
        {
            cooldownStartedThisTurn = false;
            return;
        }

        CurrentCooldownTurns--;
    }

    public string GetUseStateText()
    {
        if (Definition == null || !Definition.CanBeUsedDirectly)
        {
            return "";
        }

        string text = "";

        if (UsesCharges)
        {
            text += "Charges: " + CurrentCharges + "/" + Definition.MaxCharges;
        }

        if (IsOnCooldown)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                text += " | ";
            }

            text += "Cooldown: " + CurrentCooldownTurns;
        }

        return text;
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

        string useState = GetUseStateText();

        if (!string.IsNullOrWhiteSpace(useState))
        {
            text += " " + useState;
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

    private void InitializeUseState()
    {
        CurrentCooldownTurns = 0;
        cooldownStartedThisTurn = false;

        if (Definition != null && Definition.UsesCharges)
        {
            CurrentCharges = Definition.MaxCharges;
        }
        else
        {
            CurrentCharges = 0;
        }
    }

    private void StartCooldown(int cooldownTurns)
    {
        int safeCooldown = Mathf.Max(0, cooldownTurns);

        if (safeCooldown <= 0)
        {
            return;
        }

        CurrentCooldownTurns = safeCooldown;
        cooldownStartedThisTurn = true;
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