using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using Bannerlord.GameMaster.Information;
using Bannerlord.GameMaster.Common;
using Bannerlord.GameMaster.Behaviours;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem.Actions;

namespace Bannerlord.GameMaster.Settlements
{
    /// <summary>
    /// Manages Settlements for BLGM actions.<br/>
    /// Main entry point for settlement operations including renaming with persistence.
    /// </summary>
    public static class SettlementManager
    {
        #region AllSettlements
        /// <summary>Gets a random Settlement from All settlements. Includes Towns, Castles, and Villages. Returns Null if none found</summary>
        public static Settlement GetRandomSettlement() => Settlement.All.GetRandomElement();

        /// <summary>Gets a random Fortification from All settlements. Includes Towns and Castles. Returns Null if none found</summary>
        public static Settlement GetRandomFortification() => Settlement.All.FindAll(s => s.IsFortification).GetRandomElement();

        /// <summary>Gets a random Town from All settlements. Includes Towns only. Returns Null if none found</summary>
        public static Settlement GetRandomTown() => Settlement.All.FindAll(s => s.IsTown).GetRandomElement();

        /// <summary>Gets a random Castle from All settlements. Includes Castles only. Returns Null if none found</summary>
        public static Settlement GetRandomCastle() => Settlement.All.FindAll(s => s.IsCastle).GetRandomElement();

        /// <summary>Gets a random Village from All settlements. Includes Villages only. Returns Null if none found</summary>
        public static Settlement GetRandomVillage() => Settlement.All.FindAll(s => s.IsVillage).GetRandomElement();
        #endregion

        #region ClanSettlements
        /// <summary>Gets a random Settlement owned by the provided Clan. Includes Towns, Castles, and Villages. Returns Null if none found</summary>
        public static Settlement GetRandomClanSettlement(Clan clan) => clan?.Settlements.GetRandomElement();

        /// <summary>Gets a random Fortification owned by the provided Clan. Includes Towns and Castles. Returns Null if none found</summary>
        public static Settlement GetRandomClanFortification(Clan clan) => clan?.Settlements.FindAll(s => s.IsFortification).GetRandomElement();

        /// <summary>Gets a random Town owned by the provided Clan. Includes Towns only. Returns Null if none found</summary>
        public static Settlement GetRandomClanTown(Clan clan) => clan?.Settlements.FindAll(s => s.IsTown).GetRandomElement();

        /// <summary>Gets a random Castle owned by the provided Clan. Includes Castles only. Returns Null if none found</summary>
        public static Settlement GetRandomClanCastle(Clan clan) => clan?.Settlements.FindAll(s => s.IsCastle).GetRandomElement();

        /// <summary>Gets a random Village owned by the provided Clan. Includes Villages only. Returns Null if none found</summary>
        public static Settlement GetRandomClanVillage(Clan clan) => clan?.Settlements.FindAll(s => s.IsVillage).GetRandomElement();
        #endregion

        #region KingdomSettlements
        /// <summary>Gets a random Settlement within the provided Kingdom. Includes Towns, Castles, and Villages. Returns Null if none found</summary>
        public static Settlement GetRandomKingdomSettlement(Kingdom kingdom) => kingdom?.Settlements.GetRandomElement();

        /// <summary>Gets a random Fortification within the provided Kingdom. Includes Towns and Castles. Returns Null if none found</summary>
        public static Settlement GetRandomKingdomFortification(Kingdom kingdom) => kingdom?.Settlements.FindAll(s => s.IsFortification).GetRandomElement();

        /// <summary>Gets a random Town within the provided Kingdom. Includes Towns only. Returns Null if none found</summary>
        public static Settlement GetRandomKingdomTown(Kingdom kingdom) => kingdom?.Settlements.FindAll(s => s.IsTown).GetRandomElement();

        /// <summary>Gets a random Castle within the provided Kingdom. Includes Castles only. Returns Null if none found</summary>
        public static Settlement GetRandomKingdomCastle(Kingdom kingdom) => kingdom?.Settlements.FindAll(s => s.IsCastle).GetRandomElement();

        /// <summary>Gets a random Village within the provided Kingdom. Includes Villages only. Returns Null if none found</summary>
        public static Settlement GetRandomKingdomVillage(Kingdom kingdom) => kingdom?.Settlements.FindAll(s => s.IsVillage).GetRandomElement();
        #endregion

        /// <summary>
        /// Sets new owner of the settlement by calling ChangeOwnerOfSettlementAction
        /// </summary>
        public static void ChangeSettlementOwner(Settlement settlement, Hero newOwnerHero)
        {
            ChangeOwnerOfSettlementAction.ApplyByDefault(newOwnerHero, settlement);
        }

        /// <summary>
        /// Changes the culture of a settlement and optionally its notables and bound villages.
        /// </summary>
        /// <param name="settlement">The settlement to change culture for</param>
        /// <param name="culture">The new culture to apply</param>
        /// <param name="updateNotables">If true, updates the culture of all notables in the settlement</param>
        /// <param name="includeBoundVillages">If true and settlement is a town, recursively updates bound villages</param>
        /// <returns>True if the culture change was successful, false otherwise</returns>
        public static bool SetSettlementCulture(Settlement settlement, CultureObject culture, bool updateNotables, bool includeBoundVillages)
        {
            if (settlement == null)
            {
                InfoMessage.Error("[GameMaster] SetSettlementCulture called with null settlement");
                return false;
            }

            if (culture == null)
            {
                InfoMessage.Error("[GameMaster] SetSettlementCulture called with null culture");
                return false;
            }

            try
            {
                // Set the settlement's culture
                settlement.Culture = culture;

                // Update notables if requested
                if (updateNotables && settlement.Notables != null)
                {
                    foreach (Hero notable in settlement.Notables)
                    {
                        if (notable != null)
                        {
                            notable.Culture = culture;
                        }
                    }
                }

                // Update bound villages if requested and settlement is a town or castle
                if (includeBoundVillages && (settlement.IsTown || settlement.IsCastle) && settlement.BoundVillages != null)
                {
                    foreach (Village village in settlement.BoundVillages)
                    {
                        if (village?.Settlement != null)
                        {
                            SetSettlementCulture(village.Settlement, culture, updateNotables, false);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                InfoMessage.Error($"[GameMaster] Failed to set settlement culture: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets the count of bound villages for a settlement.
        /// </summary>
        /// <param name="settlement">The settlement to check</param>
        /// <returns>The number of bound villages, or 0 if none or settlement is null</returns>
        public static int GetBoundVillagesCount(Settlement settlement)
        {
            if (settlement == null)
            {
                return 0;
            }

            return settlement.BoundVillages?.Count ?? 0;
        }

        #region Settlement Naming

        /// <summary>
        /// Gets the SettlementNameBehavior instance from the current campaign.
        /// </summary>
        private static SettlementNameBehavior GetNameBehavior()
        {
            return Campaign.Current?.GetCampaignBehavior<SettlementNameBehavior>();
        }

        /// <summary>
        /// Renames Settlement and calls behavior to save the settlement name ensuring persistence
        /// </summary>
        /// <returns>BLGMResult indicating success or failure with a message</returns>
        public static BLGMResult RenameSettlement(Settlement settlement, string newName)
        {
            SettlementNameBehavior behavior = GetNameBehavior();
            if (behavior == null)
                return new BLGMResult(false, "Cannot rename settlement: SettlementNameBehavior not found (campaign not loaded?)");

            return behavior.RenameSettlement(settlement, newName);
        }

        /// <summary>
        /// Resets a settlement to its original name.
        /// </summary>
        /// <returns>BLGMResult indicating success or failure with a message</returns>
        public static BLGMResult ResetSettlementName(Settlement settlement)
        {
            SettlementNameBehavior behavior = GetNameBehavior();
            if (behavior == null)
                return new BLGMResult(false, "Failed to reset settlement name: SettlementNameBehavior not found (campaign not loaded?)");

            return behavior.ResetSettlementName(settlement);
        }

        /// <summary>
        /// Resets all renamed settlements to their original names.
        /// </summary>
        /// <returns>BLGMResult with count of reset settlements</returns>
        public static BLGMResult ResetAllSettlementNames()
        {
            SettlementNameBehavior behavior = GetNameBehavior();
            if (behavior == null)
                return new BLGMResult(false, "Failed to reset settlement name: SettlementNameBehavior not found (campaign not loaded?)");

            return behavior.ResetAllSettlementNames();
        }

        /// <summary>
        /// Gets the original name of a settlement if it was renamed.
        /// </summary>
        /// <param name="settlement">The settlement to check</param>
        /// <returns>Original name if renamed, null otherwise</returns>
        public static string GetOriginalSettlementName(Settlement settlement)
        {
            SettlementNameBehavior behavior = GetNameBehavior();
            return behavior?.GetOriginalName(settlement);
        }

        /// <summary>
        /// Checks if a settlement has been renamed.
        /// </summary>
        /// <param name="settlement">The settlement to check</param>
        /// <returns>True if the settlement has a custom name</returns>
        public static bool IsSettlementRenamed(Settlement settlement)
        {
            SettlementNameBehavior behavior = GetNameBehavior();
            return behavior?.IsRenamed(settlement) ?? false;
        }

        /// <summary>
        /// Gets the count of renamed settlements.
        /// </summary>
        public static int GetRenamedSettlementCount()
        {
            SettlementNameBehavior behavior = GetNameBehavior();
            return behavior?.GetRenamedSettlementCount() ?? 0;
        }

        #endregion
    }
}
