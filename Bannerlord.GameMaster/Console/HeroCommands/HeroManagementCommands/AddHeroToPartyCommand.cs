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

namespace Bannerlord.GameMaster.Console.HeroCommands.HeroManagementCommands;

/// <summary>
/// Add a hero to another hero's party. Hero leaves their current party if already in one.
/// Usage: gm.hero.add_hero_to_party [hero] [partyLeader]
/// </summary>
public static class AddHeroToPartyCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("add_hero_to_party", "gm.hero")]
    public static string AddHeroToParty(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Message;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.hero.add_hero_to_party", "<hero> <partyLeader>",
                "Adds a hero as a companion to another hero's party. The hero will leave their current party if they are already in one.\n" +
                "The hero's clan will be changed to match the party leader's clan.\n" +
                "Supports named arguments: hero:companion_1 partyLeader:player",
                "gm.hero.add_hero_to_party companion_1 player\n" +
                "gm.hero.add_hero_to_party wanderer_1 derthert");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("hero", true),
                new ArgumentDefinition("partyLeader", true, null, "leader")
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError)).Message;

            if (parsed.TotalCount < 2)
                return CommandResult.Error(usageMessage).Message;

            // MARK: Parse Arguments
            string heroArg = parsed.GetArgument("hero", 0);
            if (heroArg == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'hero'.")).Message;

            EntityFinderResult<Hero> heroResult = HeroFinder.FindSingleHero(heroArg);
            if (!heroResult.IsSuccess) return CommandResult.Error(heroResult.Message).Message;
            Hero hero = heroResult.Entity;

            string leaderArg = parsed.GetArgument("partyLeader", 1) ?? parsed.GetNamed("leader");
            if (leaderArg == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'partyLeader'.")).Message;

            EntityFinderResult<Hero> leaderResult = HeroFinder.FindSingleHero(leaderArg);
            if (!leaderResult.IsSuccess) return CommandResult.Error(leaderResult.Message).Message;
            Hero partyLeader = leaderResult.Entity;

            // MARK: Execute Logic
            Dictionary<string, string> resolvedValues = new()
            {
                { "hero", hero.Name.ToString() },
                { "partyLeader", partyLeader.Name.ToString() }
            };
            string argumentDisplay = parsed.FormatArgumentDisplay("gm.hero.add_hero_to_party", resolvedValues);

            // Validate party leader has a party
            if (partyLeader.PartyBelongedTo == null)
                return CommandResult.Error(argumentDisplay + MessageFormatter.FormatErrorMessage($"{partyLeader.Name} does not have a party.")).Message;

            if (partyLeader.PartyBelongedTo.LeaderHero != partyLeader)
                return CommandResult.Error(argumentDisplay + MessageFormatter.FormatErrorMessage($"{partyLeader.Name} is not a party leader.")).Message;

            // Check if hero is trying to join their own party
            if (hero == partyLeader)
                return CommandResult.Error(argumentDisplay + MessageFormatter.FormatErrorMessage("Cannot add a hero to their own party.")).Message;

            string previousPartyInfo = "None";
            
            // Remove hero from current party if they are in one
            if (hero.PartyBelongedTo != null)
            {
                previousPartyInfo = hero.PartyBelongedTo.Name?.ToString() ?? "Unknown";
                
                // If hero is a party leader, we need to disband their party first
                if (hero.PartyBelongedTo.LeaderHero == hero)
                {
                    // Disband the party (this is complex, so we'll just prevent it for now)
                    return CommandResult.Error(argumentDisplay + MessageFormatter.FormatErrorMessage(
                        $"{hero.Name} is currently leading their own party ({hero.PartyBelongedTo.Name}). " +
                        "Party leaders must disband their party before joining another. This is not yet implemented.")).Message;
                }
                
                // Remove hero from their current party roster
                hero.PartyBelongedTo.MemberRoster.RemoveTroop(hero.CharacterObject);
            }

            if (hero.Occupation == Occupation.Wanderer)
                partyLeader.PartyBelongedTo.AddCompanionToParty(hero);
            else
                partyLeader.PartyBelongedTo.AddLordToParty(hero);

            string fullMessage = argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"{hero.Name} has joined {partyLeader.Name}'s party.\n" +
                $"Previous party: {previousPartyInfo}\n" +
                $"New party: {partyLeader.PartyBelongedTo.Name}\n" +
                $"Clan updated to: {hero.Clan?.Name}");
            return CommandResult.Success(fullMessage).Message;
        });
    }
}
