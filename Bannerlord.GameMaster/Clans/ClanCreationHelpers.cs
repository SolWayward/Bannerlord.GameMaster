using System;
using System.Reflection;
using Bannerlord.GameMaster.Information;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace Bannerlord.GameMaster.Clans
{
    /// <summary>
    /// Helpers to set private fields that are needed for clan creation to match native patterns
    /// </summary>
    public static class ClanCreationHelpers
    {
        // MARK: Cached Reflection
        private static readonly FieldInfo _distanceCacheField = typeof(Clan).GetField(
            "_distanceToClosestNonAllyFortificationCacheDirty",
            BindingFlags.NonPublic | BindingFlags.Instance);

        [Obsolete] private static readonly FieldInfo midSettlementField = typeof(Clan).GetField("_midSettlement", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// This sets a flag to ensures the clan's distance calculations are recalculated on first access.
        /// </summary>
        public static void SetDistanceCacheDirty(Clan clan)
        {
            if (clan == null || _distanceCacheField == null)
                return;

            try
            {
                _distanceCacheField.SetValue(clan, true);
            }
            catch (Exception ex)
            {
                TaleWorlds.Library.Debug.Print($"Warning: Failed to set distance cache flag for {clan.StringId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Sets the clan's saved banner color properties (BannerBackgroundColorPrimary/Secondary, BannerIconColor)
        /// using the public Clan.UpdateBannerColor() method. These are used by native
        /// UpdateBannerColorsAccordingToKingdom() to reconstruct colors on kingdom join/leave state transitions.
        /// Note: UpdateBannerColor sets Primary = Secondary = backgroundColor (matches native rebel clan creation pattern).
        /// </summary>
        public static void SetOriginalBannerColors(Clan clan, Banner banner)
        {
            if (clan == null || banner == null)
                return;

            clan.UpdateBannerColor(banner.GetPrimaryColor(), banner.GetFirstIconColor());
        }

        /// <summary>
        /// Sets the private _midSettlement field on a clan using reflection.
        /// This is necessary to prevent crashes when the game's AI tries to access FactionMidSettlement.<br /><br />
        /// DEPRECATED: Use clan.SetInitialHomeSettlement(settlement) and then call clan.CalculateMidSettlement() instead
        /// </summary>
        [Obsolete("Use clan.SetInitialHomeSettlement(settlement) and then call clan.CalculateMidSettlement() instead")]
        private static void SetClanMidSettlement(Clan clan, Settlement settlement)
        {
            if (clan == null || settlement == null)
                return;

            try
            {
                if (midSettlementField != null)
                {
                    midSettlementField.SetValue(clan, settlement);
                }
            }
            catch (System.Exception ex)
            {
                // Log error but don't crash - the clan can still function without mid settlement
                InfoMessage.Warning($"Warning: Failed to set clan mid settlement: {ex.Message}");
            }
        }

    }
}