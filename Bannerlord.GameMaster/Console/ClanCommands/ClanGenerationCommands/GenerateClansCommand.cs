using Bannerlord.GameMaster.Console.Common;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using Bannerlord.GameMaster.Clans;
using Bannerlord.GameMaster.Cultures;

namespace Bannerlord.GameMaster.Console.ClanCommands.ClanGenerationCommands
{
    [CommandLineFunctionality.CommandLineArgumentFunction("clan", "gm")]
    public static class GenerateClansCommand
    {
        /// <summary>
        /// Generate multiple clans at once with random names from culture lists
        /// Usage: gm.clan.generate_clans <count> [cultures] [kingdom] [createParties] [companionCount]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("generate_clans", "gm.clan")]
        public static string GenerateClans(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignState(out string error))
                    return error;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.clan.generate_clans", "<count> [cultures] [kingdom] [createParties] [companionCount]",
                    "Generate multiple clans with random names from culture lists. If no culture specified, uses main_cultures.\n" +
                    "- count: required, number of clans to generate (1-50)\n" +
                    "- cultures/culture: optional, defines the pool of cultures. Use commas with no spaces for multiple cultures\n" +
                    "- kingdom: optional, kingdom for all generated clans to join (independent if omitted)\n" +
                    "- createParties/parties: optional, 'true' or 'false' to create parties for leaders (default: true)\n" +
                    "- companionCount/companions: optional, number of companions per clan (0-10, default: 2)\n" +
                    "Supports named arguments: count:5 cultures:vlandia,battania kingdom:empire parties:true companions:3",
                    "gm.clan.generate_clans 5\n" +
                    "gm.clan.generate_clans 10 vlandia,battania\n" +
                    "gm.clan.generate_clans 3 main_cultures empire\n" +
                    "gm.clan.generate_clans count:10 cultures:battania,sturgia kingdom:sturgia\n" +
                    "gm.clan.generate_clans 3 empire null false 0");

                // Parse arguments with named argument support
                var parsedArgs = CommandBase.ParseArguments(args);

                // Define valid arguments
                parsedArgs.SetValidArguments(
                    new CommandBase.ArgumentDefinition("count", true),
                    new CommandBase.ArgumentDefinition("cultures", false, null, "culture"),
                    new CommandBase.ArgumentDefinition("kingdom", false),
                    new CommandBase.ArgumentDefinition("createParties", false, null, "parties"),
                    new CommandBase.ArgumentDefinition("companionCount", false, null, "companions")
                );

                // Validate
                string validationError = parsedArgs.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                if (parsedArgs.TotalCount < 1)
                    return usageMessage;

                // Parse count (required)
                string countArg = parsedArgs.GetArgument("count", 0);
                if (countArg == null)
                    return CommandBase.FormatErrorMessage("Missing required argument 'count'.");

                if (!CommandValidator.ValidateIntegerRange(countArg, 1, 50, out int count, out string countError))
                    return CommandBase.FormatErrorMessage(countError);

                // Parse optional cultures - supports 'cultures' or 'culture'
                CultureFlags cultureFlags = CultureFlags.AllMainCultures;
                string culturesArg = parsedArgs.GetArgument("cultures", 1) ?? parsedArgs.GetArgument("culture", 1);
                if (culturesArg != null)
                {
                    cultureFlags = FlagParser.ParseCultureArgument(culturesArg);
                    if (cultureFlags == CultureFlags.None)
                        return CommandBase.FormatErrorMessage($"Invalid culture(s): '{culturesArg}'. Use culture names (e.g., vlandia,battania) or groups (main_cultures, bandit_cultures, all_cultures)");
                }

                // Parse optional kingdom
                Kingdom kingdom = null;
                string kingdomArg = parsedArgs.GetArgument("kingdom", 2);
                if (kingdomArg != null && kingdomArg.ToLower() != "null")
                {
                    var (kingdomResult, kingdomError) = CommandBase.FindSingleKingdom(kingdomArg);
                    if (kingdomError != null) return kingdomError;
                    kingdom = kingdomResult;
                }

                // Parse optional createParties - supports 'createParties' or 'parties'
                bool createParties = true;
                string partiesArg = parsedArgs.GetArgument("createParties", 3) ?? parsedArgs.GetArgument("parties", 3);
                if (partiesArg != null)
                {
                    if (!bool.TryParse(partiesArg, out createParties))
                        return CommandBase.FormatErrorMessage($"Invalid createParties value: '{partiesArg}'. Use 'true' or 'false'.");
                }

                // Parse optional companionCount - supports 'companionCount' or 'companions'
                int companionCount = 2;
                string companionsArg = parsedArgs.GetArgument("companionCount", 4) ?? parsedArgs.GetArgument("companions", 4);
                if (companionsArg != null)
                {
                    if (!CommandValidator.ValidateIntegerRange(companionsArg, 0, 10, out companionCount, out string compCountError))
                        return CommandBase.FormatErrorMessage(compCountError);
                }

                // Validate limits - each clan creates 1 leader + companions
                if (!CommandValidator.ValidateClanCreationLimit(count, out string clanLimitError))
                    return CommandBase.FormatErrorMessage(clanLimitError);

                int heroesPerClan = 1 + companionCount; // 1 leader + companions
                int totalHeroesToCreate = count * heroesPerClan;
                if (!CommandValidator.ValidateHeroCreationLimit(totalHeroesToCreate, out string heroLimitError))
                    return CommandBase.FormatErrorMessage(heroLimitError);

                // Build resolved values dictionary
                var resolvedValues = new Dictionary<string, string>
                {
                    { "count", count.ToString() },
                    { "cultures", culturesArg ?? "Main Cultures" },
                    { "kingdom", kingdom != null ? kingdom.Name.ToString() : "Independent" },
                    { "createParties", createParties.ToString() },
                    { "companionCount", companionCount.ToString() }
                };

                // Display argument header
                string argumentDisplay = parsedArgs.FormatArgumentDisplay("generate_clans", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    List<Clan> clans = ClanGenerator.GenerateClans(count, cultureFlags, kingdom, createParties, companionCount);

                    if (clans == null || clans.Count == 0)
                        return argumentDisplay + CommandBase.FormatErrorMessage("Failed to generate clans - no clans created");

                    string kingdomInfo = kingdom != null ? $" and joined {kingdom.Name}" : " as independent";
                    string partyInfo = createParties ? " (with parties)" : " (no parties)";
                    string companionInfo = companionCount > 0 ? $" and {companionCount} companions each" : "";

                    return argumentDisplay + CommandBase.FormatSuccessMessage(
                        $"Generated {clans.Count} clan(s){kingdomInfo}{partyInfo}{companionInfo}:\n" +
                        ClanQueries.GetFormattedDetails(clans));
                }, "Failed to generate clans");
            });
        }
    }
}