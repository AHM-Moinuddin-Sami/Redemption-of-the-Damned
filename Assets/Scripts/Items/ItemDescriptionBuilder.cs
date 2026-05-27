using System.Collections.Generic;
using System.Text;

/*
 * ItemDescriptionBuilder
 * ----------------------
 * Builds readable text descriptions for items.
 *
 * This class is used when the player inspects an item on the ground or later
 * when the inventory/equipment UI needs item tooltip text.
 *
 * Current responsibilities:
 * - show item name
 * - show description text
 * - show category
 * - show quantity
 * - show equipment slot
 * - show stat modifiers
 * - show consumable effects
 *
 * Important:
 * This does not create UI by itself.
 * It only builds text that other systems can display.
 *
 * Later this can expand into:
 * - rarity coloring
 * - affix text
 * - comparison against equipped item
 * - damage ranges
 * - item level
 * - flavor text
 * - value/weight
 */

public static class ItemDescriptionBuilder
{
    public static string Build(ItemInstance itemInstance)
    {
        if (itemInstance == null || itemInstance.Definition == null)
        {
            return "Unknown Item";
        }

        return Build(itemInstance.Definition, itemInstance.Quantity);
    }

    public static string Build(ItemDefinition itemDefinition, int quantity)
    {
        if (itemDefinition == null)
        {
            return "Unknown Item";
        }

        StringBuilder builder = new StringBuilder();

        builder.AppendLine(GetNameLine(itemDefinition, quantity));

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

        AppendStatModifiers(builder, itemDefinition.StatModifiers);
        AppendConsumableEffects(builder, itemDefinition.ConsumableEffects);

        return builder.ToString().TrimEnd();
    }

    private static string GetNameLine(ItemDefinition itemDefinition, int quantity)
    {
        if (quantity > 1)
        {
            return itemDefinition.DisplayName + " x" + quantity;
        }

        return itemDefinition.DisplayName;
    }

    private static void AppendStatModifiers(StringBuilder builder, IReadOnlyList<StatModifier> modifiers)
    {
        if (modifiers == null || modifiers.Count == 0)
        {
            return;
        }

        builder.AppendLine("Modifiers:");

        for (int i = 0; i < modifiers.Count; i++)
        {
            if (modifiers[i] == null)
            {
                continue;
            }

            string sign = modifiers[i].Value >= 0 ? "+" : "";
            builder.AppendLine("- " + modifiers[i].StatType + " " + sign + modifiers[i].Value);
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