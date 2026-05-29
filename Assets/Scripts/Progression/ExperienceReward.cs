using UnityEngine;

/*
 * ExperienceReward
 * ----------------
 * Stores how much XP an actor is worth when killed.
 *
 * This script should go on enemy prefabs.
 *
 * Current responsibilities:
 * - store experience reward amount
 * - expose the amount to GameBootstrap when the enemy dies
 *
 * Important:
 * This script does not detect death by itself.
 * GameBootstrap listens to enemy ActorHealth.Died and then checks this component.
 *
 * Later this can expand into:
 * - level-scaled XP
 * - elite enemy bonuses
 * - faction penalties
 * - no-XP summons
 * - quest XP
 */

public class ExperienceReward : MonoBehaviour
{
    [Header("Reward")]
    [SerializeField] private int experienceAmount = 5;

    public int ExperienceAmount
    {
        get
        {
            return Mathf.Max(0, experienceAmount);
        }
    }

    public void SetExperienceAmount(int amount)
    {
        experienceAmount = Mathf.Max(0, amount);
    }
}