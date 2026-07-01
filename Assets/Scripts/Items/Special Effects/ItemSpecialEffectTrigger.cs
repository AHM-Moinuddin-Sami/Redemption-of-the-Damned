/*
 * ItemSpecialEffectTrigger
 * ------------------------
 * Defines when an item special effect should activate.
 *
 * Current triggers:
 * - OnUse: activates when the item is used from the inventory.
 * - OnHit: activates when the actor successfully attacks a target.
 * - OnKill: activates when the actor kills a target.
 *
 * Later this can expand into:
 * - OnEquip
 * - OnUnequip
 * - OnTakeDamage
 * - OnTurnStart
 * - OnTurnEnd
 * - OnLowHealth
 */

public enum ItemSpecialEffectTrigger
{
    OnUse,
    OnHit,
    OnKill
}