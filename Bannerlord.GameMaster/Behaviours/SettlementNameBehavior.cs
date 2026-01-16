using System;
using System.Collections.Generic;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;
using Bannerlord.GameMaster.Information;
using Bannerlord.GameMaster.Common;

namespace Bannerlord.GameMaster.Behaviours
{
    /// <summary>
    /// Campaign behavior that manages custom settlement names with save/load persistence.
    /// This is the central handler for all settlement rename operations.
    /// Stores both custom names and original names to allow reset functionality.
    /// Uses parallel lists for reliable serialization with TaleWorlds save system.
    /// </summary>
    internal class SettlementNameBehavior : CampaignBehaviorBase
    {
        // Store as separate lists for reliable serialization
        private List<string> _settlementIds;
        private List<string> _customNames;
        private List<string> _originalNames;

        // Cache the reflection field for performance
        private static FieldInfo _nameField;

        public SettlementNameBehavior()
        {
            _settlementIds = new();
            _customNames = new();
            _originalNames = new();
        }


        public override void RegisterEvents()
        {
            // Register event to reapply names after load
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        }

        public override void SyncData(IDataStore dataStore)
        {
            // Sync as separate lists
            if (dataStore.IsSaving)
            {
                _settlementIds ??= new();
                _customNames ??= new();
                _originalNames ??= new();
            }
            
            dataStore.SyncData("CustomNameSettlementIds", ref _settlementIds);
            dataStore.SyncData("CustomNameValues", ref _customNames);
            dataStore.SyncData("OriginalNameValues", ref _originalNames);
            
            // Ensure lists are initialized after loading
            if (dataStore.IsLoading)
            {
                _settlementIds ??= new();
                _customNames ??= new();
                _originalNames ??= new();                 
            }
        }

        /// <summary>
        /// Called after session is loaded. Reapplies all custom settlement names.
        /// Uses ApplyNameOnly to avoid re-saving already persisted data.
        /// </summary>
        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            if (_settlementIds == null || _settlementIds.Count == 0)
                return;

            InfoMessage.Status($"[GameMaster] Reapplying {_settlementIds.Count} custom settlement names");

            for (int i = 0; i < _settlementIds.Count; i++)
            {
                Settlement settlement = Settlement.Find(_settlementIds[i]);
                if (settlement != null)
                {
                    // Apply without saving (data is already persisted)
                    ApplyNameOnly(settlement, _customNames[i]);
                }
            }
        }

        #region Core Logic

        /// <summary>
        /// Internal method that applies a name change via reflection.
        /// Does NOT persist - used for on-load restoration and by RenameSettlement.
        /// Note: The map nameplate will update automatically when the player opens a menu.
        /// The hover tooltip updates immediately.
        /// </summary>
        /// <param name="settlement">The settlement to rename</param>
        /// <param name="newName">The new name for the settlement</param>
        /// <returns>BLGMResult indicating success or failure with a message</returns>
        private BLGMResult ApplyNameOnly(Settlement settlement, string newName)
        {
            if (settlement == null)
                return new BLGMResult(false, "Cannot rename settlement: settlement is null");

            if (string.IsNullOrWhiteSpace(newName))
                return new BLGMResult(false, "Cannot rename settlement: new name is empty");

            try
            {
                // Cache reflection field for performance
                if (_nameField == null)
                {
                    _nameField = typeof(Settlement).GetField("_name", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (_nameField == null)
                        return new BLGMResult(false, "Failed renaming Settlement. Could not find Settlement._name field via reflection").Log();
                }

                string previousName = settlement.Name.ToString();

                // Apply the name change via reflection
                _nameField.SetValue(settlement, new TextObject(newName));

                return new BLGMResult(true, $"Settlement renamed from '{previousName}' to '{newName}'");
            }
            
            catch (Exception ex)
            {
                return new BLGMResult(false, $"failed renaming settlement: {ex.Message}", ex).DisplayAndLog();
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Renames a settlement and tracks the change for save persistence.
        /// Public access exposed via SettlementManager or SettlementExtensions
        /// </summary>
        /// <returns>BLGMResult indicating success or failure with a message</returns>
        internal BLGMResult RenameSettlement(Settlement settlement, string newName)
        {
            if (settlement == null)
                return new BLGMResult(false, "Cannot rename settlement: settlement is null");

            if (string.IsNullOrWhiteSpace(newName))
                return new BLGMResult(false, "Cannot rename settlement: new name is empty");

            try
            {
                // Find existing entry
                int index = _settlementIds.IndexOf(settlement.StringId);
                
                if (index >= 0)
                {
                    // Update existing custom name
                    _customNames[index] = newName;
                }
                else
                {
                    // Add new - store original name before renaming
                    _settlementIds.Add(settlement.StringId);
                    _originalNames.Add(settlement.Name.ToString());
                    _customNames.Add(newName);
                }

                // Apply the change (reflection + visual update)
                return ApplyNameOnly(settlement, newName);
            }
            catch (Exception ex)
            {
                return new BLGMResult(false, $"renaming settlement: {ex.Message}", ex).DisplayAndLog();
            }
        }

        /// <summary>
        /// Resets a settlement to its original name.
        /// Public access exposed via SettlementManager or SettlementExtensions
        /// </summary>
        /// <param name="settlement">The settlement to reset</param>
        /// <returns>BLGMResult indicating success or failure with a message</returns>
        internal BLGMResult ResetSettlementName(Settlement settlement)
        {
            if (settlement == null)
                return new BLGMResult(false, "Cannot reset settlement: settlement is null");

            int index = _settlementIds.IndexOf(settlement.StringId);
            if (index < 0)
                return new BLGMResult(false, $"Settlement '{settlement.Name}' has not been renamed");

            try
            {
                // Get original name
                string originalName = _originalNames[index];

                // Apply original name (reflection + visual update)
                BLGMResult result = ApplyNameOnly(settlement, originalName);
                if (!result.wasSuccessful)
                    return result;

                // Remove from tracking lists
                _settlementIds.RemoveAt(index);
                _customNames.RemoveAt(index);
                _originalNames.RemoveAt(index);

                return new BLGMResult(true, $"Settlement name reset to '{originalName}'");
            }
            catch (Exception ex)
            {
                return new BLGMResult(false, $"Failed resetting settlement name: {ex.Message}", ex).DisplayAndLog();
            }
        }

        /// <summary>
        /// Resets all settlements to their original names.
        /// Public access exposed via SettlementManager
        /// </summary>
        /// <returns>BLGMResult with count of reset settlements</returns>
        internal BLGMResult ResetAllSettlementNames()
        {
            if (_settlementIds.Count == 0)
                return new BLGMResult(true, "No settlements have been renamed");

            int resetCount = 0;
            List<string> idsCopy = new(_settlementIds);

            foreach (string id in idsCopy)
            {
                Settlement settlement = Settlement.Find(id);
                if (settlement != null)
                {
                    BLGMResult result = ResetSettlementName(settlement);
                    if (result.wasSuccessful)
                        resetCount++;
                }
            }

            return new BLGMResult(true, $"Reset {resetCount} settlement(s) to original names");
        }

        /// <summary>
        /// Gets the original name of a settlement if it was renamed.
        /// Public access exposed via SettlementManager or SettlementExtensions
        /// </summary>
        /// <param name="settlement">The settlement to check</param>
        /// <returns>Original name if renamed, null otherwise</returns>
        internal string GetOriginalName(Settlement settlement)
        {
            if (settlement == null)
                return null;
                
            int index = _settlementIds.IndexOf(settlement.StringId);
            if (index < 0)
                return null;

            return _originalNames[index];
        }

        /// <summary>
        /// Checks if a settlement has been renamed.
        /// Public access exposed via SettlementManager or SettlementExtensions
        /// </summary>
        /// <param name="settlement">The settlement to check</param>
        /// <returns>True if the settlement has a custom name</returns>
        internal bool IsRenamed(Settlement settlement)
        {
            return settlement != null && _settlementIds.Contains(settlement.StringId);
        }

        /// <summary>
        /// Gets the number of settlements with custom names.
        /// Public access exposed via SettlementManager
        /// </summary>
        internal int GetRenamedSettlementCount()
        {
            return _settlementIds.Count;
        }

        #endregion
    }
}
