using System.Collections.Generic;
using Bannerlord.GameMaster.Common;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Kingdoms;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.KingdomCommands.KingdomManagementCommands;

/// <summary>
/// Syncs all vassal clan banner colors to the kingdom's ruling clan banner colors.
/// Usage: gm.kingdom.sync_vassal_banners <kingdom>
/// </summary>
public static class SyncVassalBannersCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("sync_vassal_banners", "gm.kingdom")]
    public static string SyncVassalBanners(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error);

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.kingdom.sync_vassal_banners", "<kingdom>",
                "Syncs all vassal clan banner colors to the kingdom's ruling clan banner colors.\n" +
                "WARNING: This will overwrite all vassal clan banner colors with the kingdom's ruling clan banner colors.\n" +
                "Any custom clan colors for members of this kingdom will be lost.\n" +
                "Supports named arguments: kingdom:empire",
                "gm.kingdom.sync_vassal_banners empire\n" +
                "gm.kingdom.sync_vassal_banners kingdom:sturgia");

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
                    $"Kingdom '{kingdom.Name}' has no ruling clan. Cannot propagate banner colors."));

            // MARK: Execute Logic
            Dictionary<string, string> resolvedValues = new()
            {
                { "kingdom", kingdom.Name.ToString() }
            };

            BLGMResult result = kingdom.PropagateRulingClanBanner();

            if (!result.IsSuccess)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(result.Message));

            string argumentDisplay = parsed.FormatArgumentDisplay("gm.kingdom.sync_vassal_banners", resolvedValues);
            return CommandResult.Success(argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"Synced vassal banner colors for kingdom '{kingdom.Name}'.\n" +
                $"Ruling clan: {kingdom.RulingClan.Name}\n" +
                result.Message));
        }).Message;
    }
}
