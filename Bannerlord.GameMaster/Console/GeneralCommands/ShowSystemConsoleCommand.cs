using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using System.Collections.Generic;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.GeneralCommands;

/// <summary>
/// Allocates and attaches System Console for debugging and viewing command and error output
/// </summary>
public static class ShowSystemConsoleCommand
{
    /// <summary>
    /// Allocates and attaches System Console for debugging and viewing command and error output
    /// Usage: gm.show_system_console true
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("show_system_console", "gm")]
    public static string ShowSystemConsole(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // TODO: Add true / false and 1 / 0 arg valdiation for enabling/disabling and add proper command output formatting
            string command = "gm.show_system_console";

            // MARK: Execute Logic
            SystemConsoleManager.ShowConsole();

            string message = MessageFormatter.FormatSuccessMessage($"{command}\nSystem Console allocated and attached");
            return CommandResult.Success(message).Message
;
        });
    }
}
