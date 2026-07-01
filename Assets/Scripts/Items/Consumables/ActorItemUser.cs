using System;
using System.Collections.Generic;
using UnityEngine;

/*
 * ActorItemUser
 * -------------
 * Handles item use from inventory and from equipped active items.
 *
 * Current behavior:
 * - inventory use works for food, potions, and non-equipment active items
 * - equipment active use works for equipped rings/amulets/trinkets
 * - equipped active items can use cooldowns and charges
 * - cooldowns tick for both inventory items and equipped items
 *
 * Important:
 * Equipment with OnUse effects should normally be equipped first.
 * This prevents active charms from being spammed directly from the backpack.
 */

[RequireComponent(typeof(ActorInventory))]
[RequireComponent(typeof(ActorEquipment))]
public class ActorItemUser : MonoBehaviour
{
    private ActorInventory actorInventory;
    private ActorEquipment actorEquipment;
    private ActorHealth actorHealth;
    private ActorSurvival actorSurvival;
    private ActorItemSpecialEffectHandler specialEffectHandler;

    private void Awake()
    {
        actorInventory = GetComponent<ActorInventory>();
        actorEquipment = GetComponent<ActorEquipment>();
        actorHealth = GetComponent<ActorHealth>();
        actorSurvival = GetComponent<ActorSurvival>();
        specialEffectHandler = GetComponent<ActorItemSpecialEffectHandler>();
    }

    public bool TryUseItem(ItemInstance itemInstance)
    {
        if (itemInstance == null || itemInstance.Definition == null)
        {
            return false;
        }

        if (itemInstance.IsEquipment && itemInstance.Definition.CanBeUsedDirectly)
        {
            GameMessageLog.Write("Equip " + itemInstance.GetDisplayName() + " to use its active effect.");
            return false;
        }

        return TryUseItemInternal(itemInstance, false);
    }

    public bool TryUseEquippedItem(EquipmentSlotType slot)
    {
        if (actorEquipment == null)
        {
            return false;
        }

        ItemInstance equippedItem = actorEquipment.GetEquippedItem(slot);

        if (equippedItem == null)
        {
            GameMessageLog.Write("There is no item equipped in " + slot + ".");
            return false;
        }

        return TryUseItemInternal(equippedItem, true);
    }

    public bool TryUseFirstReadyEquippedActiveItem()
    {
        if (actorEquipment == null)
        {
            return false;
        }

        IReadOnlyList<ItemInstance> equippedItems = actorEquipment.GetEquippedItems();

        bool foundActiveItem = false;
        string firstFailureMessage = "";

        for (int i = 0; i < equippedItems.Count; i++)
        {
            ItemInstance item = equippedItems[i];

            if (item == null || item.Definition == null)
            {
                continue;
            }

            if (!item.Definition.CanBeUsedDirectly)
            {
                continue;
            }

            foundActiveItem = true;

            string failureMessage;

            if (!item.CanUse(out failureMessage))
            {
                if (string.IsNullOrWhiteSpace(firstFailureMessage))
                {
                    firstFailureMessage = failureMessage;
                }

                continue;
            }

            return TryUseItemInternal(item, true);
        }

        if (!foundActiveItem)
        {
            GameMessageLog.Write("You have no equipped active item.");
            return false;
        }

        GameMessageLog.Write(firstFailureMessage);
        return false;
    }

    public void OnPlayerActionCompleted()
    {
        TickAllItemCooldowns();
    }

    private bool TryUseItemInternal(ItemInstance itemInstance, bool fromEquipment)
    {
        string failureMessage;

        if (!itemInstance.CanUse(out failureMessage))
        {
            GameMessageLog.Write(failureMessage);
            return false;
        }

        bool usedConsumableEffect = ApplyConsumableEffects(itemInstance);
        bool usedSpecialEffect = ApplySpecialUseEffects(itemInstance);

        bool usedSuccessfully = usedConsumableEffect || usedSpecialEffect;

        if (!usedSuccessfully)
        {
            GameMessageLog.Write(itemInstance.GetDisplayName() + " has no useful effect right now.");
            return false;
        }

        SpendItemUse(itemInstance, fromEquipment);
        return true;
    }

    private void SpendItemUse(ItemInstance itemInstance, bool fromEquipment)
    {
        itemInstance.SpendUse();

        if (itemInstance.Definition.ConsumeOnUse)
        {
            RemoveUsedItem(itemInstance, fromEquipment);
            return;
        }

        if (itemInstance.ShouldBeRemovedBecauseChargesEmpty())
        {
            GameMessageLog.Write(itemInstance.GetDisplayName() + " crumbles after its last charge is spent.");
            RemoveUsedItem(itemInstance, fromEquipment);
        }
    }

    private void RemoveUsedItem(ItemInstance itemInstance, bool fromEquipment)
    {
        if (fromEquipment)
        {
            actorEquipment.RemoveEquippedItem(itemInstance);
            return;
        }

        actorInventory.RemoveQuantity(itemInstance, 1);
    }

    private void TickAllItemCooldowns()
    {
        HashSet<ItemInstance> tickedItems = new HashSet<ItemInstance>();

        for (int i = 0; i < actorInventory.Items.Count; i++)
        {
            ItemInstance item = actorInventory.Items[i];

            if (item == null)
            {
                continue;
            }

            if (tickedItems.Add(item))
            {
                item.TickUseCooldown();
            }
        }

        IReadOnlyList<ItemInstance> equippedItems = actorEquipment.GetEquippedItems();

        for (int i = 0; i < equippedItems.Count; i++)
        {
            ItemInstance item = equippedItems[i];

            if (item == null)
            {
                continue;
            }

            if (tickedItems.Add(item))
            {
                item.TickUseCooldown();
            }
        }
    }

    private bool ApplyConsumableEffects(ItemInstance itemInstance)
    {
        bool appliedAnyEffect = false;

        if (itemInstance.Definition.ConsumableEffects == null)
        {
            return false;
        }

        for (int i = 0; i < itemInstance.Definition.ConsumableEffects.Count; i++)
        {
            ConsumableEffect effect = itemInstance.Definition.ConsumableEffects[i];

            if (effect == null)
            {
                continue;
            }

            bool applied = ApplySingleConsumableEffect(effect);

            if (applied)
            {
                appliedAnyEffect = true;
            }
        }

        return appliedAnyEffect;
    }

    private bool ApplySingleConsumableEffect(ConsumableEffect effect)
    {
        string effectName = effect.EffectType.ToString();

        if (MatchesEffectName(effectName, "Heal") ||
            MatchesEffectName(effectName, "Health") ||
            MatchesEffectName(effectName, "RestoreHealth"))
        {
            if (actorHealth == null)
            {
                return false;
            }

            return actorHealth.Heal(effect.Value);
        }

        if (MatchesEffectName(effectName, "Hunger") ||
            MatchesEffectName(effectName, "Food") ||
            MatchesEffectName(effectName, "RestoreHunger"))
        {
            if (actorSurvival == null)
            {
                return false;
            }

            return actorSurvival.RestoreHunger(effect.Value);
        }

        if (MatchesEffectName(effectName, "Thirst") ||
            MatchesEffectName(effectName, "Water") ||
            MatchesEffectName(effectName, "RestoreThirst"))
        {
            if (actorSurvival == null)
            {
                return false;
            }

            return actorSurvival.RestoreThirst(effect.Value);
        }

        GameMessageLog.Write("Nothing happens.");
        return false;
    }

    private bool ApplySpecialUseEffects(ItemInstance itemInstance)
    {
        if (specialEffectHandler == null)
        {
            return false;
        }

        return specialEffectHandler.ApplyItemUseEffects(itemInstance);
    }

    private bool MatchesEffectName(string effectName, string expectedName)
    {
        return string.Equals(effectName, expectedName, StringComparison.OrdinalIgnoreCase);
    }
}