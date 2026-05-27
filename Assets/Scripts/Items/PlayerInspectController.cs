using UnityEngine;
using UnityEngine.InputSystem;

/*
 * PlayerInspectController
 * -----------------------
 * Handles the player's temporary inspect/look command.
 *
 * Current behavior:
 * - listens for an inspect input action
 * - checks the player's current grid cell
 * - if an item is on the tile, prints item details to the message log
 * - if a feature is on the tile, prints feature details
 * - if nothing is there, prints a simple empty message
 *
 * Main responsibilities:
 * - receive inspect input
 * - avoid input while enemy turns are processing
 * - inspect the top item on the player's current tile
 * - inspect the feature on the player's current tile
 *
 * Important:
 * Inspecting does not consume a turn.
 * Looking at an item should be free because it is an information action.
 *
 * Later this can expand into:
 * - inspecting adjacent tiles
 * - mouse hover inspection
 * - dedicated tooltip panel
 * - enemy inspection
 * - terrain inspection
 * - feature descriptions
 */

[RequireComponent(typeof(ActorGridEntity))]
public class PlayerInspectController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference inspectAction;

    private MapData mapData;
    private TurnManager turnManager;
    private ActorGridEntity actorGridEntity;
    private bool isInitialized;

    private void Awake()
    {
        actorGridEntity = GetComponent<ActorGridEntity>();
    }

    private void OnEnable()
    {
        if (inspectAction != null)
        {
            inspectAction.action.performed += OnInspectPerformed;
            inspectAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (inspectAction != null)
        {
            inspectAction.action.performed -= OnInspectPerformed;
            inspectAction.action.Disable();
        }
    }

    public void Initialize(MapData newMapData, TurnManager newTurnManager)
    {
        mapData = newMapData;
        turnManager = newTurnManager;
        isInitialized = true;
    }

    private void OnInspectPerformed(InputAction.CallbackContext context)
    {
        if (!isInitialized)
        {
            return;
        }

        if (turnManager != null && !turnManager.CanPlayerAct)
        {
            return;
        }

        InspectCurrentTile();
    }

    private void InspectCurrentTile()
    {
        ItemGridEntity itemOnGround = mapData.GetTopItemAt(actorGridEntity.GridPosition);

        if (itemOnGround != null)
        {
            GameMessageLog.Write(itemOnGround.GetInspectText());
            return;
        }

        MapFeatureEntity feature = mapData.GetFeatureAt(actorGridEntity.GridPosition);

        if (feature != null)
        {
            GameMessageLog.Write("You see " + feature.DisplayName + ".");
            return;
        }

        GameMessageLog.Write("There is nothing notable here.");
    }
}