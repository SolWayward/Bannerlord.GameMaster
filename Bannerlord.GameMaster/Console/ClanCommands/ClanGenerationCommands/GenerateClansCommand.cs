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
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
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

                ParsedArguments parsed = ArgumentParser.ParseArguments(args);

                parsed.SetValidArguments(
                    new ArgumentDefinition("count", true),
                    new ArgumentDefinition("cultures", false, null, "culture"),
                    new ArgumentDefinition("kingdom", false),
                    new ArgumentDefinition("createParties", false, null, "parties"),
                    new ArgumentDefinition("companionCount", false, null, "companions")
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return MessageFormatter.FormatErrorMessage(validationError);

                if (parsed.TotalCount < 1)
                    return usageMessage;

                // MARK: Parse Arguments
                string countArg = parsed.GetArgument("count", 0);
                if (countArg == null)
                    return MessageFormatter.FormatErrorMessage("Missing required argument 'count'.");

                if (!CommandValidator.ValidateIntegerRange(countArg, 1, 50, out int count, out string countError))
                    return MessageFormatter.FormatErrorMessage(countError);

                CultureFlags cultureFlags = CultureFlags.AllMainCultures;
                string culturesArg = parsed.GetArgument("cultures", 1) ?? parsed.GetArgument("culture", 1);
                if (culturesArg != null)
                {
                    cultureFlags = FlagParser.ParseCultureArgument(culturesArg);
                    if (cultureFlags == CultureFlags.None)
                        return MessageFormatter.FormatErrorMessage($"Invalid culture(s): '{culturesArg}'. Use culture names (e.g., vlandia,battania) or groups (main_cultures, bandit_cultures, all_cultures)");
                }

                Kingdom kingdom = null;
                string kingdomArg = parsed.GetArgument("kingdom", 2);
                if (kingdomArg != null && kingdomArg.ToLower() != "null")
                {
                    EntityFinderResult<Kingdom> kingdomResult = KingdomFinder.FindSingleKingdom(kingdomArg);
                    if (!kingdomResult.IsSuccess) return kingdomResult.Message;
                    kingdom = kingdomResult.Entity;
                }

                bool createParties = true;
                string partiesArg = parsed.GetArgument("createParties", 3) ?? parsed.GetArgument("parties", 3);
                if (partiesArg != null)
                {
                    if (!bool.TryParse(partiesArg, out createParties))
                        return MessageFormatter.FormatErrorMessage($"Invalid createParties value: '{partiesArg}'. Use 'true' or 'false'.");
                }

                int companionCount = 2;
                string companionsArg = parsed.GetArgument("companionCount", 4) ?? parsed.GetArgument("companions", 4);
                if (companionsArg != null)
                {
                    if (!CommandValidator.ValidateIntegerRange(companionsArg, 0, 10, out companionCount, out string compCountError))
                        return MessageFormatter.FormatErrorMessage(compCountError);
                }

                if (!CommandValidator.ValidateClanCreationLimit(count, out string clanLimitError))
                    return MessageFormatter.FormatErrorMessage(clanLimitError);

                int heroesPerClan = 1 + companionCount;
                int totalHeroesToCreate = count * heroesPerClan;
                if (!CommandValidator.ValidateHeroCreationLimit(totalHeroesToCreate, out string heroLimitError))
                    return MessageFormatter.FormatErrorMessage(heroLimitError);

                // MARK: Execute Logic
                Dictionary<string, string> resolvedValues = new()
                {
                    { "count", count.ToString() },
                    { "cultures", culturesArg ?? "Main Cultures" },
                    { "kingdom", kingdom != null ? kingdom.Name.ToString() : "Independent" },
                    { "createParties", createParties.ToString() },
                    { "companionCount", companionCount.ToString() }
                };

                List<Clan> clans = ClanGenerator.GenerateClans(count, cultureFlags, kingdom, createParties, companionCount);

                if (clans == null || clans.Count == 0)
                {
                    string argumentDisplayError = parsed.FormatArgumentDisplay("generate_clans", resolvedValues);
                    return argumentDisplayError + MessageFormatter.FormatErrorMessage("Failed to generate clans - no clans created");
                }

                string kingdomInfo = kingdom != null ? $" and joined {kingdom.Name}" : " as independent";
                string partyInfo = createParties ? " (with parties)" : " (no parties)";
                string companionInfo = companionCount > 0 ? $" and {companionCount} companions each" : "";

                string argumentDisplay = parsed.FormatArgumentDisplay("generate_clans", resolvedValues);
                return argumentDisplay + MessageFormatter.FormatSuccessMessage(
                    $"Generated {clans.Count} clan(s){kingdomInfo}{partyInfo}{companionInfo}:\n" +
                    ClanQueries.GetFormattedDetails(clans));
            });
        }
    }
}
