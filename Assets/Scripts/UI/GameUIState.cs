/*
 * GameUIState
 * -----------
 * Stores simple global UI/gameplay blocking flags.
 *
 * Current responsibilities:
 * - track whether the inventory UI is open
 * - track whether the run is over
 * - expose one helper property for gameplay input blocking
 *
 * Why this exists:
 * Different player input scripts need to know when normal gameplay input should
 * be ignored. For example, movement should not happen while the inventory is
 * open, and no gameplay input should happen after the player dies.
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
    public static bool IsGameOver { get; set; }

    public static bool IsGameplayInputBlocked
    {
        get
        {
            return IsInventoryOpen || IsGameOver;
        }
    }

    public static void Reset()
    {
        IsInventoryOpen = false;
        IsGameOver = false;
    }
}