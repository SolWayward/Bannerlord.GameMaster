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
/// Command to change settlement owner clan.
/// Usage: gm.settlement.set_owner_clan [settlement] [clan]
/// </summary>
public static class SetOwnerClanCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("set_owner_clan", "gm.settlement")]
    public static string SetOwnerClan(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Message
;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.settlement.set_owner_clan", "<settlement> <clan>",
                "Changes the settlement owner clan to the specified clan. Also updates the owner to the clan leader and map faction to the clan's kingdom (if any).",
                "gm.settlement.set_owner_clan pen empire_south");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);
            parsed.SetValidArguments(
                new ArgumentDefinition("settlement", true),
                new ArgumentDefinition("clan", true)
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
            string clanQuery = parsed.GetArgument("clan", 1);

            EntityFinderResult<Settlement> settlementResult = SettlementFinder.FindSingleSettlement(settlementQuery);
            if (!settlementResult.IsSuccess) return CommandResult.Error(settlementResult.Message).Message
;
            Settlement settlement = settlementResult.Entity;

            if (settlement.Town == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage($"Settlement '{settlement.Name}' has no town likely because it is not a castle of city.")).Message
;

            EntityFinderResult<Clan> clanResult = ClanFinder.FindSingleClan(clanQuery);
            if (!clanResult.IsSuccess) return CommandResult.Error(clanResult.Message).Message
;
            Clan clan = clanResult.Entity;

            if (clan.Leader == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage($"Clan '{clan.Name}' has no leader.")).Message
;

            // MARK: Execute Logic
            string previousOwner = settlement.Owner?.Name?.ToString() ?? "None";
            string previousClan = settlement.OwnerClan?.Name?.ToString() ?? "None";
            string previousFaction = settlement.MapFaction?.Name?.ToString() ?? "None";

            settlement.ChangeOwner(clan.Leader);

            Dictionary<string, string> resolvedValues = new()
            {
                ["settlement"] = settlement.Name.ToString(),
                ["clan"] = clan.Name.ToString()
            };

            string argumentDisplay = parsed.FormatArgumentDisplay("gm.settlement.set_owner_clan", resolvedValues);
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
