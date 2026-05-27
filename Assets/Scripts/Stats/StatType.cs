/*
 * StatType
 * --------
 * Defines the different actor stats that can be modified by equipment,
 * character classes, effects, buffs, debuffs, and future item affixes.
 *
 * Current stats:
 * - MaxHealth: future use for health scaling
 * - AttackDamage: used now by ActorCombat
 * - Armor: used later for reducing incoming physical damage
 *
 * This enum is intentionally small for now.
 * Later, this can expand into:
 * - Strength
 * - Dexterity
 * - Intelligence
 * - Willpower
 * - Accuracy
 * - Evasion
 * - FireResistance
 * - PoisonResistance
 * - SanityResistance
 * - HungerRate
 * - ThirstRate
 */

public enum StatType
{
    MaxHealth,
    AttackDamage,
    Armor
}