using Bannerlord.GameMaster.BLGMDebug;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Validation;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.DevCommands;

/// <summary>
/// Command to get the player's captain perks for debugging
/// </summary>
public static class GetPlayerCaptainPerksCommand
{
    /// <summary>
    /// Gets the player hero's captain perks
    /// Usage: gm.dev.get_player_captain_perks
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("get_player_captain_perks", "gm.dev")]
    public static string GetPlayerCaptainPerks(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return error;

            // MARK: Execute Logic
            return HeroDebug.CaptainOnFootPerks(Hero.MainHero);
        });
    }
}
