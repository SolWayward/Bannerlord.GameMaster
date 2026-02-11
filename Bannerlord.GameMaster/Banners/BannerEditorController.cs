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
        /// Opens the native banner editor for any clan banner.
        /// </summary>
        /// <param name="clan">The clan whose banner to edit.</param>
        /// <param name="removeExtraIcons">When true, strips the banner to a single centered icon
        /// before opening the editor. On cancel, the original multi-icon banner is fully restored.</param>
        /// <param name="onComplete">Optional callback invoked when the editor state finalizes.</param>
        public static void OpenBannerEditor(Clan clan, bool removeExtraIcons = false, Action onComplete = null)
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

            // Optionally strip to single icon before editor opens
            string originalBannerCode = null;
            if (removeExtraIcons)
            {
                originalBannerCode = clan.Banner.Serialize();
                clan.Banner.ConvertToSingleIcon();
                clan.Banner.ResetIconTransforms();
            }

            // Create state with parameterless constructor then initialize
            ClanBannerEditorState state = Game.Current.GameStateManager
                .CreateState<ClanBannerEditorState>();
            state.Initialize(clan, onComplete, originalBannerCode);

            Game.Current.GameStateManager.PushState(state, 0);
        }
    }
}
