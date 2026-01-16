using System;
using System.Reflection;
using Bannerlord.GameMaster.Behaviours;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Information;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.GameMaster
{
    public class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            //CreateMainMenuButton(); // For Testing purposes (May use for new game options or configs in the future)
        }

        protected override void OnSubModuleUnloaded()
        {
            base.OnSubModuleUnloaded();
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);
            
            if (game.GameType is Campaign && gameStarterObject is CampaignGameStarter campaignStarter)
            {
                // Register settlement name behavior for save/load persistence
                campaignStarter.AddBehavior(new SettlementNameBehavior());
                
                // Register settlement culture behavior for save/load persistence
                campaignStarter.AddBehavior(new SettlementCultureBehavior());

                // Register village trade bound behavior for save/load persistence
                campaignStarter.AddBehavior(new VillageTradeBoundBehavior());

                // Register BLGMObjectManagerBehaviour for loading blgm created objects
                campaignStarter.AddBehavior(new BLGMObjectManagerBehaviour());
            }
        }

        /// <summary>
        /// Adds a "Game Master" button to main menu of game.
        /// </summary>
        void CreateMainMenuButton()
        {
            TextObject buttonTextObj = new("{=BtnText_BLGameMaster}Game Master");   // "{=LocalizationKey}Default Text"
            int orderIndex = 9990;                                                  // Positioning index for the button in the menu

            // Creates the button option and links it to its click handler and condition evaluator
            InitialStateOption showMessageOption = new(
                "BLGameMasterMenuOption",
                buttonTextObj,
                orderIndex,
                HandleGameMasterMenuOptionClicked,
                EvaluateShowMessageOptionCondition
            );

            // Adds the button option to the main menu
            TaleWorlds.MountAndBlade.Module.CurrentModule.AddInitialStateOption(showMessageOption);
        }

        /// <summary>
        /// Runs when the "Game Master" button is clicked in main menu.
        /// </summary>
        private void HandleGameMasterMenuOptionClicked()
        {
            TextObject versionText = new("{=InfoMsg_BLGameMaster_Version}BL Game Master v{BLGAMEMASTER_VERSION}");
            versionText.SetTextVariable("BLGAMEMASTER_VERSION", GameEnvironment.BLGMVersion);

            InformationMessage infoMessage = new(versionText.ToString());
            InformationManager.DisplayMessage(infoMessage);
        }

        /// <summary>
        /// Evaluates whether the message option button should be disabled and provides the reason for its state.
        /// </summary>
        /// <returns>A tuple containing a Boolean value indicating whether the button is disabled, and a <see cref="TextObject"/>
        /// describing the reason. If the button is enabled, the reason will be empty.</returns>
        private (bool isDisabled, TextObject reasonTextObj) EvaluateShowMessageOptionCondition()
        {
            return (false, TextObject.GetEmpty());
        }    
    }
}