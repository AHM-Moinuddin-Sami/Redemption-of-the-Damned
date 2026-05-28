/*
 * GameUIState
 * -----------
 * Stores simple global UI state flags.
 *
 * Current responsibility:
 * - tells gameplay input scripts whether the inventory UI is currently open
 *
 * This is used so the player does not move, pick up items, or inspect the floor
 * while the inventory is open.
 *
 * Later this can expand into:
 * - dialogue open
 * - character screen open
 * - map screen open
 * - pause menu open
 * - targeting mode active
 */

public static class GameUIState
{
    public static bool IsInventoryOpen { get; set; }
}