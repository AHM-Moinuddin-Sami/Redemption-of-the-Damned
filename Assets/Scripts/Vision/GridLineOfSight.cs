using UnityEngine;

/*
 * GridLineOfSight
 * ---------------
 * Provides reusable grid-based line-of-sight checks.
 *
 * This is used by AI so enemies cannot detect the player through walls, closed
 * doors, or other sight-blocking map features.
 *
 * Current responsibilities:
 * - check whether one grid cell can see another grid cell
 * - use MapData.BlocksSight() for terrain and feature blocking
 * - allow the target cell itself to be seen even if it blocks sight
 *
 * Example:
 * - Player can see a wall tile.
 * - Player cannot see tiles behind that wall.
 * - Enemy can see a closed door.
 * - Enemy cannot see through that closed door.
 *
 * Important:
 * This uses Bresenham-style grid line tracing.
 * It is simple, reliable, and good enough for this prototype.
 *
 * Later this can expand into:
 * - cover checks
 * - soft visibility
 * - stealth modifiers
 * - light level checks
 * - enemy-specific vision rules
 */

public static class GridLineOfSight
{
    public static bool HasLineOfSight(MapData mapData, Vector2Int startPosition, Vector2Int targetPosition)
    {
        if (mapData == null)
        {
            return false;
        }

        if (!mapData.IsInBounds(startPosition) || !mapData.IsInBounds(targetPosition))
        {
            return false;
        }

        int x0 = startPosition.x;
        int y0 = startPosition.y;
        int x1 = targetPosition.x;
        int y1 = targetPosition.y;

        int deltaX = Mathf.Abs(x1 - x0);
        int deltaY = Mathf.Abs(y1 - y0);

        int stepX = x0 < x1 ? 1 : -1;
        int stepY = y0 < y1 ? 1 : -1;

        int error = deltaX - deltaY;

        int currentX = x0;
        int currentY = y0;

        while (true)
        {
            Vector2Int currentPosition = new Vector2Int(currentX, currentY);

            // The target itself is visible even if the target blocks sight.
            // This means actors can see a wall or closed door, but not beyond it.
            if (currentPosition == targetPosition)
            {
                return true;
            }

            // The starting cell should not block its own sight check.
            if (currentPosition != startPosition && mapData.BlocksSight(currentPosition))
            {
                return false;
            }

            int doubledError = error * 2;

            if (doubledError > -deltaY)
            {
                error -= deltaY;
                currentX += stepX;
            }

            if (doubledError < deltaX)
            {
                error += deltaX;
                currentY += stepY;
            }
        }
    }
}