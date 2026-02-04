using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using Bannerlord.GameMaster.Common;
using Bannerlord.GameMaster.Common.Interfaces;
using TaleWorlds.Localization;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem.Actions;

namespace Bannerlord.GameMaster.Clans
{
	#region Flags and Types

	[Flags]
	public enum ClanTypes
	{
		None = 0,
		Active = 1,
		Eliminated = 2,
		Bandit = 4,
		NonBandit = 8,
		MapFaction = 16,
		Noble = 32,
		MinorFaction = 64,
		Rebel = 128,
		Mercenary = 256,
		UnderMercenaryService = 512,
		Mafia = 1024,
		Outlaw = 2048,
		Nomad = 4096,
		Sect = 8192,
		WithoutKingdom = 16384,
		Empty = 32768,
		PlayerClan = 65536,
	}

	public static class ClanExtensions
	{
		/// <summary>
		/// Gets all clan type flags for this clan
		/// </summary>
		public static ClanTypes GetClanTypes(this Clan clan)
		{
			ClanTypes types = ClanTypes.None;

			if (clan.IsEliminated)
				types |= ClanTypes.Eliminated;
			else
				types |= ClanTypes.Active;

			if (clan.IsBanditFaction) types |= ClanTypes.Bandit;
			if (!clan.IsBanditFaction) types |= ClanTypes.NonBandit;
			if (clan.IsMapFaction) types |= ClanTypes.MapFaction;
			if (clan.IsNoble) types |= ClanTypes.Noble;
			if (clan.IsMinorFaction) types |= ClanTypes.MinorFaction;
			if (clan.IsRebelClan) types |= ClanTypes.Rebel;
			if (clan.IsClanTypeMercenary) types |= ClanTypes.Mercenary;
			if (clan.IsUnderMercenaryService) types |= ClanTypes.UnderMercenaryService;
			if (clan.IsMafia) types |= ClanTypes.Mafia;
			if (clan.IsOutlaw) types |= ClanTypes.Outlaw;
			if (clan.IsNomad) types |= ClanTypes.Nomad;
			if (clan.IsSect) types |= ClanTypes.Sect;
			if (clan.Kingdom == null) types |= ClanTypes.WithoutKingdom;
			if (clan.Heroes.Count == 0) types |= ClanTypes.Empty;
			if (clan == Clan.PlayerClan) types |= ClanTypes.PlayerClan;

			return types;
		}

		/// <summary>
		/// Checks if clan has ALL specified flags
		/// </summary>
		public static bool HasAllTypes(this Clan clan, ClanTypes types)
		{
			if (types == ClanTypes.None) return true;
			var clanTypes = clan.GetClanTypes();
			return (clanTypes & types) == types;
		}

		/// <summary>
		/// Checks if clan has ANY of the specified flags
		/// </summary>
		public static bool HasAnyType(this Clan clan, ClanTypes types)
		{
			if (types == ClanTypes.None) return true;
			var clanTypes = clan.GetClanTypes();
			return (clanTypes & types) != ClanTypes.None;
		}

		/// <summary>
		/// Alias for GetClanTypes to match IEntityExtensions interface
		/// </summary>
		public static ClanTypes GetTypes(this Clan clan) => clan.GetClanTypes();

	#endregion

		/// MARK: Get Settlements
		/// <summary>Gets all Fortifications (Towns and Castles) within the Clan</summary>
		public static MBReadOnlyList<Settlement> GetFortifications(this Clan clan)
		{
			MBList<Settlement> result = new();
			for (int i = 0; i < clan.Settlements.Count; i++)
			{
				Settlement settlement = clan.Settlements[i];
				if (settlement.IsFortification)
					result.Add(settlement);
			}
			return result;
		}
	
		/// <summary>Gets all Towns within the Clan</summary>
		public static MBReadOnlyList<Settlement> GetTowns(this Clan clan)
		{
			MBList<Settlement> result = new();
			for (int i = 0; i < clan.Settlements.Count; i++)
			{
				Settlement settlement = clan.Settlements[i];
				if (settlement.IsTown)
					result.Add(settlement);
			}
			return result;
		}
	
		/// <summary>Gets all Castles within the Clan</summary>
		public static MBReadOnlyList<Settlement> GetCastles(this Clan clan)
		{
			MBList<Settlement> result = new();
			for (int i = 0; i < clan.Settlements.Count; i++)
			{
				Settlement settlement = clan.Settlements[i];
				if (settlement.IsCastle)
					result.Add(settlement);
			}
			return result;
		}
	
		/// <summary>Gets all Villages within the Clan</summary>
		public static MBReadOnlyList<Settlement> GetVillages(this Clan clan)
		{
			MBList<Settlement> result = new();
			for (int i = 0; i < clan.Settlements.Count; i++)
			{
				Settlement settlement = clan.Settlements[i];
				if (settlement.IsVillage)
					result.Add(settlement);
			}
			return result;
		}

		/// <summary>
		/// Set clan tier to a specified tier between 0 and 6
		/// </summary>
		public static bool SetClanTier(this Clan clan, int targetTier)
		{
			// Clan already at target tier
			if (clan.Tier == targetTier)
				return false;

			// Invalid Tier
			if (targetTier < 0 || targetTier > 6)
				return false;

			// Allows clan tier to be lowered
			if (clan.Tier > targetTier)
				clan.ResetClanRenown();

			float requiredRenownForTargetTier = Campaign.Current.Models.ClanTierModel.GetRequiredRenownForTier(targetTier) - clan.Renown;
			clan.AddRenown(requiredRenownForTargetTier);

			return true;
		}

		/// <summary>
		/// Join kingdom default action
		/// </summary>
		public static void JoinKingdom(this Clan clan, Kingdom kingdom, bool showNotification = true)
		{
			TaleWorlds.CampaignSystem.Actions.ChangeKingdomAction.ApplyByJoinToKingdom(clan, kingdom, showNotification: showNotification);
		}

		/// <summary>
		/// Join as a mercenary clan
		/// </summary>
		public static void JoinKingdomAsMercenary(this Clan clan, Kingdom kingdom, bool showNotification = true)
		{
			TaleWorlds.CampaignSystem.Actions.ChangeKingdomAction.ApplyByJoinFactionAsMercenary(clan, kingdom, showNotification: showNotification);
		}

		/// <summary>
		/// Defect from one kingdom to another
		/// </summary>
		public static void DefectToKingdom(this Clan clan, Kingdom kingdom, Kingdom oldKindom, bool showNotification = true)
		{
			TaleWorlds.CampaignSystem.Actions.ChangeKingdomAction.ApplyByJoinToKingdomByDefection(clan, kingdom, oldKindom, showNotification: showNotification);
		}

		/// <summary>
		/// Leave kingdom
		/// </summary>
		public static void LeaveKingdom(this Clan clan, bool showNotification = true)
		{
			TaleWorlds.CampaignSystem.Actions.ChangeKingdomAction.ApplyByLeaveKingdom(clan, showNotification);
		}

		public static void DeclareWar(this Clan clan, IFaction targetFaction)
		{
			DeclareWarAction.ApplyByDefault(clan, targetFaction);
		}

		/// MARK: SetStringName
		/// <summary>
		/// Renames clan using a string instead of TextObject
		/// </summary>
		public static void SetStringName(this Clan clan, string name)
		{
			TextObject nameObj = new(name);
			clan.ChangeClanName(nameObj, nameObj);
		}

		/// Mark: Details and Wrapper
		/// <summary>
		/// Returns a formatted string containing the clan's details
		/// </summary>
		public static string FormattedDetails(this Clan clan)
		{
			return $"{clan.StringId}\t{clan.Name}\tHeroes: {clan.Heroes.Count()}\tLeader: {clan.Leader?.Name}\tKingdom: {clan.Kingdom?.Name}";
		}

		#region Kingdom Banner Propagation

		// MARK: Cached Reflection - Clan
		private static readonly Type clanType = typeof(Clan);

		private static readonly MethodInfo updateBannerColorsMethod = clanType.GetMethod(
			"UpdateBannerColorsAccordingToKingdom",
			BindingFlags.NonPublic | BindingFlags.Instance);

		/// MARK: UpdateBannerColorsForKingdom
		/// <summary>
		/// Updates this clan's banner colors to match its kingdom's PrimaryBannerColor and SecondaryBannerColor.
		/// This is a public wrapper for the native private Clan.UpdateBannerColorsAccordingToKingdom() method.
		/// </summary>
		/// <param name="clan">The clan whose banner colors should be updated.</param>
		/// <returns>BLGMResult indicating success or failure.</returns>
		public static BLGMResult UpdateBannerColorsForKingdom(this Clan clan)
		{
			if (clan == null)
			{
				BLGMResult.Error("UpdateBannerColorsForKingdom() failed, clan cannot be null",
					new ArgumentNullException(nameof(clan))).Log();
				return BLGMResult.Error("Clan cannot be null");
			}

			if (clan.Kingdom == null)
			{
				BLGMResult.Error("UpdateBannerColorsForKingdom() failed, clan has no kingdom",
					new InvalidOperationException("Clan has no kingdom")).Log();
				return BLGMResult.Error("Clan has no kingdom");
			}

			if (updateBannerColorsMethod == null)
			{
				BLGMResult.Error("UpdateBannerColorsForKingdom() failed, reflection method not found",
					new InvalidOperationException("Clan.UpdateBannerColorsAccordingToKingdom method not found via reflection")).Log();
				return BLGMResult.Error("Failed to find UpdateBannerColorsAccordingToKingdom method via reflection");
			}

			try
			{
				updateBannerColorsMethod.Invoke(clan, null);

				// Trigger visual refresh by accessing BannerVisual property
				IBannerVisual _ = clan.Banner?.BannerVisual;

				return BLGMResult.Success($"Updated banner colors for clan '{clan.Name}'");
			}
			catch (Exception ex)
			{
				BLGMResult.Error($"UpdateBannerColorsForKingdom() failed for clan '{clan.Name}'", ex).Log();
				return BLGMResult.Error($"Failed to update clan banner colors: {ex.Message}");
			}
		}

		/// MARK: IsRulingClan
		/// <summary>
		/// Checks if this clan is the ruling clan of its kingdom.
		/// </summary>
		/// <returns>True if the clan is the ruling clan of a kingdom, false otherwise.</returns>
		public static bool IsRulingClan(this Clan clan)
		{
			if (clan == null || clan.Kingdom == null)
				return false;

			return clan.Kingdom.RulingClan == clan;
		}

		#endregion

	}

	/// <summary>
	/// Wrapper class implementing IEntityExtensions interface for Clan entities
	/// </summary>
	public class ClanExtensionsWrapper : IEntityExtensions<Clan, ClanTypes>
	{
		public ClanTypes GetTypes(Clan entity) => entity.GetClanTypes();
		public bool HasAllTypes(Clan entity, ClanTypes types) => entity.HasAllTypes(types);
		public bool HasAnyType(Clan entity, ClanTypes types) => entity.HasAnyType(types);
		public string FormattedDetails(Clan entity) => entity.FormattedDetails();
	}
}