using Bannerlord.GameMaster.Console.Common.Execution;
using System.Collections.Generic;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.InfoCommands;

/// <summary>
/// Command to display count of objects created with BLGM
/// </summary>
public static class BlgmObjectCountCommand
{
    /// <summary>
    /// List count of objects created with BLGM
    /// Usage: gm.info.blgm_object_count
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("blgm_object_count", "gm.info")]
    public static string BlgmObjectCount(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            return $"{BLGMObjectManager.Instance.ObjectCount} Total objects created with BLGM\n" +
                $"Heroes: {BLGMObjectManager.BlgmHeroCount}\n" +
                $"Clans: {BLGMObjectManager.BlgmClanCount}\n" +
                $"Kingdoms: {BLGMObjectManager.BlgmKingdomCount}";
        });
    }
}
