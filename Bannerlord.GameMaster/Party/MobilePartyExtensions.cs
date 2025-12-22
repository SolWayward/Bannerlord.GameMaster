using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;

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
			hero.Clan = mobileParty.LeaderHero.Clan;  // Move to clan
			
			mobileParty.AddElementToMemberRoster(hero.CharacterObject, 1);
			hero.ChangeState(Hero.CharacterStates.Active);
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
	}
}