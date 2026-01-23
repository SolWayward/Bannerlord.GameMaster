using Bannerlord.GameMaster.BLGMDebug;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Validation;
using System.Collections.Generic;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.DevCommands;

/// <summary>
/// Command to check if heroes match their character string IDs
/// </summary>
public static class CheckHeroesMatchesCharactersStringIdCommand
{
    /// <summary>
    /// Checks if heroes' StringIds match their character object StringIds
    /// Usage: gm.dev.check_heroes_matches_characters_stringid
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("check_heroes_matches_characters_stringid", "gm.dev")]
    public static string CheckHeroesMatchesCharactersStringId(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Message
;

            // MARK: Execute Logic
            string message = HeroDebug.CheckHeroesCharacterStringId();
            return CommandResult.Success(message).Message
;
        });
    }
}
