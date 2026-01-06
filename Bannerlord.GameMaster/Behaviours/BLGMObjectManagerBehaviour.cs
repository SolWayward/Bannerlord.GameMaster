using System;
using TaleWorlds.CampaignSystem;

namespace Bannerlord.GameMaster.Behaviours
{
    /// <summary>
    /// Campaign behavior initalizes BLGMObjectManager and loads prexisting BLGM created objects into BLGMObjectManager on game start or save load <br/>
    /// BLGM created objects are actualy saved as regular objects by the game in the save, this simply just loads their reference for tracking
    /// </summary
    internal class BLGMObjectManagerBehaviour : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            // Register event to reapply names after load
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        }

        public override void SyncData(IDataStore dataStore)
        {
            // No data to sync - BLGMObjectManager is reinitialized via OnSessionLaunched
        }

        /// <summary>
        /// Called after session is loaded. Loads objects that was creating by BLGM into BLGMObjectManager when save is loaded.
        /// </summary>
        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            BLGMObjectManager.Instance.Initialize();
        }
    }
}
