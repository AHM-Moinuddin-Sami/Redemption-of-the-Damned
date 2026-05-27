using UnityEngine;

/*
 * ActorSurvival
 * -------------
 * Handles basic hunger and thirst for an actor.
 *
 * This version keeps routine hunger/thirst values in the Console only, while
 * important player-facing warnings go to GameMessageLog.
 *
 * Current responsibilities:
 * - reduce hunger/thirst over valid player actions
 * - warn when hunger/thirst becomes low or critical
 * - damage the actor when starving or dehydrated
 * - restore hunger and thirst from consumables
 *
 * Important:
 * This still has no survival UI. The message log only shows important events.
 */

[RequireComponent(typeof(ActorHealth))]
public class ActorSurvival : MonoBehaviour
{
    [Header("Maximum Values")]
    [SerializeField] private int maxHunger = 100;
    [SerializeField] private int maxThirst = 100;

    [Header("Drain Timing")]
    [SerializeField] private int hungerDecreaseInterval = 8;
    [SerializeField] private int thirstDecreaseInterval = 5;

    [Header("Drain Amount")]
    [SerializeField] private int hungerDecreaseAmount = 1;
    [SerializeField] private int thirstDecreaseAmount = 1;

    [Header("Warnings")]
    [SerializeField] private int lowHungerThreshold = 30;
    [SerializeField] private int criticalHungerThreshold = 10;
    [SerializeField] private int lowThirstThreshold = 30;
    [SerializeField] private int criticalThirstThreshold = 10;

    [Header("Damage")]
    [SerializeField] private int starvationDamage = 1;
    [SerializeField] private int dehydrationDamage = 2;

    public int CurrentHunger { get; private set; }
    public int CurrentThirst { get; private set; }
    
    public int MaxHunger
    {
        get
        {
            return maxHunger;
        }
    }

    public int MaxThirst
    {
        get
        {
            return maxThirst;
        }
    }
    private int hungerActionCounter;
    private int thirstActionCounter;

    private bool lowHungerMessageShown;
    private bool criticalHungerMessageShown;
    private bool lowThirstMessageShown;
    private bool criticalThirstMessageShown;

    private ActorHealth actorHealth;
    private ActorGridEntity actorGridEntity;

    private void Awake()
    {
        actorHealth = GetComponent<ActorHealth>();
        actorGridEntity = GetComponent<ActorGridEntity>();

        CurrentHunger = maxHunger;
        CurrentThirst = maxThirst;
    }

    public void OnActionTaken()
    {
        ProcessHungerDrain();
        ProcessThirstDrain();

        PrintSurvivalDebug();
    }

    public bool RestoreHunger(int amount)
    {
        int safeAmount = Mathf.Max(0, amount);

        if (safeAmount == 0)
        {
            return false;
        }

        if (CurrentHunger >= maxHunger)
        {
            GameMessageLog.Write(GetDisplayName() + " is already full.");
            return false;
        }

        int oldValue = CurrentHunger;

        CurrentHunger += safeAmount;

        if (CurrentHunger > maxHunger)
        {
            CurrentHunger = maxHunger;
        }

        ResetHungerWarningsIfNeeded();

        int restoredAmount = CurrentHunger - oldValue;

        GameMessageLog.Write(GetDisplayName() + " restores " + restoredAmount + " hunger.");

        PrintSurvivalDebug();

        return restoredAmount > 0;
    }

    public bool RestoreThirst(int amount)
    {
        int safeAmount = Mathf.Max(0, amount);

        if (safeAmount == 0)
        {
            return false;
        }

        if (CurrentThirst >= maxThirst)
        {
            GameMessageLog.Write(GetDisplayName() + " is not thirsty.");
            return false;
        }

        int oldValue = CurrentThirst;

        CurrentThirst += safeAmount;

        if (CurrentThirst > maxThirst)
        {
            CurrentThirst = maxThirst;
        }

        ResetThirstWarningsIfNeeded();

        int restoredAmount = CurrentThirst - oldValue;

        GameMessageLog.Write(GetDisplayName() + " restores " + restoredAmount + " thirst.");

        PrintSurvivalDebug();

        return restoredAmount > 0;
    }

    private void ProcessHungerDrain()
    {
        hungerActionCounter++;

        if (hungerActionCounter < hungerDecreaseInterval)
        {
            return;
        }

        hungerActionCounter = 0;

        if (CurrentHunger > 0)
        {
            CurrentHunger -= hungerDecreaseAmount;

            if (CurrentHunger < 0)
            {
                CurrentHunger = 0;
            }

            CheckHungerWarnings();
            return;
        }

        ApplyStarvationDamage();
    }

    private void ProcessThirstDrain()
    {
        thirstActionCounter++;

        if (thirstActionCounter < thirstDecreaseInterval)
        {
            return;
        }

        thirstActionCounter = 0;

        if (CurrentThirst > 0)
        {
            CurrentThirst -= thirstDecreaseAmount;

            if (CurrentThirst < 0)
            {
                CurrentThirst = 0;
            }

            CheckThirstWarnings();
            return;
        }

        ApplyDehydrationDamage();
    }

    private void CheckHungerWarnings()
    {
        if (CurrentHunger <= criticalHungerThreshold && !criticalHungerMessageShown)
        {
            GameMessageLog.Write(GetDisplayName() + " is starving.");
            criticalHungerMessageShown = true;
            lowHungerMessageShown = true;
            return;
        }

        if (CurrentHunger <= lowHungerThreshold && !lowHungerMessageShown)
        {
            GameMessageLog.Write(GetDisplayName() + " is getting hungry.");
            lowHungerMessageShown = true;
        }
    }

    private void CheckThirstWarnings()
    {
        if (CurrentThirst <= criticalThirstThreshold && !criticalThirstMessageShown)
        {
            GameMessageLog.Write(GetDisplayName() + " is severely thirsty.");
            criticalThirstMessageShown = true;
            lowThirstMessageShown = true;
            return;
        }

        if (CurrentThirst <= lowThirstThreshold && !lowThirstMessageShown)
        {
            GameMessageLog.Write(GetDisplayName() + " is getting thirsty.");
            lowThirstMessageShown = true;
        }
    }

    private void ResetHungerWarningsIfNeeded()
    {
        if (CurrentHunger > lowHungerThreshold)
        {
            lowHungerMessageShown = false;
            criticalHungerMessageShown = false;
        }
        else if (CurrentHunger > criticalHungerThreshold)
        {
            criticalHungerMessageShown = false;
        }
    }

    private void ResetThirstWarningsIfNeeded()
    {
        if (CurrentThirst > lowThirstThreshold)
        {
            lowThirstMessageShown = false;
            criticalThirstMessageShown = false;
        }
        else if (CurrentThirst > criticalThirstThreshold)
        {
            criticalThirstMessageShown = false;
        }
    }

    private void ApplyStarvationDamage()
    {
        if (actorHealth == null)
        {
            return;
        }

        GameMessageLog.Write(GetDisplayName() + " suffers from starvation.");
        actorHealth.TakeDamage(starvationDamage);
    }

    private void ApplyDehydrationDamage()
    {
        if (actorHealth == null)
        {
            return;
        }

        GameMessageLog.Write(GetDisplayName() + " suffers from dehydration.");
        actorHealth.TakeDamage(dehydrationDamage);
    }

    public void PrintSurvivalDebug()
    {
        Debug.Log(
            gameObject.name +
            " survival:" +
            "\n- Hunger: " + CurrentHunger + "/" + maxHunger +
            "\n- Thirst: " + CurrentThirst + "/" + maxThirst
        );
    }

    private string GetDisplayName()
    {
        if (actorGridEntity == null)
        {
            return gameObject.name;
        }

        return actorGridEntity.DisplayName;
    }
}