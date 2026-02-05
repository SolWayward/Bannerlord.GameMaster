using System;
using Bannerlord.GameMaster.Common;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.GauntletUI.BodyGenerator;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ScreenSystem;

namespace Bannerlord.GameMaster.Heroes.FaceGenerator
{
    /// <summary>
    /// Screen registered for HeroFaceGeneratorState, uses BodyGeneratorView with target hero.
    /// Based on native GauntletBarberScreen pattern but works with any hero.
    /// </summary>
    [GameStateScreen(typeof(HeroFaceGeneratorState))]
    public class HeroFaceGeneratorScreen : ScreenBase, IGameStateListener, IFaceGeneratorScreen
    {
        private readonly HeroFaceGeneratorState _state;
        private readonly BodyGeneratorView _facegenLayer;

        public IFaceGeneratorHandler Handler => _facegenLayer;

        public HeroFaceGeneratorScreen(HeroFaceGeneratorState state)
        {
            _state = state;
            LoadingWindow.EnableGlobalLoadingWindow();

            // KEY DIFFERENCE: Use state.GetCharacter() instead of Hero.MainHero
            _facegenLayer = new BodyGeneratorView(
                new ControlCharacterCreationStage(OnDone),
                GameTexts.FindText("str_done", null),
                new ControlCharacterCreationStage(OnCancel),
                GameTexts.FindText("str_cancel", null),
                _state.GetCharacter(),
                false,
                _state.GetFilter(),
                null, null, null, null, null, null);
        }

        private void OnDone()
        {
            // BodyGeneratorView's SaveCurrentCharacter calls UpdatePlayerCharacterBodyProperties
            // which only works for MainHero. For other heroes, manually apply the changes.
            Hero hero = _state.GetHero();
            if (hero != null)
            {
                // BodyGen is a public property on BodyGeneratorView - no reflection needed
                BodyGenerator bodyGenerator = _facegenLayer.BodyGen;
                if (bodyGenerator != null)
                {
                    BodyProperties newProps = bodyGenerator.CurrentBodyProperties;
                    hero.StaticBodyProperties = newProps.StaticProperties;
                    hero.Weight = newProps.Weight;
                    hero.Build = newProps.Build;
                }
                else
                {
                    BLGMResult.Error("BodyGenerator is null - cannot save hero appearance.").Log();
                }
            }

            Game.Current.GameStateManager.PopState(0);
        }


        private void OnCancel()
        {
            // BodyGeneratorView handles reset on cancel internally
            Game.Current.GameStateManager.PopState(0);
        }

        // MARK: Lifecycle Methods

        protected override void OnInitialize()
        {
            base.OnInitialize();
            Game.Current.GameStateManager.RegisterActiveStateDisableRequest(this);
            AddLayer(_facegenLayer.GauntletLayer);
            InformationManager.HideAllMessages();
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            AddLayer(_facegenLayer.SceneLayer);
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
            _facegenLayer.SceneLayer.SceneView.SetEnable(false);
            _facegenLayer.OnFinalize();
            LoadingWindow.EnableGlobalLoadingWindow();
            MBInformationManager.HideInformations();
        }

        protected override void OnFinalize()
        {
            base.OnFinalize();
            if (LoadingWindow.IsLoadingWindowActive)
            {
                LoadingWindow.DisableGlobalLoadingWindow();
            }
            Game.Current.GameStateManager.UnregisterActiveStateDisableRequest(this);
        }

        protected override void OnFrameTick(float dt)
        {
            base.OnFrameTick(dt);
            _facegenLayer.OnTick(dt);
        }

        // MARK: IGameStateListener Implementation

        void IGameStateListener.OnActivate() { }
        void IGameStateListener.OnDeactivate() { }
        void IGameStateListener.OnInitialize() { }
        void IGameStateListener.OnFinalize() { }
    }
}
