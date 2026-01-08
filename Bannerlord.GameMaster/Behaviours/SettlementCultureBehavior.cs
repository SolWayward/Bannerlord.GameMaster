using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.SaveSystem;
using TaleWorlds.ObjectSystem;
using Bannerlord.GameMaster.Information;
using Bannerlord.GameMaster.Settlements;

namespace Bannerlord.GameMaster.Behaviours
{
    /// <summary>
    /// Campaign behavior that manages custom settlement cultures with save/load persistence.
    /// Stores both custom cultures and original cultures to allow reset functionality.
    /// Uses parallel lists for reliable serialization with TaleWorlds save system.
    /// </summary>
    internal class SettlementCultureBehavior : CampaignBehaviorBase
    {
        // Store as separate lists for reliable serialization
        private List<string> _settlementIds;
        private List<string> _customCultureIds;
        private List<string> _originalCultureIds;

        public SettlementCultureBehavior()
        {
            _settlementIds = new();
            _customCultureIds = new();
            _originalCultureIds = new();
        }

        public override void RegisterEvents()
        {
            // Register event to reapply cultures after load
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        }

        public override void SyncData(IDataStore dataStore)
        {
            // Sync as separate lists
            if (dataStore.IsSaving)
            {
                if (_settlementIds == null) _settlementIds = new();
                if (_customCultureIds == null) _customCultureIds = new();
                if (_originalCultureIds == null) _originalCultureIds = new();
            }

            dataStore.SyncData("SettlementCultureIds", ref _settlementIds);
            dataStore.SyncData("CustomCultureIds", ref _customCultureIds);
            dataStore.SyncData("OriginalCultureIds", ref _originalCultureIds);

            // Ensure lists are initialized after loading
            if (dataStore.IsLoading)
            {
                if (_settlementIds == null) _settlementIds = new();
                if (_customCultureIds == null) _customCultureIds = new();
                if (_originalCultureIds == null) _originalCultureIds = new();
            }
        }

        /// <summary>
        /// Called after session is loaded. Reapplies all custom settlement cultures.
        /// </summary>
        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            if (_settlementIds == null || _settlementIds.Count == 0)
                return;

            InfoMessage.Status($"[GameMaster] Reapplying {_settlementIds.Count} custom settlement cultures");

            for (int i = 0; i < _settlementIds.Count; i++)
            {
                Settlement settlement = Settlement.Find(_settlementIds[i]);
                if (settlement != null)
                {
                    CultureObject culture = MBObjectManager.Instance.GetObject<CultureObject>(_customCultureIds[i]);
                    if (culture != null)
                    {
                        ApplyCultureChange(settlement, culture, false, false);
                    }
                }
            }
        }

        /// <summary>
        /// Changes the culture of a settlement and tracks the change for save persistence.
        /// </summary>
        /// <param name="settlement">The settlement to change culture for</param>
        /// <param name="culture">The new culture to apply</param>
        /// <param name="updateNotables">If true, updates the culture of all notables in the settlement</param>
        /// <param name="includeBoundVillages">If true and settlement is a town, recursively updates bound villages</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool SetSettlementCulture(Settlement settlement, CultureObject culture, bool updateNotables, bool includeBoundVillages)
        {
            if (settlement == null || culture == null)
            {
                InfoMessage.Error("[GameMaster] SetSettlementCulture called with null parameters");
                return false;
            }

            try
            {
                // Find existing entry
                int index = _settlementIds.IndexOf(settlement.StringId);

                if (index >= 0)
                {
                    // Update existing
                    _customCultureIds[index] = culture.StringId;
                }
                else
                {
                    // Add new - store original culture before changing
                    _settlementIds.Add(settlement.StringId);
                    _customCultureIds.Add(culture.StringId);
                    _originalCultureIds.Add(settlement.Culture.StringId);
                }

                // Apply the change
                ApplyCultureChange(settlement, culture, updateNotables, includeBoundVillages);

                // Track bound villages if they were updated
                if (includeBoundVillages && (settlement.IsTown || settlement.IsCastle) && settlement.BoundVillages != null)
                {
                    foreach (Village village in settlement.BoundVillages)
                    {
                        if (village?.Settlement != null)
                        {
                            // Track each village individually in the behavior's lists
                            int villageIndex = _settlementIds.IndexOf(village.Settlement.StringId);
                            if (villageIndex >= 0)
                            {
                                // Update existing village entry
                                _customCultureIds[villageIndex] = culture.StringId;
                            }
                            else
                            {
                                // Add new village entry - store original culture before it was changed
                                _settlementIds.Add(village.Settlement.StringId);
                                _customCultureIds.Add(culture.StringId);
                                _originalCultureIds.Add(village.Settlement.Culture.StringId);
                            }
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
        /// Resets a settlement to its original culture.
        /// </summary>
        /// <param name="settlement">The settlement to reset</param>
        /// <returns>True if reset, false if settlement culture was not changed</returns>
        public bool ResetSettlementCulture(Settlement settlement)
        {
            if (settlement == null)
                return false;

            int index = _settlementIds.IndexOf(settlement.StringId);
            if (index < 0)
                return false; // Not changed

            try
            {
                // Get original culture
                string originalCultureId = _originalCultureIds[index];
                CultureObject originalCulture = MBObjectManager.Instance.GetObject<CultureObject>(originalCultureId);

                if (originalCulture == null)
                {
                    InfoMessage.Error($"[GameMaster] Could not find original culture '{originalCultureId}' for settlement '{settlement.Name}'");
                    return false;
                }

                // Apply original culture
                ApplyCultureChange(settlement, originalCulture, false, false);

                // Remove from lists
                _settlementIds.RemoveAt(index);
                _customCultureIds.RemoveAt(index);
                _originalCultureIds.RemoveAt(index);

                return true;
            }
            catch (Exception ex)
            {
                InfoMessage.Error($"[GameMaster] Failed to reset settlement culture: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Checks if a settlement has a custom culture.
        /// </summary>
        /// <param name="settlement">The settlement to check</param>
        /// <returns>True if the settlement has a custom culture</returns>
        public bool HasCustomCulture(Settlement settlement)
        {
            return settlement != null && _settlementIds.Contains(settlement.StringId);
        }

        /// <summary>
        /// Gets the original culture of a settlement if it was changed.
        /// </summary>
        /// <param name="settlement">The settlement to check</param>
        /// <returns>Original culture if changed, null otherwise</returns>
        public CultureObject GetOriginalCulture(Settlement settlement)
        {
            if (settlement == null)
                return null;

            int index = _settlementIds.IndexOf(settlement.StringId);
            if (index < 0)
                return null;

            string originalCultureId = _originalCultureIds[index];
            return MBObjectManager.Instance.GetObject<CultureObject>(originalCultureId);
        }

        /// <summary>
        /// Gets the number of settlements with custom cultures.
        /// </summary>
        /// <returns>The count of settlements with custom cultures</returns>
        public int GetCustomCultureCount()
        {
            return _settlementIds.Count;
        }

        /// <summary>
        /// Applies a culture change to a settlement using SettlementManager.
        /// </summary>
        /// <param name="settlement">The settlement to change</param>
        /// <param name="culture">The culture to apply</param>
        /// <param name="updateNotables">If true, updates notables' cultures</param>
        /// <param name="includeBoundVillages">If true, updates bound villages' cultures</param>
        private void ApplyCultureChange(Settlement settlement, CultureObject culture, bool updateNotables, bool includeBoundVillages)
        {
            SettlementManager.SetSettlementCulture(settlement, culture, updateNotables, includeBoundVillages);
        }
    }
}
