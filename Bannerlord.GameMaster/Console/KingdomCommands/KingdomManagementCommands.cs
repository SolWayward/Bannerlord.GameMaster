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
							"Adds a clan to the specified kingdom.",
							"gm.kingdom.add_clan clan_battania_1 empire");

				if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
					return error;

				var (clan, clanError) = CommandBase.FindSingleClan(args[0]);
				if (clanError != null) return clanError;

				var (kingdom, kingdomError) = CommandBase.FindSingleKingdom(args[1]);
				if (kingdomError != null) return kingdomError;

				if (clan.Kingdom == kingdom)
					return CommandBase.FormatErrorMessage($"{clan.Name} is already part of {kingdom.Name}.");

				return CommandBase.ExecuteWithErrorHandling(() =>
					{
						string previousKingdom = clan.Kingdom?.Name?.ToString() ?? "No Kingdom";
						ChangeKingdomAction.ApplyByJoinToKingdom(clan, kingdom, showNotification: true);
						return CommandBase.FormatSuccessMessage($"{clan.Name} joined {kingdom.Name} from {previousKingdom}.");
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
									"Removes a clan from its current kingdom.",
									"gm.kingdom.remove_clan clan_empire_south_1");

				if (!CommandBase.ValidateArgumentCount(args, 1, usageMessage, out error))
					return error;

				var (clan, clanError) = CommandBase.FindSingleClan(args[0]);
				if (clanError != null) return clanError;

				if (clan.Kingdom == null)
					return CommandBase.FormatErrorMessage($"{clan.Name} is not part of any kingdom.");

				if (clan == clan.Kingdom.RulingClan)
					return CommandBase.FormatErrorMessage($"Cannot remove the ruling clan ({clan.Name}) from {clan.Kingdom.Name}.");

				return CommandBase.ExecuteWithErrorHandling(() =>
							{
						string previousKingdom = clan.Kingdom.Name.ToString();
						clan.Kingdom = null;
						return CommandBase.FormatSuccessMessage($"{clan.Name} removed from {previousKingdom}.");
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
									"Declares war between two kingdoms.",
									"gm.kingdom.declare_war empire battania");

				if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
					return error;

				var (kingdom1, kingdom1Error) = CommandBase.FindSingleKingdom(args[0]);
				if (kingdom1Error != null) return kingdom1Error;

				var (kingdom2, kingdom2Error) = CommandBase.FindSingleKingdom(args[1]);
				if (kingdom2Error != null) return kingdom2Error;

				if (kingdom1 == kingdom2)
					return CommandBase.FormatErrorMessage("A kingdom cannot declare war on itself.");

				if (FactionManager.IsAtWarAgainstFaction(kingdom1, kingdom2))
					return CommandBase.FormatErrorMessage($"{kingdom1.Name} and {kingdom2.Name} are already at war.");

				return CommandBase.ExecuteWithErrorHandling(() =>
							{
						DeclareWarAction.ApplyByDefault(kingdom1, kingdom2);
						return CommandBase.FormatSuccessMessage($"War declared between {kingdom1.Name} and {kingdom2.Name}.");
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
									"Makes peace between two kingdoms.",
									"gm.kingdom.make_peace empire battania");

				if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
					return error;

				var (kingdom1, kingdom1Error) = CommandBase.FindSingleKingdom(args[0]);
				if (kingdom1Error != null) return kingdom1Error;

				var (kingdom2, kingdom2Error) = CommandBase.FindSingleKingdom(args[1]);
				if (kingdom2Error != null) return kingdom2Error;

				if (kingdom1 == kingdom2)
					return CommandBase.FormatErrorMessage("A kingdom cannot make peace with itself.");

				if (!FactionManager.IsAtWarAgainstFaction(kingdom1, kingdom2))
					return CommandBase.FormatErrorMessage($"{kingdom1.Name} and {kingdom2.Name} are not at war.");

				return CommandBase.ExecuteWithErrorHandling(() =>
							{
						MakePeaceAction.Apply(kingdom1, kingdom2);
						return CommandBase.FormatSuccessMessage($"Peace established between {kingdom1.Name} and {kingdom2.Name}.");
					}, "Failed to make peace");
			});
		}

		#endregion

		#region Settlement Management

		/// <summary>
		/// Transfer a settlement to another kingdom
		/// Usage: gm.kingdom.give_fief [settlement] [kingdom]
		/// </summary>
		[CommandLineFunctionality.CommandLineArgumentFunction("give_fief", "gm.kingdom")]
		public static string GiveFief(List<string> args)
		{
			return Cmd.Run(args, () =>
			{
				if (!CommandBase.ValidateCampaignMode(out string error))
					return error;

				var usageMessage = CommandValidator.CreateUsageMessage(
									"gm.kingdom.give_fief", "<settlement> <kingdom>",
									"Transfers a settlement to another kingdom.",
									"gm.kingdom.give_fief town_empire_1 battania");

				if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
					return error;

				Settlement settlement = Settlement.Find(args[0]);
				if (settlement == null)
					return CommandBase.FormatErrorMessage($"Settlement '{args[0]}' not found.");

				if (!settlement.IsTown && !settlement.IsCastle)
					return CommandBase.FormatErrorMessage($"{settlement.Name} is not a town or castle.");

				var (kingdom, kingdomError) = CommandBase.FindSingleKingdom(args[1]);
				if (kingdomError != null) return kingdomError;

				if (settlement.MapFaction == kingdom)
					return CommandBase.FormatErrorMessage($"{settlement.Name} already belongs to {kingdom.Name}.");

				return CommandBase.ExecuteWithErrorHandling(() =>
							{
						string previousOwner = settlement.OwnerClan?.Name?.ToString() ?? "None";
						string previousKingdom = (settlement.MapFaction as Kingdom)?.Name?.ToString() ?? "None";

						var eligibleClans = kingdom.Clans.Where(c => !c.IsEliminated && c.Leader != null && c.Leader.IsAlive).ToList();
						if (eligibleClans.Count == 0)
							return CommandBase.FormatErrorMessage($"No valid clan found in {kingdom.Name} to receive the settlement.");

						var targetClan = eligibleClans.GetRandomElementInefficiently();
						ChangeOwnerOfSettlementAction.ApplyByGift(settlement, targetClan.Leader);

						return CommandBase.FormatSuccessMessage(
											$"{settlement.Name} transferred from {previousKingdom} ({previousOwner}) to {kingdom.Name} ({targetClan.Name}).");
					}, "Failed to transfer settlement");
			});
		}

		/// <summary>
		/// Transfer a settlement to a specific clan
		/// Usage: gm.kingdom.give_fief_to_clan [settlement] [clan]
		/// </summary>
		[CommandLineFunctionality.CommandLineArgumentFunction("give_fief_to_clan", "gm.kingdom")]
		public static string GiveFiefToClan(List<string> args)
		{
			return Cmd.Run(args, () =>
			{
				if (!CommandBase.ValidateCampaignMode(out string error))
					return error;

				var usageMessage = CommandValidator.CreateUsageMessage(
									"gm.kingdom.give_fief_to_clan", "<settlement> <clan>",
									"Transfers a settlement to a specific clan.",
									"gm.kingdom.give_fief_to_clan town_empire_1 clan_battania_1");

				if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
					return error;

				Settlement settlement = Settlement.Find(args[0]);
				if (settlement == null)
					return CommandBase.FormatErrorMessage($"Settlement '{args[0]}' not found.");

				if (!settlement.IsTown && !settlement.IsCastle)
					return CommandBase.FormatErrorMessage($"{settlement.Name} is not a town or castle.");

				var (clan, clanError) = CommandBase.FindSingleClan(args[1]);
				if (clanError != null) return clanError;

				if (clan.Leader == null || !clan.Leader.IsAlive)
					return CommandBase.FormatErrorMessage($"{clan.Name} has no living leader to receive the settlement.");

				return CommandBase.ExecuteWithErrorHandling(() =>
							{
						string previousOwner = settlement.OwnerClan?.Name?.ToString() ?? "None";
						string previousKingdom = (settlement.MapFaction as Kingdom)?.Name?.ToString() ?? "None";

						ChangeOwnerOfSettlementAction.ApplyByGift(settlement, clan.Leader);

						return CommandBase.FormatSuccessMessage(
											$"{settlement.Name} transferred from {previousKingdom} ({previousOwner}) to {clan.Name}.");
					}, "Failed to transfer settlement");
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
									"Changes the kingdom ruler.",
									"gm.kingdom.set_ruler empire lord_1_1");

				if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
					return error;

				var (kingdom, kingdomError) = CommandBase.FindSingleKingdom(args[0]);
				if (kingdomError != null) return kingdomError;

				var (hero, heroError) = CommandBase.FindSingleHero(args[1]);
				if (heroError != null) return heroError;

				if (hero.MapFaction != kingdom)
					return CommandBase.FormatErrorMessage($"{hero.Name} is not part of {kingdom.Name}.");

				if (hero.Clan == null)
					return CommandBase.FormatErrorMessage($"{hero.Name} has no clan.");

				return CommandBase.ExecuteWithErrorHandling(() =>
							{
						string previousRuler = kingdom.Leader?.Name?.ToString() ?? "None";

						kingdom.RulingClan = hero.Clan;
						if (hero.Clan.Leader != hero)
							hero.Clan.SetLeader(hero);

						return CommandBase.FormatSuccessMessage(
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
									"Destroys/eliminates the specified kingdom.",
									"gm.kingdom.destroy battania");

				if (!CommandBase.ValidateArgumentCount(args, 1, usageMessage, out error))
					return error;

				var (kingdom, kingdomError) = CommandBase.FindSingleKingdom(args[0]);
				if (kingdomError != null) return kingdomError;

				if (kingdom.IsEliminated)
					return CommandBase.FormatErrorMessage($"{kingdom.Name} is already eliminated.");

				if (kingdom == Hero.MainHero.MapFaction)
					return CommandBase.FormatErrorMessage("Cannot destroy the player's kingdom.");

				return CommandBase.ExecuteWithErrorHandling(() =>
							{
						DestroyKingdomAction.Apply(kingdom);
						return CommandBase.FormatSuccessMessage($"{kingdom.Name} has been destroyed/eliminated.");
					}, "Failed to destroy kingdom");
			});
		}

		#endregion
	}
}