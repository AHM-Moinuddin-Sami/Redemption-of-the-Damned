using UnityEngine;

/*
 * ActorSurvival
 * -------------
 * Stores and updates hunger and thirst values for an actor.
 *
 * This script is currently mainly used by the player.
 *
 * Current responsibilities:
 * - store current hunger
 * - store current thirst
 * - reduce hunger/thirst when an action is taken
 * - damage the actor when starving or dehydrated
 * - restore hunger/thirst from consumables or item special effects
 *
 * Important:
 * TurnManager calls OnActionTaken() after each valid player action.
 *
 * Later this can expand into:
 * - sanity
 * - disease
 * - fatigue
 * - sleep
 * - temperature
 * - different hunger/thirst drain rates by biome
 */

[RequireComponent(typeof(ActorHealth))]
public class ActorSurvival : MonoBehaviour
{
    [Header("Hunger")]
    [SerializeField] private float maxHunger = 100;
    [SerializeField] private float startingHunger = 100;
    [SerializeField] private float hungerLossPerAction = 1;

    [Header("Thirst")]
    [SerializeField] private float maxThirst = 100;
    [SerializeField] private float startingThirst = 100;
    [SerializeField] private float thirstLossPerAction = 1;

    [Header("Damage")]
    [SerializeField] private int starvationDamage = 1;
    [SerializeField] private int dehydrationDamage = 1;

    public float CurrentHunger { get; private set; }
    public float CurrentThirst { get; private set; }

    public float MaxHunger
    {
        get
        {
            return Mathf.Max(1.0f, maxHunger);
        }
    }

    public float MaxThirst
    {
        get
        {
            return Mathf.Max(1, maxThirst);
        }
    }

    private ActorHealth actorHealth;

    private void Awake()
    {
        actorHealth = GetComponent<ActorHealth>();

        CurrentHunger = Mathf.Clamp(startingHunger, 0, MaxHunger);
        CurrentThirst = Mathf.Clamp(startingThirst, 0, MaxThirst);
    }

    public void OnActionTaken()
    {
        ReduceHunger(hungerLossPerAction);
        ReduceThirst(thirstLossPerAction);

        ApplySurvivalDamageIfNeeded();
    }

    public bool RestoreHunger(float amount)
    {
        float safeAmount = Mathf.Max(0, amount);

        if (safeAmount == 0)
        {
            return false;
        }

        if (CurrentHunger >= MaxHunger)
        {
            GameMessageLog.Write("You are already full.");
            return false;
        }

        float oldValue = CurrentHunger;
        CurrentHunger = Mathf.Min(MaxHunger, CurrentHunger + safeAmount);

        float restored = CurrentHunger - oldValue;
        GameMessageLog.Write("You restore " + restored + " hunger.");

        return restored > 0;
    }

    public bool RestoreThirst(float amount)
    {
        float safeAmount = Mathf.Max(0, amount);

        if (safeAmount == 0)
        {
            return false;
        }

        if (CurrentThirst >= MaxThirst)
        {
            GameMessageLog.Write("You are not thirsty.");
            return false;
        }

        float oldValue = CurrentThirst;
        CurrentThirst = Mathf.Min(MaxThirst, CurrentThirst + safeAmount);

        float restored = CurrentThirst - oldValue;
        GameMessageLog.Write("You restore " + restored + " thirst.");

        return restored > 0;
    }

    public void SetToFullSurvival()
    {
        CurrentHunger = MaxHunger;
        CurrentThirst = MaxThirst;
    }

    private void ReduceHunger(float amount)
    {
        float safeAmount = Mathf.Max(0, amount);
        CurrentHunger = Mathf.Max(0, CurrentHunger - safeAmount);
    }

    private void ReduceThirst(float amount)
    {
        float safeAmount = Mathf.Max(0, amount);
        CurrentThirst = Mathf.Max(0, CurrentThirst - safeAmount);
    }

    private void ApplySurvivalDamageIfNeeded()
    {
        if (actorHealth == null || actorHealth.IsDead)
        {
            return;
        }

        if (CurrentHunger <= 0 && starvationDamage > 0)
        {
            GameMessageLog.Write("You are starving.");
            actorHealth.TakeDamage(starvationDamage);
        }

        if (CurrentThirst <= 0 && dehydrationDamage > 0)
        {
            GameMessageLog.Write("You are dehydrated.");
            actorHealth.TakeDamage(dehydrationDamage);
        }
    }
}