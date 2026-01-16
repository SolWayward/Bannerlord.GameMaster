using System;
using System.Linq;
using Bannerlord.GameMaster.Console.Common.Execution;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Behaviours
{
    /// <summary>
    /// Campaign behavior initalizes BLGMObjectManager and loads prexisting BLGM created objects into BLGMObjectManager on game start or save load <br/>
    /// BLGM created objects are actualy saved as regular objects by the game in the save, this simply just loads their reference for tracking
    /// </summary
    internal class BLGMObjectManagerBehaviour : CampaignBehaviorBase
    {
        #region Register Events
        public override void RegisterEvents()
        {
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);

            // Register event to reapply names after load
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);

            // For Heroes
            CampaignEvents.HeroKilledEvent.AddNonSerializedListener(
                this,
                new Action<Hero, Hero, KillCharacterAction.KillCharacterActionDetail, bool>(OnHeroKilled)
            );

            // For Clans
            CampaignEvents.OnClanDestroyedEvent.AddNonSerializedListener(
                this,
                new Action<Clan>(OnClanDestroyed)
            );

            // For Kingdoms
            CampaignEvents.KingdomDestroyedEvent.AddNonSerializedListener(
                this,
                new Action<Kingdom>(OnKingdomDestroyed)
            );

            // Optional: Catch heroes being removed from game system
            CampaignEvents.OnHeroUnregisteredEvent.AddNonSerializedListener(
                this,
                new Action<Hero>(OnHeroUnregistered)
            );
        }

        #endregion

        #region CallBacks


        public override void SyncData(IDataStore dataStore)
        {
            // Not syncing data
        }

        /// <summary>
        /// Initalize BLGMObjectManager and load existing objects
        /// </summary>
        private void OnGameLoaded(CampaignGameStarter starter)
        {
            BLGMObjectManager.Instance.Initialize();

            // Auto-start command logging if enabled (creates new log file each time a save is loaded)
            LoggingManager.TryAutoStart();
        }

        /// <summary>
        /// Called after session is fully launched.
        /// BLGM initialization has been moved to OnGameLoaded to prevent ButterLib crashes.
        /// </summary>
        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            // Moved BLGMObjectManager.Instance.Initialize() to OnGameLoaded to fix ButterLib crash
        }

        private void OnHeroKilled(Hero victim, Hero killer, KillCharacterAction.KillCharacterActionDetail detail, bool showNotification)
        {
            if (victim?.StringId != null && victim.StringId.StartsWith("blgm_"))
            {
                BLGMObjectManager.UnregisterHero(victim.StringId);
            }
        }

        private void OnClanDestroyed(Clan clan)
        {
            if (clan?.StringId != null && clan.StringId.StartsWith("blgm_"))
            {
                BLGMObjectManager.UnregisterClan(clan.StringId);
            }
        }

        private void OnKingdomDestroyed(Kingdom kingdom)
        {
            if (kingdom?.StringId != null && kingdom.StringId.StartsWith("blgm_"))
            {
                BLGMObjectManager.UnregisterKingdom(kingdom.StringId);
            }
        }

        private void OnHeroUnregistered(Hero hero)
        {
            // This catches heroes being removed from game system entirely
            if (hero?.StringId != null && hero.StringId.StartsWith("blgm_"))
            {
                BLGMObjectManager.UnregisterHero(hero.StringId);
            }
        }

        #endregion
    }
}