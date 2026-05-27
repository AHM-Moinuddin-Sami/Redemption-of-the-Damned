using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

/*
 * GameMessageLog
 * --------------
 * Displays gameplay messages inside the game's UI.
 *
 * This script creates a simple roguelike-style message log, similar to:
 * - "You hit the enemy."
 * - "The enemy dies."
 * - "You picked up Bread x2."
 * - "You are hungry."
 * - "You descend to the next floor."
 *
 * Current responsibilities:
 * - store recent messages
 * - display recent messages in a TextMeshPro UI text field
 * - optionally capture existing Debug.Log messages automatically
 * - provide a static Write() method for future gameplay systems
 *
 * Important:
 * This first version can capture Debug.Log automatically, so your existing
 * scripts do not all need to be rewritten immediately.
 *
 * Later this can expand into:
 * - colored messages
 * - combat message categories
 * - message history scrolling
 * - important message highlighting
 * - floating combat text
 * - UI sound effects
 */

public class GameMessageLog : MonoBehaviour
{
    public static GameMessageLog Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TMP_Text messageText;

    [Header("Message Settings")]
    [SerializeField] private int maxVisibleMessages = 8;
    [SerializeField] private bool newestMessageAtBottom = true;

    [Header("Debug Log Capture")]
    [SerializeField] private bool captureUnityDebugLogs = true;
    [SerializeField] private bool captureWarnings = false;
    [SerializeField] private bool captureErrors = false;

    [Header("Filtering")]
    [SerializeField] private bool onlyCaptureMessagesWithPrefix = false;
    [SerializeField] private string requiredPrefix = "[Game]";

    [Header("Console Output")]
    [SerializeField] private bool staticWriteAlsoLogsToConsole = true;

    private readonly List<string> messages = new List<string>();

    private bool suppressNextCapturedLog;

    private void Awake()
    {
        // This is a simple scene-level singleton.
        // There should only be one GameMessageLog in the scene.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        RefreshText();
    }

    private void OnEnable()
    {
        // Captures Debug.Log messages from existing scripts.
        // This lets the current prototype show messages without replacing every log call.
        Application.logMessageReceived += HandleUnityLogMessage;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleUnityLogMessage;
    }

    public static void Write(string message)
    {
        if (Instance == null)
        {
            Debug.Log(message);
            return;
        }

        Instance.AddMessage(message);

        if (Instance.staticWriteAlsoLogsToConsole)
        {
            // Prevent the Debug.Log below from being captured again and duplicated.
            Instance.suppressNextCapturedLog = true;
            Debug.Log(message);
        }
    }

    public void AddMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        messages.Add(message);

        // Keep only the most recent messages so the UI does not grow forever.
        while (messages.Count > maxVisibleMessages)
        {
            messages.RemoveAt(0);
        }

        RefreshText();
    }

    public void Clear()
    {
        messages.Clear();
        RefreshText();
    }

    private void HandleUnityLogMessage(string condition, string stackTrace, LogType type)
    {
        if (!captureUnityDebugLogs)
        {
            return;
        }

        if (suppressNextCapturedLog)
        {
            suppressNextCapturedLog = false;
            return;
        }

        if (!ShouldCaptureLogType(type))
        {
            return;
        }

        if (onlyCaptureMessagesWithPrefix && !condition.StartsWith(requiredPrefix))
        {
            return;
        }

        AddMessage(condition);
    }

    private bool ShouldCaptureLogType(LogType type)
    {
        if (type == LogType.Log)
        {
            return true;
        }

        if (type == LogType.Warning)
        {
            return captureWarnings;
        }

        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
        {
            return captureErrors;
        }

        return false;
    }

    private void RefreshText()
    {
        if (messageText == null)
        {
            return;
        }

        StringBuilder builder = new StringBuilder();

        if (newestMessageAtBottom)
        {
            for (int i = 0; i < messages.Count; i++)
            {
                builder.AppendLine(messages[i]);
            }
        }
        else
        {
            for (int i = messages.Count - 1; i >= 0; i--)
            {
                builder.AppendLine(messages[i]);
            }
        }

        messageText.text = builder.ToString();
    }
}