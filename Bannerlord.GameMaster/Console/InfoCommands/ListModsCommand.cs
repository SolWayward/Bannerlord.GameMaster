using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Information;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.InfoCommands;

/// <summary>
/// Command to list currently loaded mods and their load order
/// </summary>
public static class ListModsCommand
{
    /// <summary>
    /// List current loaded mods and their load order
    /// Usage: gm.info.list_mods
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("list_mods", "gm.info")]
    public static string ListMods(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            string[] moduleNames = GameEnvironment.LoadedModules;
            StringBuilder output = new();

            output.AppendLine($"Loaded Modules ({moduleNames.Length}):");
            output.AppendLine(new string('-', 50));

            foreach (string name in moduleNames)
            {
                output.AppendLine($"- {name}");
            }

            output.AppendLine("\nUse command 'gm.log.enable' before running this command to save command output to a log file you can easily copy and paste");

            return output.ToString();
        });
    }
}
