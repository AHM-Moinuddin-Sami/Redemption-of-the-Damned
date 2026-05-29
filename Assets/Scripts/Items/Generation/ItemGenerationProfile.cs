using System.Collections.Generic;
using UnityEngine;

/*
 * ItemGenerationProfile
 * ---------------------
 * Creates ItemInstance objects from loot table results.
 *
 * ItemDropTable chooses:
 * - what base item drops
 * - quantity
 *
 * ItemGenerationProfile chooses:
 * - rarity
 * - generated affixes
 *
 * Current behavior:
 * - unique item definitions always become Unique
 * - unique items do not roll random affixes
 * - stackable items always stay Normal
 * - non-equipment items stay Normal
 * - equipment can roll Magic or Rare
 * - Magic items get magicAffixCount affixes
 * - Rare items get rareAffixCount affixes
 *
 * Important:
 * Unique items are authored in ItemDefinition.
 * This generator does not randomly create unique items from normal items.
 */

[CreateAssetMenu(menuName = "Roguelike/Items/Generation/Item Generation Profile")]
public class ItemGenerationProfile : ScriptableObject
{
    [Header("Affixes")]
    [SerializeField] private ItemAffixTable affixTable;

    [Header("Base Rarity Chances")]
    [Range(0, 100)]
    [SerializeField] private int magicChancePercent = 20;

    [Range(0, 100)]
    [SerializeField] private int rareChancePercent = 5;

    [Header("Depth Scaling")]
    [SerializeField] private int magicChanceBonusPerFloor = 1;
    [SerializeField] private int rareChanceBonusEveryFloors = 3;
    [SerializeField] private int rareChanceBonusAmount = 1;

    [Header("Affix Counts")]
    [SerializeField] private int magicAffixCount = 1;
    [SerializeField] private int rareAffixCount = 2;

    public ItemInstance GenerateItemInstance(
        ItemDropResult dropResult,
        int floorNumber,
        System.Random random)
    {
        if (dropResult == null || dropResult.ItemDefinition == null)
        {
            return null;
        }

        if (random == null)
        {
            random = new System.Random();
        }

        ItemDefinition itemDefinition = dropResult.ItemDefinition;
        int quantity = Mathf.Max(1, dropResult.Quantity);

        if (itemDefinition.IsUnique)
        {
            return new ItemInstance(itemDefinition, 1);
        }

        if (!CanRollRarity(itemDefinition))
        {
            return new ItemInstance(itemDefinition, quantity);
        }

        ItemRarity rarity = RollRarity(floorNumber, random);
        List<ItemAffixDefinition> affixes = RollAffixes(itemDefinition, floorNumber, rarity, random);

        return new ItemInstance(itemDefinition, quantity, rarity, affixes);
    }

    private bool CanRollRarity(ItemDefinition itemDefinition)
    {
        if (itemDefinition == null)
        {
            return false;
        }

        if (itemDefinition.IsUnique)
        {
            return false;
        }

        if (!itemDefinition.IsEquipment)
        {
            return false;
        }

        if (itemDefinition.Stackable)
        {
            return false;
        }

        return true;
    }

    private ItemRarity RollRarity(int floorNumber, System.Random random)
    {
        int finalRareChance = GetFinalRareChance(floorNumber);
        int finalMagicChance = GetFinalMagicChance(floorNumber);

        int roll = random.Next(0, 100);

        if (roll < finalRareChance)
        {
            return ItemRarity.Rare;
        }

        if (roll < finalRareChance + finalMagicChance)
        {
            return ItemRarity.Magic;
        }

        return ItemRarity.Normal;
    }

    private int GetFinalMagicChance(int floorNumber)
    {
        int safeFloor = Mathf.Max(1, floorNumber);
        int finalChance = magicChancePercent + safeFloor * magicChanceBonusPerFloor;

        return Mathf.Clamp(finalChance, 0, 100);
    }

    private int GetFinalRareChance(int floorNumber)
    {
        int safeFloor = Mathf.Max(1, floorNumber);
        int bonus = 0;

        if (rareChanceBonusEveryFloors > 0)
        {
            bonus = (safeFloor / rareChanceBonusEveryFloors) * rareChanceBonusAmount;
        }

        int finalChance = rareChancePercent + bonus;

        return Mathf.Clamp(finalChance, 0, 100);
    }

    private List<ItemAffixDefinition> RollAffixes(
        ItemDefinition itemDefinition,
        int floorNumber,
        ItemRarity rarity,
        System.Random random)
    {
        List<ItemAffixDefinition> affixes = new List<ItemAffixDefinition>();

        if (affixTable == null)
        {
            return affixes;
        }

        int affixCount = GetAffixCountForRarity(rarity);

        if (affixCount <= 0)
        {
            return affixes;
        }

        return affixTable.RollAffixes(itemDefinition, floorNumber, affixCount, random);
    }

    private int GetAffixCountForRarity(ItemRarity rarity)
    {
        if (rarity == ItemRarity.Magic)
        {
            return Mathf.Max(0, magicAffixCount);
        }

        if (rarity == ItemRarity.Rare)
        {
            return Mathf.Max(0, rareAffixCount);
        }

        return 0;
    }
}