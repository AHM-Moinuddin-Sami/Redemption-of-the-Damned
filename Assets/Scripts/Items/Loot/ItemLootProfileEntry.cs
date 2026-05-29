using System;
using UnityEngine;

/*
 * ItemLootProfileEntry
 * --------------------
 * Defines one floor-depth range that can use a specific ItemDropTable.
 *
 * This is used by ItemLootProfile to decide which loot table should be used
 * on the current dungeon floor.
 *
 * Example:
 * Early Loot Table:
 * - Min Floor: 1
 * - Max Floor: 2
 * - Weight: 100
 *
 * Mid Loot Table:
 * - Min Floor: 3
 * - Max Floor: 5
 * - Weight: 100
 *
 * Deep Loot Table:
 * - Min Floor: 6
 * - Max Floor: 0
 * - Weight: 100
 *
 * Max Floor rule:
 * - Max Floor <= 0 means there is no upper limit.
 *
 * Important:
 * Multiple entries can match the same floor. If that happens, ItemLootProfile
 * chooses between the matching entries using their weights.
 *
 * This lets you create smoother loot progression.
 * For example:
 * - Floor 3 can mostly use early loot, with a small chance of mid loot.
 * - Floor 6 can mostly use mid loot, with a small chance of deep loot.
 */

[Serializable]
public class ItemLootProfileEntry
{
    [Header("Floor Range")]
    [SerializeField] private int minFloor = 1;
    [SerializeField] private int maxFloor = 0;

    [Header("Loot Table")]
    [SerializeField] private ItemDropTable itemDropTable;

    [Header("Weight")]
    [SerializeField] private int weight = 1;

    public ItemDropTable ItemDropTable
    {
        get
        {
            return itemDropTable;
        }
    }

    public int Weight
    {
        get
        {
            return Mathf.Max(0, weight);
        }
    }

    public bool MatchesFloor(int floorNumber)
    {
        int safeFloorNumber = Mathf.Max(1, floorNumber);
        int safeMinFloor = Mathf.Max(1, minFloor);

        if (safeFloorNumber < safeMinFloor)
        {
            return false;
        }

        // Max floor <= 0 means this loot entry has no upper limit.
        if (maxFloor <= 0)
        {
            return true;
        }

        return safeFloorNumber <= maxFloor;
    }
}