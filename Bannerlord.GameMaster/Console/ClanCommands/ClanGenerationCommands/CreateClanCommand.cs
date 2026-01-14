using Bannerlord.GameMaster.Console.Common;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using Bannerlord.GameMaster.Clans;
using Bannerlord.GameMaster.Cultures;

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
                if (!CommandBase.ValidateCampaignState(out string error))
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

                // Parse arguments with named argument support
                var parsedArgs = CommandBase.ParseArguments(args);

                // Define valid arguments
                parsedArgs.SetValidArguments(
                    new CommandBase.ArgumentDefinition("clanName", true, null, "name"),
                    new CommandBase.ArgumentDefinition("leaderHero", false, null, "leader"),
                    new CommandBase.ArgumentDefinition("kingdom", false),
                    new CommandBase.ArgumentDefinition("createParty", false, null, "party"),
                    new CommandBase.ArgumentDefinition("companionCount", false, null, "companions")
                );

                // Validate
                string validationError = parsedArgs.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                if (parsedArgs.TotalCount < 1)
                    return usageMessage;

                // Get clan name (required) - supports both 'name' and 'clanName'
                string clanName = parsedArgs.GetArgument("name", 0) ?? parsedArgs.GetArgument("clanName", 0);
                if (string.IsNullOrWhiteSpace(clanName))
                    return CommandBase.FormatErrorMessage("Clan name cannot be empty.");

                Hero leader = null;
                Kingdom kingdom = null;
                bool createParty = true;
                int companionCount = 2;

                // Parse optional leader - supports 'leader' or 'leaderHero'
                string leaderArg = parsedArgs.GetArgument("leader", 1) ?? parsedArgs.GetArgument("leaderHero", 1);
                if (leaderArg != null && leaderArg.ToLower() != "null")
                {
                    var (hero, heroError) = CommandBase.FindSingleHero(leaderArg);
                    if (heroError != null) return heroError;
                    leader = hero;
                }

                // Parse optional kingdom
                string kingdomArg = parsedArgs.GetArgument("kingdom", 2);
                if (kingdomArg != null && kingdomArg.ToLower() != "null")
                {
                    var (kingdomResult, kingdomError) = CommandBase.FindSingleKingdom(kingdomArg);
                    if (kingdomError != null) return kingdomError;
                    kingdom = kingdomResult;
                }

                // Parse optional createParty - supports 'createParty' or 'party'
                string partyArg = parsedArgs.GetArgument("createParty", 3) ?? parsedArgs.GetArgument("party", 3);
                if (partyArg != null)
                {
                    if (!bool.TryParse(partyArg, out createParty))
                        return CommandBase.FormatErrorMessage($"Invalid createParty value: '{partyArg}'. Use 'true' or 'false'.");
                }

                // Parse optional companionCount - supports 'companionCount' or 'companions'
                string companionsArg = parsedArgs.GetArgument("companionCount", 4) ?? parsedArgs.GetArgument("companions", 4);
                if (companionsArg != null)
                {
                    if (!CommandValidator.ValidateIntegerRange(companionsArg, 0, 10, out companionCount, out string countError))
                        return CommandBase.FormatErrorMessage(countError);
                }

                // Validate limits - creating 1 clan and potentially heroes (leader + companions) if no leader specified
                if (!CommandValidator.ValidateClanCreationLimit(1, out string clanLimitError))
                    return CommandBase.FormatErrorMessage(clanLimitError);

                // If no leader is provided, we'll create 1 leader + companions
                if (leader == null)
                {
                    int heroesToCreate = 1 + companionCount;
                    if (!CommandValidator.ValidateHeroCreationLimit(heroesToCreate, out string heroLimitError))
                        return CommandBase.FormatErrorMessage(heroLimitError);
                }

                // Build resolved values dictionary
                var resolvedValues = new Dictionary<string, string>
                {
                    { "clanName", clanName },
                    { "leaderHero", leader != null ? leader.Name.ToString() : "Auto-generated" },
                    { "kingdom", kingdom != null ? kingdom.Name.ToString() : "Independent" },
                    { "createParty", createParty.ToString() },
                    { "companionCount", companionCount.ToString() }
                };

                // Display argument header
                string argumentDisplay = parsedArgs.FormatArgumentDisplay("create_clan", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    Clan newClan = ClanGenerator.CreateNobleClan(clanName, leader, kingdom, createParty, companionCount);

                    string leaderInfo = leader != null ? $" with {leader.Name} as leader" : " with auto-generated leader";
                    string kingdomInfo = kingdom != null ? $" and joined {kingdom.Name}" : " as independent";
                    string partyInfo = createParty ? " (with party)" : " (no party)";
                    string companionInfo = companionCount > 0 ? $" and {companionCount} companions" : "";

                    return argumentDisplay + CommandBase.FormatSuccessMessage(
                        $"Created clan '{newClan.Name}'{leaderInfo}{kingdomInfo}{partyInfo}{companionInfo}.\n" +
                        $"Leader: {newClan.Leader.Name} (ID: {newClan.Leader.StringId})\n" +
                        $"Culture: {newClan.Culture.Name}\n" +
                        $"Clan ID: {newClan.StringId}");
                }, "Failed to create clan");
            });
        }
    }
}