using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Information;
using System.Collections.Generic;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.InfoCommands;

/// <summary>
/// Command to display the current BLGM mod version
/// </summary>
public static class BlgmVersionCommand
{
    /// <summary>
    /// List current BLGM version
    /// Usage: gm.info.blgm_version
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("blgm_version", "gm.info")]
    public static string BlgmVersion(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            return CommandResult.Success($"BLGM v{GameEnvironment.BLGMVersion}").Log().Message;
        });
    }
}
