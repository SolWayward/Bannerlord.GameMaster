using System;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.Common.Execution;

/// <summary>
/// Manages command logging lifecycle including auto-start configuration and enable/disable operations.
/// Provides a centralized interface for controlling the CommandLogger.
/// </summary>
public static class LoggingManager
{
    #region Configuration
    /// <summary>
    /// Controls whether logging should automatically start when a campaign is loaded.
    /// Set to true for auto-start, false to require manual enable via console command.
    /// </summary>
    public static bool AutoStartEnabled { get; set; } = true;
    #endregion

    #region Enable/Disable Methods
    /// <summary>
    /// Enables command logging with optional custom path.
    /// Initializes the CommandLogger if not already initialized.
    /// </summary>
    /// <param name="customPath">Optional custom directory path for log files</param>
    /// <returns>Result containing success status and message</returns>
    public static LoggingResult EnableLogging(string customPath = null)
    {
        try
        {
            // Initialize if not already done or if custom path provided
            if (string.IsNullOrEmpty(CommandLogger.LogFilePath) || !string.IsNullOrEmpty(customPath))
            {
                CommandLogger.Initialize(customPath);
            }

            CommandLogger.IsEnabled = true;
            CommandLogger.LogSessionStart();

            string path = CommandLogger.LogFilePath;
            return new(true, $"Command logging enabled.\nLog file: {path}");
        }
        catch (Exception ex)
        {
            Debug.Print($"[BLGM] LoggingManager: Failed to enable logging: {ex.Message}");
            return new(false, $"Failed to enable logging: {ex.Message}");
        }
    }

    /// <summary>
    /// Disables command logging and flushes any pending entries.
    /// </summary>
    /// <returns>Result containing success status and message</returns>
    public static LoggingResult DisableLogging()
    {
        try
        {
            if (!CommandLogger.IsEnabled)
            {
                return new(true, "Command logging was already disabled.");
            }

            // Force flush any pending entries before disabling
            CommandLogger.ForceFlush();
            CommandLogger.IsEnabled = false;

            return new(true, "Command logging disabled.");
        }
        catch (Exception ex)
        {
            Debug.Print($"[BLGM] LoggingManager: Failed to disable logging: {ex.Message}");
            return new(false, $"Failed to disable logging: {ex.Message}");
        }
    }

    /// <summary>
    /// Attempts to auto-start logging if AutoStartEnabled is true.
    /// Should be called during campaign load (OnGameLoaded or OnSessionLaunched).
    /// </summary>
    public static void TryAutoStart()
    {
        if (!AutoStartEnabled)
        {
            Debug.Print("[BLGM] LoggingManager: Auto-start is disabled, skipping automatic logging initialization.");
            return;
        }

        LoggingResult result = EnableLogging();
        if (result.WasSuccessful)
        {
            Debug.Print("[BLGM] LoggingManager: Auto-started command logging.");
        }
        else
        {
            Debug.Print($"[BLGM] LoggingManager: Auto-start failed: {result.Message}");
        }
    }
    #endregion

    #region Status Methods
    /// <summary>
    /// Gets whether logging is currently enabled.
    /// </summary>
    public static bool IsLoggingEnabled => CommandLogger.IsEnabled;

    /// <summary>
    /// Gets the current log file path, or null if not initialized.
    /// </summary>
    public static string CurrentLogFilePath => CommandLogger.LogFilePath;
    #endregion
}

/// <summary>
/// Result of a logging operation containing success status and message.
/// </summary>
public struct LoggingResult
{
    public bool WasSuccessful { get; }
    public string Message { get; }

    public LoggingResult(bool wasSuccessful, string message)
    {
        WasSuccessful = wasSuccessful;
        Message = message;
    }
}
