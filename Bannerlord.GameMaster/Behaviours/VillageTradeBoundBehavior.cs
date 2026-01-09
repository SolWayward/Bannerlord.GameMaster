using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.SaveSystem;
using Bannerlord.GameMaster.Information;

namespace Bannerlord.GameMaster.Behaviours
{
    /// <summary>
    /// Campaign behavior that manages custom village trade bound settlements with save/load persistence.
    /// When a village's trade bound is manually set, this behavior tracks and reapplies it on load.
    /// Note: Villages bound to castles need trade bounds, villages bound to towns ignore them.
    /// </summary>
    internal class VillageTradeBoundBehavior : CampaignBehaviorBase
    {
        // Store as separate lists for reliable serialization
        private List<string> _villageIds;
        private List<string> _tradeBoundSettlementIds;

        public VillageTradeBoundBehavior()
        {
            _villageIds = new List<string>();
            _tradeBoundSettlementIds = new List<string>();
        }

        public override void RegisterEvents()
        {
            // Register event to reapply trade bounds after load
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        }

        public override void SyncData(IDataStore dataStore)
        {
            // Sync as separate lists
            if (dataStore.IsSaving)
            {
                if (_villageIds == null) _villageIds = new List<string>();
                if (_tradeBoundSettlementIds == null) _tradeBoundSettlementIds = new List<string>();
            }
            
            dataStore.SyncData("VillageTradeBoundVillageIds", ref _villageIds);
            dataStore.SyncData("VillageTradeBoundSettlementIds", ref _tradeBoundSettlementIds);
            
            // Ensure lists are initialized after loading
            if (dataStore.IsLoading)
            {
                if (_villageIds == null) _villageIds = new List<string>();
                if (_tradeBoundSettlementIds == null) _tradeBoundSettlementIds = new List<string>();
            }
        }

        /// <summary>
        /// Called after session is loaded. Reapplies all custom village trade bounds.
        /// </summary>
        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            if (_villageIds == null || _villageIds.Count == 0)
                return;

            InfoMessage.Status($"[GameMaster] Reapplying {_villageIds.Count} custom village trade bounds");

            for (int i = 0; i < _villageIds.Count; i++)
            {
                Settlement villageSettlement = Settlement.Find(_villageIds[i]);
                Settlement tradeBoundSettlement = Settlement.Find(_tradeBoundSettlementIds[i]);
                
                if (villageSettlement != null && villageSettlement.IsVillage && tradeBoundSettlement != null)
                {
                    ApplyTradeBoundChange(villageSettlement.Village, tradeBoundSettlement);
                }
            }
        }

        /// <summary>
        /// Tracks a village trade bound change for save persistence.
        /// </summary>
        /// <param name="village">The village to track</param>
        /// <param name="newTradeBound">The new trade bound settlement</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool TrackTradeBoundChange(Village village, Settlement newTradeBound)
        {
            if (village == null || newTradeBound == null)
            {
                InfoMessage.Error("[GameMaster] TrackTradeBoundChange called with null parameters");
                return false;
            }

            try
            {
                // Find existing entry
                int index = _villageIds.IndexOf(village.Settlement.StringId);
                
                if (index >= 0)
                {
                    // Update existing
                    _tradeBoundSettlementIds[index] = newTradeBound.StringId;
                }
                else
                {
                    // Add new
                    _villageIds.Add(village.Settlement.StringId);
                    _tradeBoundSettlementIds.Add(newTradeBound.StringId);
                }

                // Apply the change
                ApplyTradeBoundChange(village, newTradeBound);

                return true;
            }
            catch (Exception ex)
            {
                InfoMessage.Error($"[GameMaster] Failed to track trade bound change: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Removes tracking for a village trade bound.
        /// </summary>
        /// <param name="village">The village to stop tracking</param>
        /// <returns>True if removed, false if village was not tracked</returns>
        public bool RemoveTracking(Village village)
        {
            if (village == null)
                return false;

            int index = _villageIds.IndexOf(village.Settlement.StringId);
            if (index < 0)
                return false; // Not tracked

            try
            {
                // Remove from lists
                _villageIds.RemoveAt(index);
                _tradeBoundSettlementIds.RemoveAt(index);

                return true;
            }
            catch (Exception ex)
            {
                InfoMessage.Error($"[GameMaster] Failed to remove trade bound tracking: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Checks if a village trade bound is being tracked.
        /// </summary>
        /// <param name="village">The village to check</param>
        /// <returns>True if the village trade bound is tracked</returns>
        public bool IsTracked(Village village)
        {
            return village != null && _villageIds.Contains(village.Settlement.StringId);
        }

        /// <summary>
        /// Gets the tracked trade bound settlement for a village.
        /// </summary>
        /// <param name="village">The village to check</param>
        /// <returns>The tracked trade bound settlement, or null if not tracked</returns>
        public Settlement GetTrackedTradeBound(Village village)
        {
            if (village == null)
                return null;
                
            int index = _villageIds.IndexOf(village.Settlement.StringId);
            if (index < 0)
                return null;

            return Settlement.Find(_tradeBoundSettlementIds[index]);
        }

        /// <summary>
        /// Gets the number of villages with tracked trade bounds.
        /// </summary>
        public int GetTrackedVillageCount()
        {
            return _villageIds.Count;
        }

        /// <summary>
        /// Applies a trade bound change to a village.
        /// This setter handles all synchronization automatically.
        /// </summary>
        private void ApplyTradeBoundChange(Village village, Settlement newTradeBound)
        {
            if (village == null || newTradeBound == null)
                return;

            village.TradeBound = newTradeBound;
        }
    }
}
