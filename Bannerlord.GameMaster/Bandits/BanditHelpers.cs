using System;
using System.Collections.Generic;
using Bannerlord.GameMaster.Common;
using Bannerlord.GameMaster.Cultures;
using Bannerlord.GameMaster.Party;
using Bannerlord.GameMaster.Settlements;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Bandits
{
    public static class BanditHelpers
    {
        /// MARK: RemoveAllParties
        /// <summary>
        /// Removes all parties in the provided list.
        /// </summary>
        internal static int RemoveAllParties(MBList<MobileParty> parties)
        {
            int removedCount = 0;

            for (int i = 0; i < parties.Count; i++)
            {
                MobileParty party = parties[i];
                if (party != null && party.IsActive)
                {
                    party.DestroyParty();
                    removedCount++;
                }
            }

            return removedCount;
        }

        /// MARK: RemoveRandomItems
        /// <summary>
        /// Randomly selects and removes items from a list using the provided removal function.
        /// </summary>
        /// <typeparam name="T">The type of items in the list</typeparam>
        /// <param name="items">The list of items to select from</param>
        /// <param name="countToRemove">How many items to randomly remove</param>
        /// <param name="removeAllFunc">Function that removes all items in a list and returns count removed</param>
        /// <returns>Number of items actually removed</returns>
        internal static int RemoveRandomItems<T>(MBList<T> items, int countToRemove, Func<MBList<T>, int> removeAllFunc)
        {
            if (countToRemove <= 0 || items.Count == 0)
            {
                return 0;
            }

            int actualCountToRemove = Math.Min(countToRemove, items.Count);
            MBList<T> selectedItems = new();

            // Create a copy to select from so we don't affect the source list directly during selection
            MBList<T> availableItems = new();
            for (int i = 0; i < items.Count; i++)
            {
                availableItems.Add(items[i]);
            }

            // Randomly select items
            for (int i = 0; i < actualCountToRemove; i++)
            {
                // Get a random index from the current pool
                int randomIndex = RandomNumberGen.Instance.NextRandomInt(availableItems.Count);

                // Add the selected item to our target list
                selectedItems.Add(availableItems[randomIndex]);

                // OPTIMIZATION: Swap and Pop
                // Calculate the index of the very last item in the list
                int lastIndex = availableItems.Count - 1;

                // Overwrite the item we just picked (at randomIndex) with the last item
                availableItems[randomIndex] = availableItems[lastIndex];

                // Remove the last item (which is now a duplicate). 
                // Removing from the end is O(1) because no other items need to shift.
                availableItems.RemoveAt(lastIndex);
            }

            // Remove selected items using the provided function
            return removeAllFunc(selectedItems);
        }

        /// MARK: RemoveHideouts
        /// <summary>
        /// Removes all hideouts in the provided list by destroying all bandit parties inside them.
        /// </summary>
        internal static int RemoveAllHideouts(MBList<Hideout> hideouts)
        {
            int removedCount = 0;

            for (int i = 0; i < hideouts.Count; i++)
            {
                Hideout hideout = hideouts[i];
                if (hideout != null && hideout.Settlement != null)
                {
                    if (hideout.DestroyHideout())
                    {
                        removedCount++;
                    }
                }
            }

            return removedCount;
        }

        /// MARK: GetHideoutCulture
        /// <summary>
        /// Gets the culture of bandits in the hideout.
        /// If hideout has no parties, returns terrain-appropriate culture instead of null.
        /// </summary>
        public static CultureObject GetHideoutCulture(Hideout hideout)
        {
            if (hideout == null || hideout.Settlement == null)
            {
                BLGMResult.Error("GetHideoutCulture() hideout cannot be null",
                    new ArgumentNullException(nameof(hideout))).Log();
                return null;
            }

            // Try to get culture from existing parties
            CultureObject partyCulture = GetHideoutCultureFromParties(hideout);
            if (partyCulture != null)
                return partyCulture;

            // No parties - use terrain-appropriate culture
            return GetTerrainAppropriateBanditCulture(hideout);
        }

        /// <summary>
        /// Gets culture from existing bandit parties in hideout. Returns null if no parties.
        /// </summary>
        private static CultureObject GetHideoutCultureFromParties(Hideout hideout)
        {
            List<MobileParty> parties = hideout.Settlement.Parties;
            for (int i = 0; i < parties.Count; i++)
            {
                MobileParty party = parties[i];
                if (party != null && party.IsBandit && party.ActualClan?.Culture?.IsBandit == true)
                    return party.ActualClan.Culture;
            }
            return null;
        }

        /// MARK: Terrain Culture Resolution
        /// <summary>
        /// Gets a terrain-appropriate bandit culture for a hideout.
        /// Uses geographic context when no parties are present.
        /// </summary>
        public static CultureObject GetTerrainAppropriateBanditCulture(Hideout hideout)
        {
            if (hideout == null || hideout.Settlement == null)
                return CultureLookup.RandomHideoutBanditCulture(false);

            // 1. Check if hideout settlement has a bandit culture assigned
            if (hideout.Settlement.Culture != null && hideout.Settlement.Culture.IsBandit)
                return hideout.Settlement.Culture;

            // 2. Find nearest active hideout and use its culture
            CultureObject nearbyHideoutCulture = GetNearestActiveHideoutCulture(hideout);
            if (nearbyHideoutCulture != null)
                return nearbyHideoutCulture;

            // 3. Map nearest settlement's main culture to bandit type
            Settlement nearestSettlement = SettlementDistanceHelpers.FindNearestNonHideoutSettlement(hideout.Settlement);
            if (nearestSettlement != null)
            {
                CultureObject mappedBanditCulture = MapMainCultureToBanditCulture(nearestSettlement.Culture);
                if (mappedBanditCulture != null)
                    return mappedBanditCulture;
            }

            // 4. Final fallback
            return CultureLookup.RandomHideoutBanditCulture(false);
        }

        /// <summary>
        /// Maps a main culture to its geographically-appropriate bandit culture.
        /// </summary>
        public static CultureObject MapMainCultureToBanditCulture(CultureObject mainCulture)
        {
            if (mainCulture == null) return null;
            
            return mainCulture.StringId.ToLower() switch
            {
                "aserai" => CultureLookup.DesertBandits,      // Desert region
                "battania" => CultureLookup.ForestBandits,    // Forest region
                "empire" => CultureLookup.MountainBandits,    // Mountain/central region
                "khuzait" => CultureLookup.SteppeBandits,     // Steppe region
                "sturgia" => CultureLookup.SeaRaiders,        // Coastal/northern region
                "vlandia" => CultureLookup.MountainBandits,   // Western mountains
                "nord" => CultureLookup.SeaRaiders,           // Coastal region (War Sails)
                _ => null
            };
        }

        private static CultureObject GetNearestActiveHideoutCulture(Hideout sourceHideout)
        {
            float minDistance = float.MaxValue;
            CultureObject nearestCulture = null;
            
            MBReadOnlyList<Hideout> allHideouts = Hideout.All;
            for (int i = 0; i < allHideouts.Count; i++)
            {
                Hideout other = allHideouts[i];
                if (other == sourceHideout || !other.IsInfested)
                    continue;
                    
                float distance = sourceHideout.Settlement.Position.DistanceSquared(other.Settlement.Position);
                if (distance < minDistance)
                {
                    CultureObject culture = GetHideoutCultureFromParties(other);
                    if (culture != null)
                    {
                        minDistance = distance;
                        nearestCulture = culture;
                    }
                }
            }
            
            return nearestCulture;
        }

        /// <summary>
        /// Validates and resolves a bandit culture, handling null and non-bandit cases.
        /// </summary>
        /// <param name="culture">The culture to validate</param>
        /// <param name="hideout">The hideout for terrain-based fallback</param>
        /// <param name="createDesertersForMainCultures">If true, returns main cultures as-is for deserter creation</param>
        /// <returns>A valid bandit culture, or the original main culture if deserters are enabled</returns>
        public static CultureObject ResolveAndValidateBanditCulture(
            CultureObject culture,
            Hideout hideout,
            bool createDesertersForMainCultures = true)
        {
            // Already a valid bandit culture
            if (culture != null && culture.IsBandit)
                return culture;

            // Main culture - return as-is if deserters are enabled
            if (culture != null && culture.IsMainCulture && createDesertersForMainCultures)
                return culture;

            // Null or invalid - use terrain-appropriate
            return GetTerrainAppropriateBanditCulture(hideout);
        }


        /// MARK: HideoutMatchCulture
        /// <summary>
        /// Checks if a hideout contains parties of the specified bandit culture.
        /// </summary>
        public static bool HideoutMatchesCulture(Hideout hideout, CultureObject banditCulture)
        {
            if (hideout == null || hideout.Settlement == null)
                return false;

            List<MobileParty> parties = hideout.Settlement.Parties;
            for (int i = 0; i < parties.Count; i++)
            {
                MobileParty party = parties[i];
                if (party != null && party.IsBandit && party.ActualClan != null && party.ActualClan.Culture == banditCulture)
                {
                    return true;
                }
            }

            return false;
        }

        /// MARK: PartyCountByCulture
        /// <summary>
        /// Gets the count of bandit parties for a specific culture.
        /// </summary>
        internal static int GetBanditPartyCountByCulture(CultureObject cultureObj)
        {
            MBReadOnlyList<MobileParty> allBanditParties = MobileParty.AllBanditParties;
            int count = 0;

            for (int i = 0; i < allBanditParties.Count; i++)
            {
                MobileParty party = allBanditParties[i];
                if (party != null && party.IsActive && party.ActualClan != null && party.ActualClan.Culture == cultureObj)
                {
                    count++;
                }
            }

            return count;
        }

        /// MARK: HideoutCountCulture
        /// <summary>
        /// Gets the count of hideouts for a specific culture.
        /// </summary>
        internal static int GetHideoutCountByCulture(CultureObject cultureObj)
        {
            MBReadOnlyList<Hideout> allHideouts = Hideout.All;
            int count = 0;

            for (int i = 0; i < allHideouts.Count; i++)
            {
                Hideout hideout = allHideouts[i];
                if (HideoutMatchesCulture(hideout, cultureObj))
                {
                    count++;
                }
            }

            return count;
        }

        /// MARK: HideoutBanditPartyCount
        /// <summary>
        /// Gets the count of bandit parties in a hideout.
        /// </summary>
        public static int HideoutBanditPartyCount(Hideout hideout)
        {
            if (hideout == null || hideout.Settlement == null)
                return 0;

            int count = 0;
            List<MobileParty> parties = hideout.Settlement.Parties;
            for (int i = 0; i < parties.Count; i++)
            {
                MobileParty party = parties[i];
                if (party != null && party.IsBandit)
                {
                    count++;
                }
            }

            return count;
        }

        /// MARK: HasBanditBossParty
        /// <summary>
        /// Checks if a hideout contains any bandit boss parties.
        /// </summary>
        public static bool HideoutHasBanditBossParty(Hideout hideout)
        {
            if (hideout == null || hideout.Settlement == null)
                return false;

            List<MobileParty> parties = hideout.Settlement.Parties;
            for (int i = 0; i < parties.Count; i++)
            {
                MobileParty party = parties[i];
                if (party != null && party.IsBandit && party.IsBanditBossParty)
                {
                    return true;
                }
            }

            return false;
        }

        /// MARK: BanditBossPartyCount
        /// <summary>
        /// Gets the count of bandit boss parties in a hideout.
        /// </summary>
        public static int HideoutBanditBossPartyCount(Hideout hideout)
        {
            if (hideout == null || hideout.Settlement == null)
                return 0;

            int count = 0;
            List<MobileParty> parties = hideout.Settlement.Parties;
            for (int i = 0; i < parties.Count; i++)
            {
                MobileParty party = parties[i];
                if (party != null && party.IsBandit && party.IsBanditBossParty)
                {
                    count++;
                }
            }

            return count;
        }
    }
}