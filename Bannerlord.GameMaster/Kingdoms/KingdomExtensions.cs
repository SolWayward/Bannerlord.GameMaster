using System;
using System.Collections.Generic;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using Bannerlord.GameMaster.Clans;
using Bannerlord.GameMaster.Common;
using Bannerlord.GameMaster.Common.Interfaces;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Kingdoms
{
	/// MARK: Types
	[Flags]
	public enum KingdomTypes
	{
		None = 0,
		Active = 1,
		Eliminated = 2,
		Empty = 4,
		PlayerKingdom = 8,
		AtWar = 16,
	}

	public static class KingdomExtensions
	{
		/// MARK: Kingdom Types
		/// <summary>
		/// Gets all kingdom type flags for this kingdom
		/// </summary>
		public static KingdomTypes GetKingdomTypes(this Kingdom kingdom)
		{
			KingdomTypes types = KingdomTypes.None;

			if (kingdom.IsEliminated)
				types |= KingdomTypes.Eliminated;
			else
				types |= KingdomTypes.Active;

			// Check if kingdom has no clans or heroes
			if (kingdom.Clans.Count == 0 || kingdom.Heroes.Count == 0)
				types |= KingdomTypes.Empty;

			if (kingdom == Hero.MainHero.MapFaction as Kingdom)
				types |= KingdomTypes.PlayerKingdom;

			// Check if at war with anyone
			bool atWar = false;
			MBReadOnlyList<Kingdom> allKingdoms = Kingdom.All;
			for (int i = 0; i < allKingdoms.Count; i++)
			{
				Kingdom otherKingdom = allKingdoms[i];
				if (otherKingdom != kingdom && FactionManager.IsAtWarAgainstFaction(kingdom, otherKingdom))
				{
					atWar = true;
					break;
				}
			}
			if (atWar)
				types |= KingdomTypes.AtWar;

			return types;
		}

		/// <summary>
		/// Checks if kingdom has ALL specified flags
		/// </summary>
		public static bool HasAllTypes(this Kingdom kingdom, KingdomTypes types)
		{
			if (types == KingdomTypes.None) return true;
			KingdomTypes kingdomTypes = kingdom.GetKingdomTypes();
			return (kingdomTypes & types) == types;
		}

		/// <summary>
		/// Checks if kingdom has ANY of the specified flags
		/// </summary>
		public static bool HasAnyType(this Kingdom kingdom, KingdomTypes types)
		{
			if (types == KingdomTypes.None) return true;
			KingdomTypes kingdomTypes = kingdom.GetKingdomTypes();
			return (kingdomTypes & types) != KingdomTypes.None;
		}

		/// MARK: Get Settlements
		/// <summary>Gets all Fortifications (Towns and Castles) within the Kingdom</summary>
		public static MBReadOnlyList<Settlement> GetFortifications(this Kingdom kingdom)
		{
			MBList<Settlement> result = new();
			for (int i = 0; i < kingdom.Settlements.Count; i++)
			{
				Settlement settlement = kingdom.Settlements[i];
				if (settlement.IsFortification)
					result.Add(settlement);
			}
			return result;
		}
	
		/// <summary>Gets all Towns within the Kingdom</summary>
		public static MBReadOnlyList<Settlement> GetTowns(this Kingdom kingdom)
		{
			MBList<Settlement> result = new();
			for (int i = 0; i < kingdom.Settlements.Count; i++)
			{
				Settlement settlement = kingdom.Settlements[i];
				if (settlement.IsTown)
					result.Add(settlement);
			}
			return result;
		}
	
		/// <summary>Gets all Castles within the Kingdom</summary>
		public static MBReadOnlyList<Settlement> GetCastles(this Kingdom kingdom)
		{
			MBList<Settlement> result = new();
			for (int i = 0; i < kingdom.Settlements.Count; i++)
			{
				Settlement settlement = kingdom.Settlements[i];
				if (settlement.IsCastle)
					result.Add(settlement);
			}
			return result;
		}
	
		/// <summary>Gets all Villages within the Kingdom</summary>
		public static MBReadOnlyList<Settlement> GetVillages(this Kingdom kingdom)
		{
			MBList<Settlement> result = new();
			for (int i = 0; i < kingdom.Settlements.Count; i++)
			{
				Settlement settlement = kingdom.Settlements[i];
				if (settlement.IsVillage)
					result.Add(settlement);
			}
			return result;
		}

		/// MARK: Formatted Details
		/// <summary>
		/// Returns a formatted string containing the kingdom's details
		/// </summary>
		public static string FormattedDetails(this Kingdom kingdom)
		{
			int heroCount = kingdom.Heroes.Count;
			return $"{kingdom.StringId}\t{kingdom.Name}\tClans: {kingdom.Clans.Count}\tHeroes: {heroCount}\t" +
				   $"RulingClan: {kingdom.RulingClan?.Name}\tRuler: {kingdom.Leader?.Name}";
		}

		/// <summary>
		/// Alias for GetKingdomTypes to match IEntityExtensions interface
		/// </summary>
		public static KingdomTypes GetTypes(this Kingdom kingdom) => kingdom.GetKingdomTypes();

		#region Kingdom Banner Propagation

		// MARK: Cached Reflection - Kingdom
		private static readonly Type kingdomType = typeof(Kingdom);

		private static readonly PropertyInfo primaryBannerColorProp = kingdomType.GetProperty(
			"PrimaryBannerColor",
			BindingFlags.Public | BindingFlags.Instance);

		private static readonly PropertyInfo secondaryBannerColorProp = kingdomType.GetProperty(
			"SecondaryBannerColor",
			BindingFlags.Public | BindingFlags.Instance);

		/// MARK: PropagateRulingClanBanner
		/// <summary>
		/// Propagates the ruling clan's banner colors to this kingdom and all vassal clans.
		/// Should be called after modifying a ruling clan's banner to ensure consistency.
		/// Uses reflection to set Kingdom.PrimaryBannerColor (banner background) and
		/// Kingdom.SecondaryBannerColor (icon color), then triggers banner color updates on all kingdom clans.
		/// Note: Native game only uses primary (background) and icon colors for kingdom propagation - secondary background is NOT used.
		/// </summary>
		/// <param name="kingdom">The kingdom to propagate banner colors to.</param>
		/// <returns>BLGMResult indicating success or failure.</returns>
		public static BLGMResult PropagateRulingClanBanner(this Kingdom kingdom)
		{
			if (kingdom == null)
			{
				BLGMResult.Error("PropagateRulingClanBanner() failed, kingdom cannot be null",
					new ArgumentNullException(nameof(kingdom))).Log();
				return BLGMResult.Error("Kingdom cannot be null");
			}

			Clan rulingClan = kingdom.RulingClan;
			if (rulingClan == null)
			{
				BLGMResult.Error("PropagateRulingClanBanner() failed, kingdom has no ruling clan",
					new InvalidOperationException("Kingdom has no ruling clan")).Log();
				return BLGMResult.Error("Kingdom has no ruling clan");
			}

			Banner banner = rulingClan.Banner;
			if (banner == null)
			{
				BLGMResult.Error("PropagateRulingClanBanner() failed, ruling clan has no banner",
					new InvalidOperationException("Ruling clan has no banner")).Log();
				return BLGMResult.Error("Ruling clan has no banner");
			}

			// Get colors from the ruling clan's banner
			// Note: Native only uses PrimaryBannerColor (background) and SecondaryBannerColor (icon)
			uint newPrimaryColor = banner.GetPrimaryColor();
			uint newSecondaryColor = banner.GetFirstIconColor();

			// Update kingdom banner colors using reflection
			BLGMResult kingdomResult = SetBannerColors(kingdom, newPrimaryColor, newSecondaryColor);
			if (!kingdomResult.IsSuccess)
				return kingdomResult;

			// Update all vassal clans to use the new kingdom colors
			int clansUpdated = 0;
			foreach (Clan clan in kingdom.Clans)
			{
				BLGMResult clanResult = clan.UpdateBannerColorsForKingdom();
				if (clanResult.IsSuccess)
					clansUpdated++;
			}

			return BLGMResult.Success(
				$"Propagated banner colors to kingdom '{kingdom.Name}' and {clansUpdated} clan(s)");
		}

		/// MARK: SetBannerColors
		/// <summary>
		/// Sets this kingdom's PrimaryBannerColor and SecondaryBannerColor properties using reflection.
		/// These properties have private setters in native code.
		/// </summary>
		/// <param name="kingdom">The kingdom to set banner colors for.</param>
		/// <param name="primaryColor">The primary banner color (background).</param>
		/// <param name="secondaryColor">The secondary banner color (icon).</param>
		/// <returns>BLGMResult indicating success or failure.</returns>
		public static BLGMResult SetBannerColors(this Kingdom kingdom, uint primaryColor, uint secondaryColor)
		{
			if (kingdom == null)
			{
				BLGMResult.Error("SetBannerColors() failed, kingdom cannot be null",
					new ArgumentNullException(nameof(kingdom))).Log();
				return BLGMResult.Error("Kingdom cannot be null");
			}

			if (primaryBannerColorProp == null || secondaryBannerColorProp == null)
			{
				BLGMResult.Error("SetBannerColors() failed, reflection properties not found",
					new InvalidOperationException("Kingdom banner color properties not found via reflection")).Log();
				return BLGMResult.Error("Failed to find kingdom banner color properties via reflection");
			}

			try
			{
				primaryBannerColorProp.SetValue(kingdom, primaryColor);
				secondaryBannerColorProp.SetValue(kingdom, secondaryColor);

				return BLGMResult.Success($"Set kingdom banner colors for '{kingdom.Name}'");
			}
			catch (Exception ex)
			{
				BLGMResult.Error($"SetBannerColors() failed for '{kingdom.Name}'", ex).Log();
				return BLGMResult.Error($"Failed to set kingdom banner colors: {ex.Message}");
			}
		}

		#endregion
	}

	/// MARK: Wrapper
	/// <summary>
	/// Wrapper class implementing IEntityExtensions interface for Kingdom entities
	/// </summary>
	public class KingdomExtensionsWrapper : IEntityExtensions<Kingdom, KingdomTypes>
	{
		public KingdomTypes GetTypes(Kingdom entity) => entity.GetKingdomTypes();
		public bool HasAllTypes(Kingdom entity, KingdomTypes types) => entity.HasAllTypes(types);
		public bool HasAnyType(Kingdom entity, KingdomTypes types) => entity.HasAnyType(types);
		public string FormattedDetails(Kingdom entity) => entity.FormattedDetails();
	}
}