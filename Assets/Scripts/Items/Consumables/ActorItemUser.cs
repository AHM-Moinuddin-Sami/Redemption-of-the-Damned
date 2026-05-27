using System.Collections.Generic;
using UnityEngine;

/*
 * ActorItemUser
 * -------------
 * Handles using consumable items for an actor.
 *
 * Player-facing item use messages now go to GameMessageLog.
 *
 * Current responsibilities:
 * - check whether an item can be used
 * - apply consumable effects
 * - support healing, hunger restoration, and thirst restoration
 * - remove 1 quantity after successful use
 */

[RequireComponent(typeof(ActorInventory))]
public class ActorItemUser : MonoBehaviour
{
    private ActorInventory actorInventory;
    private ActorHealth actorHealth;
    private ActorSurvival actorSurvival;

    private void Awake()
    {
        actorInventory = GetComponent<ActorInventory>();
        actorHealth = GetComponent<ActorHealth>();
        actorSurvival = GetComponent<ActorSurvival>();
    }

    public bool TryUseItem(ItemInstance itemInstance)
    {
        if (itemInstance == null || itemInstance.Definition == null)
        {
            return false;
        }

        if (!itemInstance.Definition.IsConsumable)
        {
            GameMessageLog.Write(itemInstance.GetDisplayName() + " cannot be used.");
            return false;
        }

        IReadOnlyList<ConsumableEffect> effects = itemInstance.Definition.ConsumableEffects;

        if (effects.Count == 0)
        {
            GameMessageLog.Write(itemInstance.GetDisplayName() + " has no effect.");
            return false;
        }

        bool appliedAnyEffect = ApplyEffects(effects);

        if (!appliedAnyEffect)
        {
            return false;
        }

        actorInventory.RemoveQuantity(itemInstance, 1);

        GameMessageLog.Write(gameObject.name + " uses " + itemInstance.GetDisplayName() + ".");
        actorInventory.PrintInventoryDebug();

        return true;
    }

    private bool ApplyEffects(IReadOnlyList<ConsumableEffect> effects)
    {
        bool appliedAnyEffect = false;

        for (int i = 0; i < effects.Count; i++)
        {
            if (effects[i] == null)
            {
                continue;
            }

            bool effectApplied = ApplyEffect(effects[i]);

            if (effectApplied)
            {
                appliedAnyEffect = true;
            }
        }

        return appliedAnyEffect;
    }

    private bool ApplyEffect(ConsumableEffect effect)
    {
        if (effect.EffectType == ConsumableEffectType.HealHealth)
        {
            return ApplyHealHealth(effect.Value);
        }

        if (effect.EffectType == ConsumableEffectType.RestoreHunger)
        {
            return ApplyRestoreHunger(effect.Value);
        }

        if (effect.EffectType == ConsumableEffectType.RestoreThirst)
        {
            return ApplyRestoreThirst(effect.Value);
        }

        return false;
    }

    private bool ApplyHealHealth(int amount)
    {
        if (actorHealth == null)
        {
            Debug.LogWarning(gameObject.name + " cannot be healed because it has no ActorHealth component.");
            return false;
        }

        return actorHealth.Heal(amount);
    }

    private bool ApplyRestoreHunger(int amount)
    {
        if (actorSurvival == null)
        {
            Debug.LogWarning(gameObject.name + " cannot restore hunger because it has no ActorSurvival component.");
            return false;
        }

        return actorSurvival.RestoreHunger(amount);
    }

    private bool ApplyRestoreThirst(int amount)
    {
        if (actorSurvival == null)
        {
            Debug.LogWarning(gameObject.name + " cannot restore thirst because it has no ActorSurvival component.");
            return false;
        }

        return actorSurvival.RestoreThirst(amount);
    }
}