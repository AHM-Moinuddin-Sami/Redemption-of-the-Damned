using System.Collections.Generic;
using UnityEngine;

/*
 * PlayerCharacterProfile
 * ----------------------
 * Stores the player's run identity and applies class/background setup.
 *
 * This component belongs on the player prefab.
 *
 * Current responsibilities:
 * - store character name
 * - store selected class
 * - store selected background
 * - apply class/background stat bonuses
 * - grant class/background starting items
 * - make sure setup happens only once per run
 *
 * Current flow:
 * 1. GameBootstrap spawns the player for a new run.
 * 2. GameBootstrap calls InitializeForNewRun().
 * 3. This script applies class/background stat bonuses to ActorStats.
 * 4. This script gives starting items to ActorInventory.
 * 5. ActorHealth is set to full health after stat bonuses are applied.
 *
 * Important:
 * This is not the final character creation UI.
 * For now, you select the class/background assets directly on the player prefab.
 *
 * Later this can expand into:
 * - character creation screen
 * - randomized names
 * - subclasses
 * - starting reputation
 * - background-specific quest hooks
 * - class abilities
 * - save data
 */

[RequireComponent(typeof(ActorStats))]
[RequireComponent(typeof(ActorInventory))]
[RequireComponent(typeof(ActorHealth))]
public class PlayerCharacterProfile : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private string characterName = "Wanderer";

    [Header("Character Choices")]
    [SerializeField] private CharacterClassDefinition characterClass;
    [SerializeField] private CharacterBackgroundDefinition background;

    public string CharacterName
    {
        get
        {
            if (string.IsNullOrWhiteSpace(characterName))
            {
                return "Unnamed";
            }

            return characterName;
        }
    }

    public string ClassName
    {
        get
        {
            if (characterClass == null)
            {
                return "No Class";
            }

            return characterClass.DisplayName;
        }
    }

    public string BackgroundName
    {
        get
        {
            if (background == null)
            {
                return "No Background";
            }

            return background.DisplayName;
        }
    }

    public bool HasInitializedForRun { get; private set; }

    private ActorStats actorStats;
    private ActorInventory actorInventory;
    private ActorHealth actorHealth;

    private void Awake()
    {
        actorStats = GetComponent<ActorStats>();
        actorInventory = GetComponent<ActorInventory>();
        actorHealth = GetComponent<ActorHealth>();
    }

    public void InitializeForNewRun()
    {
        if (HasInitializedForRun)
        {
            return;
        }

        ApplyCharacterStats();
        GrantStartingItems();

        if (actorHealth != null)
        {
            actorHealth.SetToFullHealth();
        }

        HasInitializedForRun = true;

        GameMessageLog.Write(
            "You are " + CharacterName +
            ", a " + BackgroundName +
            " " + ClassName + "."
        );
    }

    private void ApplyCharacterStats()
    {
        if (actorStats == null)
        {
            return;
        }

        actorStats.ClearPermanentModifiers();

        if (characterClass != null)
        {
            actorStats.AddPermanentModifiers(characterClass.StatModifiers);
        }

        if (background != null)
        {
            actorStats.AddPermanentModifiers(background.StatModifiers);
        }

        ActorEquipment actorEquipment = GetComponent<ActorEquipment>();
        actorStats.RecalculateFromEquipment(actorEquipment);
    }

    private void GrantStartingItems()
    {
        if (actorInventory == null)
        {
            return;
        }

        if (characterClass != null)
        {
            GrantItems(characterClass.StartingItems);
        }

        if (background != null)
        {
            GrantItems(background.StartingItems);
        }

        actorInventory.PrintInventoryDebug();
    }

    private void GrantItems(IReadOnlyList<StartingItemEntry> startingItems)
    {
        if (startingItems == null)
        {
            return;
        }

        for (int i = 0; i < startingItems.Count; i++)
        {
            if (startingItems[i] == null)
            {
                continue;
            }

            ItemInstance itemInstance = startingItems[i].CreateItemInstance();

            if (itemInstance == null)
            {
                continue;
            }

            // Starting items are added silently so the message log does not get
            // flooded at run start. The inventory UI will still show them.
            actorInventory.AddItem(itemInstance, false);
        }
    }
}