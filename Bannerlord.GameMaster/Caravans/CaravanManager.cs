using System;
using System.Linq;
using Bannerlord.GameMaster.Party;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace Bannerlord.GameMaster.Caravans
{
    public static class CaravanManager
    {
        #region Properties

        public static MBReadOnlyList<MobileParty> AllCaravanParties => MobileParty.AllCaravanParties;

        public static int TotalCaravanCount => MobileParty.AllCaravanParties.Count;
        public static int TotalNonDisbandingCaravans => TotalCaravanCount - TotalDisbandingCaravans;
        public static int TotalDisbandingCaravans
        {
            get
            {
                int disbandingCount = 0;
                MBReadOnlyList<MobileParty> caravans = AllCaravanParties;
                for (int i = 0; i < caravans.Count; i++)
                {
                    if (caravans[i].IsDisbanding)
                        disbandingCount++;
                }

                return disbandingCount;
            }
        }

        #endregion

        #region Owner Type Counts

        /// <summary>
        /// Gets the total count of caravans owned by the player.
        /// </summary>
        public static int TotalPlayerCaravans
        {
            get
            {
                MBReadOnlyList<MobileParty> caravans = AllCaravanParties;
                int count = 0;

                for (int i = 0; i < caravans.Count; i++)
                {
                    MobileParty caravan = caravans[i];
                    if (caravan != null && caravan.IsActive && caravan.Owner?.IsHumanPlayerCharacter == true)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        /// <summary>
        /// Gets the total count of caravans owned by notables.
        /// </summary>
        public static int TotalNotableCaravans
        {
            get
            {
                MBReadOnlyList<MobileParty> caravans = AllCaravanParties;
                int count = 0;

                for (int i = 0; i < caravans.Count; i++)
                {
                    MobileParty caravan = caravans[i];
                    if (caravan != null && caravan.IsActive && caravan.Owner?.IsNotable == true)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        /// <summary>
        /// Gets the total count of caravans owned by NPC lords.
        /// </summary>
        public static int TotalNPCLordCaravans
        {
            get
            {
                MBReadOnlyList<MobileParty> caravans = AllCaravanParties;
                int count = 0;

                for (int i = 0; i < caravans.Count; i++)
                {
                    MobileParty caravan = caravans[i];
                    if (caravan != null && caravan.IsActive && caravan.Owner?.IsLord == true && caravan.Owner?.IsHumanPlayerCharacter == false)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        /// <summary>
        /// Gets the count of player caravans currently disbanding.
        /// </summary>
        public static int TotalPlayerCaravansDisbanding
        {
            get
            {
                MBReadOnlyList<MobileParty> caravans = AllCaravanParties;
                int count = 0;

                for (int i = 0; i < caravans.Count; i++)
                {
                    MobileParty caravan = caravans[i];
                    if (caravan != null && caravan.IsActive && caravan.IsDisbanding && caravan.Owner?.IsHumanPlayerCharacter == true)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        /// <summary>
        /// Gets the count of notable caravans currently disbanding.
        /// </summary>
        public static int TotalNotableCaravansDisbanding
        {
            get
            {
                MBReadOnlyList<MobileParty> caravans = AllCaravanParties;
                int count = 0;

                for (int i = 0; i < caravans.Count; i++)
                {
                    MobileParty caravan = caravans[i];
                    if (caravan != null && caravan.IsActive && caravan.IsDisbanding && caravan.Owner?.IsNotable == true)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        /// <summary>
        /// Gets the count of NPC lord caravans currently disbanding.
        /// </summary>
        public static int TotalNPCLordCaravansDisbanding
        {
            get
            {
                MBReadOnlyList<MobileParty> caravans = AllCaravanParties;
                int count = 0;

                for (int i = 0; i < caravans.Count; i++)
                {
                    MobileParty caravan = caravans[i];
                    if (caravan != null && caravan.IsActive && caravan.IsDisbanding && caravan.Owner?.IsLord == true && caravan.Owner?.IsHumanPlayerCharacter == false)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        #endregion

        #region Creation Methods

        /// <summary>
        /// Creates a notable caravan at the specified settlement.
        /// Finds a notable without a caravan and creates one for them.
        /// </summary>
        /// <param name="settlement">The settlement where the caravan will be created. Must be a town.</param>
        /// <returns>The created MobileParty if successful, null if failed.</returns>
        public static MobileParty CreateNotableCaravan(Settlement settlement)
        {
            if (settlement == null)
                return null;

            if (!settlement.IsTown)
                return null;

            // Find a notable without a caravan
            Hero caravanOwner = settlement.Notables.FirstOrDefault(n => n.OwnedCaravans.Count == 0);
            
            if (caravanOwner == null)
                return null;

            // Get a party template for caravans
            PartyTemplateObject partyTemplate = MBObjectManager.Instance.GetObjectTypeList<PartyTemplateObject>()
                .FirstOrDefault(pt => pt.StringId.Contains("caravan"));
            
            if (partyTemplate == null)
                return null;

            // Create the caravan using the game's API
            MobileParty caravan = CaravanPartyComponent.CreateCaravanParty(
                caravanOwner,
                settlement,
                partyTemplate
            );

            return caravan;
        }

        /// <summary>
        /// Creates a player caravan at the specified settlement.
        /// </summary>
        /// <param name="settlement">The settlement where the caravan will be created. Must be a town.</param>
        /// <param name="leaderHero">Optional hero to lead the caravan. If null, will try to find an available companion.</param>
        /// <returns>The created MobileParty if successful, null if failed.</returns>
        public static MobileParty CreatePlayerCaravan(Settlement settlement, Hero leaderHero = null)
        {
            if (settlement == null)
                return null;

            if (!settlement.IsTown)
                return null;

            Hero caravanLeader = null;
            
            // If a specific leader was provided, use them
            if (leaderHero != null)
            {
                if (leaderHero.Clan != Clan.PlayerClan)
                    return null;
                
                if (leaderHero.PartyBelongedTo != null)
                    return null;
                
                caravanLeader = leaderHero;
            }
            else
            {
                // Try to find an available companion
                caravanLeader = Clan.PlayerClan.Companions.FirstOrDefault(c =>
                    c.PartyBelongedTo == null &&
                    !c.IsPrisoner &&
                    c.IsActive);
            }

            // Get a party template for caravans
            PartyTemplateObject partyTemplate = MBObjectManager.Instance.GetObjectTypeList<PartyTemplateObject>()
                .FirstOrDefault(pt => pt.StringId.Contains("caravan") && pt.StringId.Contains("template"));
            
            if (partyTemplate == null)
                return null;

            // Create caravan for player clan using proper owner
            MobileParty caravan = CaravanPartyComponent.CreateCaravanParty(
                Hero.MainHero,  // Owner is always the clan leader for player caravans
                settlement,
                partyTemplate,
                false,  // isInitialSpawn
                caravanLeader  // Optional leader companion
            );

            return caravan;
        }

        #endregion

        #region Disband Methods

        /// <summary>
        /// Disbands all caravan parties. If count is null, disbands all. If count is specified, randomly disbands that many.
        /// </summary>
        /// <param name="count">Optional number of caravans to disband. If null, disbands all.</param>
        /// <returns>The number of parties that disband was initiated for</returns>
        public static int DisbandAllCaravanParties(int? count = null)
        {
            MBList<MobileParty> allCaravans = new();
            MBReadOnlyList<MobileParty> caravans = AllCaravanParties;

            for (int i = 0; i < caravans.Count; i++)
            {
                MobileParty caravan = caravans[i];
                if (caravan != null && caravan.IsActive && !caravan.IsDisbanding)
                {
                    allCaravans.Add(caravan);
                }
            }

            if (count.HasValue)
            {
                return DisbandRandomCaravans(allCaravans, count.Value);
            }
            else
            {
                return DisbandCaravanList(allCaravans);
            }
        }

        /// <summary>
        /// Disbands random caravans from all caravans. If count is null, disbands all. If count is specified, randomly disbands that many.
        /// </summary>
        /// <param name="count">Optional number of caravans to disband. If null, disbands all.</param>
        /// <returns>The number of parties that disband was initiated for</returns>
        public static int DisbandCaravans(int? count = null)
        {
            return DisbandAllCaravanParties(count);
        }

        /// <summary>
        /// Disbands caravans owned by player. If count is null, disbands all. If count is specified, randomly disbands that many.
        /// </summary>
        /// <param name="count">Optional number of caravans to disband. If null, disbands all.</param>
        /// <returns>The number of parties that disband was initiated for</returns>
        public static int DisbandPlayerCaravans(int? count = null)
        {
            return DisbandCaravansByOwnerType(caravan => caravan.Owner?.IsHumanPlayerCharacter == true, count);
        }

        /// <summary>
        /// Disbands caravans owned by notables. If count is null, disbands all. If count is specified, randomly disbands that many.
        /// </summary>
        /// <param name="count">Optional number of caravans to disband. If null, disbands all.</param>
        /// <returns>The number of parties that disband was initiated for</returns>
        public static int DisbandNotableCaravans(int? count = null)
        {
            return DisbandCaravansByOwnerType(caravan => caravan.Owner?.IsNotable == true, count);
        }

        /// <summary>
        /// Disbands caravans owned by NPC Lords. If count is null, disbands all. If count is specified, randomly disbands that many.
        /// </summary>
        /// <param name="count">Optional number of caravans to disband. If null, disbands all.</param>
        /// <returns>The number of parties that disband was initiated for</returns>
        public static int DisbandNPCLordCaravans(int? count = null)
        {
            return DisbandCaravansByOwnerType(caravan => caravan.Owner?.IsLord == true && caravan.Owner?.IsHumanPlayerCharacter == false, count);
        }

        #endregion

        #region Cancel Disband

        public static int CancelAllDisbandingCaravans() => CancelDisbandingCaravans(AllCaravanParties);

        #endregion

        #region Destroy Caravans

        public static int ForceDestroyDisbandingCaravans()
        {
            MBList<MobileParty> disbandingCaravans = new();
            MBReadOnlyList<MobileParty> caravans = AllCaravanParties;

            // Collect disbanding caravans first
            for (int i = 0; i < caravans.Count; i++)
            {
                MobileParty caravan = caravans[i];
                if (caravan != null && caravan.IsActive && caravan.IsDisbanding)
                {
                    disbandingCaravans.Add(caravan);
                }
            }

            // Now destroy them
            int destroyedCount = 0;
            for (int i = 0; i < disbandingCaravans.Count; i++)
            {
                MobileParty caravan = disbandingCaravans[i];
                if (caravan != null && caravan.IsActive)
                {
                    caravan.DestroyParty();
                    destroyedCount++;
                }
            }

            return destroyedCount;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Disbands caravans by owner type. If count is null, disbands all. If count is specified, randomly disbands that many.
        /// </summary>
        private static int DisbandCaravansByOwnerType(Func<MobileParty, bool> predicate, int? count)
        {
            MBList<MobileParty> matchingCaravans = new();
            MBReadOnlyList<MobileParty> allCaravans = AllCaravanParties;

            // Collect matching caravans
            for (int i = 0; i < allCaravans.Count; i++)
            {
                MobileParty caravan = allCaravans[i];
                if (caravan != null && caravan.IsActive && !caravan.IsDisbanding && predicate(caravan))
                {
                    matchingCaravans.Add(caravan);
                }
            }

            // Disband caravans
            if (count.HasValue)
            {
                return DisbandRandomCaravans(matchingCaravans, count.Value);
            }
            else
            {
                return DisbandCaravanList(matchingCaravans);
            }
        }

        /// <summary>
        /// Disbands all caravans in the provided list.
        /// </summary>
        private static int DisbandCaravanList(MBList<MobileParty> caravans)
        {
            int disbandedCount = 0;

            for (int i = 0; i < caravans.Count; i++)
            {
                MobileParty caravan = caravans[i];
                if (caravan != null && caravan.IsActive && !caravan.IsDisbanding)
                {
                    caravan.Disband();
                    disbandedCount++;
                }
            }

            return disbandedCount;
        }

        /// <summary>
        /// Randomly selects and disbands caravans from a list.
        /// </summary>
        /// <param name="caravans">The list of caravans to select from</param>
        /// <param name="countToDisband">How many caravans to randomly disband</param>
        /// <returns>Number of caravans actually disbanded</returns>
        private static int DisbandRandomCaravans(MBList<MobileParty> caravans, int countToDisband)
        {
            if (countToDisband <= 0 || caravans.Count == 0)
            {
                return 0;
            }

            int actualCountToDisband = Math.Min(countToDisband, caravans.Count);
            MBList<MobileParty> selectedCaravans = new();

            // Create a copy to select from so we don't affect the source list directly during selection
            MBList<MobileParty> availableCaravans = new();
            for (int i = 0; i < caravans.Count; i++)
            {
                availableCaravans.Add(caravans[i]);
            }

            // Randomly select caravans
            for (int i = 0; i < actualCountToDisband; i++)
            {
                // Get a random index from the current pool
                int randomIndex = RandomNumberGen.Instance.NextRandomInt(availableCaravans.Count);
                
                // Add the selected item to our target list
                selectedCaravans.Add(availableCaravans[randomIndex]);

                // OPTIMIZATION: Swap and Pop
                // Calculate the index of the very last item in the list
                int lastIndex = availableCaravans.Count - 1;

                // Overwrite the item we just picked (at randomIndex) with the last item
                availableCaravans[randomIndex] = availableCaravans[lastIndex];

                // Remove the last item (which is now a duplicate). 
                // Removing from the end is O(1) because no other items need to shift.
                availableCaravans.RemoveAt(lastIndex);
            }

            // Disband selected caravans
            return DisbandCaravanList(selectedCaravans);
        }

        /// <summary>
        /// Cancel disband action of all specified caravans
        /// </summary>
        /// <returns>The number of parties that disbanding was canceled for</returns>
        private static int CancelDisbandingCaravans(MBReadOnlyList<MobileParty> caravanParties)
        {
            int disbandsCanceled = 0;

            for (int i = 0; i < caravanParties.Count; i++)
            {
                MobileParty caravan = caravanParties[i];
                if (caravan.IsCaravan && caravan.IsDisbanding)
                {
                    caravan.CancelDisband();
                    disbandsCanceled++;
                }
            }

            return disbandsCanceled;
        }

        #endregion
    }
}
