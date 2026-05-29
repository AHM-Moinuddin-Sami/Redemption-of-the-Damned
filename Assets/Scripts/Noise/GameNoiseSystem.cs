using System;
using UnityEngine;

/*
 * GameNoiseSystem
 * ---------------
 * Broadcasts gameplay noise events to interested systems.
 *
 * This is a lightweight static event system. When something noisy happens, a
 * script calls EmitNoise(). Any listening AI can then decide whether it heard
 * the event.
 *
 * Current responsibilities:
 * - emit noise events
 * - notify subscribed listeners
 *
 * Important:
 * This does not play audio.
 * Audio is separate. This is only for AI awareness.
 *
 * Later this can expand into:
 * - sound obstruction
 * - sound falloff
 * - noise priority
 * - different hearing stats per enemy
 * - global noise logging/debug visualization
 */

public static class GameNoiseSystem
{
    public static event Action<GameNoiseEvent> NoiseEmitted;

    public static void EmitNoise(
        Vector2Int position,
        int noiseRange,
        ActorGridEntity sourceActor,
        NoiseCategory category)
    {
        if (noiseRange <= 0)
        {
            return;
        }

        GameNoiseEvent noiseEvent = new GameNoiseEvent(
            position,
            noiseRange,
            sourceActor,
            category
        );

        if (NoiseEmitted != null)
        {
            NoiseEmitted.Invoke(noiseEvent);
        }
    }
}