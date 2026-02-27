using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using Bannerlord.GameMaster.Information;
using Bannerlord.GameMaster.Common;
using Bannerlord.GameMaster.Behaviours;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Library;

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

        /// MARK: ChangeOwner
        /// <summary>
        /// Sets new owner of the settlement by calling ChangeOwnerOfSettlementAction
        /// </summary>
        public static void ChangeSettlementOwner(Settlement settlement, Hero newOwnerHero)
        {
            ChangeOwnerOfSettlementAction.ApplyByDefault(newOwnerHero, settlement);
        }

        #region Settlement Culture

        /// <summary>
        /// Gets the SettlementCultureBehavior instance from the current campaign.
        /// </summary>
        private static SettlementCultureBehavior GetCultureBehavior()
        {
            return Campaign.Current?.GetCampaignBehavior<SettlementCultureBehavior>();
        }

        /// MARK: Change Culture
        /// <summary>
        /// Changes the culture of a settlement with persistence through save/load cycles.
        /// Routes through SettlementCultureBehavior for automatic persistence tracking.
        /// </summary>
        /// <param name="settlement">The settlement to change culture for</param>
        /// <param name="culture">The new culture to apply</param>
        /// <param name="updateNotables">If true, updates the culture of all notables in the settlement</param>
        /// <param name="includeBoundVillages">If true and settlement is a town/castle, recursively updates bound villages</param>
        /// <returns>BLGMResult indicating success or failure with a message</returns>
        public static BLGMResult ChangeSettlementCulture(Settlement settlement, CultureObject culture, bool updateNotables, bool includeBoundVillages)
        {
            SettlementCultureBehavior behavior = GetCultureBehavior();
            if (behavior == null)
                return BLGMResult.Error("Cannot change settlement culture: SettlementCultureBehavior not found (campaign not loaded?)");

            if (settlement == null)
                return BLGMResult.Error("SetSettlementCulture() failed, settlement cannot be null");

            if (culture == null)
                return BLGMResult.Error("SetSettlementCulture() failed, culture cannot be null");

            bool success = behavior.SetSettlementCulture(settlement, culture, updateNotables, includeBoundVillages);
            if (!success)
                return BLGMResult.Error($"Failed to change culture of '{settlement.Name}' to '{culture.Name}'");

            return BLGMResult.Success($"Changed culture of '{settlement.Name}' to '{culture.Name}'");
        }

        /// MARK: Set Culture
        /// <summary>
        /// Changes the culture of a settlement with persistence through save/load cycles.
        /// This is a compatibility overload. Prefer ChangeSettlementCulture() for richer BLGMResult error handling.
        /// </summary>
        /// <param name="settlement">The settlement to change culture for</param>
        /// <param name="culture">The new culture to apply</param>
        /// <param name="updateNotables">If true, updates the culture of all notables in the settlement</param>
        /// <param name="includeBoundVillages">If true and settlement is a town/castle, recursively updates bound villages</param>
        /// <returns>True if the culture change was successful, false otherwise</returns>
        public static bool SetSettlementCulture(Settlement settlement, CultureObject culture, bool updateNotables, bool includeBoundVillages)
        {
            return ChangeSettlementCulture(settlement, culture, updateNotables, includeBoundVillages).IsSuccess;
        }

        /// MARK: Has Custom Culture
        /// <summary>
        /// Checks if a settlement has a custom culture set by GameMaster.
        /// </summary>
        /// <param name="settlement">The settlement to check</param>
        /// <returns>True if the settlement has a custom culture</returns>
        public static bool HasCustomSettlementCulture(Settlement settlement)
        {
            SettlementCultureBehavior behavior = GetCultureBehavior();
            return behavior?.HasCustomCulture(settlement) ?? false;
        }

        /// MARK: Get Original Culture
        /// <summary>
        /// Gets the original culture of a settlement before it was changed by GameMaster.
        /// </summary>
        /// <param name="settlement">The settlement to check</param>
        /// <returns>Original culture if changed, null otherwise</returns>
        public static CultureObject GetOriginalSettlementCulture(Settlement settlement)
        {
            SettlementCultureBehavior behavior = GetCultureBehavior();
            return behavior?.GetOriginalCulture(settlement);
        }

        /// MARK: Reset Culture
        /// <summary>
        /// Resets a settlement to its original culture.
        /// </summary>
        /// <param name="settlement">The settlement to reset</param>
        /// <returns>BLGMResult indicating success or failure with a message</returns>
        public static BLGMResult ResetSettlementCulture(Settlement settlement)
        {
            SettlementCultureBehavior behavior = GetCultureBehavior();
            if (behavior == null)
                return BLGMResult.Error("Cannot reset settlement culture: SettlementCultureBehavior not found (campaign not loaded?)");

            bool success = behavior.ResetSettlementCulture(settlement);
            if (!success)
                return BLGMResult.Error($"Settlement '{settlement?.Name}' does not have a custom culture to reset");

            return BLGMResult.Success($"Reset culture of '{settlement.Name}' to original");
        }

        /// MARK: Custom Culture Count
        /// <summary>
        /// Gets the number of settlements with custom cultures.
        /// </summary>
        /// <returns>The count of settlements with custom cultures</returns>
        public static int GetCustomCultureCount()
        {
            SettlementCultureBehavior behavior = GetCultureBehavior();
            return behavior?.GetCustomCultureCount() ?? 0;
        }

        #endregion

        /// MARK: Bound Village Count
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
