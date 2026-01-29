using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using Bannerlord.GameMaster.Common.Interfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using SandBox.Issues;
using TaleWorlds.Localization;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using Helpers;
using Bannerlord.GameMaster.Settlements;
using TaleWorlds.Library;
using Bannerlord.GameMaster.Common;
using System.Drawing;
using Bannerlord.GameMaster.Party;
using TaleWorlds.CampaignSystem.ViewModelCollection.WeaponCrafting.WeaponDesign;
using Bannerlord.GameMaster.Items;

namespace Bannerlord.GameMaster.Heroes
{
	#region Flags / Types

	[Flags]
	public enum HeroTypes
	{
		None = 0,
		IsArtisan = 1,
		Lord = 2,
		Wanderer = 4,
		Notable = 8,
		Merchant = 16, Children = 32,
		Female = 64,
		Male = 128,
		ClanLeader = 256,
		KingdomRuler = 512,
		PartyLeader = 1024,
		Fugitive = 2048,
		Alive = 4096,
		Dead = 8192,
		Prisoner = 16384,
		WithoutClan = 32768,
		WithoutKingdom = 65536,
		Married = 131072,
	}

	public static class HeroExtensions
	{
		/// <summary>
		/// Gets all hero type flags for this hero
		/// </summary>
		public static HeroTypes GetHeroTypes(this Hero hero)
		{
			HeroTypes types = HeroTypes.None;

			if (hero.IsArtisan) types |= HeroTypes.IsArtisan;
			if (hero.IsLord) types |= HeroTypes.Lord;
			if (hero.IsWanderer) types |= HeroTypes.Wanderer;
			if (hero.IsNotable) types |= HeroTypes.Notable;
			if (hero.IsMerchant) types |= HeroTypes.Merchant;
			if (hero.IsChild) types |= HeroTypes.Children;
			if (hero.IsFemale) types |= HeroTypes.Female;
			if (!hero.IsFemale) types |= HeroTypes.Male;
			if (hero.Clan?.Leader == hero) types |= HeroTypes.ClanLeader;
			if (hero.Clan?.Kingdom?.Leader == hero) types |= HeroTypes.KingdomRuler;
			if (hero.PartyBelongedTo?.LeaderHero == hero) types |= HeroTypes.PartyLeader;
			if (hero.IsFugitive) types |= HeroTypes.Fugitive;
			if (hero.IsAlive) types |= HeroTypes.Alive;
			if (!hero.IsAlive) types |= HeroTypes.Dead;
			if (hero.IsPrisoner) types |= HeroTypes.Prisoner;
			if (hero.Clan == null) types |= HeroTypes.WithoutClan;
			if (hero.Clan?.Kingdom == null) types |= HeroTypes.WithoutKingdom;
			if (hero.Spouse != null) types |= HeroTypes.Married;

			return types;
		}

		/// <summary>
		/// Checks if hero has ALL specified flags
		/// </summary>
		public static bool HasAllTypes(this Hero hero, HeroTypes types)
		{
			if (types == HeroTypes.None) return true;
			var heroTypes = hero.GetHeroTypes();
			return (heroTypes & types) == types;
		}

		/// <summary>
		/// Checks if hero has ANY of the specified flags
		/// </summary>
		public static bool HasAnyType(this Hero hero, HeroTypes types)
		{
			if (types == HeroTypes.None) return true;
			var heroTypes = hero.GetHeroTypes();
			return (heroTypes & types) != HeroTypes.None;
		}

		/// <summary>
		/// Alias for GetHeroTypes to match IEntityExtensions interface
		/// </summary>
		public static HeroTypes GetTypes(this Hero hero) => hero.GetHeroTypes();

	#endregion
		#region Party

		/// <summary>
		/// Creates a party, and configuring AI to work as a normal party.<br/>
		/// </summary>
		public static MobileParty CreateParty(this Hero hero, Settlement spawnSettlement)
		{
			return MobilePartyGenerator.CreateLordParty(hero, spawnSettlement);
		}

		#endregion
		#region Settlement

		/// <summary>
		/// Sets heroes born, home, lastknownclosest and attempts to set home settlement directly to specified settlement <br />
		/// If setting home settlement directly fails, UpdateHomeSettlement() is called instead to set home settlement <br />
		/// Do not call hero.UpdateHomeSettlement() after this, as it will overwrite the Home Settlement <br /><br />
		/// If called with null homeSettlement, Overload InitializeHomeSettlement(this Hero hero) is called automatically, selecting a random clan > kingdom > all settlement
		/// </summary>
		/// <returns>The resulting new home settlement of the hero</returns>
		public static Settlement InitializeHomeSettlement(this Hero hero, Settlement homeSettlement)
		{
			if (homeSettlement == null)
				return InitializeHomeSettlement(hero);

			hero.BornSettlement = homeSettlement;
			hero.UpdateLastKnownClosestSettlement(homeSettlement);

			// Directly set settlement using reflection
			BLGMResult result = HeroManager.TrySetHomeSettlement(hero, homeSettlement);

			// if reflection fails call UpdateHomeSettlement instead
			if (!result.IsSuccess || hero.HomeSettlement != homeSettlement)
			{
				hero.UpdateHomeSettlement();
				Debug.Print($"{result.Message}\nTargetSettlement: {homeSettlement}, ActualSettlement: {hero.HomeSettlement}", color: Debug.DebugColor.Red);
			}

			return hero.HomeSettlement;
		}

		/// <summary>
		/// Sets heroes born, home, lastknownclosest a random settlment in this order: From Heroes Clan > From Heroes Kingdom > From All Settlements.
		/// Also attempts to set home settlement directly using reflection, if it fails, UpdateHomeSettlement() is called instead <br />
		/// Do not call hero.UpdateHomeSettlement() after this, as it will overwrite the Home Settlement
		/// <returns>The resulting new home settlement of the hero</returns>
		/// </summary>
		public static Settlement InitializeHomeSettlement(this Hero hero)
		{
			Settlement homeSettlement;

			// Get a random Clan > Kingdom > All Settlements
			homeSettlement = HeroManager.GetBestInitialSettlement(hero);

			return InitializeHomeSettlement(hero, homeSettlement);
		}

		/// <summary>
		/// Returns the Hero's home settlement if not null, otherwise grabs a random settlement in this order: <br/>
		/// Home Settlement > Random Clan Owned Settlement > Random Kingdom Owned Settlement > Random Settlement
		/// </summary>
		public static Settlement GetHomeOrAlternativeSettlement(this Hero hero)
		{
			//Prefer actual home settlement
			if (hero.HomeSettlement != null)
				return hero.HomeSettlement;

			// Get Random Clan > Kingdom > All Settlement
			return HeroManager.GetBestInitialSettlement(hero);
		}

		#endregion
		#region Equipment

		/// <summary>
		/// Outfits hero with apropiate gear considering stats, level, and culture.
		/// Uses the new stat-based equipment generation system. <br />
		/// Use HeroOutfitter class for more control
		/// </summary>
		public static void AutoEquipHero(this Hero hero, bool replaceCivilianEquipment)
		{
			HeroOutfitter.EquipHeroByStats(hero, replaceCivilianEquipment: replaceCivilianEquipment);
		}

		/// <summary>
		/// Obsolete, Simply reroutes to new method HeroOutfitter.EquipHeroByStats without replacing civillian gear.
		/// </summary>
		/// <param name="hero">The hero to equip.</param>
		[Obsolete("EquipHeroBasedOnCulture is deprecated. Use methods in new class HeroOutfitter.", false)]
		public static void EquipHeroBasedOnCulture(this Hero hero)
		{
			HeroOutfitter.EquipHeroByStats(hero, replaceCivilianEquipment: false);
		}

		/// <summary>
		/// Obsolete, simply reroutes to new method HeroOutfitter.EquipHeroByStats without replacing civillian gear.
		/// </summary>
		/// <param name="hero">The hero to equip as a lord.</param>
		[Obsolete("EquipLordBasedOnCulture is deprecated. Use methods in new class HeroOutfitter.", false)]
		public static void EquipLordBasedOnCulture(this Hero hero)
		{
			// Lords get at least tier 5 equipment, or higher if their level warrants it
			int baseTier = HeroOutfitter.GetTierForHero(hero);
			int lordTier = baseTier < 5 ? 5 : baseTier;
			HeroOutfitter.EquipHeroByStats(hero, lordTier, replaceCivilianEquipment: true);
		}

		#endregion

		/// <summary>
		/// Sets the hero age by setting a random birthdate based on the specified age
		/// </summary>
		/// <returns>The CampaignTime specifying the Heroes new birth date</returns>
		public static CampaignTime SetAge(this Hero hero, int age)
		{
			CampaignTime birthDate = HeroHelper.GetRandomBirthDayForAge(age);
			hero.SetBirthDay(birthDate);

			return birthDate;
		}

		/// <summary>
		/// Sets a date for the hero to die of old age, The date is a random day with year of the character reaching a random age of 55 to 92.<br />
		/// Random age is weighted to slightly favor ages below 80.
		/// </summary>
		/// <param name="hero"></param>
		/// <returns>The CampignTime containing the actual date the hero will die</returns>
		public static CampaignTime SetRandomDeathDate(this Hero hero)
		{
			int randomDeathAge = RandomNumberGen.Instance.NextRandomInt(55, 92);

			// Reduce likelihood of living longer than 80 by requiring two rolls to pass
			if (randomDeathAge > 79)
				randomDeathAge = RandomNumberGen.Instance.NextRandomInt(65, 92);

			// Calculate years until the hero reaches death age
			int yearsUntilDeath = randomDeathAge - (int)hero.Age;

			// Get a base date that many years from now
			CampaignTime deathDay = CampaignTime.YearsFromNow(yearsUntilDeath);

			// Add random days within that year (so hero doesn't die on exact anniversary)
			int randomDays = RandomNumberGen.Instance.NextRandomInt(0, CampaignTime.DaysInYear);
			deathDay += CampaignTime.Days(randomDays);

			hero.SetDeathDay(deathDay);
			return deathDay;
		}

		#region Name / Details

		/// <summary>
		/// Set heroes name using a string instead of TextObject
		/// </summary>
		public static void SetStringName(this Hero hero, string name)
		{
			TextObject nameObj = new(name);
			hero.SetName(nameObj, nameObj);
		}

		/// <summary>
		/// Returns a formatted string containing the hero's details
		/// </summary>
		public static string FormattedDetails(this Hero hero)
		{
			return $"{hero.StringId}\t{hero.Name}\tCulture: {hero.Culture?.Name}\tClan: {hero.Clan?.Name}\tKingdom: {hero.Clan?.Kingdom?.Name}";
		}
		
		#endregion
	}

	/// <summary>
	/// Wrapper class implementing IEntityExtensions interface for Hero entities
	/// </summary>
	public class HeroExtensionsWrapper : IEntityExtensions<Hero, HeroTypes>
	{
		public HeroTypes GetTypes(Hero entity) => entity.GetHeroTypes();
		public bool HasAllTypes(Hero entity, HeroTypes types) => entity.HasAllTypes(types);
		public bool HasAnyType(Hero entity, HeroTypes types) => entity.HasAnyType(types);
		public string FormattedDetails(Hero entity) => entity.FormattedDetails();
	}
}