using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/*
 * GameOverUI
 * ----------
 * Displays the game over screen when the player dies.
 *
 * This script should be placed on an always-active UI controller object, not on
 * the panel that gets disabled.
 *
 * Correct hierarchy example:
 *
 * GameOverCanvas
 * ├── GameOverUIController   <- this script goes here
 * └── GameOverPanel          <- enabled/disabled by this script
 *     ├── GameOverTitleText
 *     └── GameOverDetailsText
 *
 * Current behavior:
 * - hides the game over panel at startup
 * - shows the panel when GameBootstrap tells it the player died
 * - displays the reached floor number
 * - blocks normal gameplay through GameUIState
 * - optionally restarts the scene when the restart input is pressed
 *
 * Important:
 * Restarting simply reloads the current Unity scene.
 * Later, this can be replaced with a proper NewRunManager that handles seed,
 * character creation, permadeath records, score summaries, and save cleanup.
 */

public class GameOverUI : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference restartAction;

    [Header("UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text detailsText;

    [Header("Text")]
    [SerializeField] private string title = "Game Over";
    [SerializeField] private string restartPrompt = "Press Enter to start a new run.";

    private bool isShown;

    private void Awake()
    {
        Hide();
    }

    private void OnEnable()
    {
        if (restartAction != null)
        {
            restartAction.action.performed += OnRestartPerformed;
            restartAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (restartAction != null)
        {
            restartAction.action.performed -= OnRestartPerformed;
            restartAction.action.Disable();
        }
    }

    public void Show(int reachedFloorNumber)
    {
        isShown = true;
        GameUIState.IsGameOver = true;
        GameUIState.IsInventoryOpen = false;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (titleText != null)
        {
            titleText.text = title;
        }

        if (detailsText != null)
        {
            detailsText.text =
                "Your journey ended on floor " + reachedFloorNumber + "." +
                "\n\n" + restartPrompt;
        }
    }

    public void Hide()
    {
        isShown = false;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    private void OnRestartPerformed(InputAction.CallbackContext context)
    {
        if (!isShown)
        {
            return;
        }

        RestartCurrentScene();
    }

    private void RestartCurrentScene()
    {
        GameUIState.Reset();

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}