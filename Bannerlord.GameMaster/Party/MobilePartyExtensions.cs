using System;
using System.Collections.Generic;
using System.Linq;
using Bannerlord.GameMaster.Console.TroopCommands;
using Bannerlord.GameMaster.Troops;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Party
{
	public static class MobilePartyExtensions
	{
		/// MARK: Companions
		/// <summary>
		/// Adds the specified hero as a companion to the leader's party
		/// </summary>
		public static void AddCompanionToParty(this MobileParty mobileParty, Hero hero)
		{
			// Use the game's built-in action which handles CompanionOf properly
			AddCompanionAction.Apply(mobileParty.LeaderHero.Clan, hero);
			mobileParty.AddElementToMemberRoster(hero.CharacterObject, 1);
			hero.ChangeState(Hero.CharacterStates.Active);
		}

		/// <summary>
		/// Adds the specified list of companions to the leader's party
		/// </summary>
		public static void AddCompanionsToParty(this MobileParty mobileParty, List<Hero> heroes)
		{
			foreach (Hero hero in heroes)
				mobileParty.AddCompanionToParty(hero);
		}

		/// MARK: Lords
		/// <summary>
		/// Adds the specified lord to the leader's party
		/// </summary>
		public static void AddLordToParty(this MobileParty mobileParty, Hero hero)
		{
			//hero.Clan = mobileParty.LeaderHero.Clan;  AddHeroToParty action should automatically make them a clan member		
			//mobileParty.AddElementToMemberRoster(hero.CharacterObject, 1);
			AddHeroToPartyAction.Apply(hero, mobileParty, true);
			hero.ChangeState(Hero.CharacterStates.Active); // Needed if not manually adding to roster?
		}

		/// <summary>
		/// Adds the specified list of lords to the leader's party
		/// </summary>
		public static void AddLordsToParty(this MobileParty mobileParty, List<Hero> heroes)
		{
			foreach (Hero hero in heroes)
				mobileParty.AddLordToParty(hero);
		}


		/// MARK: Troops
		/// <summary>
		/// Adds the specified ammount of elite troops from the party leader's culture
		/// </summary>
		public static void AddBasicTroops(this MobileParty mobileParty, int count)
		{
			if (mobileParty.LeaderHero.Culture.BasicTroop != null)
				mobileParty.AddElementToMemberRoster(mobileParty.LeaderHero.Culture.BasicTroop, count);
		}

		/// <summary>
		/// Adds the specified ammount of elite troops from the party leader's culture
		/// </summary>
		public static void AddEliteTroops(this MobileParty mobileParty, int count)
		{
			if (mobileParty.LeaderHero.Culture.EliteBasicTroop != null)
				mobileParty.AddElementToMemberRoster(mobileParty.LeaderHero.Culture.EliteBasicTroop, count);
		}

		/// <summary>
		/// Randomly selects the specified count of mercenary troops from party leader's culture.
		/// </summary>
		public static void AddMercenaryTroops(this MobileParty mobileParty, int count)
		{
			List<CharacterObject> mercenaryRoster = mobileParty.LeaderHero.Culture.BasicMercenaryTroops;

			for (int i = 0; i < count; i++)
			{
				int randomIndex = RandomNumberGen.Instance.NextRandomInt(mercenaryRoster.Count);
				mobileParty.AddElementToMemberRoster(mercenaryRoster[randomIndex], 1);
			}
		}

		/// <summary>
		/// Adds the specified ammount each of basic, elite, and mercenary troops from the party leader's culture<br/>
		/// countOfEach = 10 : Will add 30 troops. 10 basic, 10 elite, 10 mercenary.
		/// </summary>
		public static void AddMixedTierTroops(this MobileParty mobileParty, int countOfEach)
		{
			AddBasicTroops(mobileParty, countOfEach);
			AddEliteTroops(mobileParty, countOfEach);
			AddMercenaryTroops(mobileParty, countOfEach);
		}

		// MARK: UpgradeTroops
		/// <summary>
		/// Upgrades all troops in the party to the specified tier while maintaining desired composition ratios.
		/// When troops have multiple upgrade paths, intelligently splits them to achieve target ratios.
		/// </summary>
		/// <param name="targetTier">Maximum tier to upgrade to (default: max tier)</param>
		/// <param name="targetRangedRatio">Desired ratio of ranged troops (0.0-1.0, null for auto)</param>
		/// <param name="targetCavalryRatio">Desired ratio of cavalry troops (0.0-1.0, null for auto)</param>
		/// <param name="targetInfantryRatio">Desired ratio of infantry troops (0.0-1.0, null for auto)</param>
		public static void UpgradeTroops(this MobileParty mobileParty,
			int targetTier = 7,
			float? targetRangedRatio = null,
			float? targetCavalryRatio = null,
			float? targetInfantryRatio = null)
		{
			TroopUpgrader.UpgradeTroops(mobileParty.MemberRoster, targetTier, targetRangedRatio, targetCavalryRatio, targetInfantryRatio);
		}

		/// MARK: AddXp
		/// <summary>
		/// Add the specified experience to every troop in the party
		/// </summary>
		public static void AddXp(this MobileParty mobileParty, int xp)
		{
			foreach (TroopRosterElement troop in mobileParty.MemberRoster.GetTroopRoster())
			{
				if (troop.Character.IsHero)
					continue;

				mobileParty.MemberRoster.AddXpToTroop(troop.Character, xp);
			}
		}

		/// <summary>
		/// Start disband action
		/// </summary>
		public static void Disband(this MobileParty mobileParty)
        {
        	DisbandPartyAction.StartDisband(mobileParty);
			mobileParty.IsDisbanding = true;
        }

		/// <summary>
		/// Cancel disband action
		/// </summary>
		public static void CancelDisband(this MobileParty mobileParty)
        {
        	DisbandPartyAction.CancelDisband(mobileParty);
			mobileParty.IsDisbanding = false;
        }

		/// <summary>
		/// Destroy party. destroyerParty is optional and defaults to null for when destroying for administrative reasons
		/// </summary>
		public static void DestroyParty(this MobileParty mobileParty, PartyBase destroyerParty = null)
        {
        	DestroyPartyAction.Apply(destroyerParty, mobileParty);
        }    
	}
}