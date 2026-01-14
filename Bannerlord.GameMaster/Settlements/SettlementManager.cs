using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using Bannerlord.GameMaster.Information;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem.Actions;

namespace Bannerlord.GameMaster.Settlements
{
    /// <summary>
    /// Manages Settlements for BLGM actions.<br/>
    /// Remaining Settlement logic should be refactored out of commands to here or similar classes
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
    }
}
