using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Bannerlord.GameMaster.Characters;
using Bannerlord.GameMaster.Clans;
using Bannerlord.GameMaster.Cultures;
using Bannerlord.GameMaster.Heroes;
using Bannerlord.GameMaster.Information;
using Bannerlord.GameMaster.Party;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace Bannerlord.GameMaster.Kingdoms
{
    public class KingdomGenerator
    {
        /// MARK: Create Kingdom
        /// <summary>
        /// Creates a new kingdom with the specified ruling clan and home settlement.
        /// </summary>
        /// <param name="homeSettlement">Capital settlement (auto-resolves if null/invalid)</param>
        /// <param name="name">Kingdom name (generates random if null)</param>
        /// <param name="rulingClanName">Name of the Clan ruling to rule the kingdom (generates random if null)</param>
        /// <returns>The created kingdom, or null if settlement cannot be resolved</returns>
        public static Kingdom CreateKingdom(Settlement homeSettlement, int vassalClanCount = 4, string name = null, string rulingClanName = null, CultureFlags cultureFlags = CultureFlags.AllMainCultures)
        {
            // Early validation of settlement
            if (homeSettlement == null || homeSettlement.Town == null)
                return null;

            if (!homeSettlement.IsTown && !homeSettlement.IsCastle)
                return null;

            // Create ruling clan
            Clan rulingClan = ClanGenerator.CreateNobleClan(rulingClanName, cultureFlags: cultureFlags);

            if (name == null || name.IsEmpty())
                name = CultureLookup.GetUniqueRandomKingdomName(rulingClan.Leader.Culture);

            Kingdom kingdom = new();
            BLGMObjectManager.AssignKingdomMBGUID(kingdom); // Assign MBGUID immediately to prevent save/load crashes
            TextObject nameObj = new(name);
            kingdom.ChangeKingdomName(nameObj, nameObj); // Set name here even though InitalizeKingdom sets name too so stringId will contain Name
            BLGMObjectManager.RegisterKingdom(kingdom); // Registers and assigns stringId

            // Prepare clan for ruling BEFORE creating kingdom
            PrepareClanToRule(rulingClan);

            CultureObject culture = rulingClan.Culture;

            // banner is null atleast on vanilla clans, so use originalBanner if null
            Banner banner;
            if (rulingClan.Banner != null)
                banner = rulingClan.Banner;
            else
                banner = rulingClan.ClanOriginalBanner;

            // Match native KingdomManager pattern: use clan.Color/Color2 (not banner icon color)
            // Native passes founderClan.Color and founderClan.Color2 to InitializeKingdom which sets
            // Kingdom.Color, Color2, PrimaryBannerColor, and SecondaryBannerColor
            uint kingdomColor1 = rulingClan.Color;   // Primary background color
            uint kingdomColor2 = rulingClan.Color2;  // Secondary background color

            // Links seem to not show up in encyclopedia, keeping them anyway as still shows text correctly.
            TextObject encyclopediaText = new($"A new rising kingdom sparked from the upstarts of {rulingClan.EncyclopediaLinkWithName}, Taking {homeSettlement.EncyclopediaLinkWithName} as their capital " +
                                            $"and first ruled by {rulingClan.Leader.EncyclopediaLinkWithName}. Will their legitimacy as a sovereign nation be challenged?");
            TextObject encyclopediaTitle = nameObj;
            TextObject encyclopediaRulerTitle = new("{=kingdom_ruler_title}{?RULER.GENDER}Queen{?}King{\\?}");

            kingdom.InitializeKingdom(
                nameObj,                        // name
                nameObj,                        // informal name
                culture,                        // culture
                banner,                         // banner
                kingdomColor1,                  // color 1
                kingdomColor2,                  // color 2
                homeSettlement,                 // home settlement
                encyclopediaText,               // encyclopedia text
                encyclopediaTitle,              // encyclopedia title
                encyclopediaRulerTitle          // encyclopedia ruler title
            );

            // Game does this after InitializeKingdom Doing it before, makes settlement have richer 2 background color banners, but the banner wont match kingdom banner
            ChangeKingdomAction.ApplyByCreateKingdom(rulingClan, kingdom, true);

            // Transfer ownership of settlement
            ChangeOwnerOfSettlementAction.ApplyByDefault(rulingClan.Leader, homeSettlement);
            
            // Change homesettlement and bound villages culture to match to new kingdom
            homeSettlement.Culture = culture;
            foreach(Village village in homeSettlement.BoundVillages)
            {
                village.Settlement.Culture = culture;
            }

            // Calculate mid settlements for both kingdom and clan
            kingdom.CalculateMidSettlement();
            rulingClan.CalculateMidSettlement();

            // Initialize kingdom Wallets
            kingdom.KingdomBudgetWallet += 100000;
            kingdom.CallToWarWallet += 100000;
            kingdom.TributeWallet += 100000;


            if (rulingClan.Leader.PartyBelongedTo != null)
            {
                MobileParty rulerParty = rulingClan.Leader.PartyBelongedTo;              
                rulerParty.AddMixedTierTroops(30);
                rulerParty.UpgradeTroops();
            }

            // Generate vassals ensuring vassal count isnt negative
            if (vassalClanCount > 0)
            {
                List<Clan> clans = ClanGenerator.GenerateClans(vassalClanCount, cultureFlags, kingdom);
                
                // Add extra lords to vassals
                foreach(Clan clan in clans)
                    HeroGenerator.CreateLords(4, clan.Culture.ToCultureFlag(), GenderFlags.Either, clan);
            }

            // Propagate ruling clan banner colors to kingdom and all vassal clans
            kingdom.PropagateRulingClanBanner();

            // Set kingdom as ready AFTER all initialization is complete
            kingdom.IsReady = true;
            CampaignEventDispatcher.Instance.OnKingdomCreated(kingdom);

            InfoMessage.Success($"Kingdom '{kingdom.Name}' created with {rulingClan.Name} as ruling clan and {homeSettlement.Name} as the capital and {vassalClanCount} vassal clans");
            
            return kingdom;
        }

        /// MARK: Prepare Clan for Rule
        /// <summary>
        /// Make sure clan is ready to be a ruling clan <br />
        /// Leaves kingdom if in one, forces correct state, sets tier to 6, add gold and influence, add 10 lords to clan
        /// </summary>
        static void PrepareClanToRule(Clan clan)
        {
            // Ensure clan isnt in kingdom already and their banner original colors are restored
            if (clan.Kingdom != null)
                clan.ClanLeaveKingdom(giveBackFiefs: false);

            if (clan.IsUnderMercenaryService)
                clan.EndMercenaryService(true);

            // Set correct states
            clan.IsNoble = true;
            clan.IsRebelClan = false;
            clan.Influence += 500;
            clan.Leader.Gold += 500000;

            if (clan.Leader.IsLord == false)
                clan.Leader.SetNewOccupation(Occupation.Lord);

            if (clan.Tier < 6)
                clan.SetClanTier(6);

            // Create extra lords for ruling clan
            HeroGenerator.CreateLords(10, clan.Culture.ToCultureFlag(), GenderFlags.Either, clan);
        }

        /// MARK: Generate Kingdoms
        /// <summary>
        /// Generates multiple kingdoms by taking settlements from existing kingdoms.
        /// Alternates between kingdoms evenly, ensuring not to take a kingdom's last settlement.
        /// Does not take settlements from the player's kingdom.
        /// </summary>
        /// <param name="count">Number of kingdoms to create</param>
        /// <param name="vassalClanCount">Number of vassal clans per kingdom</param>
        /// <param name="cultureFlags">Culture pool for kingdoms</param>
        /// <returns>List of created kingdoms</returns>
        public static List<Kingdom> GenerateKingdoms(int count, int vassalClanCount = 4, CultureFlags cultureFlags = CultureFlags.AllMainCultures)
        {
            List<Kingdom> createdKingdoms = new List<Kingdom>();

            // Get existing kingdoms that have more than 1 town/castle settlement
            var eligibleKingdoms = Kingdom.All
                .Where(k => k != Clan.PlayerClan?.Kingdom) // Exclude player kingdom
                .Where(k => k.Settlements.Count(s => s.IsTown || s.IsCastle) > 1)
                .ToList();

            if (eligibleKingdoms.Count == 0)
            {
                InfoMessage.Warning("No kingdoms with multiple settlements available for taking settlements.");
                return createdKingdoms;
            }

            // Build a pool of available settlements from these kingdoms
            var settlementPool = new List<Settlement>();
            foreach (var kingdom in eligibleKingdoms)
            {
                var kingdomSettlements = kingdom.Settlements
                    .Where(s => (s.IsTown || s.IsCastle) && s.OwnerClan != Clan.PlayerClan)
                    .ToList();

                // Only add settlements if the kingdom would still have at least 1 after taking one
                if (kingdomSettlements.Count > 1)
                {
                    // Add all but the last one to the pool (keep at least 1 for the kingdom)
                    settlementPool.AddRange(kingdomSettlements.Take(kingdomSettlements.Count - 1));
                }
            }

            if (settlementPool.Count == 0)
            {
                InfoMessage.Warning("No settlements available to create kingdoms without destroying existing ones.");
                return createdKingdoms;
            }

            // Organize settlements by their current kingdom to alternate evenly
            var settlementsByKingdom = settlementPool
                .GroupBy(s => s.MapFaction as Kingdom)
                .Where(g => g.Key != null)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Create kingdoms by alternating between source kingdoms
            int kingdomsCreated = 0;
            int currentKingdomIndex = 0;
            var kingdomList = settlementsByKingdom.Keys.ToList();

            while (kingdomsCreated < count && settlementsByKingdom.Values.Any(list => list.Count > 0))
            {
                // Find the next kingdom that still has available settlements
                Kingdom sourceKingdom = null;
                int attempts = 0;
                while (attempts < kingdomList.Count)
                {
                    var testKingdom = kingdomList[currentKingdomIndex];
                    if (settlementsByKingdom[testKingdom].Count > 0)
                    {
                        sourceKingdom = testKingdom;
                        break;
                    }
                    currentKingdomIndex = (currentKingdomIndex + 1) % kingdomList.Count;
                    attempts++;
                }

                // No more settlements available
                if (sourceKingdom == null)
                    break;

                // Take the first available settlement from this kingdom
                var settlement = settlementsByKingdom[sourceKingdom][0];
                settlementsByKingdom[sourceKingdom].RemoveAt(0);

                // Verify the source kingdom still has at least one settlement left
                int remainingSettlements = sourceKingdom.Settlements.Count(s => (s.IsTown || s.IsCastle) && s != settlement);
                if (remainingSettlements < 1)
                {
                    InfoMessage.Warning($"Skipping settlement {settlement.Name} to prevent destroying {sourceKingdom.Name}");
                    currentKingdomIndex = (currentKingdomIndex + 1) % kingdomList.Count;
                    continue;
                }

                // Create kingdom with random names
                Kingdom newKingdom = CreateKingdom(
                    homeSettlement: settlement,
                    vassalClanCount: vassalClanCount,
                    name: null, // Random name
                    rulingClanName: null, // Random clan name
                    cultureFlags: cultureFlags
                );

                if (newKingdom != null)
                {
                    createdKingdoms.Add(newKingdom);
                    kingdomsCreated++;
                }

                // Move to next kingdom for alternation
                currentKingdomIndex = (currentKingdomIndex + 1) % kingdomList.Count;
            }

            if (kingdomsCreated < count)
            {
                InfoMessage.Warning($"Only created {kingdomsCreated} of {count} requested kingdoms. No more settlements available.");
            }

            return createdKingdoms;
        }
    }
}