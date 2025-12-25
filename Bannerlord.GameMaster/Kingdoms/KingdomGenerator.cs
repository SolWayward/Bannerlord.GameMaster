using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Bannerlord.GameMaster.Characters;
using Bannerlord.GameMaster.Clans;
using Bannerlord.GameMaster.Cultures;
using Bannerlord.GameMaster.Heroes;
using Bannerlord.GameMaster.Information;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
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
            Clan rulingClan = ClanGenerator.CreateClan(rulingClanName, cultureFlags: cultureFlags);

            if (name == null || name.IsEmpty())
                name = CultureLookup.GetUniqueRandomKingdomName(rulingClan.Leader.Culture);

            TextObject nameObj = new(name);
            string stringID = ObjectManager.Instance.GetUniqueStringId(nameObj, typeof(Kingdom));

            Kingdom kingdom = Kingdom.CreateKingdom(stringID);

            // Validate settlement and set clan as owner, else return null kingdom
            if(ValidateAndResolveSettlement(homeSettlement, rulingClan) == false)
                return null;

            PrepareClanToRule(rulingClan);
            
            CultureObject culture = rulingClan.Culture;

            Banner banner = Banner.CreateRandomBanner();
            uint kingdomColor1 = 0;//GetUniqueKingdomColor();
            uint kingdomColor2 = 0;//ColorHelpers.GetDarkerShade(kingdomColor1);
        
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
                   
            ChangeKingdomAction.ApplyByCreateKingdom(rulingClan, kingdom, true);
           
            kingdom.CalculateMidSettlement();
            rulingClan.CalculateMidSettlement();
            kingdom.IsReady = true;

            // Generate vassals ensuring vassal count isnt negative
            if (vassalClanCount > 0)
            {             
                List<Clan> clans = ClanGenerator.GenerateClans(vassalClanCount, cultureFlags, kingdom);
                
                // Add extra lords to vassals
                foreach(Clan clan in clans)
                    HeroGenerator.CreateLords(4, clan.Culture.ToCultureFlag(), GenderFlags.Either, clan);
            }

            InfoMessage.Success($"Kingdom '{kingdom.Name}' created with {rulingClan.Name} as ruling clan and {homeSettlement.Name} as the capital and {vassalClanCount} vassal clans");
            
            return kingdom;
        }

        /// MARK: Validate Settlement
        /// <summary>
        /// Validates settlement and sets clan as settlement owner if not already owner
        /// </summary>
        static bool ValidateAndResolveSettlement(Settlement settlement, Clan clan)
        {
            // Return false if settlement null or settlement.town null
            if (settlement == null || settlement.Town == null)
                return false;

            // Above check should fail before this is evaluated but just incase.
            if (!settlement.IsTown && !settlement.IsCastle)
                return false;

            if (settlement.OwnerClan != clan)
                settlement.Town.OwnerClan = clan;

            return true;
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
    }
}