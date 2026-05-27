using System;
using System.Collections.Generic;
using UnityEngine;

/*
 * ActorInventory
 * --------------
 * Stores items carried by an actor.
 *
 * This inventory is still list-based and simple, but it now exposes an
 * InventoryChanged event so UI systems can refresh automatically.
 *
 * Current responsibilities:
 * - store ItemInstance objects
 * - add picked-up items
 * - merge stackable items
 * - remove items
 * - remove quantity from stacks
 * - find equipment and consumables for temporary test controls
 * - notify UI when inventory contents change
 *
 * Important:
 * This does not display a UI by itself.
 * PlayerInventoryUI listens to InventoryChanged and displays the item list.
 *
 * Later this can expand into:
 * - inventory slots
 * - item dropping
 * - selected item actions
 * - inventory capacity
 * - weight
 * - sorting/filtering
 * - item comparison
 */

public class ActorInventory : MonoBehaviour
{
    public event Action InventoryChanged;

    private readonly List<ItemInstance> items = new List<ItemInstance>();

    public IReadOnlyList<ItemInstance> Items
    {
        get
        {
            return items;
        }
    }

    public void AddItem(ItemInstance itemInstance)
    {
        AddItem(itemInstance, true);
    }

    public void AddItem(ItemInstance itemInstance, bool printMessage)
    {
        if (itemInstance == null || itemInstance.Definition == null)
        {
            return;
        }

        if (!itemInstance.IsStackable)
        {
            AddNonStackableItem(itemInstance, printMessage);
            NotifyInventoryChanged();
            return;
        }

        AddStackableItem(itemInstance, printMessage);
        NotifyInventoryChanged();
    }

    public bool RemoveItem(ItemInstance itemInstance)
    {
        if (itemInstance == null)
        {
            return false;
        }

        bool removed = items.Remove(itemInstance);

        if (removed)
        {
            NotifyInventoryChanged();
        }

        return removed;
    }

    public bool RemoveQuantity(ItemInstance itemInstance, int quantity)
    {
        if (itemInstance == null)
        {
            return false;
        }

        if (!items.Contains(itemInstance))
        {
            return false;
        }

        if (!itemInstance.IsStackable)
        {
            items.Remove(itemInstance);
            NotifyInventoryChanged();
            return true;
        }

        int removedAmount = itemInstance.RemoveQuantity(quantity);

        if (itemInstance.Quantity <= 0)
        {
            items.Remove(itemInstance);
        }

        if (removedAmount > 0)
        {
            NotifyInventoryChanged();
        }

        return removedAmount > 0;
    }

    public ItemInstance GetFirstEquipmentItem()
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null)
            {
                continue;
            }

            if (items[i].IsEquipment)
            {
                return items[i];
            }
        }

        return null;
    }

    public ItemInstance GetFirstConsumableItem()
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null || items[i].Definition == null)
            {
                continue;
            }

            if (items[i].Definition.IsConsumable)
            {
                return items[i];
            }
        }

        return null;
    }

    public ItemInstance GetFirstItemByCategory(ItemCategory category)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null)
            {
                continue;
            }

            if (items[i].Category == category)
            {
                return items[i];
            }
        }

        return null;
    }

    public List<ItemInstance> GetItemsByCategory(ItemCategory category)
    {
        List<ItemInstance> matchingItems = new List<ItemInstance>();

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null)
            {
                continue;
            }

            if (items[i].Category == category)
            {
                matchingItems.Add(items[i]);
            }
        }

        return matchingItems;
    }

    private void AddNonStackableItem(ItemInstance itemInstance, bool printMessage)
    {
        items.Add(itemInstance);

        if (printMessage)
        {
            GameMessageLog.Write(gameObject.name + " picks up " + itemInstance.GetDisplayName() + ".");
            PrintInventoryDebug();
        }
    }

    private void AddStackableItem(ItemInstance itemInstance, bool printMessage)
    {
        int remainingQuantity = itemInstance.Quantity;

        // Try to fill existing stacks first.
        for (int i = 0; i < items.Count; i++)
        {
            if (remainingQuantity <= 0)
            {
                break;
            }

            ItemInstance existingItem = items[i];

            if (existingItem == null)
            {
                continue;
            }

            if (!existingItem.CanStackWith(itemInstance.Definition))
            {
                continue;
            }

            remainingQuantity = existingItem.AddQuantity(remainingQuantity);
        }

        // Create new stacks for anything that did not fit into existing stacks.
        while (remainingQuantity > 0)
        {
            int stackQuantity = remainingQuantity;

            if (stackQuantity > itemInstance.Definition.MaxStackSize)
            {
                stackQuantity = itemInstance.Definition.MaxStackSize;
            }

            ItemInstance newStack = new ItemInstance(itemInstance.Definition, stackQuantity);
            items.Add(newStack);

            remainingQuantity -= stackQuantity;
        }

        if (printMessage)
        {
            GameMessageLog.Write(gameObject.name + " picks up " + itemInstance.GetDisplayName() + ".");
            PrintInventoryDebug();
        }
    }

    public void PrintInventoryDebug()
    {
        string inventoryText = gameObject.name + " inventory:";

        if (items.Count == 0)
        {
            Debug.Log(inventoryText + " empty");
            return;
        }

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null)
            {
                continue;
            }

            inventoryText += "\n- " + items[i].GetDebugDescription();
        }

        Debug.Log(inventoryText);
    }

    private void NotifyInventoryChanged()
    {
        if (InventoryChanged == null)
        {
            return;
        }

        InventoryChanged.Invoke();
    }
}