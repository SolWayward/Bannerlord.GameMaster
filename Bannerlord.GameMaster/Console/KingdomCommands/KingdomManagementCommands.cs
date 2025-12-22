using Bannerlord.GameMaster.Console.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.KingdomCommands
{
	[CommandLineFunctionality.CommandLineArgumentFunction("kingdom", "gm")]
	public static class KingdomManagementCommands
	{
		#region Clan Management

		/// <summary>
		/// Add a clan to a kingdom <br />
		/// Usage: gm.kingdom.add_clan [clan] [kingdom]
		/// </summary>
		[CommandLineFunctionality.CommandLineArgumentFunction("add_clan", "gm.kingdom")]
		public static string AddClanToKingdom(List<string> args)
		{
			return Cmd.Run(args, () =>
			{
				if (!CommandBase.ValidateCampaignMode(out string error))
					return error;

				var usageMessage = CommandValidator.CreateUsageMessage(
							"gm.kingdom.add_clan", "<clan> <kingdom>",
							"Adds a clan to the specified kingdom.\n" +
							"Supports named arguments: clan:clan_battania_1 kingdom:empire",
							"gm.kingdom.add_clan clan_battania_1 empire");

				// Parse arguments
				var parsedArgs = CommandBase.ParseArguments(args);

				// Define valid arguments
				parsedArgs.SetValidArguments(
					new CommandBase.ArgumentDefinition("clan", true),
					new CommandBase.ArgumentDefinition("kingdom", true)
				);

				// Validate
				string validationError = parsedArgs.GetValidationError();
				if (validationError != null)
					return CommandBase.FormatErrorMessage(validationError);

				if (parsedArgs.TotalCount < 2)
					return usageMessage;

				// Parse clan
				string clanArg = parsedArgs.GetArgument("clan", 0);
				if (clanArg == null)
					return CommandBase.FormatErrorMessage("Missing required argument 'clan'.");

				var (clan, clanError) = CommandBase.FindSingleClan(clanArg);
				if (clanError != null) return clanError;

				// Parse kingdom
				string kingdomArg = parsedArgs.GetArgument("kingdom", 1);
				if (kingdomArg == null)
					return CommandBase.FormatErrorMessage("Missing required argument 'kingdom'.");

				var (kingdom, kingdomError) = CommandBase.FindSingleKingdom(kingdomArg);
				if (kingdomError != null) return kingdomError;

				if (clan.Kingdom == kingdom)
					return CommandBase.FormatErrorMessage($"{clan.Name} is already part of {kingdom.Name}.");

				// Build display
				var resolvedValues = new Dictionary<string, string>
				{
					{ "clan", clan.Name.ToString() },
					{ "kingdom", kingdom.Name.ToString() }
				};
				string argumentDisplay = parsedArgs.FormatArgumentDisplay("add_clan", resolvedValues);

				return CommandBase.ExecuteWithErrorHandling(() =>
					{
						string previousKingdom = clan.Kingdom?.Name?.ToString() ?? "No Kingdom";
						ChangeKingdomAction.ApplyByJoinToKingdom(clan, kingdom, showNotification: true);
						return argumentDisplay + CommandBase.FormatSuccessMessage($"{clan.Name} joined {kingdom.Name} from {previousKingdom}.");
					}, "Failed to add clan to kingdom");
			});
		}

		/// <summary>
		/// Remove a clan from its kingdom
		/// Usage: gm.kingdom.remove_clan [clan]
		/// </summary>
		[CommandLineFunctionality.CommandLineArgumentFunction("remove_clan", "gm.kingdom")]
		public static string RemoveClanFromKingdom(List<string> args)
		{
			return Cmd.Run(args, () =>
			{
				if (!CommandBase.ValidateCampaignMode(out string error))
					return error;

				var usageMessage = CommandValidator.CreateUsageMessage(
									"gm.kingdom.remove_clan", "<clan>",
									"Removes a clan from its current kingdom.\n" +
									"Supports named arguments: clan:clan_empire_south_1",
									"gm.kingdom.remove_clan clan_empire_south_1");

				// Parse arguments
				var parsedArgs = CommandBase.ParseArguments(args);

				// Define valid arguments
				parsedArgs.SetValidArguments(
					new CommandBase.ArgumentDefinition("clan", true)
				);

				// Validate
				string validationError = parsedArgs.GetValidationError();
				if (validationError != null)
					return CommandBase.FormatErrorMessage(validationError);

				if (parsedArgs.TotalCount < 1)
					return usageMessage;

				// Parse clan
				string clanArg = parsedArgs.GetArgument("clan", 0);
				if (clanArg == null)
					return CommandBase.FormatErrorMessage("Missing required argument 'clan'.");

				var (clan, clanError) = CommandBase.FindSingleClan(clanArg);
				if (clanError != null) return clanError;

				if (clan.Kingdom == null)
					return CommandBase.FormatErrorMessage($"{clan.Name} is not part of any kingdom.");

				if (clan == clan.Kingdom.RulingClan)
					return CommandBase.FormatErrorMessage($"Cannot remove the ruling clan ({clan.Name}) from {clan.Kingdom.Name}.");

				// Build display
				var resolvedValues = new Dictionary<string, string>
				{
					{ "clan", clan.Name.ToString() }
				};
				string argumentDisplay = parsedArgs.FormatArgumentDisplay("remove_clan", resolvedValues);

				return CommandBase.ExecuteWithErrorHandling(() =>
							{
						string previousKingdom = clan.Kingdom.Name.ToString();
						clan.Kingdom = null;
						return argumentDisplay + CommandBase.FormatSuccessMessage($"{clan.Name} removed from {previousKingdom}.");
					}, "Failed to remove clan from kingdom");
			});
		}

		#endregion

		#region Diplomacy

		/// <summary>
		/// Declare war between two kingdoms
		/// Usage: gm.kingdom.declare_war [kingdom1] [kingdom2]
		/// </summary>
		[CommandLineFunctionality.CommandLineArgumentFunction("declare_war", "gm.kingdom")]
		public static string DeclareWar(List<string> args)
		{
			return Cmd.Run(args, () =>
			{
				if (!CommandBase.ValidateCampaignMode(out string error))
					return error;

				var usageMessage = CommandValidator.CreateUsageMessage(
									"gm.kingdom.declare_war", "<kingdom1> <kingdom2>",
									"Declares war between two kingdoms.\n" +
									"Supports named arguments: kingdom1:empire kingdom2:battania",
									"gm.kingdom.declare_war empire battania");

				// Parse arguments
				var parsedArgs = CommandBase.ParseArguments(args);

				// Define valid arguments
				parsedArgs.SetValidArguments(
					new CommandBase.ArgumentDefinition("kingdom1", true),
					new CommandBase.ArgumentDefinition("kingdom2", true)
				);

				// Validate
				string validationError = parsedArgs.GetValidationError();
				if (validationError != null)
					return CommandBase.FormatErrorMessage(validationError);

				if (parsedArgs.TotalCount < 2)
					return usageMessage;

				// Parse kingdom1
				string kingdom1Arg = parsedArgs.GetArgument("kingdom1", 0);
				if (kingdom1Arg == null)
					return CommandBase.FormatErrorMessage("Missing required argument 'kingdom1'.");

				var (kingdom1, kingdom1Error) = CommandBase.FindSingleKingdom(kingdom1Arg);
				if (kingdom1Error != null) return kingdom1Error;

				// Parse kingdom2
				string kingdom2Arg = parsedArgs.GetArgument("kingdom2", 1);
				if (kingdom2Arg == null)
					return CommandBase.FormatErrorMessage("Missing required argument 'kingdom2'.");

				var (kingdom2, kingdom2Error) = CommandBase.FindSingleKingdom(kingdom2Arg);
				if (kingdom2Error != null) return kingdom2Error;

				if (kingdom1 == kingdom2)
					return CommandBase.FormatErrorMessage("A kingdom cannot declare war on itself.");

				if (FactionManager.IsAtWarAgainstFaction(kingdom1, kingdom2))
					return CommandBase.FormatErrorMessage($"{kingdom1.Name} and {kingdom2.Name} are already at war.");

				// Build display
				var resolvedValues = new Dictionary<string, string>
				{
					{ "kingdom1", kingdom1.Name.ToString() },
					{ "kingdom2", kingdom2.Name.ToString() }
				};
				string argumentDisplay = parsedArgs.FormatArgumentDisplay("declare_war", resolvedValues);

				return CommandBase.ExecuteWithErrorHandling(() =>
							{
						DeclareWarAction.ApplyByDefault(kingdom1, kingdom2);
						return argumentDisplay + CommandBase.FormatSuccessMessage($"War declared between {kingdom1.Name} and {kingdom2.Name}.");
					}, "Failed to declare war");
			});
		}

		/// <summary>
		/// Make peace between two kingdoms
		/// Usage: gm.kingdom.make_peace [kingdom1] [kingdom2]
		/// </summary>
		[CommandLineFunctionality.CommandLineArgumentFunction("make_peace", "gm.kingdom")]
		public static string MakePeace(List<string> args)
		{
			return Cmd.Run(args, () =>
			{
				if (!CommandBase.ValidateCampaignMode(out string error))
					return error;

				var usageMessage = CommandValidator.CreateUsageMessage(
									"gm.kingdom.make_peace", "<kingdom1> <kingdom2>",
									"Makes peace between two kingdoms.\n" +
									"Supports named arguments: kingdom1:empire kingdom2:battania",
									"gm.kingdom.make_peace empire battania");

				// Parse arguments
				var parsedArgs = CommandBase.ParseArguments(args);

				// Define valid arguments
				parsedArgs.SetValidArguments(
					new CommandBase.ArgumentDefinition("kingdom1", true),
					new CommandBase.ArgumentDefinition("kingdom2", true)
				);

				// Validate
				string validationError = parsedArgs.GetValidationError();
				if (validationError != null)
					return CommandBase.FormatErrorMessage(validationError);

				if (parsedArgs.TotalCount < 2)
					return usageMessage;

				// Parse kingdom1
				string kingdom1Arg = parsedArgs.GetArgument("kingdom1", 0);
				if (kingdom1Arg == null)
					return CommandBase.FormatErrorMessage("Missing required argument 'kingdom1'.");

				var (kingdom1, kingdom1Error) = CommandBase.FindSingleKingdom(kingdom1Arg);
				if (kingdom1Error != null) return kingdom1Error;

				// Parse kingdom2
				string kingdom2Arg = parsedArgs.GetArgument("kingdom2", 1);
				if (kingdom2Arg == null)
					return CommandBase.FormatErrorMessage("Missing required argument 'kingdom2'.");

				var (kingdom2, kingdom2Error) = CommandBase.FindSingleKingdom(kingdom2Arg);
				if (kingdom2Error != null) return kingdom2Error;

				if (kingdom1 == kingdom2)
					return CommandBase.FormatErrorMessage("A kingdom cannot make peace with itself.");

				if (!FactionManager.IsAtWarAgainstFaction(kingdom1, kingdom2))
					return CommandBase.FormatErrorMessage($"{kingdom1.Name} and {kingdom2.Name} are not at war.");

				// Build display
				var resolvedValues = new Dictionary<string, string>
				{
					{ "kingdom1", kingdom1.Name.ToString() },
					{ "kingdom2", kingdom2.Name.ToString() }
				};
				string argumentDisplay = parsedArgs.FormatArgumentDisplay("make_peace", resolvedValues);

				return CommandBase.ExecuteWithErrorHandling(() =>
							{
						MakePeaceAction.Apply(kingdom1, kingdom2);
						return argumentDisplay + CommandBase.FormatSuccessMessage($"Peace established between {kingdom1.Name} and {kingdom2.Name}.");
					}, "Failed to make peace");
			});
		}

		#endregion

		#region Kingdom Properties

		/// <summary>
		/// Change kingdom ruler
		/// Usage: gm.kingdom.set_ruler [kingdom] [hero]
		/// </summary>
		[CommandLineFunctionality.CommandLineArgumentFunction("set_ruler", "gm.kingdom")]
		public static string SetKingdomRuler(List<string> args)
		{
			return Cmd.Run(args, () =>
			{
				if (!CommandBase.ValidateCampaignMode(out string error))
					return error;

				var usageMessage = CommandValidator.CreateUsageMessage(
									"gm.kingdom.set_ruler", "<kingdom> <hero>",
									"Changes the kingdom ruler.\n" +
									"Supports named arguments: kingdom:empire hero:lord_1_1",
									"gm.kingdom.set_ruler empire lord_1_1");

				// Parse arguments
				var parsedArgs = CommandBase.ParseArguments(args);

				// Define valid arguments
				parsedArgs.SetValidArguments(
					new CommandBase.ArgumentDefinition("kingdom", true),
					new CommandBase.ArgumentDefinition("hero", true)
				);

				// Validate
				string validationError = parsedArgs.GetValidationError();
				if (validationError != null)
					return CommandBase.FormatErrorMessage(validationError);

				if (parsedArgs.TotalCount < 2)
					return usageMessage;

				// Parse kingdom
				string kingdomArg = parsedArgs.GetArgument("kingdom", 0);
				if (kingdomArg == null)
					return CommandBase.FormatErrorMessage("Missing required argument 'kingdom'.");

				var (kingdom, kingdomError) = CommandBase.FindSingleKingdom(kingdomArg);
				if (kingdomError != null) return kingdomError;

				// Parse hero
				string heroArg = parsedArgs.GetArgument("hero", 1);
				if (heroArg == null)
					return CommandBase.FormatErrorMessage("Missing required argument 'hero'.");

				var (hero, heroError) = CommandBase.FindSingleHero(heroArg);
				if (heroError != null) return heroError;

				if (hero.MapFaction != kingdom)
					return CommandBase.FormatErrorMessage($"{hero.Name} is not part of {kingdom.Name}.");

				if (hero.Clan == null)
					return CommandBase.FormatErrorMessage($"{hero.Name} has no clan.");

				// Build display
				var resolvedValues = new Dictionary<string, string>
				{
					{ "kingdom", kingdom.Name.ToString() },
					{ "hero", hero.Name.ToString() }
				};
				string argumentDisplay = parsedArgs.FormatArgumentDisplay("set_ruler", resolvedValues);

				return CommandBase.ExecuteWithErrorHandling(() =>
							{
						string previousRuler = kingdom.Leader?.Name?.ToString() ?? "None";

						kingdom.RulingClan = hero.Clan;
						if (hero.Clan.Leader != hero)
							hero.Clan.SetLeader(hero);

						return argumentDisplay + CommandBase.FormatSuccessMessage(
											$"{kingdom.Name}'s ruler changed from {previousRuler} to {hero.Name}.\n" +
											$"Ruling clan is now {hero.Clan.Name}.");
					}, "Failed to set kingdom ruler");
			});
		}

		/// <summary>
		/// Destroy/Eliminate a kingdom
		/// Usage: gm.kingdom.destroy [kingdom]
		/// </summary>
		[CommandLineFunctionality.CommandLineArgumentFunction("destroy", "gm.kingdom")]
		public static string DestroyKingdom(List<string> args)
		{
			return Cmd.Run(args, () =>
			{
				if (!CommandBase.ValidateCampaignMode(out string error))
					return error;

				var usageMessage = CommandValidator.CreateUsageMessage(
									"gm.kingdom.destroy", "<kingdom>",
									"Destroys/eliminates the specified kingdom.\n" +
									"Supports named arguments: kingdom:battania",
									"gm.kingdom.destroy battania");

				// Parse arguments
				var parsedArgs = CommandBase.ParseArguments(args);

				// Define valid arguments
				parsedArgs.SetValidArguments(
					new CommandBase.ArgumentDefinition("kingdom", true)
				);

				// Validate
				string validationError = parsedArgs.GetValidationError();
				if (validationError != null)
					return CommandBase.FormatErrorMessage(validationError);

				if (parsedArgs.TotalCount < 1)
					return usageMessage;

				// Parse kingdom
				string kingdomArg = parsedArgs.GetArgument("kingdom", 0);
				if (kingdomArg == null)
					return CommandBase.FormatErrorMessage("Missing required argument 'kingdom'.");

				var (kingdom, kingdomError) = CommandBase.FindSingleKingdom(kingdomArg);
				if (kingdomError != null) return kingdomError;

				if (kingdom.IsEliminated)
					return CommandBase.FormatErrorMessage($"{kingdom.Name} is already eliminated.");

				if (kingdom == Hero.MainHero.MapFaction)
					return CommandBase.FormatErrorMessage("Cannot destroy the player's kingdom.");

				// Build display
				var resolvedValues = new Dictionary<string, string>
				{
					{ "kingdom", kingdom.Name.ToString() }
				};
				string argumentDisplay = parsedArgs.FormatArgumentDisplay("destroy", resolvedValues);

				return CommandBase.ExecuteWithErrorHandling(() =>
							{
						DestroyKingdomAction.Apply(kingdom);
						return argumentDisplay + CommandBase.FormatSuccessMessage($"{kingdom.Name} has been destroyed/eliminated.");
					}, "Failed to destroy kingdom");
			});
		}

		#endregion
	}
}
