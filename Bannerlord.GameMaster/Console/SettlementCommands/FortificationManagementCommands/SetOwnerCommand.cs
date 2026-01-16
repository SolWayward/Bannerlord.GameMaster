using System.Collections.Generic;
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
/// Command to change settlement owner to a specific hero.
/// Usage: gm.settlement.set_owner [settlement] [hero]
/// </summary>
[CommandLineFunctionality.CommandLineArgumentFunction("settlement", "gm")]
public static class SetOwnerCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("set_owner", "gm.settlement")]
    public static string SetOwner(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return error;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.settlement.set_owner", "<settlement> <hero>",
                "Changes the settlement owner to the specified hero. Also updates the owner clan to the hero's clan and map faction to the hero's faction (if any).",
                "gm.settlement.set_owner pen lord_1_1");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);
            parsed.SetValidArguments(
                new ArgumentDefinition("settlement", true),
                new ArgumentDefinition("hero", true)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return MessageFormatter.FormatErrorMessage(validationError);

            if (parsed.TotalCount < 2)
                return usageMessage;

            // MARK: Parse Arguments
            string settlementQuery = parsed.GetArgument("settlement", 0);
            string heroQuery = parsed.GetArgument("hero", 1);

            EntityFinderResult<Settlement> settlementResult = SettlementFinder.FindSingleSettlement(settlementQuery);
            if (!settlementResult.IsSuccess) return settlementResult.Message;
            Settlement settlement = settlementResult.Entity;

            if (settlement.Town == null)
                return MessageFormatter.FormatErrorMessage($"Settlement '{settlement.Name}' has no town likely because it is not a castle of city.");

            EntityFinderResult<Hero> heroResult = HeroFinder.FindSingleHero(heroQuery);
            if (!heroResult.IsSuccess) return heroResult.Message;
            Hero hero = heroResult.Entity;

            // MARK: Execute Logic
            string previousOwner = settlement.Owner?.Name?.ToString() ?? "None";
            string previousClan = settlement.OwnerClan?.Name?.ToString() ?? "None";
            string previousFaction = settlement.MapFaction?.Name?.ToString() ?? "None";

            settlement.ChangeOwner(hero);

            Dictionary<string, string> resolvedValues = new()
            {
                ["settlement"] = settlement.Name.ToString(),
                ["hero"] = hero.Name.ToString()
            };

            string argumentDisplay = parsed.FormatArgumentDisplay("set_owner", resolvedValues);
            return argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"Settlement '{settlement.Name}' (ID: {settlement.StringId}) ownership changed:\n" +
                $"Owner: {previousOwner} -> {settlement.Owner?.Name?.ToString() ?? "None"}\n" +
                $"Owner Clan: {previousClan} -> {settlement.OwnerClan?.Name?.ToString() ?? "None"}\n" +
                $"Map Faction: {previousFaction} -> {settlement.MapFaction?.Name?.ToString() ?? "None"}");
        });
    }
}
