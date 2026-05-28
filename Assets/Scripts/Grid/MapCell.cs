using System.Collections.Generic;

/*
 * MapCell
 * -------
 * Represents one cell in the gameplay grid.
 *
 * Each MapCell stores:
 * - its grid position
 * - its terrain type
 * - which actor is occupying the cell
 * - which map feature is occupying the cell
 * - which items are lying on the cell
 *
 * Actors are creatures such as the player, enemies, and NPCs.
 * Features are map objects such as stairs, doors, chests, traps, and shrines.
 * Items are pickup objects and do not block movement.
 */

public class MapCell
{
    private readonly List<ItemGridEntity> items = new List<ItemGridEntity>();

    public int X { get; private set; }
    public int Y { get; private set; }

    public CellTerrain Terrain { get; private set; }
    public ActorGridEntity OccupyingActor { get; private set; }
    public MapFeatureEntity OccupyingFeature { get; private set; }

    public IReadOnlyList<ItemGridEntity> Items
    {
        get
        {
            return items;
        }
    }

    public bool HasActor
    {
        get
        {
            return OccupyingActor != null;
        }
    }

    public bool HasFeature
    {
        get
        {
            return OccupyingFeature != null;
        }
    }

    public bool HasItems
    {
        get
        {
            return items.Count > 0;
        }
    }

    public bool HasBlockingFeature
    {
        get
        {
            return OccupyingFeature != null && OccupyingFeature.BlocksMovement;
        }
    }

    public bool HasBlockingSightFeature
    {
        get
        {
            return OccupyingFeature != null && OccupyingFeature.BlocksSight;
        }
    }

    public bool BlocksMovement
    {
        get
        {
            return Terrain == CellTerrain.Wall || Terrain == CellTerrain.Empty;
        }
    }

    public bool BlocksSight
    {
        get
        {
            return Terrain == CellTerrain.Wall || Terrain == CellTerrain.Empty;
        }
    }

    public MapCell(int x, int y, CellTerrain terrain)
    {
        X = x;
        Y = y;
        Terrain = terrain;
    }

    public void SetTerrain(CellTerrain terrain)
    {
        Terrain = terrain;
    }

    public void SetOccupyingActor(ActorGridEntity actor)
    {
        OccupyingActor = actor;
    }

    public void ClearOccupyingActor()
    {
        OccupyingActor = null;
    }

    public void SetOccupyingFeature(MapFeatureEntity feature)
    {
        OccupyingFeature = feature;
    }

    public void ClearOccupyingFeature()
    {
        OccupyingFeature = null;
    }

    public void AddItem(ItemGridEntity item)
    {
        if (item == null)
        {
            return;
        }

        if (items.Contains(item))
        {
            return;
        }

        items.Add(item);
    }

    public void RemoveItem(ItemGridEntity item)
    {
        if (item == null)
        {
            return;
        }

        items.Remove(item);
    }

    public ItemGridEntity GetTopItem()
    {
        if (items.Count == 0)
        {
            return null;
        }

        // The last item is treated as the top item in the pile.
        return items[items.Count - 1];
    }
}