using System.Collections.Generic;
using UnityEngine;

/*
 * GameBootstrap
 * -------------
 * Starts and transitions the current prototype dungeon floor.
 *
 * Current startup process:
 * 1. Generate a dungeon floor.
 * 2. Store generated MapData.
 * 3. Render the map to Tilemaps.
 * 4. Spawn the player if this is the first floor.
 * 5. Reuse the same player if changing floors.
 * 6. Move the player to the new floor's spawn position.
 * 7. Initialize FOV and world object visibility.
 * 8. Spawn stairs, doors, enemies, and items.
 * 9. Initialize enemy AI.
 * 10. Initialize TurnManager.
 * 11. Refresh FOV once all runtime objects exist.
 * 12. Assign camera/UI targets.
 *
 * Important:
 * The player is now persistent between floors.
 * This means the player's inventory, equipment, HP, hunger, thirst, stealth mode,
 * and future character data survive floor transitions.
 *
 * Runtime objects that are replaced every floor:
 * - enemies
 * - items lying on the floor
 * - stairs
 * - doors
 *
 * Runtime object that persists across floors:
 * - player
 *
 * Later, this should be split into:
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
    [SerializeField] private ActorGridEntity enemyPrefab;
    [SerializeField] private EnemySpawnTable enemySpawnTable;
    [SerializeField] private EnemySpawnProfile enemySpawnProfile;

    [Header("Features")]
    [SerializeField] private StairsDownFeature stairsDownPrefab;
    [SerializeField] private DoorFeature doorPrefab;

    [Header("Items")]
    [SerializeField] private ItemGridEntity itemPrefab;
    [SerializeField] private ItemDropTable floorLootTable;
    [SerializeField] private ItemLootProfile itemLootProfile;
    [SerializeField] private ItemGenerationProfile itemGenerationProfile;

    [Header("Scene References")]
    [SerializeField] private CameraFollowTarget cameraFollowTarget;
    [SerializeField] private Transform actorParent;
    [SerializeField] private Transform featureParent;
    [SerializeField] private Transform itemParent;

    [Header("UI")]
    [SerializeField] private PlayerHUD playerHUD;
    [SerializeField] private PlayerInventoryUI playerInventoryUI;
    [SerializeField] private PlayerEquipmentUI playerEquipmentUI;
    [SerializeField] private GameOverUI gameOverUI;

    private readonly List<SimpleEnemyAI> spawnedEnemyAIs = new List<SimpleEnemyAI>();
    private readonly List<ItemGridEntity> spawnedItems = new List<ItemGridEntity>();
    private readonly List<DoorFeature> spawnedDoors = new List<DoorFeature>();
    private readonly HashSet<string> generatedUniqueItemIds = new HashSet<string>();

    private MapData currentMapData;
    private PlayerGridMover currentPlayer;
    private ActorGridEntity currentPlayerActor;
    private ActorHealth currentPlayerHealth;
    private PlayerFieldOfView currentPlayerFieldOfView;
    private StairsDownFeature currentStairsDown;
    private int currentFloorNumber = 1;
    private ActorExperience currentPlayerExperience;

    private void Start()
    {
        GameUIState.Reset();

        if (gameOverUI != null)
        {
            gameOverUI.Hide();
        }

        GenerateCurrentFloor();
    }

    public void GoToNextFloor()
    {
        currentFloorNumber++;
        GenerateCurrentFloor();
    }

    private void GenerateCurrentFloor()
    {
        GameUIState.IsInventoryOpen = false;

        GameMessageLog.Write("Dungeon floor " + currentFloorNumber + ".");

        ClearCurrentFloorRuntimeObjects();

        GenerationResult generationResult = dungeonGenerator.Generate();

        currentMapData = generationResult.MapData;

        mapRenderer.Render(currentMapData);

        SpawnOrMovePlayer(generationResult.PlayerSpawnPosition);
        SpawnStairsDown(generationResult.StairsDownPosition);
        SpawnDoors(generationResult.DoorSpawnPositions);
        SpawnEnemies(generationResult.EnemySpawnPositions);
        SpawnItems(generationResult.ItemSpawnPositions);
        InitializeTurnManager();
        RefreshCurrentFieldOfView();
    }

    private void ClearCurrentFloorRuntimeObjects()
    {
        PlayerAwarenessContext.Clear();

        if (worldObjectVisibilityController != null)
        {
            worldObjectVisibilityController.SetTarget(null);
        }

        if (fieldOfViewRenderer != null)
        {
            fieldOfViewRenderer.Clear();
        }

        // The player persists, but they must be removed from the previous MapData
        // before the new floor is generated and registered.
        if (currentPlayerActor != null)
        {
            currentPlayerActor.ClearFromMap();
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

    private void SpawnOrMovePlayer(Vector2Int spawnPosition)
    {
        if (currentPlayer == null)
        {
            SpawnPlayerForNewRun(spawnPosition);
            return;
        }

        MoveExistingPlayerToNewFloor(spawnPosition);
    }

    private void SpawnPlayerForNewRun(Vector2Int spawnPosition)
    {
        currentPlayer = Instantiate(playerPrefab, actorParent);
        currentPlayer.Initialize(currentMapData, mapRenderer, spawnPosition, turnManager);

        currentPlayerActor = currentPlayer.GetComponent<ActorGridEntity>();
        currentPlayerHealth = currentPlayer.GetComponent<ActorHealth>();

        PlayerCharacterProfile playerProfile = currentPlayer.GetComponent<PlayerCharacterProfile>();

        if (playerProfile != null)
        {
            playerProfile.InitializeForNewRun();
        }

        if (currentPlayerHealth != null)
        {
            currentPlayerHealth.Died += OnPlayerDied;
        }

        SetupPlayerAfterPlacement();
    }

    private void MoveExistingPlayerToNewFloor(Vector2Int spawnPosition)
    {
        currentPlayer.Initialize(currentMapData, mapRenderer, spawnPosition, turnManager);

        currentPlayerActor = currentPlayer.GetComponent<ActorGridEntity>();

        SetupPlayerAfterPlacement();
    }

    private void SetupPlayerAfterPlacement()
    {
        currentPlayerFieldOfView = currentPlayer.GetComponent<PlayerFieldOfView>();

        if (currentPlayerFieldOfView != null)
        {
            currentPlayerFieldOfView.Initialize(currentMapData, fieldOfViewRenderer);
        }

        PlayerAwarenessContext.SetPlayer(currentPlayerActor, currentPlayerFieldOfView);

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

        currentPlayerExperience = currentPlayer.GetComponent<ActorExperience>();

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

        PlayerStealthController stealthController = currentPlayer.GetComponent<PlayerStealthController>();

        if (stealthController != null)
        {
            stealthController.Initialize(turnManager);
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
        if (enemyPrefab == null)
        {
            return;
        }

        System.Random random = new System.Random(System.Environment.TickCount + currentFloorNumber * 37);

        for (int i = 0; i < spawnPositions.Count; i++)
        {
            EnemyDefinition enemyDefinition = RollEnemyDefinitionForCurrentFloor(random);

            ActorGridEntity enemyEntity = Instantiate(enemyPrefab, actorParent);

            EnemyDefinitionApplier definitionApplier = enemyEntity.GetComponent<EnemyDefinitionApplier>();

            if (definitionApplier != null && enemyDefinition != null)
            {
                definitionApplier.ApplyDefinition(enemyDefinition);
            }

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

            ActorHealth enemyHealth = enemyEntity.GetComponent<ActorHealth>();

            if (enemyHealth != null)
            {
                enemyHealth.Died += OnEnemyDied;
            }
        }
    }

    private EnemyDefinition RollEnemyDefinitionForCurrentFloor(System.Random random)
    {
        if (enemySpawnProfile != null)
        {
            EnemyDefinition profileEnemy = enemySpawnProfile.RollEnemyDefinition(currentFloorNumber, random);

            if (profileEnemy != null)
            {
                return profileEnemy;
            }
        }

        if (enemySpawnTable != null)
        {
            return enemySpawnTable.Roll(random);
        }

        return null;
    }

    private void SpawnItems(IReadOnlyList<Vector2Int> spawnPositions)
    {
        if (itemPrefab == null)
        {
            Debug.LogWarning("Item prefab is not assigned on GameBootstrap.");
            return;
        }

        if (floorLootTable == null && itemLootProfile == null)
        {
            Debug.LogWarning("No item loot table or item loot profile is assigned on GameBootstrap.");
            return;
        }

        System.Random random = new System.Random(System.Environment.TickCount + currentFloorNumber * 97);

        for (int i = 0; i < spawnPositions.Count; i++)
        {
            ItemDropResult dropResult = RollItemDropForCurrentFloor(random);

            if (dropResult == null)
            {
                continue;
            }

            if (dropResult.ItemDefinition == null)
            {
                continue;
            }

            ItemInstance itemInstance = CreateGeneratedItemInstance(dropResult, random);

            if (itemInstance == null)
            {
                continue;
            }

            ItemGridEntity item = Instantiate(itemPrefab, itemParent);

            bool placed = item.Initialize(
                currentMapData,
                mapRenderer,
                spawnPositions[i],
                itemInstance
            );

            if (!placed)
            {
                Destroy(item.gameObject);
                continue;
            }

            spawnedItems.Add(item);
            RegisterGeneratedUniqueItem(itemInstance);
        }
    }

    private ItemDropResult RollItemDropForCurrentFloor(System.Random random)
    {
        if (itemLootProfile != null)
        {
            ItemDropResult profileDrop = itemLootProfile.RollDropResult(currentFloorNumber, random);

            if (profileDrop != null)
            {
                return profileDrop;
            }
        }

        if (floorLootTable != null)
        {
            return floorLootTable.Roll(random);
        }

        return null;
    }

    private ItemInstance CreateGeneratedItemInstance(ItemDropResult dropResult, System.Random random)
    {
        if (dropResult == null || dropResult.ItemDefinition == null)
        {
            return null;
        }

        if (HasUniqueItemAlreadyGenerated(dropResult.ItemDefinition))
        {
            return null;
        }

        if (itemGenerationProfile != null)
        {
            return itemGenerationProfile.GenerateItemInstance(
                dropResult,
                currentFloorNumber,
                random
            );
        }

        return new ItemInstance(
            dropResult.ItemDefinition,
            dropResult.Quantity
        );
    }

    private bool HasUniqueItemAlreadyGenerated(ItemDefinition itemDefinition)
    {
        if (itemDefinition == null)
        {
            return false;
        }

        if (!itemDefinition.IsUnique)
        {
            return false;
        }

        return generatedUniqueItemIds.Contains(itemDefinition.UniqueId);
    }

    private void RegisterGeneratedUniqueItem(ItemInstance itemInstance)
    {
        if (itemInstance == null || itemInstance.Definition == null)
        {
            return;
        }

        if (!itemInstance.IsUnique)
        {
            return;
        }

        generatedUniqueItemIds.Add(itemInstance.Definition.UniqueId);
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

    private void OnPlayerDied(ActorHealth playerHealth)
    {
        GameUIState.IsGameOver = true;
        GameUIState.IsInventoryOpen = false;

        if (playerInventoryUI != null)
        {
            playerInventoryUI.ForceClose();
        }

        GameMessageLog.Write("Your run is over.");

        if (gameOverUI != null)
        {
            gameOverUI.Show(currentFloorNumber);
        }
    }

    private void OnEnemyDied(ActorHealth enemyHealth)
    {
        if (enemyHealth == null)
        {
            return;
        }

        if (currentPlayerExperience == null)
        {
            return;
        }

        ExperienceReward experienceReward = enemyHealth.GetComponent<ExperienceReward>();

        if (experienceReward == null)
        {
            return;
        }

        currentPlayerExperience.AddExperience(experienceReward.ExperienceAmount);
    }
}