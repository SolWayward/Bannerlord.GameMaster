using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace Bannerlord.GameMaster.Banners
{
    public class ClanBannerEditorState : GameState
    {
        private Clan _targetClan;
        private Action _onEndAction;
        private string _originalBannerCode;

        public override bool IsMenuState => true;

        // Parameterless constructor for CreateState<T> new() constraint
        public ClanBannerEditorState()
        {
            // Left empty intentionally
        }

        public void Initialize(Clan targetClan, Action onEndAction = null, string originalBannerCode = null)
        {
            _targetClan = targetClan;
            _onEndAction = onEndAction;
            _originalBannerCode = originalBannerCode;
        }

        public Clan GetClan() => _targetClan;

        /// <summary>
        /// Returns the serialized banner code from before icon stripping, or null if no stripping was performed.
        /// Used to restore the original multi-icon banner on cancel.
        /// </summary>
        public string GetOriginalBannerCode() => _originalBannerCode;

        public CharacterObject GetCharacter() => _targetClan?.Leader?.CharacterObject
            ?? CharacterObject.PlayerCharacter;

        protected override void OnFinalize()
        {
            base.OnFinalize();
            _onEndAction?.Invoke();
        }
    }
}
