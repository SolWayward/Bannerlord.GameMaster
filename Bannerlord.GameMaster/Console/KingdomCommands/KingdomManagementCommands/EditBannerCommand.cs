using System.Collections.Generic;
using Bannerlord.GameMaster.Banners;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.KingdomCommands.KingdomManagementCommands;

/// <summary>
/// Opens the banner editor for a kingdom's ruling clan.
/// Usage: gm.kingdom.edit_banner <kingdom>
/// </summary>
public static class EditKingdomBannerCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("edit_banner", "gm.kingdom")]
    public static string EditBanner(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error);

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.kingdom.edit_banner", "<kingdom>",
                "Opens the native banner editor for the kingdom's ruling clan.\n" +
                "This is a shortcut so you don't need to look up the ruling clan ID first.\n" +
                "Changes to the ruling clan's banner will propagate to the kingdom and all vassal clans.\n" +
                "Supports named arguments: kingdom:empire",
                "gm.kingdom.edit_banner empire\n" +
                "gm.kingdom.edit_banner kingdom:sturgia");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("kingdom", true)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError));

            if (parsed.TotalCount < 1)
                return CommandResult.Success(usageMessage);

            // MARK: Parse Arguments
            string kingdomArg = parsed.GetArgument("kingdom", 0);

            if (string.IsNullOrWhiteSpace(kingdomArg))
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Kingdom argument cannot be empty."));

            EntityFinderResult<Kingdom> kingdomResult = KingdomFinder.FindSingleKingdom(kingdomArg);
            if (!kingdomResult.IsSuccess)
                return CommandResult.Error(kingdomResult.Message);
            Kingdom kingdom = kingdomResult.Entity;

            if (kingdom.RulingClan == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(
                    $"Kingdom '{kingdom.Name}' has no ruling clan."));

            if (!Campaign.Current.IsBannerEditorEnabled)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Banner editor is disabled in this campaign."));

            // MARK: Execute Logic
            Clan rulingClan = kingdom.RulingClan;

            Dictionary<string, string> resolvedValues = new()
            {
                { "kingdom", kingdom.Name.ToString() }
            };

            BannerEditorController.OpenBannerEditor(rulingClan);

            string argumentDisplay = parsed.FormatArgumentDisplay("gm.kingdom.edit_banner", resolvedValues);
            return CommandResult.Success(argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"Opened banner editor for kingdom '{kingdom.Name}' (ruling clan: {rulingClan.Name}, ID: {rulingClan.StringId}).\n" +
                $"Changes will propagate to the kingdom and all vassal clans."));
        }).Message;
    }
}
