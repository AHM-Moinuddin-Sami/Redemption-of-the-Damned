/*
 * ItemRarity
 * ----------
 * Defines the rarity tier of a runtime item instance.
 *
 * Current rarity behavior:
 * - Normal: no generated affixes
 * - Magic: usually 1 generated affix
 * - Rare: usually 2 generated affixes
 * - Unique: hand-authored item with fixed stats and flavour
 *
 * Important:
 * Unique items are not randomly rolled rare items.
 * They are authored through ItemDefinition and should have custom names,
 * custom stat modifiers, and custom flavour text.
 */

public enum ItemRarity
{
    Normal,
    Magic,
    Rare,
    Unique
}