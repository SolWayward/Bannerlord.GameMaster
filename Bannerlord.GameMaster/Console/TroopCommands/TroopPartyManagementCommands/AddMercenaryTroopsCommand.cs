using Bannerlord.GameMaster.Console.Common;
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

public static class AddMercenaryTroopsCommand
{
    /// <summary>
    /// Add random mercenary troops to a party leader's party
    /// Usage: gm.troops.add_mercenary [partyLeader] [count]
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("add_mercenary", "gm.troops")]
    public static string AddMercenaryTroops(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Log().Message;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.troops.add_mercenary", "<partyLeader> <count>",
                "Adds random mercenary troops from the party leader's culture to their party.\n" +
                "Supports named arguments: partyLeader:derthert count:20",
                "gm.troops.add_mercenary derthert 20\n" +
                "gm.troops.add_mercenary player 40");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("partyLeader", true, null, "leader"),
                new ArgumentDefinition("count", true)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError)).Log().Message;

            if (parsed.TotalCount < 2)
                return CommandResult.Error(usageMessage).Log().Message;

            // MARK: Parse Arguments
            string leaderArg = parsed.GetArgument("partyLeader", 0) ?? parsed.GetNamed("leader");
            if (leaderArg == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'partyLeader'.")).Log().Message;

            EntityFinderResult<Hero> heroResult = HeroFinder.FindSingleHero(leaderArg);
            if (!heroResult.IsSuccess)
                return CommandResult.Error(heroResult.Message).Log().Message;
            Hero hero = heroResult.Entity;

            string countArg = parsed.GetArgument("count", 1);
            if (countArg == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'count'.")).Log().Message;

            if (!CommandValidator.ValidateIntegerRange(countArg, 1, 10000, out int count, out string countError))
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(countError)).Log().Message;

            // MARK: Execute Logic
            if (hero.PartyBelongedTo == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage($"{hero.Name} does not belong to a party.")).Log().Message;

            if (hero.PartyBelongedTo.LeaderHero != hero)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage($"{hero.Name} is not a party leader. They belong to {hero.PartyBelongedTo.LeaderHero.Name}'s party.")).Log().Message;

            hero.PartyBelongedTo.AddMercenaryTroops(count);

            Dictionary<string, string> resolvedValues = new()
            {
                { "partyLeader", hero.Name.ToString() },
                { "count", count.ToString() }
            };

            string argumentDisplay = parsed.FormatArgumentDisplay("gm.troop.add_mercenary", resolvedValues);
            string fullMessage = argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"Added {count}x random mercenary troops from {hero.Culture?.Name} culture to {hero.Name}'s party.\n" +
                $"Party: {hero.PartyBelongedTo.Name} (Total size: {hero.PartyBelongedTo.MemberRoster.TotalManCount})");
            return CommandResult.Success(fullMessage).Log().Message;
        });
    }
}
