using System;
using System.IO;
using System.Reflection.Metadata;
using Bannerlord.GameMaster.Console.Testing;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.GameMaster
{
    public class SubModule : MBSubModuleBase
    {
        public static string ModuleVersion => "1.3.9.1";

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            CreateMainMenuButton();
        }

        protected override void OnSubModuleUnloaded()
        {
            base.OnSubModuleUnloaded();
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();
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
            Module.CurrentModule.AddInitialStateOption(showMessageOption);      
        }

        /// <summary>
        /// Runs when the "Game Master" button is clicked in main menu.
        /// </summary>
        private void HandleGameMasterMenuOptionClicked()
        {
            TextObject versionText = new("{=InfoMsg_BLGameMaster_Version}BL Game Master v{BLGAMEMASTER_VERSION}");
            versionText.SetTextVariable("BLGAMEMASTER_VERSION", ModuleVersion);

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
        
        /// <summary>
        /// Run automated tests on startup
        /// </summary>
        private void RunAutomatedTests()
        {
            try
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    "[Game Master] Auto-running tests...",
                    new TaleWorlds.Library.Color(0.2f, 0.8f, 1.0f)));
                
                // Register standard tests if none are registered
                if (TestRunner.GetRegisteredTests().Count == 0)
                {
                    StandardTests.RegisterAll();
                }
                
                // Run all tests
                var results = TestRunner.RunAllTests();
                
                // Display summary
                int passed = 0;
                int failed = 0;
                foreach (var result in results)
                {
                    if (result.Passed) passed++;
                    else failed++;
                }
                
                var summaryColor = failed == 0
                    ? new TaleWorlds.Library.Color(0.2f, 1.0f, 0.2f)  // Green for all passed
                    : new TaleWorlds.Library.Color(1.0f, 0.5f, 0.2f); // Orange for failures
                
                InformationManager.DisplayMessage(new InformationMessage(
                    $"[Game Master] Tests Complete: {passed} passed, {failed} failed",
                    summaryColor));
                
                // Log full report to console/log
                string report = TestRunner.GenerateReport(results);
                InformationManager.DisplayMessage(new InformationMessage(
                    "[Game Master] Use 'gm.test.last_results verbose' to see full test report"));
            }
            catch (Exception ex)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    $"[Game Master] Test auto-run failed: {ex.Message}",
                    new TaleWorlds.Library.Color(1.0f, 0.2f, 0.2f)));
            }
        }
    }
}