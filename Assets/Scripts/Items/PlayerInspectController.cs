using System.Collections.Generic;
using System.Text;
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
 * - if items are on the tile, lists the entire item pile
 * - if no item exists but a feature exists, describes the feature
 * - if nothing is there, prints a simple empty message
 *
 * Main responsibilities:
 * - receive inspect input
 * - avoid input while enemy turns are processing
 * - avoid ground inspection while inventory UI is open
 * - inspect all items on the player's current tile
 * - inspect the feature on the player's current tile
 *
 * Important:
 * Inspecting does not consume a turn.
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

        if (GameUIState.IsInventoryOpen)
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
        List<ItemGridEntity> itemsOnGround = mapData.GetItemsAt(actorGridEntity.GridPosition);

        if (itemsOnGround.Count > 0)
        {
            GameMessageLog.Write(BuildItemPileInspectText(itemsOnGround));
            return;
        }

        MapFeatureEntity feature = mapData.GetFeatureAt(actorGridEntity.GridPosition);

        if (feature != null)
        {
            GameMessageLog.Write(BuildFeatureInspectText(feature));
            return;
        }

        GameMessageLog.Write("There is nothing notable here.");
    }

    private string BuildItemPileInspectText(List<ItemGridEntity> itemsOnGround)
    {
        if (itemsOnGround.Count == 1)
        {
            return itemsOnGround[0].GetInspectText();
        }

        StringBuilder builder = new StringBuilder();

        builder.AppendLine("You see several items here:");

        for (int i = 0; i < itemsOnGround.Count; i++)
        {
            if (itemsOnGround[i] == null)
            {
                continue;
            }

            builder.Append("- ");
            builder.AppendLine(GetItemDisplayName(itemsOnGround[i]));
        }

        return builder.ToString().TrimEnd();
    }

    private string BuildFeatureInspectText(MapFeatureEntity feature)
    {
        StairsDownFeature stairsDownFeature = feature.GetComponent<StairsDownFeature>();

        if (stairsDownFeature != null)
        {
            return "You see " + feature.DisplayName + ". Press E to descend.";
        }

        return "You see " + feature.DisplayName + ".";
    }

    private string GetItemDisplayName(ItemGridEntity item)
    {
        if (item == null || item.ItemDefinition == null)
        {
            return "Unknown Item";
        }

        if (item.Quantity > 1)
        {
            return item.ItemDefinition.DisplayName + " x" + item.Quantity;
        }

        return item.ItemDefinition.DisplayName;
    }
}