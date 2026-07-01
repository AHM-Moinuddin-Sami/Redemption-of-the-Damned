/*
 * ItemTextFormatter
 * -----------------
 * Builds TextMeshPro rich-text strings for item names, rarity names, affixes,
 * stat modifiers, and unique flavour text.
 *
 * This is a display-only helper.
 * It does not change item stats, item rarity, inventory logic, or equipment logic.
 *
 * Current responsibilities:
 * - color item names by rarity
 * - color rarity labels
 * - color affix names
 * - color positive and negative stat modifiers
 * - color unique flavour text
 *
 * Important:
 * This assumes the target TextMeshPro text component has Rich Text enabled.
 */

public static class ItemTextFormatter
{
    private const string NormalColor = "#C8C8C8";
    private const string MagicColor = "#4DA6FF";
    private const string RareColor = "#FFD24A";
    private const string UniqueColor = "#FF9F43";
    private const string SpecialEffectColor = "#FFB86C";

    private const string AffixColor = "#D6B4FF";
    private const string PositiveStatColor = "#7CFF7C";
    private const string NegativeStatColor = "#FF7777";
    private const string FlavorColor = "#D9A066";

    public static string FormatItemName(ItemInstance itemInstance)
    {
        if (itemInstance == null)
        {
            return WrapColor("Unknown Item", NormalColor);
        }

        return WrapColor(itemInstance.GetDisplayName(), GetRarityColor(itemInstance.Rarity));
    }

    public static string FormatItemName(ItemDefinition itemDefinition, int quantity)
    {
        if (itemDefinition == null)
        {
            return WrapColor("Unknown Item", NormalColor);
        }

        ItemInstance temporaryItem = new ItemInstance(itemDefinition, quantity);
        return FormatItemName(temporaryItem);
    }

    public static string FormatRarity(ItemRarity rarity)
    {
        return WrapColor(rarity.ToString(), GetRarityColor(rarity));
    }

    public static string FormatAffixName(ItemAffixDefinition affixDefinition)
    {
        if (affixDefinition == null)
        {
            return WrapColor("Unknown Affix", AffixColor);
        }

        return WrapColor(affixDefinition.DisplayName, AffixColor);
    }

    public static string FormatStatModifier(StatModifier modifier)
    {
        if (modifier == null)
        {
            return "";
        }

        string sign = modifier.Value >= 0 ? "+" : "";
        string text = modifier.StatType + " " + sign + modifier.Value;
        string color = modifier.Value >= 0 ? PositiveStatColor : NegativeStatColor;

        return WrapColor(text, color);
    }

    public static string FormatUniqueFlavorText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return "";
        }

        return WrapColor("<i>" + text + "</i>", FlavorColor);
    }

    public static string FormatSpecialEffectDescription(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return "";
        }

        return WrapColor(text, SpecialEffectColor);
    }

    private static string GetRarityColor(ItemRarity rarity)
    {
        if (rarity == ItemRarity.Magic)
        {
            return MagicColor;
        }

        if (rarity == ItemRarity.Rare)
        {
            return RareColor;
        }

        if (rarity == ItemRarity.Unique)
        {
            return UniqueColor;
        }

        return NormalColor;
    }

    private static string WrapColor(string text, string colorHex)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            text = "";
        }

        return "<color=" + colorHex + ">" + text + "</color>";
    }
}