/*
 * ConsumableEffectType
 * --------------------
 * Defines what kind of effect a consumable item applies when used.
 *
 * Current supported effects:
 * - HealHealth: restores HP
 * - RestoreHunger: restores hunger
 * - RestoreThirst: restores thirst
 *
 * This allows food, drinks, potions, and other consumables to share the same
 * basic item-use pipeline.
 *
 * Later this can expand into:
 * - RestoreSanity
 * - CurePoison
 * - ApplyBuff
 * - RemoveDebuff
 * - Teleport
 * - RevealMap
 * - DamageUser
 */

public enum ConsumableEffectType
{
    HealHealth,
    RestoreHunger,
    RestoreThirst
}