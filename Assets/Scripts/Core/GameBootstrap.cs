using System.Collections.Generic;
using UnityEngine;

/*
 * GameBootstrap
 * -------------
 * Starts and resets the current prototype dungeon floor.
 *
 * Current startup process:
 * 1. Generate a dungeon.
 * 2. Store generated MapData.
 * 3. Render the map to Tilemaps.
 * 4. Spawn the player.
 * 5. Initialize FOV and world object visibility.
 * 6. Spawn stairs, doors, enemies, and items.
 * 7. Initialize enemy AI.
 * 8. Initialize TurnManager.
 * 9. Refresh FOV once all runtime objects exist.
 * 10. Assign camera/UI targets.
 *
 * This script is still a prototype bootstrapper.
 * Later, it should be split into:
 * - RunManager
 * - ZoneManager
 * - ActorSpawner
 * - FeatureSpawner
 * - ItemSpawner
 * - SaveManager
 */

public class GameBootstrap : MonoBehaviour
{
    [Header("Generation")]
    [SerializeField] private DungeonGenerator dungeonGenerator;
    [SerializeField] private MapRenderer mapRenderer;

    [Header("Vision")]
    [SerializeField] private FieldOfViewRenderer fieldOfViewRenderer;
    [SerializeField] private WorldObjectVisibilityController worldObjectVisibilityController;

    [Header("Turns")]
    [SerializeField] private TurnManager turnManager;

    [Header("Player")]
    [SerializeField] private PlayerGridMover playerPrefab;

    [Header("Enemies")]
    [SerializeField] private ActorGridEntity testEnemyPrefab;

    [Header("Features")]
    [SerializeField] private StairsDownFeature stairsDownPrefab;
    [SerializeField] private DoorFeature doorPrefab;

    [Header("Items")]
    [SerializeField] private ItemGridEntity itemPrefab;
    [SerializeField] private ItemDropTable floorLootTable;

    [Header("Scene References")]
    [SerializeField] private CameraFollowTarget cameraFollowTarget;
    [SerializeField] private Transform actorParent;
    [SerializeField] private Transform featureParent;
    [SerializeField] private Transform itemParent;

    [Header("UI")]
    [SerializeField] private PlayerHUD playerHUD;
    [SerializeField] private PlayerInventoryUI playerInventoryUI;
    [SerializeField] private PlayerEquipmentUI playerEquipmentUI;

    private readonly List<SimpleEnemyAI> spawnedEnemyAIs = new List<SimpleEnemyAI>();
    private readonly List<ItemGridEntity> spawnedItems = new List<ItemGridEntity>();
    private readonly List<DoorFeature> spawnedDoors = new List<DoorFeature>();

    private MapData currentMapData;
    private PlayerGridMover currentPlayer;
    private ActorGridEntity currentPlayerActor;
    private PlayerFieldOfView currentPlayerFieldOfView;
    private StairsDownFeature currentStairsDown;
    private int currentFloorNumber = 1;

    private void Start()
    {
        GenerateCurrentFloor();
    }

    public void GoToNextFloor()
    {
        currentFloorNumber++;
        GenerateCurrentFloor();
    }

    private void GenerateCurrentFloor()
    {
        GameMessageLog.Write("Dungeon floor " + currentFloorNumber + ".");

        ClearExistingRuntimeObjects();

        GenerationResult generationResult = dungeonGenerator.Generate();

        currentMapData = generationResult.MapData;

        mapRenderer.Render(currentMapData);

        SpawnPlayer(generationResult.PlayerSpawnPosition);
        SpawnStairsDown(generationResult.StairsDownPosition);
        SpawnDoors(generationResult.DoorSpawnPositions);
        SpawnEnemies(generationResult.EnemySpawnPositions);
        SpawnItems(generationResult.ItemSpawnPositions);
        InitializeTurnManager();
        RefreshCurrentFieldOfView();
    }

    private void ClearExistingRuntimeObjects()
    {
        if (worldObjectVisibilityController != null)
        {
            worldObjectVisibilityController.SetTarget(null);
        }

        if (fieldOfViewRenderer != null)
        {
            fieldOfViewRenderer.Clear();
        }

        if (currentPlayer != null)
        {
            Destroy(currentPlayer.gameObject);
            currentPlayer = null;
            currentPlayerActor = null;
            currentPlayerFieldOfView = null;
        }

        if (currentStairsDown != null)
        {
            Destroy(currentStairsDown.gameObject);
            currentStairsDown = null;
        }

        for (int i = 0; i < spawnedDoors.Count; i++)
        {
            if (spawnedDoors[i] != null)
            {
                Destroy(spawnedDoors[i].gameObject);
            }
        }

        for (int i = 0; i < spawnedEnemyAIs.Count; i++)
        {
            if (spawnedEnemyAIs[i] != null)
            {
                Destroy(spawnedEnemyAIs[i].gameObject);
            }
        }

        for (int i = 0; i < spawnedItems.Count; i++)
        {
            if (spawnedItems[i] != null)
            {
                Destroy(spawnedItems[i].gameObject);
            }
        }

        spawnedDoors.Clear();
        spawnedEnemyAIs.Clear();
        spawnedItems.Clear();
    }

    private void SpawnPlayer(Vector2Int spawnPosition)
    {
        currentPlayer = Instantiate(playerPrefab, actorParent);
        currentPlayer.Initialize(currentMapData, mapRenderer, spawnPosition, turnManager);

        currentPlayerActor = currentPlayer.GetComponent<ActorGridEntity>();
        currentPlayerFieldOfView = currentPlayer.GetComponent<PlayerFieldOfView>();

        if (currentPlayerFieldOfView != null)
        {
            currentPlayerFieldOfView.Initialize(currentMapData, fieldOfViewRenderer);
        }

        if (worldObjectVisibilityController != null)
        {
            worldObjectVisibilityController.SetTarget(currentPlayerFieldOfView);
        }

        if (playerHUD != null)
        {
            playerHUD.SetTarget(currentPlayerActor, currentFloorNumber);
        }

        ActorInventory playerInventory = currentPlayer.GetComponent<ActorInventory>();
        ActorEquipment playerEquipment = currentPlayer.GetComponent<ActorEquipment>();
        ActorItemUser playerItemUser = currentPlayer.GetComponent<ActorItemUser>();
        ActorItemDropper playerItemDropper = currentPlayer.GetComponent<ActorItemDropper>();

        if (playerItemDropper != null)
        {
            playerItemDropper.Initialize(currentMapData, mapRenderer, itemPrefab, itemParent);
        }

        if (playerInventoryUI != null)
        {
            playerInventoryUI.SetTarget(
                playerInventory,
                playerEquipment,
                playerItemUser,
                playerItemDropper,
                turnManager
            );
        }

        if (playerEquipmentUI != null)
        {
            playerEquipmentUI.SetTarget(playerEquipment);
        }

        InitializePlayerControllers();

        if (cameraFollowTarget != null)
        {
            cameraFollowTarget.SetTarget(currentPlayer.transform);
        }
    }

    private void InitializePlayerControllers()
    {
        PlayerPickupController pickupController = currentPlayer.GetComponent<PlayerPickupController>();

        if (pickupController != null)
        {
            pickupController.Initialize(currentMapData, turnManager);
        }

        PlayerInspectController inspectController = currentPlayer.GetComponent<PlayerInspectController>();

        if (inspectController != null)
        {
            inspectController.Initialize(currentMapData, turnManager);
        }

        PlayerInteractionController interactionController = currentPlayer.GetComponent<PlayerInteractionController>();

        if (interactionController != null)
        {
            interactionController.Initialize(currentMapData, turnManager);
        }

        PlayerWaitController waitController = currentPlayer.GetComponent<PlayerWaitController>();

        if (waitController != null)
        {
            waitController.Initialize(turnManager);
        }
    }

    private void SpawnStairsDown(Vector2Int stairsPosition)
    {
        if (stairsDownPrefab == null)
        {
            Debug.LogWarning("StairsDown prefab is not assigned on GameBootstrap.");
            return;
        }

        currentStairsDown = Instantiate(stairsDownPrefab, featureParent);

        bool placed = currentStairsDown.Initialize(
            currentMapData,
            mapRenderer,
            stairsPosition,
            this
        );

        if (!placed)
        {
            Destroy(currentStairsDown.gameObject);
            currentStairsDown = null;
        }
    }

    private void SpawnDoors(IReadOnlyList<Vector2Int> doorPositions)
    {
        if (doorPrefab == null)
        {
            return;
        }

        for (int i = 0; i < doorPositions.Count; i++)
        {
            DoorFeature door = Instantiate(doorPrefab, featureParent);

            bool placed = door.Initialize(
                currentMapData,
                mapRenderer,
                doorPositions[i]
            );

            if (!placed)
            {
                Destroy(door.gameObject);
                continue;
            }

            spawnedDoors.Add(door);
        }
    }

    private void SpawnEnemies(IReadOnlyList<Vector2Int> spawnPositions)
    {
        if (testEnemyPrefab == null)
        {
            return;
        }

        for (int i = 0; i < spawnPositions.Count; i++)
        {
            ActorGridEntity enemyEntity = Instantiate(testEnemyPrefab, actorParent);
            bool placed = enemyEntity.Initialize(currentMapData, mapRenderer, spawnPositions[i]);

            if (!placed)
            {
                Destroy(enemyEntity.gameObject);
                continue;
            }

            SimpleEnemyAI enemyAI = enemyEntity.GetComponent<SimpleEnemyAI>();

            if (enemyAI == null)
            {
                Debug.LogWarning(enemyEntity.DisplayName + " has no SimpleEnemyAI component.");
                continue;
            }

            enemyAI.Initialize(currentMapData, currentPlayerActor);
            spawnedEnemyAIs.Add(enemyAI);
        }
    }

    private void SpawnItems(IReadOnlyList<Vector2Int> spawnPositions)
    {
        if (itemPrefab == null)
        {
            Debug.LogWarning("Item prefab is not assigned on GameBootstrap.");
            return;
        }

        if (floorLootTable == null)
        {
            Debug.LogWarning("Floor loot table is not assigned on GameBootstrap.");
            return;
        }

        System.Random random = new System.Random(System.Environment.TickCount + currentFloorNumber);

        for (int i = 0; i < spawnPositions.Count; i++)
        {
            ItemDropResult dropResult = floorLootTable.Roll(random);

            if (dropResult == null)
            {
                continue;
            }

            if (dropResult.ItemDefinition == null)
            {
                continue;
            }

            ItemGridEntity item = Instantiate(itemPrefab, itemParent);

            bool placed = item.Initialize(
                currentMapData,
                mapRenderer,
                spawnPositions[i],
                dropResult.ItemDefinition,
                dropResult.Quantity
            );

            if (!placed)
            {
                Destroy(item.gameObject);
                continue;
            }

            spawnedItems.Add(item);
        }
    }

    private void InitializeTurnManager()
    {
        if (turnManager == null)
        {
            Debug.LogWarning("TurnManager is not assigned on GameBootstrap.");
            return;
        }

        turnManager.Initialize(currentMapData, currentPlayerActor, spawnedEnemyAIs);
    }

    private void RefreshCurrentFieldOfView()
    {
        if (currentPlayerFieldOfView == null)
        {
            return;
        }

        currentPlayerFieldOfView.RefreshVisibility();
    }
}