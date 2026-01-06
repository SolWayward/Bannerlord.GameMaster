using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.SaveSystem;
using TaleWorlds.Localization;
using TaleWorlds.Library;
using Bannerlord.GameMaster.Information;

namespace Bannerlord.GameMaster.Behaviours
{
    /// <summary>
    /// Campaign behavior that manages custom settlement names with save/load persistence.
    /// Stores both custom names and original names to allow reset functionality.
    /// Uses parallel lists for reliable serialization with TaleWorlds save system.
    /// </summary>
    internal class SettlementNameBehavior : CampaignBehaviorBase
    {
        // Store as separate lists for reliable serialization
        private List<string> _settlementIds;
        private List<string> _customNames;
        private List<string> _originalNames;
        private FieldInfo _nameField;

        public SettlementNameBehavior()
        {
            _settlementIds = new List<string>();
            _customNames = new List<string>();
            _originalNames = new List<string>();
            
            // Cache the reflection field for performance
            _nameField = typeof(Settlement).GetField("_name", BindingFlags.NonPublic | BindingFlags.Instance);
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
                if (_settlementIds == null) _settlementIds = new List<string>();
                if (_customNames == null) _customNames = new List<string>();
                if (_originalNames == null) _originalNames = new List<string>();
            }
            
            dataStore.SyncData("CustomNameSettlementIds", ref _settlementIds);
            dataStore.SyncData("CustomNameValues", ref _customNames);
            dataStore.SyncData("OriginalNameValues", ref _originalNames);
            
            // Ensure lists are initialized after loading
            if (dataStore.IsLoading)
            {
                if (_settlementIds == null) _settlementIds = new List<string>();
                if (_customNames == null) _customNames = new List<string>();
                if (_originalNames == null) _originalNames = new List<string>();                 
            }
        }

        /// <summary>
        /// Called after session is loaded. Reapplies all custom settlement names.
        /// </summary>
        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            if (_settlementIds == null || _settlementIds.Count == 0)
                return;

            InfoMessage.Status($"[GameMaster] Reapplying {_settlementIds.Count} custom settlement names");

            for (int i = 0; i < _settlementIds.Count; i++)
            {
                var settlement = Settlement.Find(_settlementIds[i]);
                if (settlement != null)
                {
                    ApplyNameChange(settlement, _customNames[i]);
                }
            }
        }

        /// <summary>
        /// Renames a settlement and tracks the change for save persistence.
        /// </summary>
        /// <param name="settlement">The settlement to rename</param>
        /// <param name="newName">The new name for the settlement</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool RenameSettlement(Settlement settlement, string newName)
        {
            if (settlement == null || string.IsNullOrWhiteSpace(newName))
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    "[GameMaster] RenameSettlement called with null/empty parameters",
                    Colors.Red));
                return false;
            }

            try
            {
                // Find existing entry
                int index = _settlementIds.IndexOf(settlement.StringId);
                
                if (index >= 0)
                {
                    // Update existing
                    _customNames[index] = newName;
                }
                
                else
                {
                    // Add new
                    _settlementIds.Add(settlement.StringId);
                    _customNames.Add(newName);
                    _originalNames.Add(settlement.Name.ToString());
                }

                // Apply the change
                ApplyNameChange(settlement, newName);

                return true;
            }
            catch (Exception ex)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    $"[GameMaster] Failed to rename settlement: {ex.Message}",
                    Colors.Red));
                
                return false;
            }
        }

        /// <summary>
        /// Resets a settlement to its original name.
        /// </summary>
        /// <param name="settlement">The settlement to reset</param>
        /// <returns>True if reset, false if settlement was not renamed</returns>
        public bool ResetSettlementName(Settlement settlement)
        {
            if (settlement == null)
                return false;

            int index = _settlementIds.IndexOf(settlement.StringId);
            if (index < 0)
                return false; // Not renamed

            try
            {
                // Get original name
                string originalName = _originalNames[index];

                // Apply original name
                ApplyNameChange(settlement, originalName);

                // Remove from lists
                _settlementIds.RemoveAt(index);
                _customNames.RemoveAt(index);
                _originalNames.RemoveAt(index);

                return true;
            }
            catch (Exception ex)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    $"[GameMaster] Failed to reset settlement name: {ex.Message}",
                    Colors.Red));
                return false;
            }
        }

        /// <summary>
        /// Resets all settlements to their original names.
        /// </summary>
        /// <returns>Number of settlements reset</returns>
        public int ResetAllSettlementNames()
        {
            if (_settlementIds.Count == 0)
                return 0;

            int resetCount = 0;
            var idsCopy = new List<string>(_settlementIds);

            foreach (var id in idsCopy)
            {
                var settlement = Settlement.Find(id);
                if (settlement != null && ResetSettlementName(settlement))
                {
                    resetCount++;
                }
            }

            return resetCount;
        }

        /// <summary>
        /// Gets the original name of a settlement if it was renamed.
        /// </summary>
        /// <param name="settlement">The settlement to check</param>
        /// <returns>Original name if renamed, null otherwise</returns>
        public string GetOriginalName(Settlement settlement)
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
        /// </summary>
        /// <param name="settlement">The settlement to check</param>
        /// <returns>True if the settlement has a custom name</returns>
        public bool IsRenamed(Settlement settlement)
        {
            return settlement != null && _settlementIds.Contains(settlement.StringId);
        }

        /// <summary>
        /// Gets the number of settlements with custom names.
        /// </summary>
        public int GetRenamedSettlementCount()
        {
            return _settlementIds.Count;
        }

        /// <summary>
        /// Applies a name change to a settlement using reflection.
        /// </summary>
        private void ApplyNameChange(Settlement settlement, string newName)
        {
            if (_nameField == null)
            {
                throw new InvalidOperationException("Unable to access Settlement._name field via reflection.");
            }

            _nameField.SetValue(settlement, new TextObject(newName));
        }
    }
}
