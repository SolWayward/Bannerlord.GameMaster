using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Party;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.TroopCommands.TroopPartyManagementCommands;

public static class AddEliteTroopsCommand
{
    /// <summary>
    /// Add elite tier troops to a party leader's party
    /// Usage: gm.troops.add_elite [partyLeader] [count]
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("add_elite", "gm.troops")]
    public static string AddEliteTroops(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return error;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.troops.add_elite", "<partyLeader> <count>",
                "Adds elite tier troops from the party leader's culture to their party.\n" +
                "Supports named arguments: partyLeader:derthert count:30",
                "gm.troops.add_elite derthert 30\n" +
                "gm.troops.add_elite player 50");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("partyLeader", true, null, "leader"),
                new ArgumentDefinition("count", true)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return MessageFormatter.FormatErrorMessage(validationError);

            if (parsed.TotalCount < 2)
                return usageMessage;

            // MARK: Parse Arguments
            string leaderArg = parsed.GetArgument("partyLeader", 0) ?? parsed.GetNamed("leader");
            if (leaderArg == null)
                return MessageFormatter.FormatErrorMessage("Missing required argument 'partyLeader'.");

            EntityFinderResult<Hero> heroResult = HeroFinder.FindSingleHero(leaderArg);
            if (!heroResult.IsSuccess)
                return heroResult.Message;
            Hero hero = heroResult.Entity;

            string countArg = parsed.GetArgument("count", 1);
            if (countArg == null)
                return MessageFormatter.FormatErrorMessage("Missing required argument 'count'.");

            if (!CommandValidator.ValidateIntegerRange(countArg, 1, 10000, out int count, out string countError))
                return MessageFormatter.FormatErrorMessage(countError);

            // MARK: Execute Logic
            if (hero.PartyBelongedTo == null)
                return MessageFormatter.FormatErrorMessage($"{hero.Name} does not belong to a party.");

            if (hero.PartyBelongedTo.LeaderHero != hero)
                return MessageFormatter.FormatErrorMessage($"{hero.Name} is not a party leader. They belong to {hero.PartyBelongedTo.LeaderHero.Name}'s party.");

            hero.PartyBelongedTo.AddEliteTroops(count);

            string troopName = hero.Culture?.EliteBasicTroop?.Name?.ToString() ?? "elite troops";

            Dictionary<string, string> resolvedValues = new()
            {
                { "partyLeader", hero.Name.ToString() },
                { "count", count.ToString() }
            };

            string argumentDisplay = parsed.FormatArgumentDisplay("add_elite", resolvedValues);
            return argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"Added {count}x {troopName} to {hero.Name}'s party.\n" +
                $"Party: {hero.PartyBelongedTo.Name} (Total size: {hero.PartyBelongedTo.MemberRoster.TotalManCount})");
        });
    }
}
