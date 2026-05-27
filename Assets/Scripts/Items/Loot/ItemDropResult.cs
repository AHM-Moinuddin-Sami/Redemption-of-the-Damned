/*
 * ItemDropResult
 * --------------
 * Stores the result of one loot table roll.
 *
 * ItemDropTable does not directly spawn an item GameObject. It only decides
 * what should be spawned. This class packages that decision into a small result.
 *
 * Current responsibilities:
 * - store the rolled ItemDefinition
 * - store the rolled quantity
 *
 * Example:
 * - Copper Coin x12
 * - Bread x2
 * - Rusty Sword x1
 *
 * Later this can expand into:
 * - rolled rarity
 * - item level
 * - affixes
 * - prefix/suffix data
 * - unique item selection
 * - generated item name data
 */

public class ItemDropResult
{
    public ItemDefinition ItemDefinition { get; private set; }
    public int Quantity { get; private set; }

    public ItemDropResult(ItemDefinition itemDefinition, int quantity)
    {
        ItemDefinition = itemDefinition;

        if (quantity < 1)
        {
            quantity = 1;
        }

        Quantity = quantity;
    }
}