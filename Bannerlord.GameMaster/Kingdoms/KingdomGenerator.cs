using System;
using System.Collections.Generic;
using System.Linq;
using Bannerlord.GameMaster.Clans;
using Bannerlord.GameMaster.Cultures;
using Bannerlord.GameMaster.Information;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
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
        /// <param name="rulingClan">Clan to rule the kingdom (generates random if null)</param>
        /// <returns>The created kingdom, or null if settlement cannot be resolved</returns>
        public static Kingdom CreateKingdom(Settlement homeSettlement, string name = null, Clan rulingClan = null)
        {
            if (rulingClan == null)
                rulingClan = ClanGenerator.CreateClan();

            if (rulingClan.IsEliminated)
                return null;

            if (name == null || name.IsEmpty())
                name = CultureLookup.GetUniqueRandomKingdomName(rulingClan.Leader.Culture);

            TextObject nameObj = new(name);
            string stringID = ObjectManager.Instance.GetUniqueStringId(nameObj, typeof(Kingdom));

            Kingdom kingdom = Kingdom.CreateKingdom(stringID);

            // Validate settlement and attempt to resolve to another settlement if invalid
            homeSettlement = ValidateAndResolveSettlement(homeSettlement, rulingClan);

            // settlement was invalid and or settlement was unable to automatically be resolved
            if (homeSettlement == null)
                return null;

            PrepareClanToRule(rulingClan);
            //rulingClan.Kingdom = kingdom;
            //kingdom.RulingClan = rulingClan;
            ChangeKingdomAction.ApplyByCreateKingdom(rulingClan, kingdom, true);

            CultureObject culture = rulingClan.Culture;

            // Create a banner with unique kingdom colors keeping original icon
            // Note: clan.banner becomes null when clan joins the kingdom for some reason even after immediately setting it again.
            // Note: clan.originalbanner returns grey colors for vanilla clans
            Banner banner = SetupKingdomBanner(rulingClan);
            
            uint kingdomColor1 = banner.GetPrimaryColor();
            uint kingdomColor2 = banner.GetSecondaryColor();

            // Links seem to not show up in encyclopedia, keeping them anyway as still shows text correctly.
            TextObject encyclopediaText = new($"A new rising kingdom sparked from the upstarts of {rulingClan.EncyclopediaLinkWithName}, Taking {homeSettlement.EncyclopediaLinkWithName} as their capital " +
                                            $"and first ruled by {rulingClan.Leader.EncyclopediaLinkWithName}. Will their legitimacy as a sovereign nation be challenged?");
            TextObject encyclopediaTitle = nameObj;
            TextObject encyclopediaRulerTitle = new("{=kingdom_ruler_title}{?RULER.GENDER}Queen{?}King{\\?}");

            kingdom.InitializeKingdom(
                nameObj,                    // name
                nameObj,                    // informal name
                culture,                    // culture
                banner,                     // banner
                kingdomColor1,              // color 1
                kingdomColor2,              // color 2
                homeSettlement,             // home settlement
                encyclopediaText,           // encyclopedia text
                encyclopediaTitle,          // encyclopedia title
                encyclopediaRulerTitle      // encyclopedia ruler title
            );

            //kingdom.Initialize(); // Does this need to be called if using InitalizeKingdom()? 
            
            kingdom.CalculateMidSettlement();
            rulingClan.CalculateMidSettlement();
            kingdom.IsReady = true;

            InfoMessage.Display($"Kingdom '{kingdom.Name}' created with {rulingClan.Name} as ruling clan and {homeSettlement.Name} as the capital");
            
            return kingdom;
        }

        /// MARK: Validate Settlement
        /// <summary>
        /// Validates settlement. If settlement was invalid attempts to resolve to another settlement. <br />
        /// Returns null if settlement invalid and unable to resolve to another settlement.
        /// </summary>
        static Settlement ValidateAndResolveSettlement(Settlement homeSettlement, Clan rulingClan)
        {
            // Attempt to resolve settlement if null or invalid
            if (homeSettlement == null
                || (homeSettlement.IsTown == false && homeSettlement.IsCastle == false))
            {
                // Use clans home settlement if clan has one, and if clan is the owner and settlement is town or castle 
                if (rulingClan.HomeSettlement != null && rulingClan.HomeSettlement.OwnerClan == rulingClan
                    && (rulingClan.HomeSettlement.IsTown || rulingClan.HomeSettlement.IsCastle))
                {
                    homeSettlement = rulingClan.HomeSettlement;
                }

                // Get random settlement owned by clan
                else if (rulingClan.Settlements != null)
                {
                    List<Settlement> eligibleSettlements = rulingClan.Settlements.Where(s => s.IsTown || s.IsCastle).ToList();
                    if (eligibleSettlements.Count > 0)
                    {
                        homeSettlement = eligibleSettlements[RandomNumberGen.Instance.NextRandomInt(eligibleSettlements.Count)];
                    }
                }

                // Unable to create kingdom without settlement
                else
                {
                    return null;
                    // Can a kingdom exist without a settlement?
                    // Add option to randomly make ruling clan owner of a settlment if its not a kingdoms only settlment?
                    // Create a completely new settlement? Probably not a good idea
                }
            }

            else
            {
                // Make ruling clan owner of settlement since it was specified directly
                if (homeSettlement.OwnerClan != rulingClan)
                { 
                    if (homeSettlement.Town != null)        
                        homeSettlement.Town.OwnerClan = rulingClan;
                
                    // Extra safety that should never happen
                    else
                        return null;
                }
            }

            return homeSettlement;
        }

        /// MARK: Prepare Clan for Rule
        /// <summary>
        /// Make sure clan is ready to be a ruling clan
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
        }

        /// <summary>
        /// Set clan banners to unique colors except for player clan
        /// Kingdom colors are set from clan banner so make sure the colors are unique, if player keep their original banner
        /// </summary>
        static Banner SetupKingdomBanner(Clan clan)
        {
            // Keep banner if player
            if (clan.Leader.IsHumanPlayerCharacter)
                return clan.ClanOriginalBanner;

            // Save original icon to restore it to new banner
            //int iconColorId = clan.ClanOriginalBanner.GetIconColorId();
            //int iconMeshId = clan.ClanOriginalBanner.GetIconMeshId();
            //Vec2 iconSize = clan.ClanOriginalBanner.GetIconSize();

            // create random banner
            Banner banner = clan.ClanOriginalBanner;
            
            // Restore icon
            //banner.SetIconColorId(iconColorId);
            //banner.SetIconMeshId(iconMeshId);
            //banner.SetIconSize((int)iconSize.x); //Stored as a vec2(float, float) by set by a single int?

            uint uniqueColor = KingdomColorPicker.GetUniqueKingdomColor(RandomNumberGen.Instance.NextRandomRGBColor);
            uint uniqueColor2 = KingdomColorPicker.GetDarkerShade(uniqueColor);
            uint iconColor = KingdomColorPicker.GetLighterShade(uniqueColor);

            banner.ChangePrimaryColor(iconColor);
            banner.ChangeBackgroundColor(uniqueColor, uniqueColor2);
            //clan.Banner = banner;
            //clan.UpdateBannerColor(uniqueColor, iconColor);
            
            return banner;
        }
    }
}