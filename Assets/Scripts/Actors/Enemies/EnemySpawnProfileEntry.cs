using System;
using UnityEngine;

/*
 * EnemySpawnProfileEntry
 * ----------------------
 * Defines one depth range that can use a specific EnemySpawnTable.
 *
 * This is used by EnemySpawnProfile to decide which enemy table should be used
 * on the current dungeon floor.
 *
 * Example:
 * Early Dungeon Table:
 * - Min Floor: 1
 * - Max Floor: 3
 * - Weight: 100
 *
 * Deep Dungeon Table:
 * - Min Floor: 6
 * - Max Floor: 0
 * - Weight: 100
 *
 * Max Floor rule:
 * - Max Floor <= 0 means there is no upper limit.
 *
 * Important:
 * Multiple entries can match the same floor. If that happens, EnemySpawnProfile
 * chooses between the matching entries using their weights.
 *
 * This lets you do things like:
 * - mostly weak enemies on floor 3
 * - small chance of harder enemies on floor 3
 * - stronger enemies becoming common later
 */

[Serializable]
public class EnemySpawnProfileEntry
{
    [Header("Floor Range")]
    [SerializeField] private int minFloor = 1;
    [SerializeField] private int maxFloor = 0;

    [Header("Table")]
    [SerializeField] private EnemySpawnTable enemySpawnTable;

    [Header("Weight")]
    [SerializeField] private int weight = 1;

    public EnemySpawnTable EnemySpawnTable
    {
        get
        {
            return enemySpawnTable;
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

        // Max floor <= 0 means this entry has no upper limit.
        if (maxFloor <= 0)
        {
            return true;
        }

        return safeFloorNumber <= maxFloor;
    }
}