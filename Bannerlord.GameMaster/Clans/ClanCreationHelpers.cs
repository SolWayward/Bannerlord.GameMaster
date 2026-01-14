using System;
using System.Reflection;
using Bannerlord.GameMaster.Information;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace Bannerlord.GameMaster.Clans
{
    /// <summary>
    /// Reflection helpers to change private fields that are needed for clan creation to match native patterns
    /// </summary>
    public static class ClanCreationHelpers
    {
        // MARK: Cached Reflection
        private static readonly Type clanType = typeof(Clan);

        private static readonly FieldInfo _distanceCacheField = typeof(Clan).GetField(
            "_distanceToClosestNonAllyFortificationCacheDirty",
            BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly PropertyInfo primaryProp = clanType.GetProperty(
            "BannerBackgroundColorPrimary",
            BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly PropertyInfo secondaryProp = clanType.GetProperty(
            "BannerBackgroundColorSecondary",
            BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly PropertyInfo iconProp = clanType.GetProperty(
            "BannerIconColor",
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
        /// Sets native private fields that store original banner colors for kingdom join/leave scenarios using reflection
        /// </summary>
        public static void SetOriginalBannerColors(Clan clan, Banner banner)
        {
            if (clan == null || banner == null)
                return;

            try
            {
                if (primaryProp != null)
                    primaryProp.SetValue(clan, banner.GetPrimaryColor());

                if (secondaryProp != null)
                    secondaryProp.SetValue(clan, banner.GetSecondaryColor());

                if (iconProp != null)
                    iconProp.SetValue(clan, banner.GetFirstIconColor());
            }
            catch (System.Exception ex)
            {
                TaleWorlds.Library.Debug.Print($"Warning: Failed to set original banner color properties for {clan.StringId}: {ex.Message}");
            }
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