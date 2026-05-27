using System.Text;
using TMPro;
using UnityEngine;

/*
 * PlayerEquipmentUI
 * -----------------
 * Displays the player's currently equipped items in a simple text-based UI panel.
 *
 * This is not the final equipment screen. It is a temporary but useful display
 * so equipped items are visible without relying on Console logs.
 *
 * Current behavior:
 * - receives the player's ActorEquipment reference from GameBootstrap
 * - displays every equipment slot
 * - refreshes automatically when equipment changes
 *
 * Current display example:
 *
 * Equipment
 *
 * Main Hand: Rusty Sword
 * Off Hand: Old Shield
 * Head: Empty
 * Body: Leather Armor
 *
 * Important:
 * This script only displays equipment data.
 * It does not equip, unequip, compare, drag, drop, or select items.
 *
 * Later this can expand into:
 * - equipment icons
 * - item tooltips
 * - stat comparison
 * - drag-and-drop slots
 * - right-click unequip
 * - visual paper doll layout
 */

public class PlayerEquipmentUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text equipmentText;

    [Header("Display")]
    [SerializeField] private string emptySlotText = "Empty";

    private ActorEquipment targetEquipment;

    private void OnDisable()
    {
        UnsubscribeFromEquipment();
    }

    public void SetTarget(ActorEquipment newEquipment)
    {
        UnsubscribeFromEquipment();

        targetEquipment = newEquipment;

        if (targetEquipment != null)
        {
            targetEquipment.EquipmentChanged += Refresh;
        }

        Refresh();
    }

    private void Refresh()
    {
        if (equipmentText == null)
        {
            return;
        }

        if (targetEquipment == null)
        {
            equipmentText.text = "Equipment\n\nNo target";
            return;
        }

        equipmentText.text = BuildEquipmentText();
    }

    private string BuildEquipmentText()
    {
        StringBuilder builder = new StringBuilder();

        builder.AppendLine("Equipment");
        builder.AppendLine();

        AppendSlot(builder, "Main Hand", EquipmentSlotType.MainHand);
        AppendSlot(builder, "Off Hand", EquipmentSlotType.OffHand);
        AppendSlot(builder, "Head", EquipmentSlotType.Head);
        AppendSlot(builder, "Body", EquipmentSlotType.Body);
        AppendSlot(builder, "Hands", EquipmentSlotType.Hands);
        AppendSlot(builder, "Feet", EquipmentSlotType.Feet);
        AppendSlot(builder, "Neck", EquipmentSlotType.Neck);
        AppendSlot(builder, "Ring", EquipmentSlotType.Ring);

        return builder.ToString();
    }

    private void AppendSlot(StringBuilder builder, string label, EquipmentSlotType slot)
    {
        ItemInstance item = targetEquipment.GetEquippedItem(slot);

        builder.Append(label);
        builder.Append(": ");

        if (item == null)
        {
            builder.AppendLine(emptySlotText);
            return;
        }

        builder.AppendLine(item.GetDisplayName());
    }

    private void UnsubscribeFromEquipment()
    {
        if (targetEquipment == null)
        {
            return;
        }

        targetEquipment.EquipmentChanged -= Refresh;
        targetEquipment = null;
    }
}