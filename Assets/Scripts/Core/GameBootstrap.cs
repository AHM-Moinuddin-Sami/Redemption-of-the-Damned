using System.Collections.Generic;
using UnityEngine;

/*
 * GameBootstrap
 * -------------
 * Starts and resets the current prototype dungeon floor.
 *
 * Current startup process:
 * 1. Generate a dungeon.
 * 2. Store the generated MapData.
 * 3. Render the map to Unity Tilemaps.
 * 4. Spawn the player.
 * 5. Initialize player controllers.
 * 6. Spawn stairs.
 * 7. Spawn enemies.
 * 8. Spawn random item drops.
 * 9. Initialize enemy AI.
 * 10. Initialize the TurnManager.
 * 11. Assign the camera follow target.
 *
 * This script still wires many prototype systems together directly.
 * Later, this should be split into cleaner systems.
 */

public class GameBootstrap : MonoBehaviour
{
    [Header("Generation")]
    [SerializeField] private DungeonGenerator dungeonGenerator;
    [SerializeField] private MapRenderer mapRenderer;

    [Header("Turns")]
    [SerializeField] private TurnManager turnManager;

    [Header("Player")]
    [SerializeField] private PlayerGridMover playerPrefab;

    [Header("Enemies")]
    [SerializeField] private ActorGridEntity testEnemyPrefab;

    [Header("Features")]
    [SerializeField] private StairsDownFeature stairsDownPrefab;

    [Header("Items")]
    [SerializeField] private ItemGridEntity itemPrefab;
    [SerializeField] private ItemDropTable floorLootTable;
    [Header("UI")]
    [SerializeField] private PlayerHUD playerHUD;
    [SerializeField] private PlayerInventoryUI playerInventoryUI;
    [SerializeField] private PlayerEquipmentUI playerEquipmentUI;

    [Header("Scene References")]
    [SerializeField] private CameraFollowTarget cameraFollowTarget;
    [SerializeField] private Transform actorParent;
    [SerializeField] private Transform featureParent;
    [SerializeField] private Transform itemParent;

    private readonly List<SimpleEnemyAI> spawnedEnemyAIs = new List<SimpleEnemyAI>();
    private readonly List<ItemGridEntity> spawnedItems = new List<ItemGridEntity>();

    private MapData currentMapData;
    private PlayerGridMover currentPlayer;
    private ActorGridEntity currentPlayerActor;
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
        SpawnEnemies(generationResult.EnemySpawnPositions);
        SpawnItems(generationResult.ItemSpawnPositions);
        InitializeTurnManager();
    }

    private void ClearExistingRuntimeObjects()
    {
        if (currentPlayer != null)
        {
            Destroy(currentPlayer.gameObject);
            currentPlayer = null;
            currentPlayerActor = null;
        }

        if (currentStairsDown != null)
        {
            Destroy(currentStairsDown.gameObject);
            currentStairsDown = null;
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

        spawnedEnemyAIs.Clear();
        spawnedItems.Clear();
    }

    private void SpawnPlayer(Vector2Int spawnPosition)
    {
        currentPlayer = Instantiate(playerPrefab, actorParent);
        currentPlayer.Initialize(currentMapData, mapRenderer, spawnPosition, turnManager);

        currentPlayerActor = currentPlayer.GetComponent<ActorGridEntity>();

        if (playerHUD != null)
        {
            playerHUD.SetTarget(currentPlayerActor, currentFloorNumber);
        }

        ActorInventory playerInventory = currentPlayer.GetComponent<ActorInventory>();

        if (playerInventoryUI != null)
        {
            playerInventoryUI.SetTarget(playerInventory);
        }

        ActorEquipment playerEquipment = currentPlayer.GetComponent<ActorEquipment>();

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

        PlayerEquipmentController equipmentController = currentPlayer.GetComponent<PlayerEquipmentController>();

        if (equipmentController != null)
        {
            equipmentController.Initialize(turnManager);
        }

        PlayerItemUseController itemUseController = currentPlayer.GetComponent<PlayerItemUseController>();

        if (itemUseController != null)
        {
            itemUseController.Initialize(turnManager);
        }

        PlayerInspectController inspectController = currentPlayer.GetComponent<PlayerInspectController>();

        if (inspectController != null)
        {
            inspectController.Initialize(currentMapData, turnManager);
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
}