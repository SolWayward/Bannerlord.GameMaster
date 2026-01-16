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

public static class GiveXpCommand
{
    /// <summary>
    /// Give XP to all troops in a party leader's party
    /// Usage: gm.troops.give_xp [partyLeader] [xp]
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("give_xp", "gm.troops")]
    public static string GiveXp(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return error;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.troops.give_xp", "<partyLeader> <xp>",
                "Adds the specified experience to all troops in the hero's party.\n" +
                "Supports named arguments: partyLeader:derthert xp:1000",
                "gm.troops.give_xp derthert 1000\n" +
                "gm.troops.give_xp player 500");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("partyLeader", true, null, "leader"),
                new ArgumentDefinition("xp", true)
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

            string xpArg = parsed.GetArgument("xp", 1);
            if (xpArg == null)
                return MessageFormatter.FormatErrorMessage("Missing required argument 'xp'.");

            if (!CommandValidator.ValidateIntegerRange(xpArg, 1, 1000000, out int xp, out string xpError))
                return MessageFormatter.FormatErrorMessage(xpError);

            // MARK: Execute Logic
            if (hero.PartyBelongedTo == null)
                return MessageFormatter.FormatErrorMessage($"{hero.Name} does not belong to a party.");

            if (hero.PartyBelongedTo.LeaderHero != hero)
                return MessageFormatter.FormatErrorMessage($"{hero.Name} is not a party leader. They belong to {hero.PartyBelongedTo.LeaderHero.Name}'s party.");

            // Count non-hero troops before adding XP
            int troopCount = 0;
            foreach (TaleWorlds.CampaignSystem.Roster.TroopRosterElement troop in hero.PartyBelongedTo.MemberRoster.GetTroopRoster())
            {
                if (!troop.Character.IsHero)
                    troopCount++;
            }

            hero.PartyBelongedTo.AddXp(xp);

            Dictionary<string, string> resolvedValues = new()
            {
                { "partyLeader", hero.Name.ToString() },
                { "xp", xp.ToString() }
            };

            string argumentDisplay = parsed.FormatArgumentDisplay("give_xp", resolvedValues);
            return argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"Added {xp} XP to {troopCount} troop types in {hero.Name}'s party.\n" +
                $"Party: {hero.PartyBelongedTo.Name} (Total size: {hero.PartyBelongedTo.MemberRoster.TotalManCount})");
        });
    }
}
