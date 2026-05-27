/*
 * ItemCategory
 * ------------
 * Defines the broad gameplay category of an item.
 *
 * This is used so the game can understand what kind of item it is dealing with.
 * For example, a sword and a potion are both items, but they should not behave
 * the same way.
 *
 * Current usage:
 * - ItemDefinition uses this to classify items.
 * - Inventory debug output can show item categories.
 * - Later systems can check item category before using/equipping items.
 *
 * Later this can support:
 * - equipment rules
 * - consumable item usage
 * - food/hunger systems
 * - shop filters
 * - loot table restrictions
 * - rarity and affix rules
 */

public enum ItemCategory
{
    Junk,
    Currency,
    Food,
    Consumable,
    Weapon,
    Armor,
    Shield,
    Quest
}