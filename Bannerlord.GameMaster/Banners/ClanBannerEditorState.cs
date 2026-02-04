using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace Bannerlord.GameMaster.Banners
{
    public class ClanBannerEditorState : GameState
    {
        private Clan _targetClan;
        private Action _onEndAction;

        public override bool IsMenuState => true;

        // Parameterless constructor for CreateState<T> new() constraint
        public ClanBannerEditorState()
        {
            // Left empty intentionally
        }

        public void Initialize(Clan targetClan, Action onEndAction = null)
        {
            _targetClan = targetClan;
            _onEndAction = onEndAction;
        }

        public Clan GetClan() => _targetClan;

        public CharacterObject GetCharacter() => _targetClan?.Leader?.CharacterObject
            ?? CharacterObject.PlayerCharacter;

        protected override void OnFinalize()
        {
            base.OnFinalize();
            _onEndAction?.Invoke();
        }
    }
}
