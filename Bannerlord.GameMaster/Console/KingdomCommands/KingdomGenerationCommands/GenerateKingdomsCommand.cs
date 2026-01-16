using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Cultures;
using Bannerlord.GameMaster.Kingdoms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.KingdomCommands.KingdomGenerationCommands;

public static class GenerateKingdomsCommand
{
    /// <summary>
    /// Generate multiple kingdoms by taking settlements from existing kingdoms
    /// Usage: gm.kingdom.generate_kingdoms &lt;count&gt; [vassalCount] [cultures]
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("generate_kingdoms", "gm.kingdom")]
    public static string GenerateKingdoms(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return error;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.kingdom.generate_kingdoms", "<count> [vassalCount] [cultures]",
                "Generates multiple kingdoms by taking settlements from existing kingdoms.\n" +
                "Alternates between kingdoms evenly, ensuring not to take a kingdom's last settlement.\n" +
                "Will not take settlements from the player's kingdom.\n" +
                "- count: required, number of kingdoms to generate (1-5)\n" +
                "- vassalCount/vassals: optional, number of vassal clans per kingdom (0-10, default: 4)\n" +
                "- cultures/culture: optional, culture pool for kingdoms and clans. Defaults to main_cultures\n" +
                "Supports named arguments: count:5 vassals:3 cultures:vlandia,battania",
                "gm.kingdom.generate_kingdoms 3\n" +
                "gm.kingdom.generate_kingdoms 5 6\n" +
                "gm.kingdom.generate_kingdoms count:2 vassals:4 cultures:empire,aserai");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("count", true),
                new ArgumentDefinition("vassalCount", false, null, "vassals"),
                new ArgumentDefinition("cultures", false, null, "culture")
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

            if (!CommandValidator.ValidateIntegerRange(countArg, 1, 5, out int count, out string countError))
                return MessageFormatter.FormatErrorMessage(countError);

            // Parse optional vassalCount
            int vassalCount = 4;
            string vassalCountArg = parsed.GetArgument("vassalCount", 1) ?? parsed.GetNamed("vassals");
            if (vassalCountArg != null)
            {
                if (!CommandValidator.ValidateIntegerRange(vassalCountArg, 0, 10, out vassalCount, out string vassalError))
                    return MessageFormatter.FormatErrorMessage(vassalError);
            }

            // Parse optional cultures
            CultureFlags cultureFlags = CultureFlags.AllMainCultures;
            string culturesArg = parsed.GetArgument("cultures", 2) ?? parsed.GetNamed("culture");
            if (culturesArg != null)
            {
                cultureFlags = FlagParser.ParseCultureArgument(culturesArg);
                if (cultureFlags == CultureFlags.None)
                    return MessageFormatter.FormatErrorMessage($"Invalid culture(s): '{culturesArg}'. Use culture names (e.g., vlandia,battania) or groups (main_cultures, bandit_cultures, all_cultures)");
            }

            // Validate limits - each kingdom creates 1 ruling clan + vassalCount clans, and heroes for each clan
            if (!CommandValidator.ValidateKingdomCreationLimit(count, out string kingdomLimitError))
                return MessageFormatter.FormatErrorMessage(kingdomLimitError);

            int clansPerKingdom = 1 + vassalCount; // 1 ruling clan + vassals
            int totalClans = count * clansPerKingdom;
            if (!CommandValidator.ValidateClanCreationLimit(totalClans, out string clanLimitError))
                return MessageFormatter.FormatErrorMessage(clanLimitError);

            // Each clan creates 1 leader + 2 companions by default (estimate)
            int heroesPerClan = 3;
            int totalHeroesToCreate = totalClans * heroesPerClan;
            if (!CommandValidator.ValidateHeroCreationLimit(totalHeroesToCreate, out string heroLimitError))
                return MessageFormatter.FormatErrorMessage(heroLimitError);

            // MARK: Execute Logic
            Dictionary<string, string> resolvedValues = new()
            {
                { "count", count.ToString() },
                { "vassalCount", vassalCount.ToString() },
                { "cultures", culturesArg ?? "Main Cultures" }
            };

            List<Kingdom> createdKingdoms = KingdomGenerator.GenerateKingdoms(count, vassalCount, cultureFlags);

            string argumentDisplay = parsed.FormatArgumentDisplay("generate_kingdoms", resolvedValues);

            if (createdKingdoms == null || createdKingdoms.Count == 0)
                return argumentDisplay + MessageFormatter.FormatErrorMessage("Failed to generate kingdoms - no suitable settlements available or all kingdoms exhausted.");

            // Build detailed output with settlement names
            StringBuilder detailsBuilder = new();
            detailsBuilder.AppendLine($"Successfully created {createdKingdoms.Count} kingdom(s):");

            foreach (Kingdom kingdom in createdKingdoms)
            {
                // Get the first town or castle settlement as the capital
                TaleWorlds.CampaignSystem.Settlements.Settlement capital = kingdom.Settlements.FirstOrDefault(s => s.IsTown || s.IsCastle);
                string capitalName = capital?.Name?.ToString() ?? "Unknown";
                detailsBuilder.AppendLine($"  - {kingdom.Name} (Capital: {capitalName})");
            }

            detailsBuilder.AppendLine();
            detailsBuilder.Append(KingdomQueries.GetFormattedDetails(createdKingdoms));

            if (createdKingdoms.Count < count)
            {
                detailsBuilder.AppendLine($"\nWarning: Only {createdKingdoms.Count} of {count} requested kingdoms were created. " +
                    "No more suitable settlements available.");
            }

            return argumentDisplay + MessageFormatter.FormatSuccessMessage(detailsBuilder.ToString());
        });
    }
}
