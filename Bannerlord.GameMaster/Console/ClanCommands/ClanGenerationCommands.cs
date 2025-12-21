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

namespace Bannerlord.GameMaster.Console.ClanCommands
{
	[CommandLineFunctionality.CommandLineArgumentFunction("clan", "gm")]
	public static class ClanGenerationCommands
	{
		//MARK: create_clan
		/// <summary>
		/// Create a clan with the specified name. Optionally set a hero as leader and assign to kingdom.
		/// Usage: gm.clan.create_clan <clanName> [leaderHero] [kingdom] [createParty] [companionCount]
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
					"- clanName: required, name for the clan. Use SINGLE QUOTES for multi-word names\n" +
					"- leaderHero: optional, existing hero ID or name to make leader (creates new hero if omitted)\n" +
					"- kingdom: optional, kingdom ID or name for clan to join (independent if omitted)\n" +
					"- createParty: optional, 'true' or 'false' to create party for leader (default: true)\n" +
					"- companionCount: optional, number of companions to add (0-10, default: 2)\n" +
					"Use SINGLE QUOTES for multi-word clan names (double quotes don't work).",
					"gm.clan.create_clan Highlanders\n" +
					"gm.clan.create_clan 'The Highland Clan' derthert\n" +
					"gm.clan.create_clan NewClan myHero empire\n" +
					"gm.clan.create_clan 'House Stark' null sturgia true 5\n" +
					"gm.clan.create_clan TradingFamily null null false 0");

				if (!CommandBase.ValidateArgumentCount(args, 1, usageMessage, out error))
					return error;

				string clanName = args[0];
				Hero leader = null;
				Kingdom kingdom = null;
				bool createParty = true;
				int companionCount = 2;

				// Parse optional leader (args[1])
				if (args.Count > 1 && args[1].ToLower() != "null")
				{
					var (hero, heroError) = CommandBase.FindSingleHero(args[1]);
					if (heroError != null) return heroError;
					leader = hero;
				}

				// Parse optional kingdom (args[2])
				if (args.Count > 2 && args[2].ToLower() != "null")
				{
					var (kingdomResult, kingdomError) = CommandBase.FindSingleKingdom(args[2]);
					if (kingdomError != null) return kingdomError;
					kingdom = kingdomResult;
				}

				// Parse optional createParty (args[3])
				if (args.Count > 3)
				{
					if (!bool.TryParse(args[3], out createParty))
						return CommandBase.FormatErrorMessage($"Invalid createParty value: '{args[3]}'. Use 'true' or 'false'.");
				}

				// Parse optional companionCount (args[4])
				if (args.Count > 4)
				{
					if (!CommandValidator.ValidateIntegerRange(args[4], 0, 10, out companionCount, out string countError))
						return CommandBase.FormatErrorMessage(countError);
				}

				return CommandBase.ExecuteWithErrorHandling(() =>
				{
					Clan newClan = ClanGenerator.CreateClan(clanName, leader, kingdom, createParty, companionCount);
					
					string leaderInfo = leader != null ? $" with {leader.Name} as leader" : " with auto-generated leader";
					string kingdomInfo = kingdom != null ? $" and joined {kingdom.Name}" : " as independent";
					string partyInfo = createParty ? " (with party)" : " (no party)";
					string companionInfo = companionCount > 0 ? $" and {companionCount} companions" : "";
					
					return CommandBase.FormatSuccessMessage(
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
			return Cmd.Run(args, () =>
			{
				if (!CommandBase.ValidateCampaignMode(out string error))
					return error;

				string usageMessage = CommandValidator.CreateUsageMessage(
					"gm.clan.generate_clans", "<count> [cultures] [kingdom] [createParties] [companionCount]",
					"Generate multiple clans with random names from culture lists. If no culture specified, uses main_cultures.\n" +
					"- count: required, number of clans to generate (1-50)\n" +
					"- cultures: optional, defines the pool of cultures. Use ; (semi-colon) with no spaces for multiple\n" +
					"- kingdom: optional, kingdom for all generated clans to join (independent if omitted)\n" +
					"- createParties: optional, 'true' or 'false' to create parties for leaders (default: true)\n" +
					"- companionCount: optional, number of companions per clan (0-10, default: 2)\n",
					"gm.clan.generate_clans 5\n" +
					"gm.clan.generate_clans 10 vlandia;battania\n" +
					"gm.clan.generate_clans 3 main_cultures empire\n" +
					"gm.clan.generate_clans 7 aserai;khuzait sturgia true 5\n" +
					"gm.clan.generate_clans 3 empire null false 0");

				if (!CommandBase.ValidateArgumentCount(args, 1, usageMessage, out error))
					return error;

				// Parse count (required)
				if (!CommandValidator.ValidateIntegerRange(args[0], 1, 50, out int count, out string countError))
					return CommandBase.FormatErrorMessage(countError);

				// Parse optional cultures (args[1])
				CultureFlags cultureFlags = CultureFlags.AllMainCultures;
				if (args.Count > 1)
				{
					cultureFlags = FlagParser.ParseCultureArgument(args[1]);
					if (cultureFlags == CultureFlags.None)
						return CommandBase.FormatErrorMessage($"Invalid culture(s): '{args[1]}'. Use culture names (e.g., vlandia;battania) or groups (main_cultures, bandit_cultures, all_cultures)");
				}

				// Parse optional kingdom (args[2])
				Kingdom kingdom = null;
				if (args.Count > 2 && args[2].ToLower() != "null")
				{
					var (kingdomResult, kingdomError) = CommandBase.FindSingleKingdom(args[2]);
					if (kingdomError != null) return kingdomError;
					kingdom = kingdomResult;
				}

				// Parse optional createParties (args[3])
				bool createParties = true;
				if (args.Count > 3)
				{
					if (!bool.TryParse(args[3], out createParties))
						return CommandBase.FormatErrorMessage($"Invalid createParties value: '{args[3]}'. Use 'true' or 'false'.");
				}

				// Parse optional companionCount (args[4])
				int companionCount = 2;
				if (args.Count > 4)
				{
					if (!CommandValidator.ValidateIntegerRange(args[4], 0, 10, out companionCount, out string compCountError))
						return CommandBase.FormatErrorMessage(compCountError);
				}

				return CommandBase.ExecuteWithErrorHandling(() =>
				{
					List<Clan> clans = ClanGenerator.GenerateClans(count, cultureFlags, kingdom, createParties, companionCount);

					if (clans == null || clans.Count == 0)
						return CommandBase.FormatErrorMessage("Failed to generate clans - no clans created");

					string kingdomInfo = kingdom != null ? $" and joined {kingdom.Name}" : " as independent";
					string partyInfo = createParties ? " (with parties)" : " (no parties)";
					string companionInfo = companionCount > 0 ? $" and {companionCount} companions each" : "";
					
					return CommandBase.FormatSuccessMessage(
						$"Generated {clans.Count} clan(s){kingdomInfo}{partyInfo}{companionInfo}:\n" +
						ClanQueries.GetFormattedDetails(clans));
				}, "Failed to generate clans");
			});
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
					"- clanName: required, name for the minor clan. Use SINGLE QUOTES for multi-word names\n" +
					"- leaderHero: optional, existing hero ID or name to make leader (creates new hero if omitted)\n" +
					"- cultures: optional, culture for template selection (default: main_cultures)\n" +
					"- createParty: optional, 'true' or 'false' to create party for leader (default: true)\n",
					"gm.clan.create_minor_clan 'Mercenary Company'\n" +
					"gm.clan.create_minor_clan Bandits null bandit_cultures\n" +
					"gm.clan.create_minor_clan 'Free Traders' myHero empire false");

				if (!CommandBase.ValidateArgumentCount(args, 1, usageMessage, out error))
					return error;

				string clanName = args[0];
				Hero leader = null;
				CultureFlags cultureFlags = CultureFlags.AllMainCultures;
				bool createParty = true;

				// Parse optional leader (args[1])
				if (args.Count > 1 && args[1].ToLower() != "null")
				{
					var (hero, heroError) = CommandBase.FindSingleHero(args[1]);
					if (heroError != null) return heroError;
					leader = hero;
				}

				// Parse optional cultures (args[2])
				if (args.Count > 2)
				{
					cultureFlags = FlagParser.ParseCultureArgument(args[2]);
					if (cultureFlags == CultureFlags.None)
						return CommandBase.FormatErrorMessage($"Invalid culture(s): '{args[2]}'");
				}

				// Parse optional createParty (args[3])
				if (args.Count > 3)
				{
					if (!bool.TryParse(args[3], out createParty))
						return CommandBase.FormatErrorMessage($"Invalid createParty value: '{args[3]}'. Use 'true' or 'false'.");
				}

				return CommandBase.ExecuteWithErrorHandling(() =>
				{
					Clan minorClan = ClanGenerator.CreateMinorClan(clanName, leader, cultureFlags, createParty);
					
					string leaderInfo = leader != null ? $" with {leader.Name} as leader" : " with auto-generated leader";
					string partyInfo = createParty ? " (with party)" : " (no party)";
					
					return CommandBase.FormatSuccessMessage(
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
