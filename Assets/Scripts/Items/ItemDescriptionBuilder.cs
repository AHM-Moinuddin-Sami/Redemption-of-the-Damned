using System.Collections.Generic;
using System.Text;

/*
 * ItemDescriptionBuilder
 * ----------------------
 * Builds readable text descriptions for items.
 *
 * This version supports:
 * - colored item names
 * - colored rarity text
 * - colored affix names
 * - colored positive/negative stat modifiers
 * - unique item flavour text
 *
 * Current responsibilities:
 * - show item name
 * - show rarity
 * - show description text
 * - show category
 * - show equipment slot
 * - show base stat modifiers
 * - show generated affixes and their modifiers
 * - show unique flavour text
 * - show consumable effects
 */

public static class ItemDescriptionBuilder
{
    public static string Build(ItemInstance itemInstance)
    {
        if (itemInstance == null || itemInstance.Definition == null)
        {
            return "Unknown Item";
        }

        StringBuilder builder = new StringBuilder();

        builder.AppendLine(ItemTextFormatter.FormatItemName(itemInstance));
        builder.AppendLine("Rarity: " + ItemTextFormatter.FormatRarity(itemInstance.Rarity));

        ItemDefinition itemDefinition = itemInstance.Definition;

        if (!string.IsNullOrWhiteSpace(itemDefinition.Description))
        {
            builder.AppendLine(itemDefinition.Description);
        }

        builder.AppendLine("Category: " + itemDefinition.Category);

        if (itemDefinition.IsEquipment)
        {
            builder.AppendLine("Slot: " + itemDefinition.EquipmentSlot);

            if (itemDefinition.TwoHanded)
            {
                builder.AppendLine("Two-handed");
            }
        }

        AppendStatModifiers(builder, "Base Modifiers:", itemDefinition.StatModifiers);
        AppendAffixes(builder, itemInstance.RolledAffixes);
        AppendUniqueFlavorText(builder, itemDefinition);
        AppendConsumableEffects(builder, itemDefinition.ConsumableEffects);

        return builder.ToString().TrimEnd();
    }

    public static string Build(ItemDefinition itemDefinition, int quantity)
    {
        ItemInstance temporaryInstance = new ItemInstance(itemDefinition, quantity);
        return Build(temporaryInstance);
    }

    private static void AppendAffixes(StringBuilder builder, IReadOnlyList<ItemAffixDefinition> affixes)
    {
        if (affixes == null || affixes.Count == 0)
        {
            return;
        }

        builder.AppendLine("Rolled Affixes:");

        for (int i = 0; i < affixes.Count; i++)
        {
            if (affixes[i] == null)
            {
                continue;
            }

            builder.AppendLine("- " + ItemTextFormatter.FormatAffixName(affixes[i]));
            AppendStatModifiers(builder, "  Modifiers:", affixes[i].StatModifiers);
        }
    }

    private static void AppendUniqueFlavorText(StringBuilder builder, ItemDefinition itemDefinition)
    {
        if (itemDefinition == null)
        {
            return;
        }

        if (!itemDefinition.IsUnique)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(itemDefinition.UniqueFlavorText))
        {
            return;
        }

        builder.AppendLine(ItemTextFormatter.FormatUniqueFlavorText(itemDefinition.UniqueFlavorText));
    }

    private static void AppendStatModifiers(
        StringBuilder builder,
        string header,
        IReadOnlyList<StatModifier> modifiers)
    {
        if (modifiers == null || modifiers.Count == 0)
        {
            return;
        }

        builder.AppendLine(header);

        for (int i = 0; i < modifiers.Count; i++)
        {
            if (modifiers[i] == null)
            {
                continue;
            }

            builder.AppendLine("- " + ItemTextFormatter.FormatStatModifier(modifiers[i]));
        }
    }

    private static void AppendConsumableEffects(StringBuilder builder, IReadOnlyList<ConsumableEffect> effects)
    {
        if (effects == null || effects.Count == 0)
        {
            return;
        }

        builder.AppendLine("Effects:");

        for (int i = 0; i < effects.Count; i++)
        {
            if (effects[i] == null)
            {
                continue;
            }

            builder.AppendLine("- " + effects[i].EffectType + " +" + effects[i].Value);
        }
    }
}