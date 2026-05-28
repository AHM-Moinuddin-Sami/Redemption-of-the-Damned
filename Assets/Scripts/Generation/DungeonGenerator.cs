using System.Collections.Generic;
using UnityEngine;

/*
 * DungeonGenerator
 * ----------------
 * Generates a simple room-and-corridor dungeon using gameplay grid data.
 *
 * Current generation process:
 * 1. Create a blank MapData grid filled with Empty cells.
 * 2. Try to place random rectangular rooms.
 * 3. Reject rooms that overlap existing rooms.
 * 4. Carve accepted rooms into Floor cells.
 * 5. If no rooms are created, create one fallback room.
 * 6. Connect each room to the previous room with an L-shaped corridor.
 * 7. Convert Empty cells beside floors into Wall cells.
 * 8. Choose player spawn, stairs spawn, enemy spawns, item spawns, and door spawns.
 * 9. Return a GenerationResult containing the map and useful positions.
 *
 * Door generation:
 * Doors are currently placed on narrow floor chokepoints.
 * A horizontal doorway candidate has floor left/right and wall up/down.
 * A vertical doorway candidate has floor up/down and wall left/right.
 *
 * Important:
 * This is still a foundation generator.
 * Later, door placement should become room-aware so doors appear specifically
 * at room entrances instead of any valid chokepoint.
 */

public class DungeonGenerator : MonoBehaviour
{
    [Header("Map Size")]
    [SerializeField] private int width = 80;
    [SerializeField] private int height = 45;

    [Header("Rooms")]
    [SerializeField] private int roomAttempts = 80;
    [SerializeField] private int minRoomSize = 5;
    [SerializeField] private int maxRoomSize = 12;

    [Header("Temporary Spawning")]
    [SerializeField] private int maxEnemySpawnPositions = 5;
    [SerializeField] private int maxItemSpawnPositions = 6;
    [SerializeField] private int maxDoorSpawnPositions = 12;

    [Header("Seed")]
    [SerializeField] private bool useRandomSeed = true;
    [SerializeField] private int seed = 12345;

    private readonly List<RectInt> rooms = new List<RectInt>();

    public GenerationResult Generate()
    {
        rooms.Clear();

        int activeSeed = useRandomSeed ? Random.Range(int.MinValue, int.MaxValue) : seed;
        System.Random random = new System.Random(activeSeed);

        MapData mapData = new MapData(width, height);

        GenerateRooms(mapData, random);

        if (rooms.Count == 0)
        {
            CreateFallbackRoom(mapData);
        }

        ConnectRooms(mapData);
        BuildWalls(mapData);

        Vector2Int playerSpawnPosition = GetRoomCenter(rooms[0]);
        Vector2Int stairsDownPosition = CreateStairsDownPosition(mapData, playerSpawnPosition);

        List<Vector2Int> enemySpawnPositions = CreateEnemySpawnPositions();
        List<Vector2Int> doorSpawnPositions = CreateDoorSpawnPositions(mapData, random, playerSpawnPosition, stairsDownPosition);
        List<Vector2Int> itemSpawnPositions = CreateItemSpawnPositions(
            mapData,
            random,
            playerSpawnPosition,
            stairsDownPosition,
            enemySpawnPositions,
            doorSpawnPositions
        );

        return new GenerationResult(
            mapData,
            playerSpawnPosition,
            stairsDownPosition,
            enemySpawnPositions,
            itemSpawnPositions,
            doorSpawnPositions
        );
    }

    private void GenerateRooms(MapData mapData, System.Random random)
    {
        for (int i = 0; i < roomAttempts; i++)
        {
            int roomWidth = random.Next(minRoomSize, maxRoomSize + 1);
            int roomHeight = random.Next(minRoomSize, maxRoomSize + 1);

            int x = random.Next(1, mapData.Width - roomWidth - 1);
            int y = random.Next(1, mapData.Height - roomHeight - 1);

            RectInt newRoom = new RectInt(x, y, roomWidth, roomHeight);

            if (DoesRoomOverlap(newRoom))
            {
                continue;
            }

            rooms.Add(newRoom);
            CarveRoom(mapData, newRoom);
        }
    }

    private void CreateFallbackRoom(MapData mapData)
    {
        int fallbackWidth = 7;
        int fallbackHeight = 7;

        int x = mapData.Width / 2 - fallbackWidth / 2;
        int y = mapData.Height / 2 - fallbackHeight / 2;

        RectInt fallbackRoom = new RectInt(x, y, fallbackWidth, fallbackHeight);

        rooms.Add(fallbackRoom);
        CarveRoom(mapData, fallbackRoom);
    }

    private Vector2Int CreateStairsDownPosition(MapData mapData, Vector2Int playerSpawnPosition)
    {
        if (rooms.Count > 1)
        {
            return GetRoomCenter(rooms[rooms.Count - 1]);
        }

        Vector2Int[] fallbackOffsets =
        {
            new Vector2Int(2, 0),
            new Vector2Int(-2, 0),
            new Vector2Int(0, 2),
            new Vector2Int(0, -2),
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        for (int i = 0; i < fallbackOffsets.Length; i++)
        {
            Vector2Int candidatePosition = playerSpawnPosition + fallbackOffsets[i];

            if (mapData.IsTerrainWalkable(candidatePosition))
            {
                return candidatePosition;
            }
        }

        return playerSpawnPosition;
    }

    private List<Vector2Int> CreateEnemySpawnPositions()
    {
        List<Vector2Int> spawnPositions = new List<Vector2Int>();

        for (int i = 1; i < rooms.Count - 1; i++)
        {
            if (spawnPositions.Count >= maxEnemySpawnPositions)
            {
                break;
            }

            spawnPositions.Add(GetRoomCenter(rooms[i]));
        }

        return spawnPositions;
    }

    private List<Vector2Int> CreateDoorSpawnPositions(
        MapData mapData,
        System.Random random,
        Vector2Int playerSpawnPosition,
        Vector2Int stairsDownPosition)
    {
        List<Vector2Int> candidates = new List<Vector2Int>();

        for (int x = 1; x < mapData.Width - 1; x++)
        {
            for (int y = 1; y < mapData.Height - 1; y++)
            {
                Vector2Int position = new Vector2Int(x, y);

                if (position == playerSpawnPosition || position == stairsDownPosition)
                {
                    continue;
                }

                if (!mapData.IsTerrainWalkable(position))
                {
                    continue;
                }

                if (IsDoorCandidate(mapData, position))
                {
                    candidates.Add(position);
                }
            }
        }

        Shuffle(candidates, random);

        List<Vector2Int> doorPositions = new List<Vector2Int>();

        for (int i = 0; i < candidates.Count; i++)
        {
            if (doorPositions.Count >= maxDoorSpawnPositions)
            {
                break;
            }

            // Avoid placing doors directly next to each other.
            if (IsNearExistingDoor(candidates[i], doorPositions))
            {
                continue;
            }

            doorPositions.Add(candidates[i]);
        }

        return doorPositions;
    }

    private bool IsDoorCandidate(MapData mapData, Vector2Int position)
    {
        bool floorLeft = IsFloor(mapData, position + Vector2Int.left);
        bool floorRight = IsFloor(mapData, position + Vector2Int.right);
        bool floorUp = IsFloor(mapData, position + Vector2Int.up);
        bool floorDown = IsFloor(mapData, position + Vector2Int.down);

        bool wallLeft = IsWall(mapData, position + Vector2Int.left);
        bool wallRight = IsWall(mapData, position + Vector2Int.right);
        bool wallUp = IsWall(mapData, position + Vector2Int.up);
        bool wallDown = IsWall(mapData, position + Vector2Int.down);

        bool horizontalDoorway = floorLeft && floorRight && wallUp && wallDown;
        bool verticalDoorway = floorUp && floorDown && wallLeft && wallRight;

        return horizontalDoorway || verticalDoorway;
    }

    private bool IsNearExistingDoor(Vector2Int candidatePosition, List<Vector2Int> existingDoors)
    {
        for (int i = 0; i < existingDoors.Count; i++)
        {
            int distance = Mathf.Abs(candidatePosition.x - existingDoors[i].x) +
                           Mathf.Abs(candidatePosition.y - existingDoors[i].y);

            if (distance <= 2)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsFloor(MapData mapData, Vector2Int position)
    {
        MapCell cell = mapData.GetCell(position);

        return cell != null && cell.Terrain == CellTerrain.Floor;
    }

    private bool IsWall(MapData mapData, Vector2Int position)
    {
        MapCell cell = mapData.GetCell(position);

        return cell != null && cell.Terrain == CellTerrain.Wall;
    }

    private List<Vector2Int> CreateItemSpawnPositions(
        MapData mapData,
        System.Random random,
        Vector2Int playerSpawnPosition,
        Vector2Int stairsDownPosition,
        List<Vector2Int> enemySpawnPositions,
        List<Vector2Int> doorSpawnPositions)
    {
        List<Vector2Int> itemSpawnPositions = new List<Vector2Int>();

        if (rooms.Count == 0)
        {
            return itemSpawnPositions;
        }

        int attempts = maxItemSpawnPositions * 20;

        for (int i = 0; i < attempts; i++)
        {
            if (itemSpawnPositions.Count >= maxItemSpawnPositions)
            {
                break;
            }

            RectInt room = rooms[random.Next(0, rooms.Count)];

            Vector2Int candidatePosition = new Vector2Int(
                random.Next(room.xMin, room.xMax),
                random.Next(room.yMin, room.yMax)
            );

            if (!mapData.IsTerrainWalkable(candidatePosition))
            {
                continue;
            }

            if (candidatePosition == playerSpawnPosition || candidatePosition == stairsDownPosition)
            {
                continue;
            }

            if (enemySpawnPositions.Contains(candidatePosition))
            {
                continue;
            }

            if (doorSpawnPositions.Contains(candidatePosition))
            {
                continue;
            }

            if (itemSpawnPositions.Contains(candidatePosition))
            {
                continue;
            }

            itemSpawnPositions.Add(candidatePosition);
        }

        return itemSpawnPositions;
    }

    private bool DoesRoomOverlap(RectInt newRoom)
    {
        RectInt paddedRoom = new RectInt(
            newRoom.xMin - 1,
            newRoom.yMin - 1,
            newRoom.width + 2,
            newRoom.height + 2
        );

        for (int i = 0; i < rooms.Count; i++)
        {
            if (paddedRoom.Overlaps(rooms[i]))
            {
                return true;
            }
        }

        return false;
    }

    private void CarveRoom(MapData mapData, RectInt room)
    {
        for (int x = room.xMin; x < room.xMax; x++)
        {
            for (int y = room.yMin; y < room.yMax; y++)
            {
                mapData.SetTerrain(new Vector2Int(x, y), CellTerrain.Floor);
            }
        }
    }

    private void ConnectRooms(MapData mapData)
    {
        for (int i = 1; i < rooms.Count; i++)
        {
            Vector2Int previousCenter = GetRoomCenter(rooms[i - 1]);
            Vector2Int currentCenter = GetRoomCenter(rooms[i]);

            CarveCorridor(mapData, previousCenter, currentCenter);
        }
    }

    private void CarveCorridor(MapData mapData, Vector2Int from, Vector2Int to)
    {
        Vector2Int current = from;

        while (current.x != to.x)
        {
            mapData.SetTerrain(current, CellTerrain.Floor);

            if (to.x > current.x)
            {
                current.x++;
            }
            else
            {
                current.x--;
            }
        }

        while (current.y != to.y)
        {
            mapData.SetTerrain(current, CellTerrain.Floor);

            if (to.y > current.y)
            {
                current.y++;
            }
            else
            {
                current.y--;
            }
        }

        mapData.SetTerrain(to, CellTerrain.Floor);
    }

    private void BuildWalls(MapData mapData)
    {
        for (int x = 1; x < mapData.Width - 1; x++)
        {
            for (int y = 1; y < mapData.Height - 1; y++)
            {
                Vector2Int position = new Vector2Int(x, y);
                MapCell cell = mapData.GetCell(position);

                if (cell.Terrain != CellTerrain.Empty)
                {
                    continue;
                }

                if (IsBesideFloor(mapData, position))
                {
                    mapData.SetTerrain(position, CellTerrain.Wall);
                }
            }
        }
    }

    private bool IsBesideFloor(MapData mapData, Vector2Int position)
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                {
                    continue;
                }

                Vector2Int checkPosition = position + new Vector2Int(x, y);
                MapCell checkCell = mapData.GetCell(checkPosition);

                if (checkCell != null && checkCell.Terrain == CellTerrain.Floor)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void Shuffle(List<Vector2Int> positions, System.Random random)
    {
        for (int i = positions.Count - 1; i > 0; i--)
        {
            int swapIndex = random.Next(0, i + 1);

            Vector2Int temporary = positions[i];
            positions[i] = positions[swapIndex];
            positions[swapIndex] = temporary;
        }
    }

    private Vector2Int GetRoomCenter(RectInt room)
    {
        return new Vector2Int(
            room.xMin + room.width / 2,
            room.yMin + room.height / 2
        );
    }
}