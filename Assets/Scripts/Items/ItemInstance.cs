/*
 * ItemInstance
 * ------------
 * Represents a specific runtime copy or stack of an item.
 *
 * ItemDefinition is the authored template.
 * ItemInstance is the actual item stack carried by the player, lying on the
 * ground, or stored in a container.
 *
 * Current responsibilities:
 * - store the item definition
 * - store quantity
 * - expose item category through the definition
 * - check stack compatibility
 * - add/remove quantity safely
 * - provide display text for debug output
 *
 * Important:
 * For now, stack compatibility only checks ItemDefinition.
 * Later, generated item data such as rarity, affixes, durability, and item level
 * will also affect whether two items can stack.
 */

public class ItemInstance
{
    public ItemDefinition Definition { get; private set; }
    public int Quantity { get; private set; }

    public ItemCategory Category
    {
        get
        {
            if (Definition == null)
            {
                return ItemCategory.Junk;
            }

            return Definition.Category;
        }
    }

    public bool IsStackable
    {
        get
        {
            return Definition != null && Definition.Stackable;
        }
    }

    public bool IsFullStack
    {
        get
        {
            if (!IsStackable)
            {
                return true;
            }

            return Quantity >= Definition.MaxStackSize;
        }
    }

    public bool IsEquipment
    {
        get
        {
            return Definition != null && Definition.IsEquipment;
        }
    }

    public ItemInstance(ItemDefinition definition, int quantity)
    {
        Definition = definition;

        int safeQuantity = quantity;

        if (safeQuantity < 1)
        {
            safeQuantity = 1;
        }

        if (definition != null && definition.Stackable)
        {
            safeQuantity = ClampToMaxStackSize(safeQuantity);
        }
        else
        {
            safeQuantity = 1;
        }

        Quantity = safeQuantity;
    }

    public bool CanStackWith(ItemInstance otherItem)
    {
        if (otherItem == null)
        {
            return false;
        }

        return CanStackWith(otherItem.Definition);
    }

    public bool CanStackWith(ItemDefinition otherDefinition)
    {
        if (Definition == null || otherDefinition == null)
        {
            return false;
        }

        return IsStackable &&
               Definition == otherDefinition &&
               !IsFullStack;
    }

    public int GetRemainingStackSpace()
    {
        if (!IsStackable)
        {
            return 0;
        }

        return Definition.MaxStackSize - Quantity;
    }

    public int AddQuantity(int amount)
    {
        if (!IsStackable)
        {
            return amount;
        }

        if (amount <= 0)
        {
            return 0;
        }

        int remainingSpace = GetRemainingStackSpace();
        int amountToAdd = amount;

        if (amountToAdd > remainingSpace)
        {
            amountToAdd = remainingSpace;
        }

        Quantity += amountToAdd;

        // Return the amount that could not fit.
        return amount - amountToAdd;
    }

    public int RemoveQuantity(int amount)
    {
        if (amount <= 0)
        {
            return 0;
        }

        int amountToRemove = amount;

        if (amountToRemove > Quantity)
        {
            amountToRemove = Quantity;
        }

        Quantity -= amountToRemove;

        return amountToRemove;
    }

    public string GetDisplayName()
    {
        if (Definition == null)
        {
            return "Unknown Item";
        }

        if (Quantity > 1)
        {
            return Definition.DisplayName + " x" + Quantity;
        }

        return Definition.DisplayName;
    }

    public string GetDebugDescription()
    {
        if (Definition == null)
        {
            return "Unknown Item";
        }

        return GetDisplayName() + " [" + Definition.Category + "]";
    }

    private int ClampToMaxStackSize(int quantity)
    {
        if (Definition == null)
        {
            return quantity;
        }

        if (!Definition.Stackable)
        {
            return 1;
        }

        if (quantity > Definition.MaxStackSize)
        {
            return Definition.MaxStackSize;
        }

        return quantity;
    }
}