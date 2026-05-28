using System;
using UnityEngine;

/*
 * PlayerFieldOfView
 * -----------------
 * Calculates which map cells are visible to the player.
 *
 * This script belongs on the player. It uses the player's current grid position
 * as the center of vision, checks line of sight to nearby cells, and marks cells
 * as visible/explored.
 *
 * Current behavior:
 * - player has a circular vision radius
 * - walls block vision
 * - closed doors block vision
 * - open doors allow vision
 * - visible cells become explored permanently
 * - FieldOfViewRenderer draws fog based on visible/explored cells
 * - WorldObjectVisibilityController can query visible/explored cells
 *
 * Main responsibilities:
 * - calculate visible cells
 * - store explored cells
 * - expose visibility queries to other systems
 * - notify listeners when visibility refreshes
 *
 * Important:
 * The visibility arrays are the source of truth for object visibility.
 * Actors and items should check current visibility.
 * Static remembered features can check explored visibility.
 */

[RequireComponent(typeof(ActorGridEntity))]
public class PlayerFieldOfView : MonoBehaviour
{
    public event Action VisibilityRefreshed;

    [Header("Vision")]
    [SerializeField] private int visionRadius = 8;

    private MapData mapData;
    private FieldOfViewRenderer fieldOfViewRenderer;
    private ActorGridEntity actorGridEntity;

    private bool[,] visibleCells;
    private bool[,] exploredCells;
    private bool isInitialized;

    private void Awake()
    {
        actorGridEntity = GetComponent<ActorGridEntity>();
    }

    public void Initialize(MapData newMapData, FieldOfViewRenderer newFieldOfViewRenderer)
    {
        mapData = newMapData;
        fieldOfViewRenderer = newFieldOfViewRenderer;

        if (mapData == null)
        {
            isInitialized = false;
            return;
        }

        visibleCells = new bool[mapData.Width, mapData.Height];
        exploredCells = new bool[mapData.Width, mapData.Height];

        isInitialized = true;

        RefreshVisibility();
    }

    public bool IsCellVisible(Vector2Int position)
    {
        if (!isInitialized)
        {
            return false;
        }

        if (!mapData.IsInBounds(position))
        {
            return false;
        }

        return visibleCells[position.x, position.y];
    }

    public bool IsCellExplored(Vector2Int position)
    {
        if (!isInitialized)
        {
            return false;
        }

        if (!mapData.IsInBounds(position))
        {
            return false;
        }

        return exploredCells[position.x, position.y];
    }

    public void RefreshVisibility()
    {
        if (!isInitialized)
        {
            return;
        }

        ClearVisibleCells();

        Vector2Int center = actorGridEntity.GridPosition;

        for (int x = center.x - visionRadius; x <= center.x + visionRadius; x++)
        {
            for (int y = center.y - visionRadius; y <= center.y + visionRadius; y++)
            {
                Vector2Int targetPosition = new Vector2Int(x, y);

                if (!mapData.IsInBounds(targetPosition))
                {
                    continue;
                }

                if (!IsInsideVisionRadius(center, targetPosition))
                {
                    continue;
                }

                if (!HasLineOfSight(center, targetPosition))
                {
                    continue;
                }

                visibleCells[x, y] = true;
                exploredCells[x, y] = true;
            }
        }

        if (fieldOfViewRenderer != null)
        {
            fieldOfViewRenderer.Render(mapData, visibleCells, exploredCells);
        }

        NotifyVisibilityRefreshed();
    }

    private void ClearVisibleCells()
    {
        for (int x = 0; x < mapData.Width; x++)
        {
            for (int y = 0; y < mapData.Height; y++)
            {
                visibleCells[x, y] = false;
            }
        }
    }

    private bool IsInsideVisionRadius(Vector2Int center, Vector2Int target)
    {
        int deltaX = target.x - center.x;
        int deltaY = target.y - center.y;

        return deltaX * deltaX + deltaY * deltaY <= visionRadius * visionRadius;
    }

    private bool HasLineOfSight(Vector2Int startPosition, Vector2Int targetPosition)
    {
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

            // The target cell itself should be visible even if it blocks sight.
            // This lets the player see the wall/closed door, but not beyond it.
            if (currentPosition == targetPosition)
            {
                return true;
            }

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

    private void NotifyVisibilityRefreshed()
    {
        if (VisibilityRefreshed == null)
        {
            return;
        }

        VisibilityRefreshed.Invoke();
    }
}