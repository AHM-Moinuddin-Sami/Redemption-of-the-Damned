/*
 * EquipmentSlotType
 * -----------------
 * Defines which equipment slot an item can be equipped into.
 *
 * This is not used for actual equipment yet.
 * It is added now so ItemDefinition can already store equipment-related data.
 *
 * Current usage:
 * - ItemDefinition stores an equipment slot.
 * - Weapons, armor, and shields can declare where they belong.
 *
 * Later this will be used by:
 * - ActorEquipment
 * - equipment UI
 * - stat modifiers
 * - weapon rules
 * - two-handed weapon handling
 */

public enum EquipmentSlotType
{
    None,
    MainHand,
    OffHand,
    Head,
    Body,
    Hands,
    Feet,
    Neck,
    Ring
}