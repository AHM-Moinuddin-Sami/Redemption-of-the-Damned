using System.Collections.Generic;
using UnityEngine;

/*
 * GridPathfinder
 * --------------
 * Finds simple 4-directional paths through the gameplay grid.
 *
 * This is a lightweight Breadth-First Search pathfinder for the current
 * roguelike prototype.
 *
 * Current responsibilities:
 * - search from a start cell to a goal cell
 * - avoid walls, empty space, closed doors, and occupied actor cells
 * - allow the goal cell to be occupied by the target actor
 * - return a list of grid positions from start to goal
 *
 * Path result format:
 * - path[0] is the start position
 * - path[path.Count - 1] is the goal position
 *
 * Example:
 * Enemy at (2, 2)
 * Player at (5, 2)
 * Path:
 * (2,2), (3,2), (4,2), (5,2)
 *
 * Important:
 * This is not A* yet.
 * BFS is easier to understand and reliable for small maps. Later, if map sizes
 * become much larger, this can be replaced with A* using the same public method.
 *
 * Later this can expand into:
 * - diagonal movement
 * - movement costs
 * - dangerous tile avoidance
 * - door-opening path logic
 * - faction blocking rules
 * - cached paths
 */

public static class GridPathfinder
{
    private static readonly Vector2Int[] CardinalDirections =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    public static bool TryFindPath(
        MapData mapData,
        Vector2Int startPosition,
        Vector2Int goalPosition,
        int maxSearchDistance,
        out List<Vector2Int> path)
    {
        path = new List<Vector2Int>();

        if (mapData == null)
        {
            return false;
        }

        if (!mapData.IsInBounds(startPosition) || !mapData.IsInBounds(goalPosition))
        {
            return false;
        }

        if (startPosition == goalPosition)
        {
            path.Add(startPosition);
            return true;
        }

        Queue<Vector2Int> frontier = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        Dictionary<Vector2Int, int> distanceFromStart = new Dictionary<Vector2Int, int>();

        frontier.Enqueue(startPosition);
        cameFrom[startPosition] = startPosition;
        distanceFromStart[startPosition] = 0;

        while (frontier.Count > 0)
        {
            Vector2Int currentPosition = frontier.Dequeue();

            if (currentPosition == goalPosition)
            {
                path = BuildPath(cameFrom, startPosition, goalPosition);
                return true;
            }

            int currentDistance = distanceFromStart[currentPosition];

            if (currentDistance >= maxSearchDistance)
            {
                continue;
            }

            for (int i = 0; i < CardinalDirections.Length; i++)
            {
                Vector2Int nextPosition = currentPosition + CardinalDirections[i];

                if (cameFrom.ContainsKey(nextPosition))
                {
                    continue;
                }

                if (!mapData.IsPathableForActor(nextPosition, goalPosition))
                {
                    continue;
                }

                frontier.Enqueue(nextPosition);
                cameFrom[nextPosition] = currentPosition;
                distanceFromStart[nextPosition] = currentDistance + 1;
            }
        }

        return false;
    }

    private static List<Vector2Int> BuildPath(
        Dictionary<Vector2Int, Vector2Int> cameFrom,
        Vector2Int startPosition,
        Vector2Int goalPosition)
    {
        List<Vector2Int> reversedPath = new List<Vector2Int>();

        Vector2Int currentPosition = goalPosition;
        reversedPath.Add(currentPosition);

        while (currentPosition != startPosition)
        {
            currentPosition = cameFrom[currentPosition];
            reversedPath.Add(currentPosition);
        }

        reversedPath.Reverse();

        return reversedPath;
    }
}