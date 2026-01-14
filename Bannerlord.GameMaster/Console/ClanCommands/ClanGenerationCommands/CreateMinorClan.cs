using Bannerlord.GameMaster.Console.Common;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using Bannerlord.GameMaster.Clans;
using Bannerlord.GameMaster.Cultures;

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
                if (!CommandBase.ValidateCampaignState(out string error))
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

                // Parse arguments with named argument support
                var parsedArgs = CommandBase.ParseArguments(args);

                // Define valid arguments
                parsedArgs.SetValidArguments(
                    new CommandBase.ArgumentDefinition("clanName", true, null, "name"),
                    new CommandBase.ArgumentDefinition("leaderHero", false, null, "leader"),
                    new CommandBase.ArgumentDefinition("cultures", false, null, "culture"),
                    new CommandBase.ArgumentDefinition("createParty", false, null, "party")
                );

                // Validate
                string validationError = parsedArgs.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                if (parsedArgs.TotalCount < 1)
                    return usageMessage;

                // Get clan name (required)
                string clanName = parsedArgs.GetArgument("name", 0) ?? parsedArgs.GetArgument("clanName", 0);
                if (string.IsNullOrWhiteSpace(clanName))
                    return CommandBase.FormatErrorMessage("Clan name cannot be empty.");

                Hero leader = null;
                CultureFlags cultureFlags = CultureFlags.AllMainCultures;
                bool createParty = true;

                // Parse optional leader
                string leaderArg = parsedArgs.GetArgument("leader", 1) ?? parsedArgs.GetArgument("leaderHero", 1);
                if (leaderArg != null && leaderArg.ToLower() != "null")
                {
                    var (hero, heroError) = CommandBase.FindSingleHero(leaderArg);
                    if (heroError != null) return heroError;
                    leader = hero;
                }

                // Parse optional cultures
                string culturesArg = parsedArgs.GetArgument("cultures", 2) ?? parsedArgs.GetArgument("culture", 2);
                if (culturesArg != null)
                {
                    cultureFlags = FlagParser.ParseCultureArgument(culturesArg);
                    if (cultureFlags == CultureFlags.None)
                        return CommandBase.FormatErrorMessage($"Invalid culture(s): '{culturesArg}'");
                }

                // Parse optional createParty
                string partyArg = parsedArgs.GetArgument("createParty", 3) ?? parsedArgs.GetArgument("party", 3);
                if (partyArg != null)
                {
                    if (!bool.TryParse(partyArg, out createParty))
                        return CommandBase.FormatErrorMessage($"Invalid createParty value: '{partyArg}'. Use 'true' or 'false'.");
                }

                // Validate limits - creating 1 clan and potentially 1 hero if no leader specified
                if (!CommandValidator.ValidateClanCreationLimit(1, out string clanLimitError))
                    return CommandBase.FormatErrorMessage(clanLimitError);

                if (leader == null)
                {
                    if (!CommandValidator.ValidateHeroCreationLimit(1, out string heroLimitError))
                        return CommandBase.FormatErrorMessage(heroLimitError);
                }

                // Build resolved values dictionary
                var resolvedValues = new Dictionary<string, string>
                {
                    { "clanName", clanName },
                    { "leaderHero", leader != null ? leader.Name.ToString() : "Auto-generated" },
                    { "cultures", culturesArg ?? "Main Cultures" },
                    { "createParty", createParty.ToString() }
                };

                // Display argument header
                string argumentDisplay = parsedArgs.FormatArgumentDisplay("create_minor_clan", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    Clan minorClan = ClanGenerator.CreateMinorClan(clanName, leader, cultureFlags, createParty);

                    string leaderInfo = leader != null ? $" with {leader.Name} as leader" : " with auto-generated leader";
                    string partyInfo = createParty ? " (with party)" : " (no party)";

                    return argumentDisplay + CommandBase.FormatSuccessMessage(
                        $"Created minor clan '{minorClan.Name}'{leaderInfo}{partyInfo}.\n" +
                        $"Leader: {minorClan.Leader.Name} (ID: {minorClan.Leader.StringId})\n" +
                        $"Culture: {minorClan.Culture.Name}\n" +
                        $"Clan ID: {minorClan.StringId}\n" +
                        $"Type: Minor Faction (Tier {minorClan.Tier})");
                }, "Failed to create minor clan");
            });
        }
    }
}