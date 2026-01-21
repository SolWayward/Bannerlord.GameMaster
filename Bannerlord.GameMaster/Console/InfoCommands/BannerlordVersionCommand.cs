using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Information;
using System.Collections.Generic;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.InfoCommands;

/// <summary>
/// Command to display the current Bannerlord game version
/// </summary>
public static class BannerlordVersionCommand
{
    /// <summary>
    /// List current Bannerlord game version
    /// Usage: gm.info.bannerlord_version
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("bannerlord_version", "gm.info")]
    public static string BannerlordVersion(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            return CommandResult.Success($"Bannerlord {GameEnvironment.BannerlordVersion}").Log().Message;
        });
    }
}
