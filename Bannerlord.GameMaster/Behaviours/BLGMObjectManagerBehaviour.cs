using System;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Information;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.GameMaster.Behaviours
{
    /// <summary>
    /// Campaign behavior initalizes BLGMObjectManager and loads prexisting BLGM created objects into BLGMObjectManager on game start or save load <br/>
    /// BLGM created objects are actualy saved as regular objects by the game in the save, this simply just loads their reference for tracking
    /// </summary
    internal class BLGMObjectManagerBehaviour : CampaignBehaviorBase
    {
        bool isNewGame = false;
        bool initialized = false;

        #region Register Events
        public override void RegisterEvents()
        {
            // New Game
            CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, OnNewGameCreated);

            // Reset
            CampaignEvents.OnGameEarlyLoadedEvent.AddNonSerializedListener(this, OnGameEarlyLoaded);
            
            // Game is loading
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);          

            // Initialize for new games
            CampaignEvents.TickEvent.AddNonSerializedListener(this, OnTick);

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
        /// Allows BLGMObjectManager to Initialize if new game
        /// </summary>
        private void OnNewGameCreated(CampaignGameStarter starter)
        {
            isNewGame = true;
            initialized = false;
        }

        /// <summary>
        /// Allows BLGMObjectManager to Initialize if new game
        /// </summary>
        private void OnTick(float dt)
        {
            // Runs When campaign map is full loaded and ready one time
            if (!initialized)
            {
                if (Hero.MainHero?.PartyBelongedTo != null && Campaign.Current?.CurrentGame != null)
                {
                    if (isNewGame)
                        InitializeObjectManager();

                    SetGameCampaignReady();
                }
            }
        }

        // Prevent gm commands from being ran if a new game is loaded from another session
        private void OnGameEarlyLoaded(CampaignGameStarter starter)
        {
            initialized = false;
            BLGMObjectManager.Instance._campaignFullyLoaded = false; // New save is loading, reset to false
        }

        /// <summary>
        /// Initalize BLGMObjectManager and load existing objects
        /// </summary>
        private void OnGameLoaded(CampaignGameStarter starter)
        {
            InitializeObjectManager();
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

        void InitializeObjectManager()
        {
            BLGMObjectManager.Instance.Initialize();

            // Auto-start command logging if enabled (creates new log file each time a save is loaded)
            LoggingManager.TryAutoStart();
        }

        void SetGameCampaignReady()
        {
            BLGMObjectManager.Instance._campaignFullyLoaded = true;
            isNewGame = false;
            initialized = true;
        }

        #endregion
    }
}