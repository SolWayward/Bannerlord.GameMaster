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

public static class AddMixedTroopsCommand
{
    /// <summary>
    /// Add mixed tier troops (basic, elite, and mercenary) to a party leader's party
    /// Usage: gm.troops.add_mixed [partyLeader] [countOfEach]
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("add_mixed", "gm.troops")]
    public static string AddMixedTroops(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return error;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.troops.add_mixed", "<partyLeader> <countOfEach>",
                "Adds mixed tier troops to the party. The count specifies how many of EACH type (basic, elite, mercenary) to add.\n" +
                "For example, countOfEach=10 will add 30 total troops: 10 basic + 10 elite + 10 mercenary.\n" +
                "Supports named arguments: partyLeader:derthert countOfEach:15",
                "gm.troops.add_mixed derthert 15\n" +
                "gm.troops.add_mixed player 20");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("partyLeader", true, null, "leader"),
                new ArgumentDefinition("countOfEach", true, null, "count")
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

            string countArg = parsed.GetArgument("countOfEach", 1) ?? parsed.GetNamed("count");
            if (countArg == null)
                return MessageFormatter.FormatErrorMessage("Missing required argument 'countOfEach'.");

            if (!CommandValidator.ValidateIntegerRange(countArg, 1, 3000, out int countOfEach, out string countError))
                return MessageFormatter.FormatErrorMessage(countError);

            // MARK: Execute Logic
            if (hero.PartyBelongedTo == null)
                return MessageFormatter.FormatErrorMessage($"{hero.Name} does not belong to a party.");

            if (hero.PartyBelongedTo.LeaderHero != hero)
                return MessageFormatter.FormatErrorMessage($"{hero.Name} is not a party leader. They belong to {hero.PartyBelongedTo.LeaderHero.Name}'s party.");

            hero.PartyBelongedTo.AddMixedTierTroops(countOfEach);

            int totalAdded = countOfEach * 3;

            Dictionary<string, string> resolvedValues = new()
            {
                { "partyLeader", hero.Name.ToString() },
                { "countOfEach", countOfEach.ToString() }
            };

            string argumentDisplay = parsed.FormatArgumentDisplay("add_mixed", resolvedValues);
            return argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"Added {totalAdded} mixed tier troops to {hero.Name}'s party ({countOfEach} of each: basic, elite, mercenary).\n" +
                $"Party: {hero.PartyBelongedTo.Name} (Total size: {hero.PartyBelongedTo.MemberRoster.TotalManCount})");
        });
    }
}
