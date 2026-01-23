using System.Collections.Generic;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Settlements;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.SettlementCommands.FortificationManagementCommands;

/// <summary>
/// Command to change settlement owner kingdom.
/// Usage: gm.settlement.set_owner_kingdom [settlement] [kingdom]
/// </summary>
public static class SetOwnerKingdomCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("set_owner_kingdom", "gm.settlement")]
    public static string SetOwnerKingdom(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Message
;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.settlement.set_owner_kingdom", "<settlement> <kingdom>",
                "Changes the settlement owner kingdom. Also updates the owner to the kingdom ruler, owner clan to the ruler's clan, and map faction to the kingdom.",
                "gm.settlement.set_owner_kingdom pen empire");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);
            parsed.SetValidArguments(
                new ArgumentDefinition("settlement", true),
                new ArgumentDefinition("kingdom", true)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError)).Message
;

            if (parsed.TotalCount < 2)
                return CommandResult.Error(usageMessage).Message
;

            // MARK: Parse Arguments
            string settlementQuery = parsed.GetArgument("settlement", 0);
            string kingdomQuery = parsed.GetArgument("kingdom", 1);

            EntityFinderResult<Settlement> settlementResult = SettlementFinder.FindSingleSettlement(settlementQuery);
            if (!settlementResult.IsSuccess) return CommandResult.Error(settlementResult.Message).Message
;
            Settlement settlement = settlementResult.Entity;

            if (settlement.Town == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage($"Settlement '{settlement.Name}' has no town likely because it is not a castle of city.")).Message
;

            EntityFinderResult<Kingdom> kingdomResult = KingdomFinder.FindSingleKingdom(kingdomQuery);
            if (!kingdomResult.IsSuccess) return CommandResult.Error(kingdomResult.Message).Message
;
            Kingdom kingdom = kingdomResult.Entity;

            if (kingdom.Leader == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage($"Kingdom '{kingdom.Name}' has no ruler.")).Message
;

            if (kingdom.RulingClan == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage($"Kingdom '{kingdom.Name}' has no ruling clan.")).Message
;

            // MARK: Execute Logic
            string previousOwner = settlement.Owner?.Name?.ToString() ?? "None";
            string previousClan = settlement.OwnerClan?.Name?.ToString() ?? "None";
            string previousFaction = settlement.MapFaction?.Name?.ToString() ?? "None";

            settlement.ChangeOwner(kingdom.Leader);

            Dictionary<string, string> resolvedValues = new()
            {
                ["settlement"] = settlement.Name.ToString(),
                ["kingdom"] = kingdom.Name.ToString()
            };

            string argumentDisplay = parsed.FormatArgumentDisplay("gm.settlement.set_owner_kingdom", resolvedValues);
            string fullMessage = argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"Settlement '{settlement.Name}' (ID: {settlement.StringId}) ownership changed:\n" +
                $"Owner: {previousOwner} -> {settlement.Owner?.Name?.ToString() ?? "None"}\n" +
                $"Owner Clan: {previousClan} -> {settlement.OwnerClan?.Name?.ToString() ?? "None"}\n" +
                $"Map Faction: {previousFaction} -> {settlement.MapFaction?.Name?.ToString() ?? "None"}");
            return CommandResult.Success(fullMessage).Message
;
        });
    }
}
