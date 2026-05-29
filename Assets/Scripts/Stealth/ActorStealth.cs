using UnityEngine;

/*
 * ActorStealth
 * ------------
 * Stores stealth-related state for an actor.
 *
 * This script is currently intended for the player, but it is written generally
 * enough that enemies or NPCs can also use stealth later.
 *
 * Current responsibilities:
 * - store whether the actor is sneaking
 * - reduce movement noise while sneaking
 * - reduce enemy detection range against this actor while sneaking
 * - expose helper methods for other systems
 *
 * Current stealth behavior:
 * - sneaking lowers movement noise range
 * - sneaking makes enemies require closer line-of-sight detection
 *
 * Example:
 * Player movement noise range: 3
 * Sneak movement noise multiplier: 0.35
 * Final sneaking movement noise range: 1
 *
 * Example:
 * Enemy detection range: 10
 * Sneak detection range penalty: 4
 * Enemy detects sneaking player at range 6 instead of 10
 *
 * Important:
 * This does not make the actor invisible.
 * It only makes the actor quieter and harder to notice at long range.
 *
 * Later this can expand into:
 * - stealth skill levels
 * - equipment noise penalties
 * - light/darkness modifiers
 * - enemy perception stats
 * - backstab bonuses
 * - stealth break on attack
 * - crouch animation
 */

public class ActorStealth : MonoBehaviour
{
    [Header("State")]
    [SerializeField] private bool startsSneaking = false;

    [Header("Noise")]
    [SerializeField] private float sneakingMovementNoiseMultiplier = 0.35f;
    [SerializeField] private int minimumSneakingNoiseRange = 1;

    [Header("Detection")]
    [SerializeField] private int sneakingDetectionRangePenalty = 4;
    [SerializeField] private int minimumDetectionRangeAgainstSneaking = 1;

    public bool IsSneaking { get; private set; }

    private void Awake()
    {
        IsSneaking = startsSneaking;
    }

    public void SetSneaking(bool value)
    {
        IsSneaking = value;
    }

    public void ToggleSneaking()
    {
        IsSneaking = !IsSneaking;
    }

    public int GetModifiedMovementNoiseRange(int baseNoiseRange)
    {
        int safeBaseRange = Mathf.Max(0, baseNoiseRange);

        if (!IsSneaking)
        {
            return safeBaseRange;
        }

        int modifiedRange = Mathf.RoundToInt(safeBaseRange * sneakingMovementNoiseMultiplier);

        if (modifiedRange < minimumSneakingNoiseRange)
        {
            modifiedRange = minimumSneakingNoiseRange;
        }

        return modifiedRange;
    }

    public int GetDetectionRangeAgainstActor(int observerBaseDetectionRange)
    {
        int safeBaseRange = Mathf.Max(0, observerBaseDetectionRange);

        if (!IsSneaking)
        {
            return safeBaseRange;
        }

        int modifiedRange = safeBaseRange - sneakingDetectionRangePenalty;

        if (modifiedRange < minimumDetectionRangeAgainstSneaking)
        {
            modifiedRange = minimumDetectionRangeAgainstSneaking;
        }

        return modifiedRange;
    }
}