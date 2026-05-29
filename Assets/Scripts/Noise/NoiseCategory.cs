/*
 * NoiseCategory
 * -------------
 * Defines the broad type of a noise event.
 *
 * Current categories:
 * - Movement: footsteps or similar movement sounds
 * - Combat: attacks, impacts, fighting
 * - Door: doors opening or closing
 * - Item: dropping or handling items
 * - Other: fallback category
 *
 * These categories let AI react differently later.
 * For now, SimpleEnemyAI treats all noise categories as investigation targets.
 */

public enum NoiseCategory
{
    Movement,
    Combat,
    Door,
    Item,
    Other
}