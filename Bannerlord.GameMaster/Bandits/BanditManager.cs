using System;
using System.Collections.Generic;
using Bannerlord.GameMaster.Cultures;
using Bannerlord.GameMaster.Information;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Bandits
{
    public static class BanditManager
    {
        #region Bandit Party Removal

        /// <summary>
        /// Removes desert bandit parties. If count is null, removes all. If count is specified, randomly removes that many.
        /// </summary>
        public static int RemoveDeserterBanditParties(int? count = null)
        {
            return RemoveBanditPartiesByCulture(CultureLookup.Deserters, count);
        }

        /// <summary>
        /// Removes forest bandit parties. If count is null, removes all. If count is specified, randomly removes that many.
        /// </summary>
        public static int RemoveForestBanditParties(int? count = null)
        {
            return RemoveBanditPartiesByCulture(CultureLookup.ForestBandits, count);
        }

        /// <summary>
        /// Removes looter parties. If count is null, removes all. If count is specified, randomly removes that many.
        /// </summary>
        public static int RemoveLooterParties(int? count = null)
        {
            return RemoveBanditPartiesByCulture(CultureLookup.Looters, count);
        }

        /// <summary>
        /// Removes mountain bandit parties. If count is null, removes all. If count is specified, randomly removes that many.
        /// </summary>
        public static int RemoveMountainBanditParties(int? count = null)
        {
            return RemoveBanditPartiesByCulture(CultureLookup.MountainBandits, count);
        }

        /// <summary>
        /// Removes sea raider parties. If count is null, removes all. If count is specified, randomly removes that many.
        /// </summary>
        public static int RemoveSeaRaiderParties(int? count = null)
        {
            return RemoveBanditPartiesByCulture(CultureLookup.SeaRaiders, count);
        }

        /// <summary>
        /// Removes corsair parties. If count is null, removes all. If count is specified, randomly removes that many.
        /// Returns 0 if Warsails DLC is not loaded.
        /// </summary>
        public static int RemoveCorsairParties(int? count = null)
        {
            // Dont execute if Warsails isnt loaded
            if (!GameEnvironment.IsWarsailsDlcLoaded)
                return 0;

            return RemoveBanditPartiesByCulture(CultureLookup.Corsairs, count);
        }

        /// <summary>
        /// Removes steppe bandit parties. If count is null, removes all. If count is specified, randomly removes that many.
        /// </summary>
        public static int RemoveSteppeBanditParties(int? count = null)
        {
            return RemoveBanditPartiesByCulture(CultureLookup.SteppeBandits, count);
        }

        /// <summary>
        /// Removes all bandit parties. If count is null, removes all. If count is specified, randomly removes that many from all bandit types.
        /// Note: If all bandit parties linked to a hideout are destroyed the hideout is also considered cleared by the game
        /// </summary>
        public static int RemoveAllBanditParties(int? count = null)
        {

            // Gather ALL bandit parties regardless of culture
            MBList<MobileParty> allBanditParties = new();
            foreach (var party in MobileParty.AllBanditParties)
            {
                if (party != null && party.IsActive) allBanditParties.Add(party);
            }

            if (count.HasValue)
            {
                return RemoveRandomItems(allBanditParties, count.Value, RemoveAllParties);
            }
            else
            {
                // Faster direct removal
                return RemoveAllParties(allBanditParties);
            }
        }

        #endregion

        #region Hideout Removal

        /// <summary>
        /// Removes desert bandit hideouts. If count is null, removes all. If count is specified, randomly removes that many.
        /// </summary>
        public static int RemoveDeserterBanditHideouts(int? count = null)
        {
            return RemoveHideoutsByCulture(CultureLookup.Deserters, count);
        }

        /// <summary>
        /// Removes forest bandit hideouts. If count is null, removes all. If count is specified, randomly removes that many.
        /// </summary>
        public static int RemoveForestBanditHideouts(int? count = null)
        {
            return RemoveHideoutsByCulture(CultureLookup.ForestBandits, count);
        }

        /// <summary>
        /// Removes looter hideouts. If count is null, removes all. If count is specified, randomly removes that many.
        /// </summary>
        public static int RemoveLooterHideouts(int? count = null)
        {
            return RemoveHideoutsByCulture(CultureLookup.Looters, count);
        }

        /// <summary>
        /// Removes mountain bandit hideouts. If count is null, removes all. If count is specified, randomly removes that many.
        /// </summary>
        public static int RemoveMountainBanditHideouts(int? count = null)
        {
            return RemoveHideoutsByCulture(CultureLookup.MountainBandits, count);
        }

        /// <summary>
        /// Removes sea raider hideouts. If count is null, removes all. If count is specified, randomly removes that many.
        /// </summary>
        public static int RemoveSeaRaiderHideouts(int? count = null)
        {
            return RemoveHideoutsByCulture(CultureLookup.SeaRaiders, count);
        }

        /// <summary>
        /// Removes corsair hideouts. If count is null, removes all. If count is specified, randomly removes that many.
        /// Returns 0 if Warsails DLC is not loaded.
        /// </summary>
        public static int RemoveCorsairHideouts(int? count = null)
        {
            // Dont execute if Warsails isnt active
            if (!GameEnvironment.IsWarsailsDlcLoaded)
                return 0;

            return RemoveHideoutsByCulture(CultureLookup.Corsairs, count);
        }

        /// <summary>
        /// Removes steppe bandit hideouts. If count is null, removes all. If count is specified, randomly removes that many.
        /// </summary>
        public static int RemoveSteppeBanditHideouts(int? count = null)
        {
            return RemoveHideoutsByCulture(CultureLookup.SteppeBandits, count);
        }

        /// <summary>
        /// Removes all hideouts. If count is null, removes all. If count is specified, randomly removes that many from all hideout types.
        /// </summary>
        public static int RemoveAllHideouts(int? count = null)
        {
            // Gather ALL active hideouts regardless of culture
            MBList<Hideout> allHideouts = new();
            MBReadOnlyList<Hideout> hideouts = Hideout.All;

            for (int i = 0; i < hideouts.Count; i++)
            {
                Hideout hideout = hideouts[i];
                // Ensure the hideout is actually inhabited/active before adding
                if (hideout != null && hideout.Settlement != null && hideout.Settlement.Parties.Count > 0)
                {
                    allHideouts.Add(hideout);
                }
            }

            if (count.HasValue)
            {
                return RemoveRandomItems(allHideouts, count.Value, RemoveAllHideouts);
            }
            else
            {
                // Directly remove all found hideouts
                return RemoveAllHideouts(allHideouts);
            }
        }

        #endregion

        #region Party Count

        public static int LootersPartyCount => GetBanditPartyCountByCulture(CultureLookup.Looters);
        public static int DeserterPartyCount => GetBanditPartyCountByCulture(CultureLookup.Deserters);
        public static int ForestBanditPartyCount => GetBanditPartyCountByCulture(CultureLookup.ForestBandits);
        public static int MountainBanditPartyCount => GetBanditPartyCountByCulture(CultureLookup.MountainBandits);
        public static int SeaRaiderPartyCount => GetBanditPartyCountByCulture(CultureLookup.SeaRaiders);
        public static int SteppeBanditPartyCount => GetBanditPartyCountByCulture(CultureLookup.SteppeBandits);
        public static int CorsairPartyCount => GameEnvironment.IsWarsailsDlcLoaded ? GetBanditPartyCountByCulture(CultureLookup.Corsairs) : 0; //Return 0 if Warsails isnt loaded

        /// <summary>
        /// Gets the total count of all bandit parties.
        /// </summary>
        public static int TotalBanditPartyCount
        {
            get
            {
                MBReadOnlyList<MobileParty> banditParties = MobileParty.AllBanditParties;
                int count = 0;

                for (int i = 0; i < banditParties.Count; i++)
                {
                    if (banditParties[i] != null && banditParties[i].IsActive)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        #endregion

        #region Hideout Count

        public static int LooterHideoutCount => GetHideoutCountByCulture(CultureLookup.Looters);
        public static int DeserterHideoutCount => GetHideoutCountByCulture(CultureLookup.Deserters);
        public static int ForestBanditHideoutCount => GetHideoutCountByCulture(CultureLookup.ForestBandits);
        public static int MountainBanditHideoutCount => GetHideoutCountByCulture(CultureLookup.MountainBandits);
        public static int SeaRaiderHideoutCount => GetHideoutCountByCulture(CultureLookup.SeaRaiders);
        public static int SteppeBanditHideoutCount => GetHideoutCountByCulture(CultureLookup.SteppeBandits);
        public static int CorsairHideoutCount => GameEnvironment.IsWarsailsDlcLoaded ? GetHideoutCountByCulture(CultureLookup.Corsairs) : 0; //Return 0 if Warsails isnt loaded

        /// <summary>
        /// Gets the total count of all hideouts.
        /// </summary>
        public static int TotalHideoutCount
        {
            get
            {
                MBReadOnlyList<Hideout> hideouts = Hideout.All;
                int count = 0;

                for (int i = 0; i < hideouts.Count; i++)
                {
                    if (hideouts[i] != null && hideouts[i].Settlement != null && hideouts[i].Settlement.Parties.Count > 0)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        #endregion

        #region Helper Methods



        /// <summary>
        /// Removes bandit parties of a specific culture. If count is null, removes all. If count is specified, randomly removes that many.
        /// </summary>
        public static int RemoveBanditPartiesByCulture(CultureObject cultureObj, int? count)
        {
            MBList<MobileParty> matchingParties = new();
            MBReadOnlyList<MobileParty> allBanditParties = MobileParty.AllBanditParties;

            // Collect matching parties
            for (int i = 0; i < allBanditParties.Count; i++)
            {
                MobileParty party = allBanditParties[i];
                if (party != null && party.IsActive && party.ActualClan != null && party.ActualClan.Culture == cultureObj)
                {
                    matchingParties.Add(party);
                }
            }

            // Remove parties
            if (count.HasValue)
            {
                return RemoveRandomItems(matchingParties, count.Value, RemoveAllParties);
            }
            else
            {
                return RemoveAllParties(matchingParties);
            }
        }

        /// <summary>
        /// Removes hideouts of a specific culture. If count is null, removes all. If count is specified, randomly removes that many.
        /// Hideouts are removed by destroying all bandit parties inside them, causing the hideout to deactivate naturally.
        /// </summary>
        public static int RemoveHideoutsByCulture(CultureObject cultureObj, int? count)
        {
            MBList<Hideout> matchingHideouts = new();
            MBReadOnlyList<Hideout> allHideouts = Hideout.All;

            // Collect matching hideouts
            for (int i = 0; i < allHideouts.Count; i++)
            {
                Hideout hideout = allHideouts[i];
                if (HideoutMatchesCulture(hideout, cultureObj))
                {
                    matchingHideouts.Add(hideout);
                }
            }

            // Remove hideouts
            if (count.HasValue)
            {
                return RemoveRandomItems(matchingHideouts, count.Value, RemoveAllHideouts);
            }
            else
            {
                return RemoveAllHideouts(matchingHideouts);
            }
        }

        /// <summary>
        /// Removes all parties in the provided list.
        /// </summary>
        private static int RemoveAllParties(MBList<MobileParty> parties)
        {
            int removedCount = 0;

            for (int i = 0; i < parties.Count; i++)
            {
                MobileParty party = parties[i];
                if (party != null && party.IsActive)
                {
                    DestroyPartyAction.Apply(null, party);
                    removedCount++;
                }
            }

            return removedCount;
        }

        /// <summary>
        /// Randomly selects and removes items from a list using the provided removal function.
        /// </summary>
        /// <typeparam name="T">The type of items in the list</typeparam>
        /// <param name="items">The list of items to select from</param>
        /// <param name="countToRemove">How many items to randomly remove</param>
        /// <param name="removeAllFunc">Function that removes all items in a list and returns count removed</param>
        /// <returns>Number of items actually removed</returns>
        private static int RemoveRandomItems<T>(MBList<T> items, int countToRemove, Func<MBList<T>, int> removeAllFunc)
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

        /// <summary>
        /// Removes all hideouts in the provided list by destroying all bandit parties inside them.
        /// </summary>
        private static int RemoveAllHideouts(MBList<Hideout> hideouts)
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

        /// <summary>
        /// Checks if a hideout contains parties of the specified bandit culture.
        /// </summary>
        private static bool HideoutMatchesCulture(Hideout hideout, CultureObject banditCulture)
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

        /// <summary>
        /// Gets the count of bandit parties for a specific culture.
        /// </summary>
        private static int GetBanditPartyCountByCulture(CultureObject cultureObj)
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

        /// <summary>
        /// Gets the count of hideouts for a specific culture.
        /// </summary>
        private static int GetHideoutCountByCulture(CultureObject cultureObj)
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

        #endregion
    }
}
