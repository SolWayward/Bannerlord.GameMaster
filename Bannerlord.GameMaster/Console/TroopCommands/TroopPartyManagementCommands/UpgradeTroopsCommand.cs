using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Party;
using Bannerlord.GameMaster.Troops;
using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.TroopCommands.TroopPartyManagementCommands;

public static class UpgradeTroopsCommand
{
    /// <summary>
    /// Upgrade all troops in a party leader's party to specified tier
    /// Usage: gm.troops.upgrade_troops [partyLeader] [tier] [infantryRatio] [rangedRatio] [cavalryRatio]
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("upgrade_troops", "gm.troops")]
    public static string UpgradeTroops(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Message;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.troops.upgrade_troops", "<partyLeader> [tier] [infantryRatio] [rangedRatio] [cavalryRatio]",
                "Upgrades all troops in the hero's party to specified tier or max tier of the troop if specified tier is higher.\n" +
                "Attempts to maintain a ratio of troop types.\n" +
                "Optional tier defaults to 7. Optional ratios 0 to 1 (defaults to infantry:0.5, ranged:0.3, cavalry:0.2).\n" +
                "All ratios must add up to 1. If only one or two ratios are specified, remaining ratios will default to evenly add up to 1.\n" +
                "Supports named arguments: partyLeader:derthert tier:6 infantryRatio:0.5 rangedRatio:0.3 cavalryRatio:0.2",
                "gm.troops.upgrade_troops derthert\n" +
                "gm.troops.upgrade_troops player 6\n" +
                "gm.troops.upgrade_troops derthert 7 0.4 0.4 0.2");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("partyLeader", true, null, "leader"),
                new ArgumentDefinition("tier", false),
                new ArgumentDefinition("infantryRatio", false, null, "infantry"),
                new ArgumentDefinition("rangedRatio", false, null, "ranged"),
                new ArgumentDefinition("cavalryRatio", false, null, "cavalry")
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError)).Message;

            if (parsed.TotalCount < 1)
                return CommandResult.Error(usageMessage).Message;

            // MARK: Parse Arguments
            string leaderArg = parsed.GetArgument("partyLeader", 0) ?? parsed.GetNamed("leader");
            if (leaderArg == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'partyLeader'.")).Message;

            EntityFinderResult<Hero> heroResult = HeroFinder.FindSingleHero(leaderArg);
            if (!heroResult.IsSuccess)
                return CommandResult.Error(heroResult.Message).Message;
            Hero hero = heroResult.Entity;

            // Parse tier (optional, default 7)
            int tier = 7;
            string tierArg = parsed.GetArgument("tier", 1);
            if (tierArg != null)
            {
                if (!CommandValidator.ValidateIntegerRange(tierArg, 1, 10, out tier, out string tierError))
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage(tierError)).Message;
            }

            // Parse ratios (optional)
            float? infantryRatio = null;
            string infantryArg = parsed.GetArgument("infantryRatio", 2) ?? parsed.GetNamed("infantry");
            if (infantryArg != null)
            {
                if (!CommandValidator.ValidateFloatRange(infantryArg, 0f, 1f, out float infantry, out string infantryError))
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage(infantryError)).Message;
                infantryRatio = infantry;
            }

            float? rangedRatio = null;
            string rangedArg = parsed.GetArgument("rangedRatio", 3) ?? parsed.GetNamed("ranged");
            if (rangedArg != null)
            {
                if (!CommandValidator.ValidateFloatRange(rangedArg, 0f, 1f, out float ranged, out string rangedError))
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage(rangedError)).Message;
                rangedRatio = ranged;
            }

            float? cavalryRatio = null;
            string cavalryArg = parsed.GetArgument("cavalryRatio", 4) ?? parsed.GetNamed("cavalry");
            if (cavalryArg != null)
            {
                if (!CommandValidator.ValidateFloatRange(cavalryArg, 0f, 1f, out float cavalry, out string cavalryError))
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage(cavalryError)).Message;
                cavalryRatio = cavalry;
            }

            // Validate ratios sum
            if (infantryRatio.HasValue && rangedRatio.HasValue && cavalryRatio.HasValue)
            {
                float sum = infantryRatio.Value + rangedRatio.Value + cavalryRatio.Value;
                if (Math.Abs(sum - 1.0f) > 0.01f)
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage($"Troop ratios must add up to 1.0. Current sum: {sum:F2}")).Message;
            }

            // Calculate actual ratios that will be used (including defaults for unspecified)
            (float actualRangedRatio, float actualCavalryRatio, float actualInfantryRatio) =
                TroopUpgrader.NormalizeRatios(rangedRatio, cavalryRatio, infantryRatio);

            // MARK: Execute Logic
            if (hero.PartyBelongedTo == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage($"{hero.Name} does not belong to a party.")).Message;

            if (hero.PartyBelongedTo.LeaderHero != hero)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage($"{hero.Name} is not a party leader. They belong to {hero.PartyBelongedTo.LeaderHero.Name}'s party.")).Message;

            // Call the extension method with proper parameter order (ranged, cavalry, infantry)
            hero.PartyBelongedTo.UpgradeTroops(tier, rangedRatio, cavalryRatio, infantryRatio);

            Dictionary<string, string> resolvedValues = new()
            {
                { "partyLeader", hero.Name.ToString() },
                { "tier", tier.ToString() },
                { "infantryRatio", actualInfantryRatio.ToString("F2") },
                { "rangedRatio", actualRangedRatio.ToString("F2") },
                { "cavalryRatio", actualCavalryRatio.ToString("F2") }
            };

            StringBuilder result = new();
            result.AppendLine($"Upgraded troops in {hero.Name}'s party to tier {tier}.");

            if (infantryRatio.HasValue || rangedRatio.HasValue || cavalryRatio.HasValue)
            {
                result.Append("Target ratios - ");
                if (infantryRatio.HasValue)
                    result.Append($"Infantry: {infantryRatio.Value:P0} ");
                if (rangedRatio.HasValue)
                    result.Append($"Ranged: {rangedRatio.Value:P0} ");
                if (cavalryRatio.HasValue)
                    result.Append($"Cavalry: {cavalryRatio.Value:P0}");
                result.AppendLine();
            }

            result.AppendLine($"Party: {hero.PartyBelongedTo.Name} (Total size: {hero.PartyBelongedTo.MemberRoster.TotalManCount})");

            string argumentDisplay = parsed.FormatArgumentDisplay("gm.troop.upgrade_troops", resolvedValues);
            string fullMessage = argumentDisplay + MessageFormatter.FormatSuccessMessage(result.ToString());
            return CommandResult.Success(fullMessage).Message;
        });
    }
}
