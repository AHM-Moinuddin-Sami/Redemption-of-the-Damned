using System.Collections.Generic;
using UnityEngine;

/*
 * CharacterBackgroundDefinition
 * -----------------------------
 * Stores authored data for a character background.
 *
 * Backgrounds are separate from classes. A class defines the character's combat
 * or gameplay archetype, while a background defines where they came from.
 *
 * Example backgrounds:
 * - Exile
 * - Caravan Guard
 * - Ruin Scholar
 * - Desert Nomad
 * - Failed Alchemist
 *
 * Current responsibilities:
 * - store background display name
 * - store background description
 * - store flat stat bonuses
 * - store starting items
 *
 * Important:
 * This is the foundation only.
 * Later backgrounds can affect reputation, starting factions, dialogue options,
 * quest hooks, starting locations, or world generation rules.
 */

[CreateAssetMenu(menuName = "Roguelike/Characters/Background Definition")]
public class CharacterBackgroundDefinition : ScriptableObject
{
    [Header("Display")]
    [SerializeField] private string displayName = "Exile";

    [TextArea(2, 5)]
    [SerializeField] private string description = "Cast out, but still alive.";

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