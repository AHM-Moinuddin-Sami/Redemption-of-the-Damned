using System;
using UnityEngine;

/*
 * EnemySpawnTableEntry
 * --------------------
 * Stores one weighted enemy entry inside an EnemySpawnTable.
 *
 * Example:
 * - Rat, Weight 40
 * - Bandit, Weight 20
 * - Skeleton, Weight 10
 *
 * Higher weight means the enemy is more likely to be selected.
 */

[Serializable]
public class EnemySpawnTableEntry
{
    [SerializeField] private EnemyDefinition enemyDefinition;
    [SerializeField] private int weight = 1;

    public EnemyDefinition EnemyDefinition
    {
        get
        {
            return enemyDefinition;
        }
    }

    public int Weight
    {
        get
        {
            return Mathf.Max(0, weight);
        }
    }
}