using UnityEngine;

/*
 * MapData
 * -------
 * Stores the full gameplay grid for one generated map or dungeon floor.
 *
 * This class owns a 2D array of MapCell objects. Each cell knows:
 * - what terrain exists there
 * - whether an actor is standing there
 * - whether a feature exists there
 * - which items are lying there
 *
 * Main responsibilities:
 * - create the grid
 * - answer walkability questions
 * - place and move actors
 * - place and remove map features
 * - place and remove items
 *
 * Important:
 * MapData is the gameplay source of truth.
 * Unity Tilemaps are only visual.
 */

public class MapData
{
    private readonly MapCell[,] cells;

    public int Width { get; private set; }
    public int Height { get; private set; }

    public MapData(int width, int height)
    {
        Width = width;
        Height = height;

        cells = new MapCell[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                cells[x, y] = new MapCell(x, y, CellTerrain.Empty);
            }
        }
    }

    public bool IsInBounds(Vector2Int position)
    {
        return position.x >= 0 &&
               position.y >= 0 &&
               position.x < Width &&
               position.y < Height;
    }

    public MapCell GetCell(Vector2Int position)
    {
        if (!IsInBounds(position))
        {
            return null;
        }

        return cells[position.x, position.y];
    }

    public void SetTerrain(Vector2Int position, CellTerrain terrain)
    {
        MapCell cell = GetCell(position);

        if (cell == null)
        {
            return;
        }

        cell.SetTerrain(terrain);
    }

    public bool IsTerrainWalkable(Vector2Int position)
    {
        MapCell cell = GetCell(position);

        if (cell == null)
        {
            return false;
        }

        return !cell.BlocksMovement;
    }

    public bool IsWalkable(Vector2Int position)
    {
        MapCell cell = GetCell(position);

        if (cell == null)
        {
            return false;
        }

        return !cell.BlocksMovement &&
               !cell.HasActor &&
               !cell.HasBlockingFeature;
    }

    public ActorGridEntity GetActorAt(Vector2Int position)
    {
        MapCell cell = GetCell(position);

        if (cell == null)
        {
            return null;
        }

        return cell.OccupyingActor;
    }

    public MapFeatureEntity GetFeatureAt(Vector2Int position)
    {
        MapCell cell = GetCell(position);

        if (cell == null)
        {
            return null;
        }

        return cell.OccupyingFeature;
    }

    public ItemGridEntity GetTopItemAt(Vector2Int position)
    {
        MapCell cell = GetCell(position);

        if (cell == null)
        {
            return null;
        }

        return cell.GetTopItem();
    }

    public bool TryPlaceActor(ActorGridEntity actor, Vector2Int position)
    {
        if (actor == null)
        {
            return false;
        }

        MapCell cell = GetCell(position);

        if (cell == null)
        {
            return false;
        }

        if (cell.BlocksMovement || cell.HasActor || cell.HasBlockingFeature)
        {
            return false;
        }

        cell.SetOccupyingActor(actor);
        return true;
    }

    public bool TryMoveActor(ActorGridEntity actor, Vector2Int fromPosition, Vector2Int toPosition)
    {
        if (actor == null)
        {
            return false;
        }

        MapCell fromCell = GetCell(fromPosition);
        MapCell toCell = GetCell(toPosition);

        if (fromCell == null || toCell == null)
        {
            return false;
        }

        if (fromCell.OccupyingActor != actor)
        {
            return false;
        }

        if (toCell.BlocksMovement || toCell.HasActor || toCell.HasBlockingFeature)
        {
            return false;
        }

        fromCell.ClearOccupyingActor();
        toCell.SetOccupyingActor(actor);

        return true;
    }

    public bool TryRemoveActor(ActorGridEntity actor, Vector2Int position)
    {
        if (actor == null)
        {
            return false;
        }

        MapCell cell = GetCell(position);

        if (cell == null)
        {
            return false;
        }

        if (cell.OccupyingActor != actor)
        {
            return false;
        }

        cell.ClearOccupyingActor();
        return true;
    }

    public bool TryPlaceFeature(MapFeatureEntity feature, Vector2Int position)
    {
        if (feature == null)
        {
            return false;
        }

        MapCell cell = GetCell(position);

        if (cell == null)
        {
            return false;
        }

        if (cell.BlocksMovement || cell.HasFeature)
        {
            return false;
        }

        if (feature.BlocksMovement && cell.HasActor)
        {
            return false;
        }

        cell.SetOccupyingFeature(feature);
        return true;
    }

    public bool TryRemoveFeature(MapFeatureEntity feature, Vector2Int position)
    {
        if (feature == null)
        {
            return false;
        }

        MapCell cell = GetCell(position);

        if (cell == null)
        {
            return false;
        }

        if (cell.OccupyingFeature != feature)
        {
            return false;
        }

        cell.ClearOccupyingFeature();
        return true;
    }

    public bool TryPlaceItem(ItemGridEntity item, Vector2Int position)
    {
        if (item == null)
        {
            return false;
        }

        MapCell cell = GetCell(position);

        if (cell == null)
        {
            return false;
        }

        // Items can only be placed on valid terrain.
        // They can share a cell with actors because standing on loot is allowed.
        if (cell.BlocksMovement)
        {
            return false;
        }

        cell.AddItem(item);
        return true;
    }

    public bool TryRemoveItem(ItemGridEntity item, Vector2Int position)
    {
        if (item == null)
        {
            return false;
        }

        MapCell cell = GetCell(position);

        if (cell == null)
        {
            return false;
        }

        cell.RemoveItem(item);
        return true;
    }
}