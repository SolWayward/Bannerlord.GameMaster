using System;
using Bannerlord.GameMaster.Clans;
using Bannerlord.GameMaster.Kingdoms;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.MountAndBlade.View.Tableaus;
using TaleWorlds.MountAndBlade.View.Tableaus.Thumbnails;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace Bannerlord.GameMaster.Banners.UI
{
    [GameStateScreen(typeof(ClanBannerEditorState))]
    public class ClanBannerEditorScreen : ScreenBase, IGameStateListener
    {
        private readonly ClanBannerEditorState _state;
        private GauntletLayer _gauntletLayer;
        private GauntletMovieIdentifier _gauntletMovie;
        private BannerEditorVM _dataSource;
        private SpriteCategory _bannerIconsCategory;

        // MARK: 3D Scene Fields
        private SceneLayer _sceneLayer;
        private Scene _scene;
        private Camera _camera;
        private MBAgentRendererSceneController _agentRendererSceneController;
        private AgentVisuals[] _agentVisuals;
        private int _agentVisualToShowIndex;
        private bool _checkWhetherAgentVisualIsReady;
        private bool _firstCharacterRender = true;
        private bool _refreshBannersNextFrame;
        private bool _refreshCharacterAndShieldNextFrame;
        private MatrixFrame _characterFrame;
        private MissionWeapon _shieldWeapon;
        private Equipment _weaponEquipment;
        private Banner _currentBanner;
        private BannerEditorTextureCreationData _latestBannerTextureCreationData;
        private BannerEditorTextureCreationData _latestShieldTextureCreationData;

        // MARK: Camera Control
        private float _cameraCurrentRotation;
        private float _cameraTargetRotation;
        private float _cameraCurrentDistanceAdder;
        private float _cameraTargetDistanceAdder;
        private float _cameraCurrentElevationAdder;
        private float _cameraTargetElevationAdder;

        public ClanBannerEditorScreen(ClanBannerEditorState state)
        {
            LoadingWindow.EnableGlobalLoadingWindow();
            _state = state;
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            Game.Current.GameStateManager.RegisterActiveStateDisableRequest(this);

            // Flush stale banner texture cache from previous editor sessions (matches native BannerEditorView pattern)
            // Prevents black banner rendering when reopening editor on a previously edited clan
            BannerEditorTextureCache.Current?.FlushCache();

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

            // Create 3D scene
            CreateScene();

            // Create GauntletLayer - movie will be loaded later when scene is ready
            _gauntletLayer = new GauntletLayer("ClanBannerEditor", 1, false);
            _gauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
            _gauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("FaceGenHotkeyCategory"));
            _gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            _gauntletLayer.IsFocusLayer = true;
            ScreenManager.TrySetFocus(_gauntletLayer);
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            // Add GauntletLayer first, then SceneLayer (matches native order)
            AddLayer(_gauntletLayer);
            AddLayer(_sceneLayer);
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
            // Clean up scene resources on deactivate (matches native pattern)
            if (_agentVisuals != null)
            {
                _agentVisuals[0]?.Reset();
                _agentVisuals[1]?.Reset();
            }

            if (_scene != null && _agentRendererSceneController != null)
            {
                MBAgentRendererSceneController.DestructAgentRendererSceneController(_scene, _agentRendererSceneController, false);
                _agentRendererSceneController = null;
            }

            _scene?.ClearAll();
            _scene?.ManualInvalidate();
            _scene = null;
        }

        private void OnExit(bool cancelled)
        {
            if (!cancelled)
            {
                Clan targetClan = _state.GetClan();
                Banner banner = targetClan.Banner;

                // Update clan colors from the edited banner (matches native GauntletBannerEditorScreen.OnDone)
                // These are used by the engine as ClothColor1/ClothColor2 for troop and hero equipment rendering
                uint primaryColor = banner.GetPrimaryColor();
                uint firstIconColor = banner.GetFirstIconColor();
                targetClan.Color = primaryColor;
                targetClan.Color2 = firstIconColor;

                // Persist banner colors to clan saveable fields (BannerBackgroundColorPrimary/Secondary, BannerIconColor)
                // These are used by native UpdateBannerColorsAccordingToKingdom() to reconstruct colors on state transitions
                targetClan.UpdateBannerColor(primaryColor, firstIconColor);

                // Trigger banner visual refresh
                IBannerVisual _ = banner.BannerVisual;

                // If this is the ruling clan, propagate banner colors to kingdom and vassal clans
                if (targetClan.IsRulingClan())
                {
                    targetClan.Kingdom.PropagateRulingClanBanner();
                }

                // Mark all clan party visuals as dirty so map icons and troop visuals refresh
                SetClanPartyVisualsAsDirty(targetClan);
            }

            Game.Current.GameStateManager.PopState(0);
        }

        /// <summary>
        /// Marks all party visuals for the target clan as dirty so map icons refresh with new banner colors.
        /// Follows the native SetMapIconAsDirtyForAllPlayerClanParties pattern but works for any clan.
        /// </summary>
        private static void SetClanPartyVisualsAsDirty(Clan clan)
        {
            // War parties (lord parties, patrols, etc.)
            foreach (WarPartyComponent warPartyComponent in clan.WarPartyComponents)
            {
                warPartyComponent.MobileParty.Party?.SetVisualAsDirty();
            }

            // Caravans owned by clan heroes
            foreach (Hero hero in clan.Heroes)
            {
                foreach (CaravanPartyComponent caravanComponent in hero.OwnedCaravans)
                {
                    caravanComponent.MobileParty.Party?.SetVisualAsDirty();
                }
            }

            // Settlement parties (garrisons, villager parties)
            foreach (Settlement settlement in clan.Settlements)
            {
                if (settlement.IsVillage && settlement.Village.VillagerPartyComponent != null)
                {
                    settlement.Village.VillagerPartyComponent.MobileParty.Party?.SetVisualAsDirty();
                }
                else if ((settlement.IsCastle || settlement.IsTown) && settlement.Town.GarrisonParty != null)
                {
                    settlement.Town.GarrisonParty.Party?.SetVisualAsDirty();
                }
            }
        }

        private void RefreshBanner()
        {
            _dataSource.BannerVM.OnPropertyChanged();
            RefreshShieldAndCharacter();
        }

        protected override void OnFrameTick(float dt)
        {
            base.OnFrameTick(dt);

            // Update camera
            UpdateCamera(dt);

            // Load movie only after scene is ready to render (matches native pattern)
            if (_sceneLayer != null && _sceneLayer.ReadyToRender())
            {
                LoadingWindow.DisableGlobalLoadingWindow();
                if (_gauntletMovie == null)
                {
                    _gauntletMovie = _gauntletLayer.LoadMovie("BannerEditor", _dataSource);
                }
            }

            // Tick scene
            _scene?.Tick(dt);

            // Handle banner refresh
            if (_refreshBannersNextFrame)
            {
                UpdateBanners();
                _refreshBannersNextFrame = false;
            }

            if (_refreshCharacterAndShieldNextFrame)
            {
                RefreshCharacterAux();
                _refreshCharacterAndShieldNextFrame = false;
            }

            // Handle character visual ready check
            if (_checkWhetherAgentVisualIsReady)
            {
                int otherVisual = (_agentVisualToShowIndex + 1) % 2;
                if (_agentVisuals[_agentVisualToShowIndex].GetEntity().CheckResources(_firstCharacterRender, true))
                {
                    _agentVisuals[otherVisual].SetVisible(false);
                    _agentVisuals[_agentVisualToShowIndex].SetVisible(true);
                    _checkWhetherAgentVisualIsReady = false;
                    _firstCharacterRender = false;
                }
                else if (!_firstCharacterRender)
                {
                    // Keep showing the previous visual while loading
                    _agentVisuals[otherVisual].SetVisible(true);
                    _agentVisuals[_agentVisualToShowIndex].SetVisible(false);
                }
            }

            // Escape key handling
            if (_gauntletLayer.Input.IsHotKeyReleased("Exit"))
            {
                _dataSource?.ExecuteCancel();
            }
        }

        protected override void OnFinalize()
        {
            base.OnFinalize();

            // Flush banner texture cache on finalize (matches native BannerEditorView.OnFinalize pattern)
            BannerEditorTextureCache.Current?.FlushCache();

            // Disable loading window if still active
            if (LoadingWindow.IsLoadingWindowActive)
            {
                LoadingWindow.DisableGlobalLoadingWindow();
            }

            // Unregister state disable request
            Game.Current.GameStateManager.UnregisterActiveStateDisableRequest(this);

            // Clean up texture creation data
            if (_latestBannerTextureCreationData != null)
            {
                ThumbnailCacheManager.Current.DestroyTexture(_latestBannerTextureCreationData);
                _latestBannerTextureCreationData = null;
            }

            if (_latestShieldTextureCreationData != null)
            {
                ThumbnailCacheManager.Current.DestroyTexture(_latestShieldTextureCreationData);
                _latestShieldTextureCreationData = null;
            }

            if (_sceneLayer != null)
                RemoveLayer(_sceneLayer);

            // Unload sprites
            _bannerIconsCategory?.Unload();

            if (_gauntletMovie != null)
                _gauntletLayer?.ReleaseMovie(_gauntletMovie);
            if (_gauntletLayer != null)
                RemoveLayer(_gauntletLayer);
            _dataSource?.OnFinalize();
            _dataSource = null;
            _gauntletLayer = null;
            _gauntletMovie = null;
        }

        // MARK: 3D Scene Methods

        private void CreateScene()
        {
            _scene = Scene.CreateNewScene(true, true, (DecalAtlasGroup)2, "mono_renderscene");
            _scene.SetName("MBBannerEditorScreen");

            SceneInitializationData sceneInitData = default;
            sceneInitData.InitPhysicsWorld = false;
            _scene.Read("banner_editor_scene", ref sceneInitData, "");
            _scene.SetShadow(true);
            _scene.DisableStaticShadows(true);
            _scene.SetDynamicShadowmapCascadesRadiusMultiplier(0.1f);

            _agentRendererSceneController = MBAgentRendererSceneController.CreateNewAgentRendererSceneController(_scene);

            float aspectRatio = Screen.AspectRatio;
            GameEntity spawnPoint = _scene.FindEntityWithTag("spawnpoint_player");
            _characterFrame = spawnPoint.GetFrame();
            _characterFrame.rotation.OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();

            // Camera defaults
            _cameraTargetDistanceAdder = 3.5f;
            _cameraCurrentDistanceAdder = _cameraTargetDistanceAdder;
            _cameraTargetElevationAdder = 1.15f;
            _cameraCurrentElevationAdder = _cameraTargetElevationAdder;
            _cameraTargetRotation = 0f;
            _cameraCurrentRotation = 0f;

            _camera = Camera.CreateCamera();
            _camera.SetFovVertical(0.6981317f, aspectRatio, 0.2f, 200f);

            _sceneLayer = new SceneLayer(true, true);
            _sceneLayer.IsFocusLayer = true;
            _sceneLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
            _sceneLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("FaceGenHotkeyCategory"));
            _sceneLayer.SetScene(_scene);
            UpdateCamera(0f);
            _sceneLayer.SetSceneUsesShadows(true);
            _sceneLayer.SceneView.SetResolutionScaling(true);

            int postfxConfig = -1;
            postfxConfig &= -5;
            _sceneLayer.SetPostfxConfigParams(postfxConfig);

            AddCharacterEntity(ActionIndexCache.act_walk_idle_1h_with_shield_left_stance);
        }

        private void AddCharacterEntity(ActionIndexCache action)
        {
            CharacterObject character = _state.GetCharacter();
            Banner banner = _state.GetClan().Banner;
            _currentBanner = banner;

            _weaponEquipment = new Equipment();
            for (int i = 0; i < 12; i++)
            {
                EquipmentElement element = character.Equipment.GetEquipmentFromSlot((EquipmentIndex)i);
                if (element.Item?.PrimaryWeapon == null ||
                    (!element.Item.PrimaryWeapon.IsShield && !element.Item.ItemFlags.HasAllFlags(ItemFlags.DropOnWeaponChange)))
                {
                    _weaponEquipment.AddEquipmentToSlotWithoutAgent((EquipmentIndex)i, element);
                }
            }

            // Add shield from BannerEditorVM
            ItemRosterElement shieldElement = _dataSource.ShieldRosterElement;
            int shieldSlot = _dataSource.ShieldSlotIndex;
            _weaponEquipment.AddEquipmentToSlotWithoutAgent((EquipmentIndex)shieldSlot, shieldElement.EquipmentElement);
            _shieldWeapon = new MissionWeapon(shieldElement.EquipmentElement.Item, shieldElement.EquipmentElement.ItemModifier, banner);

            Monster monster = TaleWorlds.Core.FaceGen.GetBaseMonsterFromRace(character.Race);

            _agentVisuals = new AgentVisuals[2];
            for (int i = 0; i < 2; i++)
            {
                _agentVisuals[i] = AgentVisuals.Create(
                    new AgentVisualsData()
                        .Equipment(_weaponEquipment)
                        .BodyProperties(character.GetBodyProperties(_weaponEquipment, -1))
                        .Frame(_characterFrame)
                        .ActionSet(MBGlobals.GetActionSetWithSuffix(monster, character.IsFemale, "_facegen"))
                        .ActionCode(action)
                        .Scene(_scene)
                        .Monster(monster)
                        .Race(character.Race)
                        .SkeletonType(character.IsFemale ? SkeletonType.Female : SkeletonType.Male)
                        .PrepareImmediately(true)
                        .UseMorphAnims(true)
                        .RightWieldedItemIndex(-1)
                        .LeftWieldedItemIndex(shieldSlot)
                        .Banner(banner)
                        .ClothColor1(banner.GetPrimaryColor())
                        .ClothColor2(banner.GetFirstIconColor()),
                    "BannerEditorChar", false, false, true);

                _agentVisuals[i].SetAgentLodZeroOrMaxExternal(true);
                _agentVisuals[i].SetVisible(false);
                _agentVisuals[i].GetEntity().CheckResources(true, true);
            }

            _checkWhetherAgentVisualIsReady = true;
            _firstCharacterRender = true;
            UpdateBanners();
        }

        private void UpdateCamera(float dt)
        {
            float amount = MBMath.ClampFloat(10f * dt, 0f, 1f);
            _cameraCurrentRotation = MBMath.Lerp(_cameraCurrentRotation, _cameraTargetRotation, amount);
            _cameraCurrentElevationAdder = MBMath.Lerp(_cameraCurrentElevationAdder, _cameraTargetElevationAdder, amount);
            _cameraCurrentDistanceAdder = MBMath.Lerp(_cameraCurrentDistanceAdder, _cameraTargetDistanceAdder, amount);

            MatrixFrame frame = _characterFrame;
            frame.rotation.RotateAboutUp(_cameraCurrentRotation);
            frame.origin += _cameraCurrentElevationAdder * frame.rotation.u + _cameraCurrentDistanceAdder * frame.rotation.f;
            frame.rotation.RotateAboutSide(-1.5707964f);
            frame.rotation.RotateAboutUp(3.1415927f);
            frame.rotation.RotateAboutForward(-0.18849556f);

            _camera.Frame = frame;
            _sceneLayer.SetCamera(_camera);
        }

        private void UpdateBanners()
        {
            if (_latestBannerTextureCreationData != null)
            {
                ThumbnailCacheManager.Current.DestroyTexture(_latestBannerTextureCreationData);
                _latestBannerTextureCreationData = null;
            }

            Banner banner = _state.GetClan().Banner;
            BannerDebugInfo bannerDebugInfo = BannerDebugInfo.CreateManual(GetType().Name);
            BannerVisualExtensions.GetTableauTextureLargeForBannerEditor(banner, in bannerDebugInfo, OnNewBannerReadyForBanners, out _latestBannerTextureCreationData);
        }

        private void OnNewBannerReadyForBanners(TaleWorlds.Engine.Texture newTexture)
        {
            if (_scene != null && _currentBanner.BannerCode == _state.GetClan().Banner.BannerCode)
            {
                GameEntity bannerEntity = _scene.FindEntityWithTag("banner") ?? _scene.FindEntityWithTag("banner_2");

                if (bannerEntity != null)
                {
                    Mesh mesh = bannerEntity.GetFirstMesh();
                    if (mesh != null)
                    {
                        mesh.GetMaterial().SetTexture((TaleWorlds.Engine.Material.MBTextureType)1, newTexture);
                    }
                }
                _refreshCharacterAndShieldNextFrame = true;
            }
        }

        private void RefreshShieldAndCharacter()
        {
            _currentBanner = _state.GetClan().Banner;
            _refreshBannersNextFrame = true;
        }

        private void RefreshCharacterAux()
        {
            if (_latestShieldTextureCreationData != null)
            {
                ThumbnailCacheManager.Current.DestroyTexture(_latestShieldTextureCreationData);
                _latestShieldTextureCreationData = null;
            }

            Banner banner = _state.GetClan().Banner;
            BannerDebugInfo bannerDebugInfo = BannerDebugInfo.CreateManual(GetType().Name);
            BannerVisualExtensions.GetTableauTextureLargeForBannerEditor(banner, in bannerDebugInfo, OnNewBannerReadyForShield, out _latestShieldTextureCreationData);

            int shieldSlot = _dataSource.ShieldSlotIndex;

            int nextVisual = (_agentVisualToShowIndex + 1) % 2;
            _agentVisualToShowIndex = nextVisual;

            AgentVisualsData data = _agentVisuals[_agentVisualToShowIndex].GetCopyAgentVisualsData();
            data.Equipment(_weaponEquipment)
                .RightWieldedItemIndex(-1)
                .LeftWieldedItemIndex(shieldSlot)
                .Banner(banner)
                .Frame(_characterFrame)
                .ClothColor1(banner.GetPrimaryColor())
                .ClothColor2(banner.GetFirstIconColor());

            _agentVisuals[_agentVisualToShowIndex].Refresh(false, data, true);
            _agentVisuals[_agentVisualToShowIndex].GetEntity().CheckResources(true, true);
            _checkWhetherAgentVisualIsReady = true;
        }

        private void OnNewBannerReadyForShield(TaleWorlds.Engine.Texture newTexture)
        {
            _shieldWeapon.GetWeaponData(false).TableauMaterial.SetTexture((TaleWorlds.Engine.Material.MBTextureType)1, newTexture);
        }

        void IGameStateListener.OnActivate() { }
        void IGameStateListener.OnDeactivate() { }
        void IGameStateListener.OnInitialize() { }
        void IGameStateListener.OnFinalize() { }
    }
}
