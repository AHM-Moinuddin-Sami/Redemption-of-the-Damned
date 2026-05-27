using System.Collections.Generic;
using UnityEngine;

/*
 * GenerationResult
 * ----------------
 * Stores the result of a dungeon generation pass.
 *
 * Current responsibilities:
 * - store the generated MapData
 * - store the player's starting grid position
 * - store stairs down position
 * - store enemy spawn positions
 * - store item spawn positions
 *
 * Later this can expand to include:
 * - stairs up position
 * - boss room position
 * - room metadata
 * - faction ownership data
 * - biome/zone information
 * - generated quest hooks
 */

public class GenerationResult
{
    public MapData MapData { get; private set; }
    public Vector2Int PlayerSpawnPosition { get; private set; }
    public Vector2Int StairsDownPosition { get; private set; }
    public IReadOnlyList<Vector2Int> EnemySpawnPositions { get; private set; }
    public IReadOnlyList<Vector2Int> ItemSpawnPositions { get; private set; }

    public GenerationResult(
        MapData mapData,
        Vector2Int playerSpawnPosition,
        Vector2Int stairsDownPosition,
        List<Vector2Int> enemySpawnPositions,
        List<Vector2Int> itemSpawnPositions)
    {
        MapData = mapData;
        PlayerSpawnPosition = playerSpawnPosition;
        StairsDownPosition = stairsDownPosition;
        EnemySpawnPositions = enemySpawnPositions;
        ItemSpawnPositions = itemSpawnPositions;
    }
}