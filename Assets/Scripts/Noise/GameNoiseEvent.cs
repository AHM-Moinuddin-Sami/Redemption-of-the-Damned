using UnityEngine;

/*
 * GameNoiseEvent
 * --------------
 * Stores one emitted noise event in the world.
 *
 * A noise event is not a sound effect. It is gameplay information that AI can
 * react to.
 *
 * Current responsibilities:
 * - store the grid position where the noise happened
 * - store the range at which the noise can be heard
 * - store the actor that caused the noise, if any
 * - store the noise category
 *
 * Example:
 * Player moves:
 * - Position: player's grid position
 * - Range: 3
 * - Source Actor: player
 * - Category: Movement
 *
 * Later this can expand into:
 * - loudness
 * - noise text
 * - faction ownership
 * - muffled/blocked sound
 * - material-based sound propagation
 */

public class GameNoiseEvent
{
    public Vector2Int Position { get; private set; }
    public int NoiseRange { get; private set; }
    public ActorGridEntity SourceActor { get; private set; }
    public NoiseCategory Category { get; private set; }

    public GameNoiseEvent(
        Vector2Int position,
        int noiseRange,
        ActorGridEntity sourceActor,
        NoiseCategory category)
    {
        Position = position;
        NoiseRange = Mathf.Max(0, noiseRange);
        SourceActor = sourceActor;
        Category = category;
    }
}