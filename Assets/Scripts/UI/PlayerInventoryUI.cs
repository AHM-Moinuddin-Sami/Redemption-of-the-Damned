using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/*
 * PlayerInventoryUI
 * -----------------
 * Displays the player's inventory and allows simple selected item actions.
 *
 * This script should be placed on an always-active object, such as:
 *
 * InventoryCanvas
 * ├── InventoryUIController   <- this script
 * └── InventoryPanel          <- enabled/disabled by this script
 *     └── InventoryText
 *
 * Current behavior:
 * - I toggles inventory
 * - Tab selects the next item
 * - T inspects the selected item
 * - F equips the selected item if it is equipment
 * - R uses the selected item if it is consumable
 * - D drops the selected item onto the ground
 *
 * Main responsibilities:
 * - display inventory items
 * - track selected item index
 * - prevent gameplay input while inventory is open
 * - inspect/equip/use/drop the selected item
 * - consume a turn only when equip/use/drop succeeds
 *
 * Important:
 * This is still not the final inventory UI.
 * It is a temporary text-based control layer so item interaction is selected
 * instead of always using the first valid item.
 *
 * Later this can expand into:
 * - previous item selection
 * - mouse selection
 * - item details panel
 * - split stack dropping
 * - equipment comparison
 * - drag-and-drop inventory
 */

public class PlayerInventoryUI : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference toggleInventoryAction;
    [SerializeField] private InputActionReference selectNextItemAction;
    [SerializeField] private InputActionReference inspectSelectedItemAction;
    [SerializeField] private InputActionReference equipSelectedItemAction;
    [SerializeField] private InputActionReference useSelectedItemAction;
    [SerializeField] private InputActionReference dropSelectedItemAction;

    [Header("UI")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private TMP_Text inventoryText;

    [Header("Display")]
    [SerializeField] private string emptyInventoryText = "Inventory\n\nEmpty";
    [SerializeField] private bool startHidden = true;

    private ActorInventory targetInventory;
    private ActorEquipment targetEquipment;
    private ActorItemUser targetItemUser;
    private ActorItemDropper targetItemDropper;
    private TurnManager turnManager;

    private int selectedIndex;
    private bool isOpen;

    private void Awake()
    {
        SetOpen(!startHidden);
    }

    private void OnEnable()
    {
        EnableInput();
    }

    private void OnDisable()
    {
        DisableInput();
        UnsubscribeFromInventory();

        // Safety reset so gameplay input does not stay blocked if this object is disabled.
        if (GameUIState.IsInventoryOpen)
        {
            GameUIState.IsInventoryOpen = false;
        }
    }

    public void SetTarget(
        ActorInventory newInventory,
        ActorEquipment newEquipment,
        ActorItemUser newItemUser,
        ActorItemDropper newItemDropper,
        TurnManager newTurnManager)
    {
        UnsubscribeFromInventory();

        targetInventory = newInventory;
        targetEquipment = newEquipment;
        targetItemUser = newItemUser;
        targetItemDropper = newItemDropper;
        turnManager = newTurnManager;
        selectedIndex = 0;

        if (targetInventory != null)
        {
            targetInventory.InventoryChanged += OnInventoryChanged;
        }

        Refresh();
    }

    private void EnableInput()
    {
        SubscribeAction(toggleInventoryAction, OnToggleInventoryPerformed);
        SubscribeAction(selectNextItemAction, OnSelectNextItemPerformed);
        SubscribeAction(inspectSelectedItemAction, OnInspectSelectedItemPerformed);
        SubscribeAction(equipSelectedItemAction, OnEquipSelectedItemPerformed);
        SubscribeAction(useSelectedItemAction, OnUseSelectedItemPerformed);
        SubscribeAction(dropSelectedItemAction, OnDropSelectedItemPerformed);
    }

    private void DisableInput()
    {
        UnsubscribeAction(toggleInventoryAction, OnToggleInventoryPerformed);
        UnsubscribeAction(selectNextItemAction, OnSelectNextItemPerformed);
        UnsubscribeAction(inspectSelectedItemAction, OnInspectSelectedItemPerformed);
        UnsubscribeAction(equipSelectedItemAction, OnEquipSelectedItemPerformed);
        UnsubscribeAction(useSelectedItemAction, OnUseSelectedItemPerformed);
        UnsubscribeAction(dropSelectedItemAction, OnDropSelectedItemPerformed);
    }

    private void SubscribeAction(InputActionReference actionReference, System.Action<InputAction.CallbackContext> callback)
    {
        if (actionReference == null)
        {
            return;
        }

        actionReference.action.performed += callback;
        actionReference.action.Enable();
    }

    private void UnsubscribeAction(InputActionReference actionReference, System.Action<InputAction.CallbackContext> callback)
    {
        if (actionReference == null)
        {
            return;
        }

        actionReference.action.performed -= callback;
        actionReference.action.Disable();
    }

    private void OnToggleInventoryPerformed(InputAction.CallbackContext context)
    {
        if (GameUIState.IsGameOver)
        {
            return;
        }

        SetOpen(!isOpen);
    }
    private void OnSelectNextItemPerformed(InputAction.CallbackContext context)
    {
        if (!isOpen)
        {
            return;
        }

        SelectNextItem();
    }

    private void OnInspectSelectedItemPerformed(InputAction.CallbackContext context)
    {
        if (!isOpen)
        {
            return;
        }

        InspectSelectedItem();
    }

    private void OnEquipSelectedItemPerformed(InputAction.CallbackContext context)
    {
        if (!isOpen)
        {
            return;
        }

        TryEquipSelectedItem();
    }

    private void OnUseSelectedItemPerformed(InputAction.CallbackContext context)
    {
        if (!isOpen)
        {
            return;
        }

        TryUseSelectedItem();
    }

    private void OnDropSelectedItemPerformed(InputAction.CallbackContext context)
    {
        if (!isOpen)
        {
            return;
        }

        TryDropSelectedItem();
    }

    private void SetOpen(bool open)
    {
        isOpen = open;
        GameUIState.IsInventoryOpen = isOpen;

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(isOpen);
        }

        if (isOpen)
        {
            Refresh();
        }
    }

    private void OnInventoryChanged()
    {
        ClampSelectedIndex();
        Refresh();
    }

    private void SelectNextItem()
    {
        if (targetInventory == null || targetInventory.Items.Count == 0)
        {
            selectedIndex = 0;
            Refresh();
            return;
        }

        selectedIndex++;

        if (selectedIndex >= targetInventory.Items.Count)
        {
            selectedIndex = 0;
        }

        Refresh();
    }

    private void InspectSelectedItem()
    {
        ItemInstance selectedItem = GetSelectedItem();

        if (selectedItem == null)
        {
            GameMessageLog.Write("There is no item selected.");
            return;
        }

        GameMessageLog.Write(ItemDescriptionBuilder.Build(selectedItem));
    }

    private void TryEquipSelectedItem()
    {
        ItemInstance selectedItem = GetSelectedItem();

        if (selectedItem == null)
        {
            GameMessageLog.Write("There is no item selected.");
            return;
        }

        if (!selectedItem.IsEquipment)
        {
            GameMessageLog.Write(selectedItem.GetDisplayName() + " cannot be equipped.");
            return;
        }

        if (targetEquipment == null)
        {
            GameMessageLog.Write("There is no equipment system available.");
            return;
        }

        bool equipped = targetEquipment.TryEquip(selectedItem, targetInventory);

        if (equipped && turnManager != null)
        {
            turnManager.PlayerTookAction();
        }
    }

    private void TryUseSelectedItem()
    {
        ItemInstance selectedItem = GetSelectedItem();

        if (selectedItem == null)
        {
            GameMessageLog.Write("There is no item selected.");
            return;
        }

        if (selectedItem.Definition == null || !selectedItem.Definition.CanBeUsedDirectly)
        {
            GameMessageLog.Write(selectedItem.GetDisplayName() + " cannot be used.");
            return;
        }
        
        if (selectedItem.IsEquipment)
        {
            GameMessageLog.Write("Equip " + selectedItem.GetDisplayName() + " to use its active effect.");
            return;
        }

        if (targetItemUser == null)
        {
            GameMessageLog.Write("There is no item use system available.");
            return;
        }

        bool used = targetItemUser.TryUseItem(selectedItem);

        if (used && turnManager != null)
        {
            turnManager.PlayerTookAction();
        }
    }

    private void TryDropSelectedItem()
    {
        ItemInstance selectedItem = GetSelectedItem();

        if (selectedItem == null)
        {
            GameMessageLog.Write("There is no item selected.");
            return;
        }

        if (targetItemDropper == null)
        {
            GameMessageLog.Write("There is no item drop system available.");
            return;
        }

        bool dropped = targetItemDropper.TryDropItem(selectedItem);

        if (dropped && turnManager != null)
        {
            turnManager.PlayerTookAction();
        }
    }

    private ItemInstance GetSelectedItem()
    {
        if (targetInventory == null)
        {
            return null;
        }

        if (targetInventory.Items.Count == 0)
        {
            return null;
        }

        ClampSelectedIndex();

        return targetInventory.Items[selectedIndex];
    }

    private void ClampSelectedIndex()
    {
        if (targetInventory == null || targetInventory.Items.Count == 0)
        {
            selectedIndex = 0;
            return;
        }

        if (selectedIndex < 0)
        {
            selectedIndex = 0;
        }

        if (selectedIndex >= targetInventory.Items.Count)
        {
            selectedIndex = targetInventory.Items.Count - 1;
        }
    }

    private void Refresh()
    {
        if (inventoryText == null)
        {
            return;
        }

        if (targetInventory == null || targetInventory.Items.Count == 0)
        {
            inventoryText.text = emptyInventoryText;
            return;
        }

        ClampSelectedIndex();
        inventoryText.text = BuildInventoryText();
    }

    private string BuildInventoryText()
    {
        StringBuilder builder = new StringBuilder();

        builder.AppendLine("Inventory");
        builder.AppendLine("Tab: Select | T: Inspect | F: Equip | R: Use | D: Drop");
        builder.AppendLine();

        for (int i = 0; i < targetInventory.Items.Count; i++)
        {
            ItemInstance item = targetInventory.Items[i];

            if (item == null)
            {
                continue;
            }

            if (i == selectedIndex)
            {
                builder.Append("> ");
            }
            else
            {
                builder.Append("  ");
            }

            builder.Append(i + 1);
            builder.Append(". ");
            builder.Append(ItemTextFormatter.FormatItemName(item));
            builder.Append(" [");
            builder.Append(item.Category);
            builder.AppendLine("]");

            string useStateText = item.GetUseStateText();

            if (!string.IsNullOrWhiteSpace(useStateText))
            {
                builder.Append(" (");
                builder.Append(useStateText);
                builder.Append(")");
            }

            builder.AppendLine();
        }

        return builder.ToString();
    }

    private void UnsubscribeFromInventory()
    {
        if (targetInventory == null)
        {
            return;
        }

        targetInventory.InventoryChanged -= OnInventoryChanged;
        targetInventory = null;
    }

    public void ForceClose()
    {
        SetOpen(false);
    }
}