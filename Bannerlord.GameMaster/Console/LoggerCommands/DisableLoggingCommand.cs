using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using System.Collections.Generic;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.LoggerCommands;

/// <summary>
/// Command to disable command logging
/// </summary>
public static class DisableLoggingCommand
{
    /// <summary>
    /// Disable command logging
    /// Usage: gm.log.disable
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("disable", "gm.log")]
    public static string DisableLogging(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            LoggingResult result = LoggingManager.DisableLogging();

            if (!result.WasSuccessful)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(result.Message)).Message;

            return CommandResult.Success(MessageFormatter.FormatSuccessMessage(result.Message)).Message;
        });
    }
}
