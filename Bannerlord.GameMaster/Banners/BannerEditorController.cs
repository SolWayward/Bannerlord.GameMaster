using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using Bannerlord.GameMaster.Information;
using Bannerlord.GameMaster.Common;

namespace Bannerlord.GameMaster.Banners
{
    public static class BannerEditorController
    {
        /// <summary>
        /// Opens the native banner editor for any clan banner
        /// </summary>
        public static void OpenBannerEditor(Clan clan, Action onComplete = null)
        {
            if (clan == null)
            {
                BLGMResult.Error("Cannot open banner editor: Clan is null").Log();
                return;
            }

            if (!Campaign.Current.IsBannerEditorEnabled)
            {
                InfoMessage.Log("Banner editor is disabled in this campaign");
                return;
            }

            // Create state with parameterless constructor then initialize
            ClanBannerEditorState state = Game.Current.GameStateManager
                .CreateState<ClanBannerEditorState>();
            state.Initialize(clan, onComplete);

            Game.Current.GameStateManager.PushState(state, 0);
        }
    }
}
