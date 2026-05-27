using System;
using System.Collections.Generic;
using UnityEngine;

/*
 * ActorEquipment
 * --------------
 * Stores the equipment currently worn or held by an actor.
 *
 * This script handles equipment placement logic and tells ActorStats to
 * recalculate whenever equipment changes.
 *
 * Current responsibilities:
 * - equip items into valid equipment slots
 * - remove equipped items from inventory
 * - return replaced equipment back to inventory
 * - handle two-handed weapon rules
 * - provide equipped items to ActorStats
 * - expose equipped items to equipment UI
 * - notify UI when equipment changes
 *
 * Current equipment rules:
 * - MainHand items equip into the main hand.
 * - OffHand items equip into the off-hand.
 * - Two-handed weapons clear the off-hand slot.
 * - Equipping an off-hand item removes a two-handed main-hand weapon.
 * - Armor/accessory slots replace only their matching slot.
 *
 * Important:
 * This script does not calculate stats directly.
 * ActorStats recalculates by reading equipped items from this script.
 */

public class ActorEquipment : MonoBehaviour
{
    public event Action EquipmentChanged;

    private ItemInstance mainHand;
    private ItemInstance offHand;
    private ItemInstance head;
    private ItemInstance body;
    private ItemInstance hands;
    private ItemInstance feet;
    private ItemInstance neck;
    private ItemInstance ring;

    private ActorStats actorStats;

    private void Awake()
    {
        actorStats = GetComponent<ActorStats>();
    }

    private void Start()
    {
        RecalculateStats();
        NotifyEquipmentChanged();
    }

    public bool TryEquip(ItemInstance itemInstance, ActorInventory inventory)
    {
        if (itemInstance == null || itemInstance.Definition == null)
        {
            return false;
        }

        if (!itemInstance.IsEquipment)
        {
            GameMessageLog.Write(itemInstance.GetDisplayName() + " is not equipment.");
            return false;
        }

        EquipmentSlotType slot = itemInstance.Definition.EquipmentSlot;

        if (slot == EquipmentSlotType.None)
        {
            GameMessageLog.Write(itemInstance.GetDisplayName() + " cannot be equipped.");
            return false;
        }

        if (inventory != null && !inventory.RemoveItem(itemInstance))
        {
            Debug.LogWarning("Could not equip " + itemInstance.GetDisplayName() + " because it was not found in inventory.");
            return false;
        }

        EquipIntoSlot(itemInstance, slot, inventory);

        GameMessageLog.Write(gameObject.name + " equips " + itemInstance.GetDisplayName() + ".");

        RecalculateStats();
        NotifyEquipmentChanged();
        PrintEquipmentDebug();

        if (inventory != null)
        {
            inventory.PrintInventoryDebug();
        }

        return true;
    }

    public ItemInstance GetEquippedItem(EquipmentSlotType slot)
    {
        if (slot == EquipmentSlotType.MainHand)
        {
            return mainHand;
        }

        if (slot == EquipmentSlotType.OffHand)
        {
            return offHand;
        }

        if (slot == EquipmentSlotType.Head)
        {
            return head;
        }

        if (slot == EquipmentSlotType.Body)
        {
            return body;
        }

        if (slot == EquipmentSlotType.Hands)
        {
            return hands;
        }

        if (slot == EquipmentSlotType.Feet)
        {
            return feet;
        }

        if (slot == EquipmentSlotType.Neck)
        {
            return neck;
        }

        if (slot == EquipmentSlotType.Ring)
        {
            return ring;
        }

        return null;
    }

    public IReadOnlyList<ItemInstance> GetEquippedItems()
    {
        List<ItemInstance> equippedItems = new List<ItemInstance>();

        AddIfNotNull(equippedItems, mainHand);
        AddIfNotNull(equippedItems, offHand);
        AddIfNotNull(equippedItems, head);
        AddIfNotNull(equippedItems, body);
        AddIfNotNull(equippedItems, hands);
        AddIfNotNull(equippedItems, feet);
        AddIfNotNull(equippedItems, neck);
        AddIfNotNull(equippedItems, ring);

        return equippedItems;
    }

    private void EquipIntoSlot(ItemInstance itemInstance, EquipmentSlotType slot, ActorInventory inventory)
    {
        if (slot == EquipmentSlotType.MainHand)
        {
            EquipMainHand(itemInstance, inventory);
            return;
        }

        if (slot == EquipmentSlotType.OffHand)
        {
            EquipOffHand(itemInstance, inventory);
            return;
        }

        EquipArmorOrAccessorySlot(itemInstance, slot, inventory);
    }

    private void EquipMainHand(ItemInstance itemInstance, ActorInventory inventory)
    {
        ReturnEquippedItemToInventory(mainHand, inventory);
        mainHand = itemInstance;

        // Two-handed weapons occupy both hands.
        // If an off-hand item is equipped, it must be removed.
        if (itemInstance.Definition.TwoHanded)
        {
            ReturnEquippedItemToInventory(offHand, inventory);
            offHand = null;
        }
    }

    private void EquipOffHand(ItemInstance itemInstance, ActorInventory inventory)
    {
        // If the main hand is two-handed, equipping an off-hand item removes it.
        if (mainHand != null && mainHand.Definition != null && mainHand.Definition.TwoHanded)
        {
            ReturnEquippedItemToInventory(mainHand, inventory);
            mainHand = null;
        }

        ReturnEquippedItemToInventory(offHand, inventory);
        offHand = itemInstance;
    }

    private void EquipArmorOrAccessorySlot(ItemInstance itemInstance, EquipmentSlotType slot, ActorInventory inventory)
    {
        ItemInstance replacedItem = GetEquippedItem(slot);

        ReturnEquippedItemToInventory(replacedItem, inventory);
        SetEquippedItem(slot, itemInstance);
    }

    private void ReturnEquippedItemToInventory(ItemInstance itemInstance, ActorInventory inventory)
    {
        if (itemInstance == null || inventory == null)
        {
            return;
        }

        // This is silent because the item is being unequipped, not picked up.
        inventory.AddItem(itemInstance, false);
    }

    private void SetEquippedItem(EquipmentSlotType slot, ItemInstance itemInstance)
    {
        if (slot == EquipmentSlotType.MainHand)
        {
            mainHand = itemInstance;
            return;
        }

        if (slot == EquipmentSlotType.OffHand)
        {
            offHand = itemInstance;
            return;
        }

        if (slot == EquipmentSlotType.Head)
        {
            head = itemInstance;
            return;
        }

        if (slot == EquipmentSlotType.Body)
        {
            body = itemInstance;
            return;
        }

        if (slot == EquipmentSlotType.Hands)
        {
            hands = itemInstance;
            return;
        }

        if (slot == EquipmentSlotType.Feet)
        {
            feet = itemInstance;
            return;
        }

        if (slot == EquipmentSlotType.Neck)
        {
            neck = itemInstance;
            return;
        }

        if (slot == EquipmentSlotType.Ring)
        {
            ring = itemInstance;
        }
    }

    private void RecalculateStats()
    {
        if (actorStats == null)
        {
            return;
        }

        actorStats.RecalculateFromEquipment(this);
    }

    private void NotifyEquipmentChanged()
    {
        if (EquipmentChanged == null)
        {
            return;
        }

        EquipmentChanged.Invoke();
    }

    private void AddIfNotNull(List<ItemInstance> equippedItems, ItemInstance itemInstance)
    {
        if (itemInstance == null)
        {
            return;
        }

        equippedItems.Add(itemInstance);
    }

    public void PrintEquipmentDebug()
    {
        string equipmentText = gameObject.name + " equipment:";

        equipmentText += "\n- Main Hand: " + GetSlotDebugName(mainHand);
        equipmentText += "\n- Off Hand: " + GetSlotDebugName(offHand);
        equipmentText += "\n- Head: " + GetSlotDebugName(head);
        equipmentText += "\n- Body: " + GetSlotDebugName(body);
        equipmentText += "\n- Hands: " + GetSlotDebugName(hands);
        equipmentText += "\n- Feet: " + GetSlotDebugName(feet);
        equipmentText += "\n- Neck: " + GetSlotDebugName(neck);
        equipmentText += "\n- Ring: " + GetSlotDebugName(ring);

        Debug.Log(equipmentText);
    }

    private string GetSlotDebugName(ItemInstance itemInstance)
    {
        if (itemInstance == null)
        {
            return "Empty";
        }

        return itemInstance.GetDebugDescription();
    }
}