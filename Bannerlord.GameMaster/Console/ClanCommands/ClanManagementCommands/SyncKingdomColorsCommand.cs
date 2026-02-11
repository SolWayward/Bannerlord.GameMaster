using System.Collections.Generic;
using Bannerlord.GameMaster.Clans;
using Bannerlord.GameMaster.Common;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.ClanCommands.ClanManagementCommands;

/// <summary>
/// Syncs a single clan's banner colors to match the kingdom colors.
/// Usage: gm.clan.sync_kingdom_colors <clan>
/// </summary>
public static class SyncKingdomColorsCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("sync_kingdom_colors", "gm.clan")]
    public static string SyncKingdomColors(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error);

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.clan.sync_kingdom_colors", "<clan>",
                "Syncs a single clan's banner colors to match the kingdom colors.\n" +
                "The clan must be a member of a kingdom and must NOT be the ruling clan.\n" +
                "WARNING: This will overwrite the specified clan's banner colors with the kingdom colors.\n" +
                "Supports named arguments: clan:Meroc",
                "gm.clan.sync_kingdom_colors Meroc\n" +
                "gm.clan.sync_kingdom_colors clan:sturgia");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("clan", true)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError));

            if (parsed.TotalCount < 1)
                return CommandResult.Success(usageMessage);

            // MARK: Parse Arguments
            string clanArg = parsed.GetArgument("clan", 0);

            if (string.IsNullOrWhiteSpace(clanArg))
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Clan argument cannot be empty."));

            EntityFinderResult<Clan> clanResult = ClanFinder.FindSingleClan(clanArg);
            if (!clanResult.IsSuccess)
                return CommandResult.Error(clanResult.Message);
            Clan clan = clanResult.Entity;

            if (clan.Kingdom == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(
                    $"Clan '{clan.Name}' is not a member of any kingdom."));

            if (clan.IsRulingClan())
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(
                    $"Cannot sync kingdom colors for the ruling clan. " +
                    $"The ruling clan defines the kingdom colors. " +
                    $"Use gm.kingdom.sync_vassal_banners to propagate the ruling clan's banner to all vassals instead."));

            // MARK: Execute Logic
            Dictionary<string, string> resolvedValues = new()
            {
                { "clan", clan.Name.ToString() }
            };

            BLGMResult result = clan.UpdateBannerColorsForKingdom();

            if (!result.IsSuccess)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(result.Message));

            string argumentDisplay = parsed.FormatArgumentDisplay("gm.clan.sync_kingdom_colors", resolvedValues);
            return CommandResult.Success(argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"Synced banner colors for clan '{clan.Name}' to kingdom '{clan.Kingdom.Name}' colors."));
        }).Message;
    }
}
