using System.Collections.Generic;
using UnityEngine;

/*
 * ActorItemSpecialEffectHandler
 * -----------------------------
 * Applies item special effects.
 *
 * This script belongs on an actor that can use or benefit from item special
 * effects. For now, put it on the player prefab.
 *
 * Current responsibilities:
 * - read equipped item effects for OnHit and OnKill
 * - apply direct item OnUse effects from an inventory item
 * - add bonus damage on hit
 * - heal the user
 * - restore hunger/thirst
 * - print optional flavour messages
 *
 * Current supported effects:
 * - OnUse + HealSelf
 * - OnUse + RestoreHunger
 * - OnUse + RestoreThirst
 * - OnUse + MessageOnly
 * - OnHit + BonusDamage
 * - OnHit + HealSelf
 * - OnHit + MessageOnly
 * - OnKill + HealSelf
 * - OnKill + MessageOnly
 */

[RequireComponent(typeof(ActorEquipment))]
[RequireComponent(typeof(ActorHealth))]
[RequireComponent(typeof(ActorGridEntity))]
public class ActorItemSpecialEffectHandler : MonoBehaviour
{
    private ActorEquipment actorEquipment;
    private ActorHealth actorHealth;
    private ActorSurvival actorSurvival;
    private ActorGridEntity actorGridEntity;

    private void Awake()
    {
        actorEquipment = GetComponent<ActorEquipment>();
        actorHealth = GetComponent<ActorHealth>();
        actorSurvival = GetComponent<ActorSurvival>();
        actorGridEntity = GetComponent<ActorGridEntity>();
    }

    public bool ApplyItemUseEffects(ItemInstance usedItem)
    {
        if (usedItem == null || usedItem.Definition == null)
        {
            return false;
        }

        bool usedSuccessfully = false;
        IReadOnlyList<ItemSpecialEffectDefinition> effects = usedItem.Definition.SpecialEffects;

        for (int i = 0; i < effects.Count; i++)
        {
            ItemSpecialEffectDefinition effect = effects[i];

            if (effect == null)
            {
                continue;
            }

            if (effect.Trigger != ItemSpecialEffectTrigger.OnUse)
            {
                continue;
            }

            if (!effect.PassesChance())
            {
                usedSuccessfully = true;
                continue;
            }

            bool effectApplied = ApplyEffect(effect, null);

            if (effectApplied)
            {
                usedSuccessfully = true;
            }
        }

        return usedSuccessfully;
    }

    public int ModifyOutgoingAttackDamage(ActorGridEntity target, int baseDamage)
    {
        int finalDamage = Mathf.Max(0, baseDamage);
        List<ItemSpecialEffectDefinition> effects = GetEquippedSpecialEffects();

        for (int i = 0; i < effects.Count; i++)
        {
            ItemSpecialEffectDefinition effect = effects[i];

            if (effect == null)
            {
                continue;
            }

            if (effect.Trigger != ItemSpecialEffectTrigger.OnHit)
            {
                continue;
            }

            if (effect.EffectType != ItemSpecialEffectType.BonusDamage)
            {
                continue;
            }

            if (!effect.PassesChance())
            {
                continue;
            }

            finalDamage += Mathf.Max(0, effect.Value);
            WriteActivationMessage(effect);
        }

        return finalDamage;
    }

    public void OnHitTarget(ActorGridEntity target)
    {
        ApplyTriggeredEquippedEffects(ItemSpecialEffectTrigger.OnHit, target);
    }

    public void OnKilledTarget(ActorGridEntity target)
    {
        ApplyTriggeredEquippedEffects(ItemSpecialEffectTrigger.OnKill, target);
    }

    private void ApplyTriggeredEquippedEffects(ItemSpecialEffectTrigger trigger, ActorGridEntity target)
    {
        List<ItemSpecialEffectDefinition> effects = GetEquippedSpecialEffects();

        for (int i = 0; i < effects.Count; i++)
        {
            ItemSpecialEffectDefinition effect = effects[i];

            if (effect == null)
            {
                continue;
            }

            if (effect.Trigger != trigger)
            {
                continue;
            }

            if (effect.EffectType == ItemSpecialEffectType.BonusDamage)
            {
                continue;
            }

            if (!effect.PassesChance())
            {
                continue;
            }

            ApplyEffect(effect, target);
        }
    }

    private bool ApplyEffect(ItemSpecialEffectDefinition effect, ActorGridEntity target)
    {
        if (effect == null)
        {
            return false;
        }

        if (effect.EffectType == ItemSpecialEffectType.HealSelf)
        {
            bool healed = HealSelf(effect.Value);

            if (healed)
            {
                WriteActivationMessage(effect);
            }

            return healed;
        }

        if (effect.EffectType == ItemSpecialEffectType.RestoreHunger)
        {
            bool restored = RestoreHunger(effect.Value);

            if (restored)
            {
                WriteActivationMessage(effect);
            }

            return restored;
        }

        if (effect.EffectType == ItemSpecialEffectType.RestoreThirst)
        {
            bool restored = RestoreThirst(effect.Value);

            if (restored)
            {
                WriteActivationMessage(effect);
            }

            return restored;
        }

        if (effect.EffectType == ItemSpecialEffectType.MessageOnly)
        {
            WriteActivationMessage(effect);
            return true;
        }

        return false;
    }

    private bool HealSelf(int amount)
    {
        if (actorHealth == null)
        {
            return false;
        }

        return actorHealth.Heal(Mathf.Max(0, amount));
    }

    private bool RestoreHunger(int amount)
    {
        if (actorSurvival == null)
        {
            return false;
        }

        return actorSurvival.RestoreHunger(Mathf.Max(0, amount));
    }

    private bool RestoreThirst(int amount)
    {
        if (actorSurvival == null)
        {
            return false;
        }

        return actorSurvival.RestoreThirst(Mathf.Max(0, amount));
    }

    private List<ItemSpecialEffectDefinition> GetEquippedSpecialEffects()
    {
        List<ItemSpecialEffectDefinition> effects = new List<ItemSpecialEffectDefinition>();

        if (actorEquipment == null)
        {
            return effects;
        }

        IReadOnlyList<ItemInstance> equippedItems = actorEquipment.GetEquippedItems();

        for (int i = 0; i < equippedItems.Count; i++)
        {
            AddItemEffects(effects, equippedItems[i]);
        }

        return effects;
    }

    private void AddItemEffects(List<ItemSpecialEffectDefinition> effects, ItemInstance itemInstance)
    {
        if (effects == null || itemInstance == null || itemInstance.Definition == null)
        {
            return;
        }

        IReadOnlyList<ItemSpecialEffectDefinition> itemEffects = itemInstance.Definition.SpecialEffects;

        for (int i = 0; i < itemEffects.Count; i++)
        {
            if (itemEffects[i] == null)
            {
                continue;
            }

            effects.Add(itemEffects[i]);
        }
    }

    private void WriteActivationMessage(ItemSpecialEffectDefinition effect)
    {
        if (effect == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(effect.ActivationMessage))
        {
            return;
        }

        bool actorIsPlayer = PlayerAwarenessContext.IsPlayer(actorGridEntity);
        bool actorIsVisible = PlayerAwarenessContext.CanSeeActor(actorGridEntity);

        if (!actorIsPlayer && !actorIsVisible)
        {
            return;
        }

        GameMessageLog.Write(effect.ActivationMessage);
    }
}