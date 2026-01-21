using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Cultures;
using Bannerlord.GameMaster.Kingdoms;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.KingdomCommands.KingdomGenerationCommands;

[CommandLineFunctionality.CommandLineArgumentFunction("kingdom", "gm")]
public static class CreateKingdomCommand
{
    /// <summary>
    /// Create a new kingdom with a specified settlement as capital
    /// Usage: gm.kingdom.create_kingdom &lt;settlement&gt; [kingdomName] [clanName] [vassalCount] [cultures]
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("create_kingdom", "gm.kingdom")]
    public static string CreateKingdom(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return error;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.kingdom.create_kingdom", "<settlement> [kingdomName] [clanName] [vassalCount] [cultures]",
                "Creates a new kingdom with the specified settlement as capital. A ruling clan is generated if not specified.\n" +
                "- settlement: required, settlement ID or name to become the kingdom capital (must be a city or castle)\n" +
                "- kingdomName/name: optional, name for the kingdom. Defaults to random name from culture\n" +
                "- clanName/clan: optional, name of the ruling clan. Defaults to random name from culture\n" +
                "- vassalCount/vassals: optional, number of vassal clans to create (0-10, default: 4)\n" +
                "- cultures/culture: optional, culture pool for kingdom and clans. Defaults to main_cultures\n" +
                "Supports named arguments: settlement:pen name:'New Empire' clan:'House Stark' vassals:5 cultures:empire",
                "gm.kingdom.create_kingdom pen\n" +
                "gm.kingdom.create_kingdom pen 'Northern Kingdom'\n" +
                "gm.kingdom.create_kingdom pen 'Empire of the North' 'House Stark' 6\n" +
                "gm.kingdom.create_kingdom settlement:zeonica name:'Desert Kingdom' clan:'Nomad Tribe' vassals:3 cultures:aserai");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("settlement", true),
                new ArgumentDefinition("kingdomName", false, null, "name"),
                new ArgumentDefinition("clanName", false, null, "clan"),
                new ArgumentDefinition("vassalCount", false, null, "vassals"),
                new ArgumentDefinition("cultures", false, null, "culture")
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return MessageFormatter.FormatErrorMessage(validationError);

            if (parsed.TotalCount < 1)
                return usageMessage;

            // MARK: Parse Arguments
            string settlementArg = parsed.GetArgument("settlement", 0);
            if (settlementArg == null)
                return MessageFormatter.FormatErrorMessage("Missing required argument 'settlement'.");

            EntityFinderResult<Settlement> settlementResult = SettlementFinder.FindSingleSettlement(settlementArg);
            if (!settlementResult.IsSuccess)
                return settlementResult.Message;
            Settlement settlement = settlementResult.Entity;

            // Validate settlement type
            if (!settlement.IsTown && !settlement.IsCastle)
                return MessageFormatter.FormatErrorMessage($"Settlement '{settlement.Name}' must be a city or castle to become a kingdom capital.");

            if (settlement.Town == null)
                return MessageFormatter.FormatErrorMessage($"Settlement '{settlement.Name}' has no town component.");

            // Ensure settlement is not owned by player
            if (settlement.OwnerClan == Clan.PlayerClan)
                return MessageFormatter.FormatErrorMessage($"Settlement '{settlement.Name}' is owned by the player. Cannot use player settlements for kingdom creation.");

            // Parse optional kingdomName
            string kingdomName = parsed.GetArgument("kingdomName", 1) ?? parsed.GetNamed("name");
            if (kingdomName != null && kingdomName.ToLower() == "null")
                kingdomName = null;

            // Parse optional clanName
            string clanName = parsed.GetArgument("clanName", 2) ?? parsed.GetNamed("clan");
            if (clanName != null && clanName.ToLower() == "null")
                clanName = null;

            // Parse optional vassalCount
            int vassalCount = 4;
            string vassalCountArg = parsed.GetArgument("vassalCount", 3) ?? parsed.GetNamed("vassals");
            if (vassalCountArg != null)
            {
                if (!CommandValidator.ValidateIntegerRange(vassalCountArg, 0, 10, out vassalCount, out string vassalError))
                    return MessageFormatter.FormatErrorMessage(vassalError);
            }

            // Parse optional cultures
            CultureFlags cultureFlags = CultureFlags.AllMainCultures;
            string culturesArg = parsed.GetArgument("cultures", 4) ?? parsed.GetNamed("culture");
            if (culturesArg != null)
            {
                cultureFlags = FlagParser.ParseCultureArgument(culturesArg);
                if (cultureFlags == CultureFlags.None)
                    return MessageFormatter.FormatErrorMessage($"Invalid culture(s): '{culturesArg}'. Use culture names (e.g., vlandia,battania) or groups (main_cultures, bandit_cultures, all_cultures)");
            }

            // Validate limits - creating 1 kingdom, 1 ruling clan + vassalCount clans, and heroes for each clan
            if (!CommandValidator.ValidateKingdomCreationLimit(1, out string kingdomLimitError))
                return MessageFormatter.FormatErrorMessage(kingdomLimitError);

            int totalClans = 1 + vassalCount; // 1 ruling clan + vassals
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
                { "settlement", settlement.Name.ToString() },
                { "kingdomName", kingdomName ?? "Random" },
                { "clanName", clanName ?? "Random" },
                { "vassalCount", vassalCount.ToString() },
                { "cultures", culturesArg ?? "Main Cultures" }
            };

            Kingdom kingdom = KingdomGenerator.CreateKingdom(
                homeSettlement: settlement,
                vassalClanCount: vassalCount,
                name: kingdomName,
                rulingClanName: clanName,
                cultureFlags: cultureFlags
            );

            if (kingdom == null)
            {
                string argumentDisplayError = parsed.FormatArgumentDisplay("create_kingdom", resolvedValues);
                return argumentDisplayError + MessageFormatter.FormatErrorMessage("Failed to create kingdom - settlement could not be resolved or assigned.");
            }

            string argumentDisplay = parsed.FormatArgumentDisplay("create_kingdom", resolvedValues);
            return argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"Created kingdom '{kingdom.Name}' (ID: {kingdom.StringId}):\n" +
                $"Capital: {settlement.Name}\n" +
                $"Ruling Clan: {kingdom.RulingClan.Name}\n" +
                $"Ruler: {kingdom.Leader.Name}\n" +
                $"Culture: {kingdom.Culture.Name}\n" +
                $"Vassal Clans: {vassalCount}\n" +
                $"Total Clans: {kingdom.Clans.Count}");
        });
    }
}
