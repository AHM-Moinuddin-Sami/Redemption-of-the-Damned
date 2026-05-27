using TMPro;
using UnityEngine;

/*
 * PlayerHUD
 * ---------
 * Displays the player's important runtime stats on the screen.
 *
 * This is the first simple HUD for the roguelike prototype.
 * It reads data from the currently spawned player and updates the UI text.
 *
 * Current displayed values:
 * - current dungeon floor
 * - player HP
 * - hunger
 * - thirst
 * - attack damage
 * - armor
 *
 * Main responsibilities:
 * - receive the current player reference from GameBootstrap
 * - cache useful player components
 * - refresh the HUD text every frame
 *
 * Important:
 * This HUD does not control gameplay.
 * It only reads values from gameplay systems and displays them.
 *
 * Later this can expand into:
 * - HP bars
 * - hunger/thirst bars
 * - equipment icons
 * - inventory button
 * - message log integration
 * - status effect icons
 * - minimap
 */

public class PlayerHUD : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text statusText;

    private ActorGridEntity playerActor;
    private ActorHealth playerHealth;
    private ActorSurvival playerSurvival;
    private ActorStats playerStats;

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

        // Cache the components so Update does not repeatedly call GetComponent.
        playerHealth = playerActor.GetComponent<ActorHealth>();
        playerSurvival = playerActor.GetComponent<ActorSurvival>();
        playerStats = playerActor.GetComponent<ActorStats>();

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

        string hpText = GetHealthText();
        string hungerText = GetHungerText();
        string thirstText = GetThirstText();
        string attackText = GetAttackDamageText();
        string armorText = GetArmorText();

        statusText.text =
            "Floor: " + currentFloorNumber +
            "\nHP: " + hpText +
            "\nHunger: " + hungerText +
            "\nThirst: " + thirstText +
            "\nAttack: " + attackText +
            "\nArmor: " + armorText;
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

    private void ClearHUD()
    {
        if (statusText == null)
        {
            return;
        }

        statusText.text = "";
    }
}