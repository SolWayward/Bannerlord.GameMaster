using System;
using System.Collections.Generic;
using Bannerlord.GameMaster.Console.Common;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console
{
    /// <summary>
    /// Console commands for managing command logging
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("log", "gm")]
    public static class LoggerCommands
    {
        /// <summary>
        /// Enable command logging
        /// Usage: gm.log.enable [custom_path]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("enable", "gm.log")]
        public static string EnableLogging(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                try
                {
                    string customPath = args != null && args.Count > 0 ? args[0] : null;
                    
                    // Initialize if not already done or if custom path provided
                    if (string.IsNullOrEmpty(CommandLogger.LogFilePath) || !string.IsNullOrEmpty(customPath))
                    {
                        CommandLogger.Initialize(customPath);
                    }

                    CommandLogger.IsEnabled = true;
                    CommandLogger.LogSessionStart();

                    string path = CommandLogger.LogFilePath;
                    return $"Success: Command logging enabled.\nLog file: {path}\n";
                }
                catch (Exception ex)
                {
                    return $"Error: Failed to enable logging: {ex.Message}\n";
                }
            });
        }

        /// <summary>
        /// Disable command logging
        /// Usage: gm.log.disable
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("disable", "gm.log")]
        public static string DisableLogging(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                CommandLogger.IsEnabled = false;
                return "Success: Command logging disabled.\n";
            });
        }

        /// <summary>
        /// Show current logging status
        /// Usage: gm.log.status
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("status", "gm.log")]
        public static string ShowStatus(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                string status = CommandLogger.IsEnabled ? "ENABLED" : "DISABLED";
                string path = CommandLogger.LogFilePath ?? "Not initialized";
                long size = CommandLogger.GetLogFileSize();
                int entries = CommandLogger.GetLogEntryCount();

                string sizeFormatted = FormatFileSize(size);

                return $"Command Logger Status:\n" +
                       $"Status: {status}\n" +
                       $"Log File: {path}\n" +
                       $"File Size: {sizeFormatted}\n" +
                       $"Log Entries: {entries}\n";
            });
        }

        /// <summary>
        /// Clear the log file
        /// Usage: gm.log.clear
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("clear", "gm.log")]
        public static string ClearLog(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                try
                {
                    if (string.IsNullOrEmpty(CommandLogger.LogFilePath))
                    {
                        return "Error: Logger not initialized. Enable logging first with gm.log.enable\n";
                    }

                    CommandLogger.ClearLog();
                    return $"Success: Log file cleared.\nLog file: {CommandLogger.LogFilePath}\n";
                }
                catch (Exception ex)
                {
                    return $"Error: Failed to clear log: {ex.Message}\n";
                }
            });
        }

        /// <summary>
        /// Show logging help
        /// Usage: gm.log.help
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("help", "gm.log")]
        public static string ShowHelp(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                return "=== GAME MASTER LOGGING COMMANDS ===\n\n" +
                       "Logger Control:\n" +
                       "  gm.log.enable [path]     - Enable command logging (optional custom path)\n" +
                       "  gm.log.disable           - Disable command logging\n" +
                       "  gm.log.status            - Show current logging status and statistics\n" +
                       "  gm.log.clear             - Clear the log file\n\n" +
                       "Usage Examples:\n" +
                       "  gm.log.enable                              - Enable with default path\n" +
                       "  gm.log.enable C:\\MyLogs\\commands.txt       - Enable with custom path\n" +
                       "  gm.log.status                              - Check if logging is active\n" +
                       "  gm.log.clear                               - Clear all logged commands\n\n" +
                       "Default Log Location:\n" +
                       "  Documents\\Mount and Blade II Bannerlord\\Configs\\GameMaster\\command_log.txt\n\n" +
                       "Note: Commands integrated with logging will be logged when enabled.\n" +
                       "See LOGGING.md for integration guide.\n";
            });
        }

        /// <summary>
        /// Format file size in human-readable format
        /// </summary>
        private static string FormatFileSize(long bytes)
        {
            if (bytes == 0) return "0 bytes";
            if (bytes < 1024) return $"{bytes} bytes";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F2} KB";
            if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024):F2} MB";
            return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";
        }
    }
}