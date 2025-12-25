using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Cultures;
using Bannerlord.GameMaster.Kingdoms;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.KingdomCommands
{
	[CommandLineFunctionality.CommandLineArgumentFunction("kingdom", "gm")]
	public static class KingdomGenerationCommands
	{
		//MARK: create_kingdom
		/// <summary>
		/// Create a new kingdom with a specified settlement as capital
		/// Usage: gm.kingdom.create_kingdom &lt;settlement&gt; [kingdomName] [clanName] [vassalCount] [cultures]
		/// </summary>
		[CommandLineFunctionality.CommandLineArgumentFunction("create_kingdom", "gm.kingdom")]
		public static string CreateKingdom(List<string> args)
		{
			return Cmd.Run(args, () =>
			{
				if (!CommandBase.ValidateCampaignMode(out string error))
					return error;

				var usageMessage = CommandValidator.CreateUsageMessage(
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

				// Parse arguments
				var parsedArgs = CommandBase.ParseArguments(args);

				// Define valid arguments for validation
				parsedArgs.SetValidArguments(
					new CommandBase.ArgumentDefinition("settlement", true),
					new CommandBase.ArgumentDefinition("kingdomName", false, null, "name"),
					new CommandBase.ArgumentDefinition("clanName", false, null, "clan"),
					new CommandBase.ArgumentDefinition("vassalCount", false, null, "vassals"),
					new CommandBase.ArgumentDefinition("cultures", false, null, "culture")
				);

				// Check for validation errors
				string validationError = parsedArgs.GetValidationError();
				if (validationError != null)
					return CommandBase.FormatErrorMessage(validationError);

				if (parsedArgs.TotalCount < 1)
					return usageMessage;

				// Parse settlement (required)
				string settlementArg = parsedArgs.GetArgument("settlement", 0);
				if (settlementArg == null)
					return CommandBase.FormatErrorMessage("Missing required argument 'settlement'.");

				var (settlement, settlementError) = CommandBase.FindSingleSettlement(settlementArg);
				if (settlementError != null)
					return settlementError;

				// Validate settlement type
				if (!settlement.IsTown && !settlement.IsCastle)
					return CommandBase.FormatErrorMessage($"Settlement '{settlement.Name}' must be a city or castle to become a kingdom capital.");

				if (settlement.Town == null)
					return CommandBase.FormatErrorMessage($"Settlement '{settlement.Name}' has no town component.");

				// Ensure settlement is not owned by player
				if (settlement.OwnerClan == Clan.PlayerClan)
					return CommandBase.FormatErrorMessage($"Settlement '{settlement.Name}' is owned by the player. Cannot use player settlements for kingdom creation.");

				// Parse optional kingdomName
				string kingdomName = parsedArgs.GetArgument("kingdomName", 1) ?? parsedArgs.GetNamed("name");
				if (kingdomName != null && kingdomName.ToLower() == "null")
					kingdomName = null;

				// Parse optional clanName
				string clanName = parsedArgs.GetArgument("clanName", 2) ?? parsedArgs.GetNamed("clan");
				if (clanName != null && clanName.ToLower() == "null")
					clanName = null;

				// Parse optional vassalCount
				int vassalCount = 4;
				string vassalCountArg = parsedArgs.GetArgument("vassalCount", 3) ?? parsedArgs.GetNamed("vassals");
				if (vassalCountArg != null)
				{
					if (!CommandValidator.ValidateIntegerRange(vassalCountArg, 0, 10, out vassalCount, out string vassalError))
						return CommandBase.FormatErrorMessage(vassalError);
				}

				// Parse optional cultures
				CultureFlags cultureFlags = CultureFlags.AllMainCultures;
				string culturesArg = parsedArgs.GetArgument("cultures", 4) ?? parsedArgs.GetNamed("culture");
				if (culturesArg != null)
				{
					cultureFlags = FlagParser.ParseCultureArgument(culturesArg);
					if (cultureFlags == CultureFlags.None)
						return CommandBase.FormatErrorMessage($"Invalid culture(s): '{culturesArg}'. Use culture names (e.g., vlandia,battania) or groups (main_cultures, bandit_cultures, all_cultures)");
				}

				// Build resolved values dictionary for display
				var resolvedValues = new Dictionary<string, string>
				{
					{ "settlement", settlement.Name.ToString() },
					{ "kingdomName", kingdomName ?? "Random" },
					{ "clanName", clanName ?? "Random" },
					{ "vassalCount", vassalCount.ToString() },
					{ "cultures", culturesArg ?? "Main Cultures" }
				};

				// Display argument header
				string argumentDisplay = parsedArgs.FormatArgumentDisplay("create_kingdom", resolvedValues);

				return CommandBase.ExecuteWithErrorHandling(() =>
				{
					Kingdom kingdom = KingdomGenerator.CreateKingdom(
						homeSettlement: settlement,
						vassalClanCount: vassalCount,
						name: kingdomName,
						rulingClanName: clanName,
						cultureFlags: cultureFlags
					);

					if (kingdom == null)
						return argumentDisplay + CommandBase.FormatErrorMessage("Failed to create kingdom - settlement could not be resolved or assigned.");

					return argumentDisplay + CommandBase.FormatSuccessMessage(
						$"Created kingdom '{kingdom.Name}' (ID: {kingdom.StringId}):\n" +
						$"Capital: {settlement.Name}\n" +
						$"Ruling Clan: {kingdom.RulingClan.Name}\n" +
						$"Ruler: {kingdom.Leader.Name}\n" +
						$"Culture: {kingdom.Culture.Name}\n" +
						$"Vassal Clans: {vassalCount}\n" +
						$"Total Clans: {kingdom.Clans.Count}");
				}, "Failed to create kingdom");
			});
		}

		//MARK: generate_kingdoms
		/// <summary>
		/// Generate multiple kingdoms by taking settlements from existing kingdoms
		/// Usage: gm.generate_kingdoms &lt;count&gt; [vassalCount] [cultures]
		/// </summary>
		[CommandLineFunctionality.CommandLineArgumentFunction("generate_kingdoms", "gm.kingdom")]
		public static string GenerateKingdoms(List<string> args)
		{
			return Cmd.Run(args, () =>
			{
				if (!CommandBase.ValidateCampaignMode(out string error))
					return error;

				var usageMessage = CommandValidator.CreateUsageMessage(
					"gm.kingdom.generate_kingdoms", "<count> [vassalCount] [cultures]",
					"Generates multiple kingdoms by taking settlements from existing kingdoms.\n" +
					"Alternates between kingdoms evenly, ensuring not to take a kingdom's last settlement.\n" +
					"Will not take settlements from the player's kingdom.\n" +
					"- count: required, number of kingdoms to generate (1-20)\n" +
					"- vassalCount/vassals: optional, number of vassal clans per kingdom (0-10, default: 4)\n" +
					"- cultures/culture: optional, culture pool for kingdoms and clans. Defaults to main_cultures\n" +
					"Supports named arguments: count:5 vassals:3 cultures:vlandia,battania",
					"gm.kingdom.generate_kingdoms 3\n" +
					"gm.kingdom.generate_kingdoms 5 6\n" +
					"gm.kingdom.generate_kingdoms count:2 vassals:4 cultures:empire,aserai");

				// Parse arguments
				var parsedArgs = CommandBase.ParseArguments(args);

				// Define valid arguments for validation
				parsedArgs.SetValidArguments(
					new CommandBase.ArgumentDefinition("count", true),
					new CommandBase.ArgumentDefinition("vassalCount", false, null, "vassals"),
					new CommandBase.ArgumentDefinition("cultures", false, null, "culture")
				);

				// Check for validation errors
				string validationError = parsedArgs.GetValidationError();
				if (validationError != null)
					return CommandBase.FormatErrorMessage(validationError);

				if (parsedArgs.TotalCount < 1)
					return usageMessage;

				// Parse count (required)
				string countArg = parsedArgs.GetArgument("count", 0);
				if (countArg == null)
					return CommandBase.FormatErrorMessage("Missing required argument 'count'.");

				if (!CommandValidator.ValidateIntegerRange(countArg, 1, 20, out int count, out string countError))
					return CommandBase.FormatErrorMessage(countError);

				// Parse optional vassalCount
				int vassalCount = 4;
				string vassalCountArg = parsedArgs.GetArgument("vassalCount", 1) ?? parsedArgs.GetNamed("vassals");
				if (vassalCountArg != null)
				{
					if (!CommandValidator.ValidateIntegerRange(vassalCountArg, 0, 10, out vassalCount, out string vassalError))
						return CommandBase.FormatErrorMessage(vassalError);
				}

				// Parse optional cultures
				CultureFlags cultureFlags = CultureFlags.AllMainCultures;
				string culturesArg = parsedArgs.GetArgument("cultures", 2) ?? parsedArgs.GetNamed("culture");
				if (culturesArg != null)
				{
					cultureFlags = FlagParser.ParseCultureArgument(culturesArg);
					if (cultureFlags == CultureFlags.None)
						return CommandBase.FormatErrorMessage($"Invalid culture(s): '{culturesArg}'. Use culture names (e.g., vlandia,battania) or groups (main_cultures, bandit_cultures, all_cultures)");
				}

				// Build resolved values dictionary for display
				var resolvedValues = new Dictionary<string, string>
				{
					{ "count", count.ToString() },
					{ "vassalCount", vassalCount.ToString() },
					{ "cultures", culturesArg ?? "Main Cultures" }
				};

				// Display argument header
				string argumentDisplay = parsedArgs.FormatArgumentDisplay("generate_kingdoms", resolvedValues);

				return CommandBase.ExecuteWithErrorHandling(() =>
				{
					List<Kingdom> createdKingdoms = KingdomGenerator.GenerateKingdoms(count, vassalCount, cultureFlags);

					if (createdKingdoms == null || createdKingdoms.Count == 0)
						return argumentDisplay + CommandBase.FormatErrorMessage("Failed to generate kingdoms - no suitable settlements available or all kingdoms exhausted.");

					// Build detailed output with settlement names
					var detailsBuilder = new System.Text.StringBuilder();
					detailsBuilder.AppendLine($"Successfully created {createdKingdoms.Count} kingdom(s):");
					
					foreach (var kingdom in createdKingdoms)
					{
						// Get the first town or castle settlement as the capital
						var capital = kingdom.Settlements.FirstOrDefault(s => s.IsTown || s.IsCastle);
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

					return argumentDisplay + CommandBase.FormatSuccessMessage(detailsBuilder.ToString());
				}, "Failed to generate kingdoms");
			});
		}
	}
}
