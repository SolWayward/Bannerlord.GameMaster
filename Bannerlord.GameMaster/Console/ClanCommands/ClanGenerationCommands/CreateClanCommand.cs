using Bannerlord.GameMaster.Clans;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.ClanCommands.ClanGenerationCommands
{
    [CommandLineFunctionality.CommandLineArgumentFunction("clan", "gm")]
    public static class CreateClanCommand
    {
        /// <summary>
		/// Create a clan with the specified name. Optionally set a hero as leader and assign to kingdom.
		/// Usage: gm.clan.create_clan &lt;clanName&gt; [leaderHero] [kingdom] [createParty] [companionCount]
		/// </summary>
		[CommandLineFunctionality.CommandLineArgumentFunction("create_clan", "gm.clan")]
        public static string CreateClan(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return error;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.clan.create_clan", "<clanName> [leaderHero] [kingdom] [createParty] [companionCount]",
                    "Create a new clan with the specified name. If no leader is specified, a new hero will be generated.\n" +
                    "Optionally, specify a kingdom for clan, if no kingdom specified, clan is independent.\n" +
                    "- clanName/name: required, name for the clan. Use SINGLE QUOTES for multi-word names\n" +
                    "- leaderHero/leader: optional, existing hero ID or name to make leader (creates new hero if omitted)\n" +
                    "- kingdom: optional, kingdom ID or name for clan to join (independent if omitted)\n" +
                    "- createParty/party: optional, 'true' or 'false' to create party for leader (default: true)\n" +
                    "- companionCount/companions: optional, number of companions to add (0-10, default: 2)\n" +
                    "Supports named arguments: name:'The Highland Clan' leader:derthert kingdom:empire party:true companions:5",
                    "gm.clan.create_clan Highlanders\n" +
                    "gm.clan.create_clan 'The Highland Clan' derthert\n" +
                    "gm.clan.create_clan NewClan myHero empire\n" +
                    "gm.clan.create_clan name:'House Stark' kingdom:sturgia party:true companions:5\n" +
                    "gm.clan.create_clan TradingFamily null null false 0");

                ParsedArguments parsed = ArgumentParser.ParseArguments(args);

                parsed.SetValidArguments(
                    new ArgumentDefinition("clanName", true, null, "name"),
                    new ArgumentDefinition("leaderHero", false, null, "leader"),
                    new ArgumentDefinition("kingdom", false),
                    new ArgumentDefinition("createParty", false, null, "party"),
                    new ArgumentDefinition("companionCount", false, null, "companions")
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
                Kingdom kingdom = null;
                bool createParty = true;
                int companionCount = 2;

                string leaderArg = parsed.GetArgument("leader", 1) ?? parsed.GetArgument("leaderHero", 1);
                if (leaderArg != null && leaderArg.ToLower() != "null")
                {
                    EntityFinderResult<Hero> heroResult = HeroFinder.FindSingleHero(leaderArg);
                    if (!heroResult.IsSuccess) return heroResult.Message;
                    leader = heroResult.Entity;
                }

                string kingdomArg = parsed.GetArgument("kingdom", 2);
                if (kingdomArg != null && kingdomArg.ToLower() != "null")
                {
                    EntityFinderResult<Kingdom> kingdomResult = KingdomFinder.FindSingleKingdom(kingdomArg);
                    if (!kingdomResult.IsSuccess) return kingdomResult.Message;
                    kingdom = kingdomResult.Entity;
                }

                string partyArg = parsed.GetArgument("createParty", 3) ?? parsed.GetArgument("party", 3);
                if (partyArg != null)
                {
                    if (!bool.TryParse(partyArg, out createParty))
                        return MessageFormatter.FormatErrorMessage($"Invalid createParty value: '{partyArg}'. Use 'true' or 'false'.");
                }

                string companionsArg = parsed.GetArgument("companionCount", 4) ?? parsed.GetArgument("companions", 4);
                if (companionsArg != null)
                {
                    if (!CommandValidator.ValidateIntegerRange(companionsArg, 0, 10, out companionCount, out string countError))
                        return MessageFormatter.FormatErrorMessage(countError);
                }

                if (!CommandValidator.ValidateClanCreationLimit(1, out string clanLimitError))
                    return MessageFormatter.FormatErrorMessage(clanLimitError);

                if (leader == null)
                {
                    int heroesToCreate = 1 + companionCount;
                    if (!CommandValidator.ValidateHeroCreationLimit(heroesToCreate, out string heroLimitError))
                        return MessageFormatter.FormatErrorMessage(heroLimitError);
                }

                // MARK: Execute Logic
                Dictionary<string, string> resolvedValues = new()
                {
                    { "clanName", clanName },
                    { "leaderHero", leader != null ? leader.Name.ToString() : "Auto-generated" },
                    { "kingdom", kingdom != null ? kingdom.Name.ToString() : "Independent" },
                    { "createParty", createParty.ToString() },
                    { "companionCount", companionCount.ToString() }
                };

                Clan newClan = ClanGenerator.CreateNobleClan(clanName, leader, kingdom, createParty, companionCount);

                string leaderInfo = leader != null ? $" with {leader.Name} as leader" : " with auto-generated leader";
                string kingdomInfo = kingdom != null ? $" and joined {kingdom.Name}" : " as independent";
                string partyInfo = createParty ? " (with party)" : " (no party)";
                string companionInfo = companionCount > 0 ? $" and {companionCount} companions" : "";

                string argumentDisplay = parsed.FormatArgumentDisplay("gm.clan.create_clan", resolvedValues);
                return argumentDisplay + MessageFormatter.FormatSuccessMessage(
                    $"Created clan '{newClan.Name}'{leaderInfo}{kingdomInfo}{partyInfo}{companionInfo}.\n" +
                    $"Leader: {newClan.Leader.Name} (ID: {newClan.Leader.StringId})\n" +
                    $"Culture: {newClan.Culture.Name}\n" +
                    $"Clan ID: {newClan.StringId}");
            });
        }
    }
}
