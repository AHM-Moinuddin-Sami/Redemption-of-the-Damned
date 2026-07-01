/*
 * ItemSpecialEffectType
 * ---------------------
 * Defines what an item special effect does.
 *
 * Current effect types:
 * - BonusDamage: adds flat damage to an attack.
 * - HealSelf: heals the item user/wearer.
 * - RestoreHunger: restores hunger.
 * - RestoreThirst: restores thirst.
 * - MessageOnly: only prints a flavour message.
 *
 * Important:
 * BonusDamage is mainly useful for OnHit.
 * HealSelf, RestoreHunger, RestoreThirst, and MessageOnly can work well for OnUse.
 */

public enum ItemSpecialEffectType
{
    BonusDamage,
    HealSelf,
    RestoreHunger,
    RestoreThirst,
    MessageOnly
}