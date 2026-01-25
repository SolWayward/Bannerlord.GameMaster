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
		/// Equips hero with random pieces of different elite troop equipment based on hero's culture
		/// </summary>
		public static void EquipHeroBasedOnCulture(this Hero hero)
		{
			// Get a random high-tier troop from hero's culture for battle equipment
			var cultureTroops = CharacterObject.All
				.Where(c => c.Culture == hero.Culture
							&& c.IsSoldier
							&& !c.IsHero
							&& c.Tier >= 4  // Tier 4+ troops (elite)
							&& c.Equipment != null
							&& !c.Equipment[EquipmentIndex.Weapon0].IsEmpty)
				.ToList();

			if (cultureTroops.Count > 0)
			{
				var randomTroop = cultureTroops[RandomNumberGen.Instance.NextRandomInt(cultureTroops.Count)];
				hero.BattleEquipment.FillFrom(randomTroop.Equipment);
			}

			// Civilian equipment from culture roster
			var civilianRoster = hero.Culture.DefaultCivilianEquipmentRoster;
			if (civilianRoster != null && civilianRoster.AllEquipments.Count > 0)
			{
				int civilianIndex = RandomNumberGen.Instance.NextRandomInt(civilianRoster.AllEquipments.Count);
				hero.CivilianEquipment.FillFrom(civilianRoster.AllEquipments[civilianIndex]);
			}
		}

		/// <summary>
		/// Equips hero with random different pieces of gear from a pool of existing lords and high tier troops based on gender and culture of hero
		/// </summary>
		/// <param name="hero"></param>
		public static void EquipLordBasedOnCulture(this Hero hero)
		{
			// Gather equipment pool from lords and elite troops (same culture AND gender)
			var equipmentSources = new List<Equipment>();

			// Add equipment from lords (best quality) - same culture AND gender
			var cultureLords = Hero.AllAliveHeroes
				.Where(h => h.Culture == hero.Culture
							&& h.IsFemale == hero.IsFemale  // Same gender
							&& h.IsLord
							&& h != hero
							&& h.BattleEquipment != null)
				.Take(20);  // Limit to avoid performance issues

			foreach (var lord in cultureLords)
			{
				if (!lord.BattleEquipment[EquipmentIndex.Weapon0].IsEmpty)
					equipmentSources.Add(lord.BattleEquipment);
			}

			// Add equipment from tier 5 troops (elite quality) - same culture AND gender
			var eliteTroops = CharacterObject.All
				.Where(c => c.Culture == hero.Culture
							&& c.IsFemale == hero.IsFemale  // Same gender
							&& c.IsSoldier
							&& !c.IsHero
							&& c.Tier >= 5
							&& c.Equipment != null
							&& !c.Equipment[EquipmentIndex.Weapon0].IsEmpty)
				.Take(20);

			foreach (var troop in eliteTroops)
			{
				equipmentSources.Add(troop.Equipment);
			}

			// Build randomized battle equipment by mixing pieces
			if (equipmentSources.Count > 0)
			{
				for (int i = 0; i < 12; i++)  // All equipment slots
				{
					var randomSource = equipmentSources[RandomNumberGen.Instance.NextRandomInt(equipmentSources.Count)];
					var equipmentElement = randomSource[i];

					if (!equipmentElement.IsEmpty)
						hero.BattleEquipment[i] = equipmentElement;
				}
			}

			// Civilian equipment - mix from lords' civilian equipment (same gender)
			var civilianSources = cultureLords
				.Where(l => l.CivilianEquipment != null && !l.CivilianEquipment[EquipmentIndex.Body].IsEmpty)
				.Select(l => l.CivilianEquipment)
				.ToList();

			if (civilianSources.Count > 0)
			{
				for (int i = 0; i < 12; i++)
				{
					var randomSource = civilianSources[RandomNumberGen.Instance.NextRandomInt(civilianSources.Count)];
					var equipmentElement = randomSource[i];

					if (!equipmentElement.IsEmpty)
						hero.CivilianEquipment[i] = equipmentElement;
				}
			}
			else
			{
				// Fallback to culture civilian roster
				var civilianRoster = hero.Culture.DefaultCivilianEquipmentRoster;
				if (civilianRoster != null && civilianRoster.AllEquipments.Count > 0)
				{
					int civilianIndex = RandomNumberGen.Instance.NextRandomInt(civilianRoster.AllEquipments.Count);
					hero.CivilianEquipment.FillFrom(civilianRoster.AllEquipments[civilianIndex]);
				}
			}
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