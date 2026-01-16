using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using System.Collections.Generic;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.LoggerCommands;

/// <summary>
/// Command to show current logging status
/// </summary>
public static class LoggingStatusCommand
{
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

            string sizeFormatted = LoggerCommandHelpers.FormatFileSize(size);

            return $"Command Logger Status:\n" +
                   $"Status: {status}\n" +
                   $"Log File: {path}\n" +
                   $"File Size: {sizeFormatted}\n" +
                   $"Log Entries: {entries}\n";
        });
    }
}
