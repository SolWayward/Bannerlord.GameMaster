using System;
using System.Collections.Generic;
using Bannerlord.GameMaster.Cultures;
using Bannerlord.GameMaster.Information;
using Bannerlord.GameMaster.Party;
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
                return BanditHelpers.RemoveRandomItems(allBanditParties, count.Value, BanditHelpers.RemoveAllParties);
            }
            else
            {
                // Faster direct removal
                return BanditHelpers.RemoveAllParties(allBanditParties);
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
                return BanditHelpers.RemoveRandomItems(allHideouts, count.Value, BanditHelpers.RemoveAllHideouts);
            }
            else
            {
                // Directly remove all found hideouts
                return BanditHelpers.RemoveAllHideouts(allHideouts);
            }
        }

        #endregion

        #region Party Count

        public static int LootersPartyCount => BanditHelpers.GetBanditPartyCountByCulture(CultureLookup.Looters);
        public static int DeserterPartyCount => BanditHelpers.GetBanditPartyCountByCulture(CultureLookup.Deserters);
        public static int ForestBanditPartyCount => BanditHelpers.GetBanditPartyCountByCulture(CultureLookup.ForestBandits);
        public static int MountainBanditPartyCount => BanditHelpers.GetBanditPartyCountByCulture(CultureLookup.MountainBandits);
        public static int SeaRaiderPartyCount => BanditHelpers.GetBanditPartyCountByCulture(CultureLookup.SeaRaiders);
        public static int SteppeBanditPartyCount => BanditHelpers.GetBanditPartyCountByCulture(CultureLookup.SteppeBandits);
        public static int CorsairPartyCount => GameEnvironment.IsWarsailsDlcLoaded ? BanditHelpers.GetBanditPartyCountByCulture(CultureLookup.Corsairs) : 0; //Return 0 if Warsails isnt loaded

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

        public static int LooterHideoutCount => BanditHelpers.GetHideoutCountByCulture(CultureLookup.Looters);
        public static int DeserterHideoutCount => BanditHelpers.GetHideoutCountByCulture(CultureLookup.Deserters);
        public static int ForestBanditHideoutCount => BanditHelpers.GetHideoutCountByCulture(CultureLookup.ForestBandits);
        public static int MountainBanditHideoutCount => BanditHelpers.GetHideoutCountByCulture(CultureLookup.MountainBandits);
        public static int SeaRaiderHideoutCount => BanditHelpers.GetHideoutCountByCulture(CultureLookup.SeaRaiders);
        public static int SteppeBanditHideoutCount => BanditHelpers.GetHideoutCountByCulture(CultureLookup.SteppeBandits);
        public static int CorsairHideoutCount => GameEnvironment.IsWarsailsDlcLoaded ? BanditHelpers.GetHideoutCountByCulture(CultureLookup.Corsairs) : 0; //Return 0 if Warsails isnt loaded

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
                    if (hideouts[i] != null && hideouts[i].IsInfested)
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
                return BanditHelpers.RemoveRandomItems(matchingParties, count.Value, BanditHelpers.RemoveAllParties);
            }
            else
            {
                return BanditHelpers.RemoveAllParties(matchingParties);
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
                if (BanditHelpers.HideoutMatchesCulture(hideout, cultureObj))
                {
                    matchingHideouts.Add(hideout);
                }
            }

            // Remove hideouts
            if (count.HasValue)
            {
                return BanditHelpers.RemoveRandomItems(matchingHideouts, count.Value, BanditHelpers.RemoveAllHideouts);
            }
            else
            {
                return BanditHelpers.RemoveAllHideouts(matchingHideouts);
            }
        }

        #endregion
    }
}
