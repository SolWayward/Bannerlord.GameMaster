using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Information;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.InfoCommands;

/// <summary>
/// Command to list loaded mods in launch.json format for easy copy/paste
/// </summary>
public static class ListModsLaunchCommand
{
    /// <summary>
    /// List current loaded mods in launch.json format for easy copy/paste
    /// Usage: gm.info.list_mods_launch
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("list_mods_launch", "gm.info")]
    public static string ListModsLaunch(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            string[] moduleNames = GameEnvironment.LoadedModules;
            StringBuilder output = new();

            // Build the launch format string
            string launchFormat = "_MODULES_*" + string.Join("*", moduleNames) + "*_MODULES_";

            output.AppendLine($"Loaded Modules in launch.json format ({moduleNames.Length} modules):");
            output.AppendLine(new string('-', 50));
            output.AppendLine(launchFormat);
            output.AppendLine();
            output.AppendLine("Copy the line above and paste it into your launch.json args section:");
            output.AppendLine("\"args\": [");
            output.AppendLine("    \"/singleplayer\",");
            output.AppendLine("    \"/continuegame\",");
            output.AppendLine($"    \"{launchFormat}\"");
            output.AppendLine("]");

            return CommandResult.Success(output.ToString()).Log().Message;
        });
    }
}
