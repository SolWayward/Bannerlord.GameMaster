using Bannerlord.GameMaster.Banners;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using Bannerlord.GameMaster.Console.Common;

namespace Bannerlord.GameMaster.Console.ClanCommands.ClanManagementCommands;

/// <summary>
/// Opens the banner editor for a specified clan
/// Usage: gm.clan.edit_banner [clan]
/// </summary>
public static class EditBannerCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("edit_banner", "gm.clan")]
    public static string EditBanner(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error);

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.clan.edit_banner", "[clan]",
                "Opens the native banner editor for the specified clan.\n" +
                "If no clan is specified, opens the editor for the player's clan.\n" +
                "Supports named arguments: clan:empire_south",
                "gm.clan.edit_banner\n" +
                "gm.clan.edit_banner empire_south\n" +
                "gm.clan.edit_banner clan:sturgia");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("clan", false)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError));

            // MARK: Parse Arguments
            Clan clan;
            string clanArg = parsed.GetArgument("clan", 0);

            if (string.IsNullOrWhiteSpace(clanArg))
            {
                // Default to player clan if no clan specified
                clan = Clan.PlayerClan;
                if (clan == null)
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage("No clan specified and player clan is not available."));
            }
            else
            {
                EntityFinderResult<Clan> clanResult = ClanFinder.FindSingleClan(clanArg);
                if (!clanResult.IsSuccess)
                    return CommandResult.Error(clanResult.Message);
                clan = clanResult.Entity;
            }

            // Check if banner editor is enabled
            if (!Campaign.Current.IsBannerEditorEnabled)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Banner editor is disabled in this campaign."));

            // MARK: Execute Logic
            Dictionary<string, string> resolvedValues = new()
            {
                { "clan", clan.Name.ToString() }
            };

            BannerEditorController.OpenBannerEditor(clan);

            string argumentDisplay = parsed.FormatArgumentDisplay("gm.clan.edit_banner", resolvedValues);
            return CommandResult.Success(argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"Opened banner editor for clan '{clan.Name}' (ID: {clan.StringId})"));
        }).Message;
    }
}
