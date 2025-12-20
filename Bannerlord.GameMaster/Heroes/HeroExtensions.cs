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

namespace Bannerlord.GameMaster.Heroes
{
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
		/// Returns a formatted string containing the hero's details
		/// </summary>
		public static string FormattedDetails(this Hero hero)
		{
			return $"{hero.StringId}\t{hero.Name}\tClan: {hero.Clan?.Name}\tKingdom: {hero.Clan?.Kingdom?.Name}";
		}

		/// <summary>
		/// Alias for GetHeroTypes to match IEntityExtensions interface
		/// </summary>
		public static HeroTypes GetTypes(this Hero hero) => hero.GetHeroTypes();

		public static MobileParty CreateParty(this Hero hero, Settlement spawnSettlement)
		{
			MobileParty party = LordPartyComponent.CreateLordParty(
			stringId: "party_" + hero.StringId,
			hero: hero,
			position: spawnSettlement.GatePosition,
			spawnRadius: 0.5f,
			spawnSettlement: spawnSettlement,
			partyLeader: hero
			);
			
			party.DesiredAiNavigationType = MobileParty.NavigationType.All;
			party.Aggressiveness = Math.Max(0.3f, RandomNumberGen.Instance.NextRandomFloat());
			party.PartyTradeGold = 20000;
			//party.PartyMoveMode = MoveModeType.Party; // For caravans?

			//party.InitializeMobilePartyAtPosition(hero.Clan.DefaultPartyTemplate, spawnSettlement.GatePosition);
			party.AddElementToMemberRoster(hero.Culture.BasicTroop, 10);
			party.Ai.EnableAi();
			party.SetMovePatrolAroundSettlement(spawnSettlement, party.DesiredAiNavigationType, false);

			return party;
		}

		/// <summary>
		/// Returns the Hero's home settlement if not null, otherwise grabs a random settlement in this order: <br/>
		/// Home Settlement > Random Clan Owned Settlement > Random Kingdom Owned Settlement > Random Settlement
		/// </summary>
		public static Settlement GetHomeOrAlternativeSettlement(this Hero hero)
		{
			if (hero.HomeSettlement != null)
				return hero.HomeSettlement;

			Settlement alternativeSettlement;
			if (hero.Clan != null && hero.Clan.Settlements != null && hero.Clan.Settlements[0] != null)
			{
				int randomIndex = RandomNumberGen.Instance.NextRandomInt(hero.Clan.Settlements.Count);
				alternativeSettlement = hero.Clan.Settlements[randomIndex];
			}

			else if (hero.Clan != null && hero.Clan.Kingdom != null && hero.Clan.Kingdom.Settlements != null && hero.Clan.Kingdom.Settlements[0] != null)
			{
				int randomIndex = RandomNumberGen.Instance.NextRandomInt(hero.Clan.Kingdom.Settlements.Count);
				alternativeSettlement = hero.Clan.Kingdom.Settlements[randomIndex];
			}

			else
			{
				int randomIndex = RandomNumberGen.Instance.NextRandomInt(Settlement.All.Count);
				alternativeSettlement = Settlement.All[randomIndex];
			}

			return alternativeSettlement;
		}

		/// <summary>
		/// Set heroes name using a string instead of TextObject
		/// </summary>
		public static void SetStringName(this Hero hero, string name)
		{
			TextObject nameObj = new(name);
			hero.SetName(nameObj, nameObj);
		}

		public static void EquipHeroBasedOnCulture(this Hero hero)
		{
			int randomBattleIndex = RandomNumberGen.Instance.NextRandomInt(hero.Culture.DuelPresetEquipmentRoster.AllEquipments.Count);
			int randomCivilianIndex = RandomNumberGen.Instance.NextRandomInt(hero.Culture.DefaultCivilianEquipmentRoster.AllEquipments.Count);

			hero.BattleEquipment.FillFrom(hero.Culture.DuelPresetEquipmentRoster.AllEquipments[randomBattleIndex]);
			hero.CivilianEquipment.FillFrom(hero.Culture.DefaultCivilianEquipmentRoster.AllEquipments[randomCivilianIndex]);
		}
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