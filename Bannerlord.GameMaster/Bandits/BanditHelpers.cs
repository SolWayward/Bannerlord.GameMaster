using System;
using System.Collections.Generic;
using Bannerlord.GameMaster.Party;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Bandits
{
    internal static class BanditHelpers
    {
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

        /// <summary>
        /// Checks if a hideout contains parties of the specified bandit culture.
        /// </summary>
        internal static bool HideoutMatchesCulture(Hideout hideout, CultureObject banditCulture)
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
    }
}