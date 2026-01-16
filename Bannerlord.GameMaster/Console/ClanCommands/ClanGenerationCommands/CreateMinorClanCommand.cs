using Bannerlord.GameMaster.Clans;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Cultures;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.ClanCommands.ClanGenerationCommands
{
    [CommandLineFunctionality.CommandLineArgumentFunction("clan", "gm")]
    public static class CreateMinorClanCommand
    {
        /// <summary>
        /// Create a minor faction clan (not a noble house)
        /// Usage: gm.clan.create_minor_clan <clanName> [leaderHero] [cultures] [createParty]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("create_minor_clan", "gm.clan")]
        public static string CreateMinorClan(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return error;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.clan.create_minor_clan", "<clanName> [leaderHero] [cultures] [createParty]",
                    "Create a minor faction clan (not a noble house). Useful for mercenary companies or bandit factions.\n" +
                    "Minor clans start at tier 1 with less gold and influence than noble clans.\n" +
                    "- clanName/name: required, name for the minor clan. Use SINGLE QUOTES for multi-word names\n" +
                    "- leaderHero/leader: optional, existing hero ID or name to make leader (creates new hero if omitted)\n" +
                    "- cultures/culture: optional, culture for template selection (default: main_cultures)\n" +
                    "- createParty/party: optional, 'true' or 'false' to create party for leader (default: true)\n" +
                    "Supports named arguments: name:'Mercenary Company' cultures:bandit_cultures party:true",
                    "gm.clan.create_minor_clan 'Mercenary Company'\n" +
                    "gm.clan.create_minor_clan Bandits null bandit_cultures\n" +
                    "gm.clan.create_minor_clan name:'Free Traders' leader:myHero cultures:empire party:false");

                ParsedArguments parsed = ArgumentParser.ParseArguments(args);

                parsed.SetValidArguments(
                    new ArgumentDefinition("clanName", true, null, "name"),
                    new ArgumentDefinition("leaderHero", false, null, "leader"),
                    new ArgumentDefinition("cultures", false, null, "culture"),
                    new ArgumentDefinition("createParty", false, null, "party")
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return MessageFormatter.FormatErrorMessage(validationError);

                if (parsed.TotalCount < 1)
                    return usageMessage;

                // MARK: Parse Arguments
                string clanName = parsed.GetArgument("name", 0) ?? parsed.GetArgument("clanName", 0);
                if (string.IsNullOrWhiteSpace(clanName))
                    return MessageFormatter.FormatErrorMessage("Clan name cannot be empty.");

                Hero leader = null;
                CultureFlags cultureFlags = CultureFlags.AllMainCultures;
                bool createParty = true;

                string leaderArg = parsed.GetArgument("leader", 1) ?? parsed.GetArgument("leaderHero", 1);
                if (leaderArg != null && leaderArg.ToLower() != "null")
                {
                    EntityFinderResult<Hero> heroResult = HeroFinder.FindSingleHero(leaderArg);
                    if (!heroResult.IsSuccess) return heroResult.Message;
                    leader = heroResult.Entity;
                }

                string culturesArg = parsed.GetArgument("cultures", 2) ?? parsed.GetArgument("culture", 2);
                if (culturesArg != null)
                {
                    cultureFlags = FlagParser.ParseCultureArgument(culturesArg);
                    if (cultureFlags == CultureFlags.None)
                        return MessageFormatter.FormatErrorMessage($"Invalid culture(s): '{culturesArg}'");
                }

                string partyArg = parsed.GetArgument("createParty", 3) ?? parsed.GetArgument("party", 3);
                if (partyArg != null)
                {
                    if (!bool.TryParse(partyArg, out createParty))
                        return MessageFormatter.FormatErrorMessage($"Invalid createParty value: '{partyArg}'. Use 'true' or 'false'.");
                }

                if (!CommandValidator.ValidateClanCreationLimit(1, out string clanLimitError))
                    return MessageFormatter.FormatErrorMessage(clanLimitError);

                if (leader == null)
                {
                    if (!CommandValidator.ValidateHeroCreationLimit(1, out string heroLimitError))
                        return MessageFormatter.FormatErrorMessage(heroLimitError);
                }

                // MARK: Execute Logic
                Dictionary<string, string> resolvedValues = new()
                {
                    { "clanName", clanName },
                    { "leaderHero", leader != null ? leader.Name.ToString() : "Auto-generated" },
                    { "cultures", culturesArg ?? "Main Cultures" },
                    { "createParty", createParty.ToString() }
                };

                Clan minorClan = ClanGenerator.CreateMinorClan(clanName, leader, cultureFlags, createParty);

                string leaderInfo = leader != null ? $" with {leader.Name} as leader" : " with auto-generated leader";
                string partyInfo = createParty ? " (with party)" : " (no party)";

                string argumentDisplay = parsed.FormatArgumentDisplay("create_minor_clan", resolvedValues);
                return argumentDisplay + MessageFormatter.FormatSuccessMessage(
                    $"Created minor clan '{minorClan.Name}'{leaderInfo}{partyInfo}.\n" +
                    $"Leader: {minorClan.Leader.Name} (ID: {minorClan.Leader.StringId})\n" +
                    $"Culture: {minorClan.Culture.Name}\n" +
                    $"Clan ID: {minorClan.StringId}\n" +
                    $"Type: Minor Faction (Tier {minorClan.Tier})");
            });
        }
    }
}
