using System;
using Bannerlord.GameMaster.Common;
using Bannerlord.GameMaster.Heroes.FaceGenerator;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace Bannerlord.GameMaster.Heroes
{
    /// <summary>
    /// Sub-editor for opening the native face generator UI.
    /// This is the normal hero appearance editor, but can be used with any hero
    /// Accessed via HeroEditor.HeroAppearanceEditorUI.Open()
    /// </summary>
    public class HeroAppearanceEditorUI
    {
        private readonly Hero _hero;

        public HeroAppearanceEditorUI(Hero hero)
        {
            _hero = hero;
        }

        /// MARK: Open
        /// <summary>
        /// Opens the native face generator UI for the hero.
        /// </summary>
        /// <param name="filter">Optional filter to restrict available options</param>
        /// <param name="onComplete">Optional callback when editor closes</param>
        /// <returns>Result indicating success/failure</returns>
        public BLGMResult Open(IFaceGeneratorCustomFilter filter = null, Action onComplete = null)
        {
            if (_hero == null)
                return BLGMResult.Error("Cannot open face generator: Hero is null");

            if (!Campaign.Current.IsFaceGenEnabled)
                return BLGMResult.Error("Face generator is disabled in this campaign");

            // Create state with parameters - the screen is created during CreateState()
            // so parameters must be passed via constructor, not a separate Initialize() call
            HeroFaceGeneratorState state = Game.Current.GameStateManager
                .CreateState<HeroFaceGeneratorState>(_hero, filter, onComplete);

            Game.Current.GameStateManager.PushState(state, 0);
            return BLGMResult.Success($"Opened face generator for {_hero.Name}");
        }

        /// MARK: OpenWithDefaultFilter
        /// <summary>
        /// Opens the face generator with the default barber filter (hair/beard only).
        /// </summary>
        /// <param name="onComplete">Optional callback when editor closes</param>
        /// <returns>Result indicating success/failure</returns>
        public BLGMResult OpenWithDefaultFilter(Action onComplete = null)
        {
            IFaceGeneratorCustomFilter filter = CharacterHelper.GetFaceGeneratorFilter();
            return Open(filter, onComplete);
        }

        /// MARK: OpenFullEditor
        /// <summary>
        /// Opens the face generator with all options available (no filter).
        /// </summary>
        /// <param name="onComplete">Optional callback when editor closes</param>
        /// <returns>Result indicating success/failure</returns>
        public BLGMResult OpenFullEditor(Action onComplete = null)
        {
            return Open(null, onComplete);
        }
    }
}
