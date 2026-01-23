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

public static class AddBasicTroopsCommand
{
    /// <summary>
    /// Add basic tier troops to a party leader's party
    /// Usage: gm.troops.add_basic [partyLeader] [count]
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("add_basic", "gm.troops")]
    public static string AddBasicTroops(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Message
;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.troops.add_basic", "<partyLeader> <count>",
                "Adds basic tier troops from the party leader's culture to their party.\n" +
                "Supports named arguments: partyLeader:derthert count:50",
                "gm.troops.add_basic derthert 50\n" +
                "gm.troops.add_basic player 100");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("partyLeader", true, null, "leader"),
                new ArgumentDefinition("count", true)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError)).Message
;

            if (parsed.TotalCount < 2)
                return CommandResult.Error(usageMessage).Message
;

            // MARK: Parse Arguments
            string leaderArg = parsed.GetArgument("partyLeader", 0) ?? parsed.GetNamed("leader");
            if (leaderArg == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'partyLeader'.")).Message
;

            EntityFinderResult<Hero> heroResult = HeroFinder.FindSingleHero(leaderArg);
            if (!heroResult.IsSuccess)
                return CommandResult.Error(heroResult.Message).Message
;
            Hero hero = heroResult.Entity;

            string countArg = parsed.GetArgument("count", 1);
            if (countArg == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'count'.")).Message
;

            if (!CommandValidator.ValidateIntegerRange(countArg, 1, 10000, out int count, out string countError))
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(countError)).Message
;

            // MARK: Execute Logic
            if (hero.PartyBelongedTo == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage($"{hero.Name} does not belong to a party.")).Message
;

            if (hero.PartyBelongedTo.LeaderHero != hero)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage($"{hero.Name} is not a party leader. They belong to {hero.PartyBelongedTo.LeaderHero.Name}'s party.")).Message
;

            hero.PartyBelongedTo.AddBasicTroops(count);

            string troopName = hero.Culture?.BasicTroop?.Name?.ToString() ?? "basic troops";

            Dictionary<string, string> resolvedValues = new()
            {
                { "partyLeader", hero.Name.ToString() },
                { "count", count.ToString() }
            };

            string argumentDisplay = parsed.FormatArgumentDisplay("gm.troop.add_basic", resolvedValues);
            string fullMessage = argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"Added {count}x {troopName} to {hero.Name}'s party.\n" +
                $"Party: {hero.PartyBelongedTo.Name} (Total size: {hero.PartyBelongedTo.MemberRoster.TotalManCount})");
            return CommandResult.Success(fullMessage).Message
;
        });
    }
}
