using UnityEngine;
using UnityEngine.InputSystem;

/*
 * PlayerInteractionController
 * ---------------------------
 * Handles player interaction with map features.
 *
 * Current behavior:
 * - checks the current tile first
 * - checks adjacent tiles next
 * - uses stairs if standing on stairs
 * - toggles doors if next to a door
 * - ignores interaction while inventory UI is open
 * - ignores interaction while enemy turns are processing
 *
 * Current supported features:
 * - StairsDownFeature
 * - DoorFeature
 *
 * Important:
 * Stairs do not consume a turn because the floor changes immediately.
 * Doors do consume a turn because opening/closing a door is a normal action.
 */

[RequireComponent(typeof(ActorGridEntity))]
public class PlayerInteractionController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference interactAction;

    private MapData mapData;
    private TurnManager turnManager;
    private ActorGridEntity actorGridEntity;
    private bool isInitialized;
    private PlayerFieldOfView playerFieldOfView;

    private readonly Vector2Int[] adjacentDirections =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    private void Awake()
    {
        actorGridEntity = GetComponent<ActorGridEntity>();
        playerFieldOfView = GetComponent<PlayerFieldOfView>();
    }

    private void OnEnable()
    {
        if (interactAction != null)
        {
            interactAction.action.performed += OnInteractPerformed;
            interactAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (interactAction != null)
        {
            interactAction.action.performed -= OnInteractPerformed;
            interactAction.action.Disable();
        }
    }

    public void Initialize(MapData newMapData, TurnManager newTurnManager)
    {
        mapData = newMapData;
        turnManager = newTurnManager;
        isInitialized = true;
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (!isInitialized)
        {
            return;
        }

        if (GameUIState.IsInventoryOpen)
        {
            return;
        }

        if (turnManager != null && !turnManager.CanPlayerAct)
        {
            return;
        }

        TryInteract();
    }

    private void TryInteract()
    {
        MapFeatureEntity currentTileFeature = mapData.GetFeatureAt(actorGridEntity.GridPosition);

        if (TryUseFeature(currentTileFeature, false))
        {
            return;
        }

        for (int i = 0; i < adjacentDirections.Length; i++)
        {
            Vector2Int checkPosition = actorGridEntity.GridPosition + adjacentDirections[i];
            MapFeatureEntity adjacentFeature = mapData.GetFeatureAt(checkPosition);

            if (TryUseFeature(adjacentFeature, true))
            {
                return;
            }
        }

        GameMessageLog.Write("There is nothing here to interact with.");
    }

    private bool TryUseFeature(MapFeatureEntity feature, bool consumeTurnForNormalFeature)
    {
        if (feature == null)
        {
            return false;
        }

        StairsDownFeature stairsDownFeature = feature.GetComponent<StairsDownFeature>();

        if (stairsDownFeature != null)
        {
            stairsDownFeature.Use();
            return true;
        }

        DoorFeature doorFeature = feature.GetComponent<DoorFeature>();

        if (doorFeature != null)
        {
            doorFeature.Toggle();
            RefreshFieldOfView();

            if (consumeTurnForNormalFeature && turnManager != null)
            {
                turnManager.PlayerTookAction();
            }

            return true;
        }

        GameMessageLog.Write("You do not know how to use " + feature.DisplayName + ".");
        return true;
    }

    private void RefreshFieldOfView()
    {
        if (playerFieldOfView == null)
        {
            return;
        }

        playerFieldOfView.RefreshVisibility();
    }
}