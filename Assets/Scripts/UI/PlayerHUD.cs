using TMPro;
using UnityEngine;

/*
 * PlayerHUD
 * ---------
 * Displays the player's important runtime stats on the screen.
 *
 * Current displayed values:
 * - character name
 * - class/background
 * - current dungeon floor
 * - level
 * - XP
 * - player HP
 * - hunger
 * - thirst
 * - attack damage
 * - armor
 * - stance/mode
 *
 * Main responsibilities:
 * - receive the current player reference from GameBootstrap
 * - cache useful player components
 * - refresh the HUD text every frame
 *
 * Important:
 * This HUD does not control gameplay.
 * It only reads values from gameplay systems and displays them.
 */

public class PlayerHUD : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text statusText;

    private ActorGridEntity playerActor;
    private ActorHealth playerHealth;
    private ActorSurvival playerSurvival;
    private ActorStats playerStats;
    private ActorStealth playerStealth;
    private ActorExperience playerExperience;
    private PlayerCharacterProfile playerProfile;

    private int currentFloorNumber;

    public void SetTarget(ActorGridEntity newPlayerActor, int newFloorNumber)
    {
        playerActor = newPlayerActor;
        currentFloorNumber = newFloorNumber;

        if (playerActor == null)
        {
            ClearHUD();
            return;
        }

        playerHealth = playerActor.GetComponent<ActorHealth>();
        playerSurvival = playerActor.GetComponent<ActorSurvival>();
        playerStats = playerActor.GetComponent<ActorStats>();
        playerStealth = playerActor.GetComponent<ActorStealth>();
        playerExperience = playerActor.GetComponent<ActorExperience>();
        playerProfile = playerActor.GetComponent<PlayerCharacterProfile>();

        RefreshHUD();
    }

    private void Update()
    {
        if (playerActor == null)
        {
            return;
        }

        RefreshHUD();
    }

    private void RefreshHUD()
    {
        if (statusText == null)
        {
            return;
        }

        statusText.text =
            GetProfileText() +
            "\nFloor: " + currentFloorNumber +
            "\nLevel: " + GetLevelText() +
            "\nXP: " + GetExperienceText() +
            "\nHP: " + GetHealthText() +
            "\nHunger: " + GetHungerText() +
            "\nThirst: " + GetThirstText() +
            "\nAttack: " + GetAttackDamageText() +
            "\nArmor: " + GetArmorText() +
            "\nMode: " + GetModeText();
    }

    private string GetProfileText()
    {
        if (playerProfile == null)
        {
            return "Character: Unknown";
        }

        return playerProfile.CharacterName +
               "\n" + playerProfile.BackgroundName +
               " " + playerProfile.ClassName;
    }

    private string GetLevelText()
    {
        if (playerExperience == null)
        {
            return "-";
        }

        return playerExperience.CurrentLevel.ToString();
    }

    private string GetExperienceText()
    {
        if (playerExperience == null)
        {
            return "-";
        }

        return playerExperience.CurrentExperience + "/" + playerExperience.ExperienceToNextLevel;
    }

    private string GetHealthText()
    {
        if (playerHealth == null)
        {
            return "-";
        }

        return playerHealth.CurrentHealth + "/" + playerHealth.MaxHealth;
    }

    private string GetHungerText()
    {
        if (playerSurvival == null)
        {
            return "-";
        }

        return playerSurvival.CurrentHunger + "/" + playerSurvival.MaxHunger;
    }

    private string GetThirstText()
    {
        if (playerSurvival == null)
        {
            return "-";
        }

        return playerSurvival.CurrentThirst + "/" + playerSurvival.MaxThirst;
    }

    private string GetAttackDamageText()
    {
        if (playerStats == null)
        {
            return "-";
        }

        return playerStats.GetStat(StatType.AttackDamage).ToString();
    }

    private string GetArmorText()
    {
        if (playerStats == null)
        {
            return "-";
        }

        return playerStats.GetStat(StatType.Armor).ToString();
    }

    private string GetModeText()
    {
        if (playerStealth == null)
        {
            return "Normal";
        }

        if (playerStealth.IsSneaking)
        {
            return "Sneaking";
        }

        return "Normal";
    }

    private void ClearHUD()
    {
        if (statusText == null)
        {
            return;
        }

        statusText.text = "";
    }
}