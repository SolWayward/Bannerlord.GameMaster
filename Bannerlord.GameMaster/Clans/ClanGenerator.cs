using System.Collections.Generic;
using Bannerlord.GameMaster.Banners;
using Bannerlord.GameMaster.Characters;
using Bannerlord.GameMaster.Cultures;
using Bannerlord.GameMaster.Heroes;
using Bannerlord.GameMaster.Party;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace Bannerlord.GameMaster.Clans
{
	/// <summary>
	/// Provides functionality for creating and configuring clans with customizable options.
	/// Uses the new HeroGenerator architecture for reliable hero creation.
	/// </summary>
	public static class ClanGenerator
	{
		/// MARK: CreateNobleClan
		/// <summary>
		/// Create a new clan with the specified name. The clan will have a party created for its leader filled with troops and companions.<br/>
		/// Optional specify a hero to be moved to the clan and be its leader, defaults to null.
		/// A new hero will be created if hero is null<br/>
		/// Optionally assign clan to kingdom, Kingdom defaults to null (Independent)
		/// </summary>
		/// <param name="name">Name of clan (if null, generates random name from culture)</param>
		/// <param name="leader">Hero to be moved to clan and assigned as it's leader (if null, creates new hero)</param>
		/// <param name="kingdom">Optional: Kingdom to assign clan to, defaults to null (Independent)</param>
		/// <param name="createParty">If true, creates a party for the clan leader (default: true)</param>
		/// <param name="companionCount">Number of companions to add to leader's party (default: 2, 0 to skip)</param>
		/// <param name="cultureFlags">Culture pool for leader creation if leader is null (default: AllMainCultures)</param>
		/// <returns>The created clan</returns>
		public static Clan CreateNobleClan(string name = null, Hero leader = null, Kingdom kingdom = null, bool createParty = true, int companionCount = 2, CultureFlags cultureFlags = CultureFlags.AllMainCultures)
		{
			Clan clan = CreateBaseClan(name, leader, cultureFlags, isMinorFaction: false, createParty);
			
			// Noble tier range: 3-5
			int tier = RandomNumberGen.Instance.NextRandomInt(3, 6);
			
			// Populate party with troops scaled to tier + companions
			PopulateParty(clan.Leader, tier, companionCount);
			
			// Kingdom membership
			if (kingdom != null)
				ChangeKingdomAction.ApplyByJoinToKingdom(clan, kingdom);
			
			FinalizeClan(clan, tier);
			return clan;
		}

		/// MARK: GenerateClans
		/// <summary>
		/// Generate multiple clans with random names from culture lists.
		/// Uses the new hero creation architecture to prevent crashes from state conflicts.
		/// </summary>
		/// <param name="count">Number of clans to generate</param>
		/// <param name="cultureFlags">Culture pool to select from (default: AllMainCultures)</param>
		/// <param name="kingdom">Optional kingdom for all clans to join (default: null/Independent)</param>
		/// <param name="createParties">If true, creates parties for clan leaders (default: true)</param>
		/// <param name="companionCount">Number of companions per clan (default: 2)</param>
		/// <returns>List of created clans</returns>
		public static List<Clan> GenerateClans(int count, CultureFlags cultureFlags = CultureFlags.AllMainCultures, Kingdom kingdom = null, bool createParties = true, int companionCount = 2)
		{
			List<Clan> clans = new();
	
			// Create each clan individually, letting CreateNobleClan handle hero creation
			// This ensures each hero is properly initialized with clan association from the start
			for (int i = 0; i < count; i++)
			{
				// Pass null for name and leader to auto-generate
				Clan clan = CreateNobleClan(null, null, kingdom, createParties, companionCount, cultureFlags);
				clans.Add(clan);
			}
	
			return clans;
		}

		/// MARK: CreateMinorClan
		/// <summary>
		/// Creates a minor faction clan (not a noble house).
		/// Useful for creating mercenary companies, bandit factions, or other minor groups.
		/// </summary>
		/// <param name="name">Name of the minor clan (if null, generates random name)</param>
		/// <param name="leader">Optional leader hero (creates one if null)</param>
		/// <param name="cultureFlags">Culture for the clan (default: AllMainCultures)</param>
		/// <param name="createParty">If true, creates a party for the leader (default: true)</param>
		/// <param name="companionCount">Number of companions to add to leader's party (default: 0)</param>
		/// <returns>The created minor clan</returns>
		public static Clan CreateMinorClan(string name = null, Hero leader = null, CultureFlags cultureFlags = CultureFlags.AllMainCultures, bool createParty = true, int companionCount = 0)
		{
			Clan clan = CreateBaseClan(name, leader, cultureFlags, isMinorFaction: true, createParty);
			
			// Minor tier range: 1-3
			int tier = RandomNumberGen.Instance.NextRandomInt(1, 4);
			
			// Populate party with troops scaled to tier + optional companions
			PopulateParty(clan.Leader, tier, companionCount);
			
			FinalizeClan(clan, tier);
			return clan;
		}

		/// MARK: CreateBaseClan
		/// <summary>
		/// Creates the base clan with all common initialization.
		/// Type-specific configuration (troops, kingdom, finalization) handled by caller.
		/// </summary>
		private static Clan CreateBaseClan(string name, Hero leader, CultureFlags cultureFlags, bool isMinorFaction, bool createParty)
		{
			// 1. Initialize clan object
			Clan clan = InitializeClanObject();
			
			// 2. Create or prepare leader
			leader = CreateOrPrepareLeader(leader, clan, cultureFlags);
			
			// 3. Set name and register
			SetClanNameAndRegister(clan, name, leader.Culture);
			
			// 4. Configure clan and leader relationship
			ConfigureClanAndLeader(clan, leader, isMinorFaction);
			
			// 5. Create banner
			CreateClanBanner(clan);
			
			// 6. Set noble/rebel flags
			clan.IsNoble = !isMinorFaction;
			clan.IsRebelClan = false;
			
			// 7. Set home settlement
			SetClanHomeSettlement(clan);
			
			// 8. Initialize clan
			clan.Initialize();
			
			// 9. Create party if requested
			if (createParty && (leader.PartyBelongedTo == null || !leader.IsPartyLeader))
				leader.CreateParty(clan.HomeSettlement);
			
			return clan;
		}

		/// MARK: InitializeClanObject
		/// <summary>
		/// Creates new Clan instance with MBGUID and temporary name to prevent crashes
		/// </summary>
		private static Clan InitializeClanObject()
		{
			Clan clan = new();
			BLGMObjectManager.AssignClanMBGUID(clan);
			clan.SetStringName("uninitialized");
			return clan;
		}

		/// MARK: CreateOrPrepareLeade
		/// <summary>
		/// Creates new leader or prepares existing hero for clan leadership.
		/// ConfigureClanAndLeader handles cleanup, so no need to do it here.
		/// </summary>
		private static Hero CreateOrPrepareLeader(Hero leader, Clan clan, CultureFlags cultureFlags)
		{
			if (leader == null)
				return HeroGenerator.CreateLords(1, cultureFlags, GenderFlags.Either, clan, withParties: false, randomFactor: 1f)[0];
			
			// Existing hero - cleanup handled by ConfigureClanAndLeader
			return leader;
		}

		/// MARK: SetClanNameRegister
		/// <summary>
		/// Sets clan name (from parameter or generates from culture) and registers with game systems
		/// </summary>
		private static void SetClanNameAndRegister(Clan clan, string name, CultureObject culture)
		{
			TextObject nameObj = string.IsNullOrEmpty(name) 
				? new(CultureLookup.GetUniqueRandomClanName(culture))
				: new(name);
			
			clan.ChangeClanName(nameObj, nameObj);
			BLGMObjectManager.RegisterClan(clan);
		}

		/// MARK: PopulateParty
		/// <summary>
		/// Populates leader's party with mixed-tier troops scaled to clan tier and optionally companions
		/// </summary>
		private static void PopulateParty(Hero leader, int clanTier, int companionCount)
		{
			if (leader.PartyBelongedTo == null)
				return;
			
			// Add companions if requested
			if (companionCount > 0)
			{
				List<Hero> companions = HeroGenerator.CreateCompanions(companionCount, leader.Culture.ToCultureFlag(), randomFactor: 1);
				leader.PartyBelongedTo.AddCompanionsToParty(companions);
			}
			
			// Add troops scaled to clan tier (always mixed)
			leader.PartyBelongedTo.AddMixedTierTroops(10 * clanTier);
			leader.PartyBelongedTo.UpgradeTroops(targetTier: clanTier);
		}

		/// MARK: ConfigureClan
		/// <summary>
		/// Configure clan culture, troop and prepares Hero as clan leader
		/// </summary>
		private static void ConfigureClanAndLeader(Clan clan, Hero leader, bool isMinorFaction)
		{
			HeroGenerator.CleanupHeroState(leader);

			leader.SetNewOccupation(Occupation.Lord);
			leader.IsMinorFactionHero = isMinorFaction;
			
			clan.SetLeader(leader); //Also moves hero to. (Change leader campaign action is intended for old clans not new clans)
			
			clan.Culture = leader.Culture;
			clan.BasicTroop = leader.Culture.BasicTroop;
		}

		/// MARK: SetClanHomeSettlem
		/// <summary>
		/// Sets Clan and Leader's Home Settlement and then calculate mid settlement
		/// </summary>
		private static Settlement SetClanHomeSettlement(Clan clan)
		{
			Settlement homeSettlement = clan.Leader.GetHomeOrAlternativeSettlement();
			clan.SetInitialHomeSettlement(homeSettlement);
			
			ClanCreationHelpers.SetDistanceCacheDirty(clan);
			clan.CalculateMidSettlement();

			return homeSettlement;
		}

		/// MARK: CreateClanBanner
		/// <summary>
		/// Creates and assigns a random banner to clan. Banner uses smart 2 color complimentary scheme
		/// </summary>
		private static Banner CreateClanBanner(Clan clan)
		{
			Banner banner = Banner.CreateRandomClanBanner(RandomNumberGen.Instance.NextRandomInt());
			
			banner.ApplyUniqueColorScheme();
			clan.Banner = banner;
			clan.Color = banner.GetPrimaryColor();
			clan.Color2 = banner.GetSecondaryColor();

			ClanCreationHelpers.SetOriginalBannerColors(clan, clan.Banner);

			return banner;
		}

		/// MARK: FinalizeClan
		/// <summary>
		/// Sets clan Tier, Then gold and influence based on tier, Marks clan ready and dispatches clan created event
		/// </summary>
		private static void FinalizeClan(Clan clan, int tier)
		{
			int baseGold = 100000;
			int baseInfluence = 250;

			// Basic properties
			clan.SetClanTier(tier);
			clan.Leader.ChangeHeroGold(baseGold * tier);
			clan.Influence = baseInfluence * tier;

			clan.UpdateCurrentStrength();
			clan.IsReady = true;

			// Notified game systems and AI of clan
			CampaignEventDispatcher.Instance.OnClanCreated(clan, false);
		}
	}
}