/*
 * CellTerrain
 * -----------
 * Represents the basic terrain type of a single cell in the gameplay grid.
 *
 * This enum is used by MapCell and MapData to decide what exists at each grid
 * position before the map is rendered to Unity Tilemaps.
 *
 * Important:
 * - This is gameplay data, not visual data.
 * - The Tilemap only displays the result of this data.
 * - Movement, sight blocking, spawning, and pathfinding should use this data.
 */

public enum CellTerrain
{
    Empty,
    Floor,
    Wall
}