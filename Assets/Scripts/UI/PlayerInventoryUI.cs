using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/*
 * PlayerInventoryUI
 * -----------------
 * Displays the player's current inventory in a simple text-based UI panel.
 *
 * This is not the final inventory system. It is a temporary but useful UI so you
 * can see what the player is carrying without relying on Console debug output.
 *
 * Current behavior:
 * - listens for an inventory toggle input action
 * - opens/closes the inventory panel
 * - lists all carried items
 * - shows item quantities
 * - shows item categories
 * - refreshes automatically when the inventory changes
 *
 * Current display example:
 * Inventory
 * 1. Bread x3 [Food]
 * 2. Rusty Sword [Weapon]
 * 3. Copper Coin x12 [Currency]
 *
 * Important:
 * This script only displays inventory data.
 * It does not equip, use, drop, sort, or select items yet.
 *
 * Later this can expand into:
 * - selectable inventory rows
 * - item details panel
 * - use/equip/drop buttons
 * - equipment comparison
 * - inventory grid
 * - drag-and-drop
 * - item filtering by category
 */

public class PlayerInventoryUI : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference toggleInventoryAction;

    [Header("UI")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private TMP_Text inventoryText;

    [Header("Display")]
    [SerializeField] private string emptyInventoryText = "Inventory\n\nEmpty";
    [SerializeField] private bool startHidden = true;

    private ActorInventory targetInventory;
    private bool isOpen;

    private void Awake()
    {
        if (startHidden)
        {
            SetOpen(false);
        }
        else
        {
            SetOpen(true);
        }
    }

    private void OnEnable()
    {
        if (toggleInventoryAction != null)
        {
            toggleInventoryAction.action.performed += OnToggleInventoryPerformed;
            toggleInventoryAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (toggleInventoryAction != null)
        {
            toggleInventoryAction.action.performed -= OnToggleInventoryPerformed;
            toggleInventoryAction.action.Disable();
        }

        UnsubscribeFromInventory();
    }

    public void SetTarget(ActorInventory newInventory)
    {
        UnsubscribeFromInventory();

        targetInventory = newInventory;

        if (targetInventory != null)
        {
            targetInventory.InventoryChanged += Refresh;
        }

        Refresh();
    }

    private void OnToggleInventoryPerformed(InputAction.CallbackContext context)
    {
        SetOpen(!isOpen);
    }

    private void SetOpen(bool open)
    {
        isOpen = open;

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(isOpen);
        }

        if (isOpen)
        {
            Refresh();
        }
    }

    private void Refresh()
    {
        if (inventoryText == null)
        {
            return;
        }

        if (targetInventory == null)
        {
            inventoryText.text = emptyInventoryText;
            return;
        }

        if (targetInventory.Items.Count == 0)
        {
            inventoryText.text = emptyInventoryText;
            return;
        }

        inventoryText.text = BuildInventoryText();
    }

    private string BuildInventoryText()
    {
        StringBuilder builder = new StringBuilder();

        builder.AppendLine("Inventory");
        builder.AppendLine();

        for (int i = 0; i < targetInventory.Items.Count; i++)
        {
            ItemInstance item = targetInventory.Items[i];

            if (item == null)
            {
                continue;
            }

            builder.Append(i + 1);
            builder.Append(". ");
            builder.Append(item.GetDisplayName());
            builder.Append(" [");
            builder.Append(item.Category);
            builder.AppendLine("]");
        }

        return builder.ToString();
    }

    private void UnsubscribeFromInventory()
    {
        if (targetInventory == null)
        {
            return;
        }

        targetInventory.InventoryChanged -= Refresh;
        targetInventory = null;
    }
}