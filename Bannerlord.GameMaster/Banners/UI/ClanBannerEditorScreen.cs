using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace Bannerlord.GameMaster.Banners.UI
{
    [GameStateScreen(typeof(ClanBannerEditorState))]
    public class ClanBannerEditorScreen : ScreenBase, IGameStateListener
    {
        private readonly ClanBannerEditorState _state;
        private GauntletLayer _gauntletLayer;
        private GauntletMovieIdentifier _gauntletMovie;  // Correct type
        private BannerEditorVM _dataSource;

        public ClanBannerEditorScreen(ClanBannerEditorState state)
        {
            _state = state;
        }

        private SpriteCategory _bannerIconsCategory;

        protected override void OnInitialize()
        {
            base.OnInitialize();

            // Load the banner icons sprite category
            _bannerIconsCategory = UIResourceManager.LoadSpriteCategory("ui_bannericons");

            Clan targetClan = _state.GetClan();
            CharacterObject character = _state.GetCharacter();
            Banner banner = targetClan.Banner;

            // Create the ViewModel
            _dataSource = new BannerEditorVM(
                character,
                banner,
                OnExit,           // Action<bool> onExit
                RefreshBanner,    // Action refresh
                0,                // currentStageIndex
                1,                // totalStagesCount
                0,                // furthestIndex
                _ => { }          // Action<int> goToIndex
            );

            // Correct GauntletLayer constructor: (string name, int localOrder, bool shouldClear)
            _gauntletLayer = new GauntletLayer("ClanBannerEditor", 1, true);
            _gauntletLayer.Input.RegisterHotKeyCategory(
                HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));

            // LoadMovie returns GauntletMovieIdentifier
            _gauntletMovie = _gauntletLayer.LoadMovie("BannerEditor", _dataSource);

            // SetInputRestrictions: 7 is the numeric value for InputUsageMask.All
            _gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            _gauntletLayer.IsFocusLayer = true;

            AddLayer(_gauntletLayer);
            ScreenManager.TrySetFocus(_gauntletLayer);
        }

        private void OnExit(bool cancelled)
        {
            if (!cancelled)
            {
                Clan targetClan = _state.GetClan();
                // Access BannerVisual property to trigger refresh
                IBannerVisual _ = targetClan.Banner.BannerVisual;
            }

            Game.Current.GameStateManager.PopState(0);
        }

        private void RefreshBanner()
        {
            _dataSource.BannerVM.OnPropertyChanged();
        }

        protected override void OnFrameTick(float dt)
        {
            base.OnFrameTick(dt);

            if (_gauntletLayer.Input.IsHotKeyReleased("Exit"))
            {
                _dataSource?.ExecuteCancel();
            }
        }

        protected override void OnFinalize()
        {
            // Unload sprites
            _bannerIconsCategory?.Unload();

            if (_gauntletMovie != null)
                _gauntletLayer?.ReleaseMovie(_gauntletMovie);  // Both are GauntletMovieIdentifier
            if (_gauntletLayer != null)
                RemoveLayer(_gauntletLayer);
            _dataSource?.OnFinalize();
            _dataSource = null;
            _gauntletLayer = null;
            _gauntletMovie = null;
            base.OnFinalize();
        }

        void IGameStateListener.OnActivate() { }
        void IGameStateListener.OnDeactivate() { }
        void IGameStateListener.OnInitialize() { }
        void IGameStateListener.OnFinalize() { }
    }
}
