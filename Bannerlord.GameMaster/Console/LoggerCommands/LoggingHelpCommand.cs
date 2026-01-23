using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using System.Collections.Generic;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.LoggerCommands;

/// <summary>
/// Command to show logging help
/// </summary>
public static class LoggingHelpCommand
{
    /// <summary>
    /// Show logging help
    /// Usage: gm.log.help
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("help", "gm.log")]
    public static string ShowHelp(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            string helpMessage = "=== GAME MASTER LOGGING COMMANDS ===\n\n" +
                   "Logger Control:\n" +
                   "  gm.log.enable [path:path]  - Enable command logging (optional custom path)\n" +
                   "  gm.log.disable             - Disable command logging\n" +
                   "  gm.log.status              - Show current logging status and statistics\n" +
                   "  gm.log.clear               - Clear the log file\n\n" +
                   "Usage Examples:\n" +
                   "  gm.log.enable                              - Enable with default path\n" +
                   "  gm.log.enable path:C:\\MyLogs\\commands.txt  - Enable with custom path\n" +
                   "  gm.log.status                              - Check if logging is active\n" +
                   "  gm.log.clear                               - Clear all logged commands\n\n" +
                   "Default Log Location:\n" +
                   "  Documents\\Mount and Blade II Bannerlord\\Configs\\GameMaster\\command_log.txt\n\n" +
                   "Note: Commands integrated with logging will be logged when enabled.\n" +
                   "See LOGGING.md for integration guide.\n";
            return CommandResult.Success(helpMessage).Message
;
        });
    }
}
