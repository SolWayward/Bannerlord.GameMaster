using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using System;
using System.Collections.Generic;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.LoggerCommands;

/// <summary>
/// Command to clear the log file
/// </summary>
public static class ClearLogCommand
{
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
                    return MessageFormatter.FormatErrorMessage(
                        "Logger not initialized. Enable logging first with gm.log.enable");
                }

                CommandLogger.ClearLog();
                return MessageFormatter.FormatSuccessMessage(
                    $"Log file cleared.\nLog file: {CommandLogger.LogFilePath}");
            }
            catch (Exception ex)
            {
                return MessageFormatter.FormatErrorMessage($"Failed to clear log: {ex.Message}");
            }
        });
    }
}
