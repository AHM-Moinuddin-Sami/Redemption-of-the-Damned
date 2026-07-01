using System;
using UnityEngine;

/*
 * EnemyGuaranteedDropEntry
 * ------------------------
 * Defines one specific item that an enemy can drop on death.
 *
 * Despite the name, the drop is not always guaranteed.
 * The Drop Chance Percent controls whether it drops.
 *
 * Example:
 * Rat:
 * - Bread Crumb
 * - Drop Chance: 25%
 * - Min Quantity: 1
 * - Max Quantity: 1
 *
 * Boss:
 * - Unique Sword
 * - Drop Chance: 100%
 * - Min Quantity: 1
 * - Max Quantity: 1
 *
 * Current responsibilities:
 * - store item definition
 * - store drop chance
 * - roll quantity
 * - return an ItemDropResult for GameBootstrap to generate into an ItemInstance
 */

[Serializable]
public class EnemyGuaranteedDropEntry
{
    [Header("Item")]
    [SerializeField] private ItemDefinition itemDefinition;

    [Header("Chance")]
    [Range(0, 100)]
    [SerializeField] private int dropChancePercent = 100;

    [Header("Quantity")]
    [SerializeField] private int minQuantity = 1;
    [SerializeField] private int maxQuantity = 1;

    public ItemDropResult Roll(System.Random random)
    {
        if (itemDefinition == null)
        {
            return null;
        }

        if (random == null)
        {
            random = new System.Random();
        }

        int safeChance = Mathf.Clamp(dropChancePercent, 0, 100);

        if (safeChance <= 0)
        {
            return null;
        }

        int chanceRoll = random.Next(0, 100);

        if (chanceRoll >= safeChance)
        {
            return null;
        }

        int safeMinQuantity = Mathf.Max(1, minQuantity);
        int safeMaxQuantity = Mathf.Max(safeMinQuantity, maxQuantity);
        int quantity = random.Next(safeMinQuantity, safeMaxQuantity + 1);

        return new ItemDropResult(itemDefinition, quantity);
    }
}