using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using Bannerlord.GameMaster.Information;

namespace Bannerlord.GameMaster.Settlements
{
    /// <summary>
    /// Manages Settlements for BLGM actions.<br/>
    /// Remaining Settlement logic should be refactored out of commands to here or similar classes
    /// </summary>
    public static class SettlementManager
    {
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
