using Bannerlord.GameMaster.Console.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Library;
using System.Reflection;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;
using Bannerlord.GameMaster.Clans;
using Bannerlord.GameMaster.Characters;
using Bannerlord.GameMaster.Cultures;

namespace Bannerlord.GameMaster.Console.ClanCommands
{
	[CommandLineFunctionality.CommandLineArgumentFunction("clan", "gm")]
	public static class ClanGenerationCommands
	{
		//MARK: create_clan
		/// <summary>
		/// Create a clan with the specified name. Optionally set a hero as leader and assign to kingdom.
		/// Usage: gm.clan.create_clan &lt;clanName&gt; [leaderHero] [kingdom] [createParty] [companionCount]
		/// </summary>
		[CommandLineFunctionality.CommandLineArgumentFunction("create_clan", "gm.clan")]
		public static string CreateClan(List<string> args)
		{
			return Cmd.Run(args, () =>
			{
				if (!CommandBase.ValidateCampaignMode(out string error))
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
					Clan newClan = ClanGenerator.CreateClan(clanName, leader, kingdom, createParty, companionCount);
					
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

		//MARK: generate_clans
		/// <summary>
		/// Generate multiple clans at once with random names from culture lists
		/// Usage: gm.clan.generate_clans <count> [cultures] [kingdom] [createParties] [companionCount]
		/// </summary>
		[CommandLineFunctionality.CommandLineArgumentFunction("generate_clans", "gm.clan")]
		public static string GenerateClans(List<string> args)
		{
			//return Cmd.Run(args, () =>
			//{
				if (!CommandBase.ValidateCampaignMode(out string error))
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

				//return CommandBase.ExecuteWithErrorHandling(() =>
				//{
					List<Clan> clans = ClanGenerator.GenerateClans(count, cultureFlags, kingdom, createParties, companionCount);

					if (clans == null || clans.Count == 0)
						return argumentDisplay + CommandBase.FormatErrorMessage("Failed to generate clans - no clans created");

					string kingdomInfo = kingdom != null ? $" and joined {kingdom.Name}" : " as independent";
					string partyInfo = createParties ? " (with parties)" : " (no parties)";
					string companionInfo = companionCount > 0 ? $" and {companionCount} companions each" : "";
					
					return argumentDisplay + CommandBase.FormatSuccessMessage(
						$"Generated {clans.Count} clan(s){kingdomInfo}{partyInfo}{companionInfo}:\n" +
						ClanQueries.GetFormattedDetails(clans));
				//}, "Failed to generate clans");
			//});
		}

		//MARK: create_minor_clan
		/// <summary>
		/// Create a minor faction clan (not a noble house)
		/// Usage: gm.clan.create_minor_clan <clanName> [leaderHero] [cultures] [createParty]
		/// </summary>
		[CommandLineFunctionality.CommandLineArgumentFunction("create_minor_clan", "gm.clan")]
		public static string CreateMinorClan(List<string> args)
		{
			return Cmd.Run(args, () =>
			{
				if (!CommandBase.ValidateCampaignMode(out string error))
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
						$"Type: Minor Faction (Tier 1)");
				}, "Failed to create minor clan");
			});
		}
	}
}
