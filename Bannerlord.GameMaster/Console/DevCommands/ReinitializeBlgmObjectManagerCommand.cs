using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using System.Collections.Generic;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.DevCommands;

/// <summary>
/// Command to reinitialize the BLGMObjectManager for testing purposes
/// </summary>
public static class ReinitializeBlgmObjectManagerCommand
{
    /// <summary>
    /// Reinitializes BLGMObjectManager for testing
    /// Usage: gm.dev.reinitialize_blgm_objectmanager
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("reinitialize_blgm_objectmanager", "gm.dev")]
    public static string ReinitializeBlgmObjectManager(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Execute Logic
            BLGMObjectManager.Instance.Initialize();
            
            string message = MessageFormatter.FormatSuccessMessage(
                $"BLGMObjectManager Reinitialized: {BLGMObjectManager.Instance.ObjectCount} BLGM created objects loaded");
            return CommandResult.Success(message).Message
;
        });
    }
}
