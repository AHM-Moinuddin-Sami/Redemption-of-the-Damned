using System.Collections.Generic;
using UnityEngine;

/*
 * CharacterClassDefinition
 * ------------------------
 * Stores authored data for a playable character class.
 *
 * This is a ScriptableObject, so each class is created as an asset.
 *
 * Example classes:
 * - Wanderer
 * - Fighter
 * - Scavenger
 * - Occultist
 * - Tinker
 *
 * Current responsibilities:
 * - store class display name
 * - store class description
 * - store flat stat bonuses
 * - store starting items
 *
 * Important:
 * This is the foundation only.
 * It does not yet handle abilities, skill trees, subclasses, class quests,
 * special powers, or level-up rules.
 */

[CreateAssetMenu(menuName = "Roguelike/Characters/Class Definition")]
public class CharacterClassDefinition : ScriptableObject
{
    [Header("Display")]
    [SerializeField] private string displayName = "Wanderer";

    [TextArea(2, 5)]
    [SerializeField] private string description = "A general survivor with no strong specialization.";

    [Header("Stat Bonuses")]
    [SerializeField] private List<StatModifier> statModifiers = new List<StatModifier>();

    [Header("Starting Items")]
    [SerializeField] private List<StartingItemEntry> startingItems = new List<StartingItemEntry>();

    public string DisplayName
    {
        get
        {
            return displayName;
        }
    }

    public string Description
    {
        get
        {
            return description;
        }
    }

    public IReadOnlyList<StatModifier> StatModifiers
    {
        get
        {
            return statModifiers;
        }
    }

    public IReadOnlyList<StartingItemEntry> StartingItems
    {
        get
        {
            return startingItems;
        }
    }
}