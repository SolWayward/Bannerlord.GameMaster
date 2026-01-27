using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Settlements
{
    /// <summary>
    /// Provides methods for finding the nearest settlement of specific types.
    /// Supports both map distance (straight-line, fast) and path distance (terrain-aware, slower) calculations.
    /// </summary>
    public static class SettlementNearestFinder
    {
        #region Find by Map Distance

        /// <summary>
        /// Finds the nearest hideout to the specified settlement using map distance (straight-line).
        /// </summary>
        /// <param name="from">The source settlement to measure from</param>
        /// <returns>The nearest hideout, or null if none found or from is null</returns>
        public static Settlement FindNearestHideout(Settlement from)
        {
            return FindNearestSettlementOfType(from, s => s.IsHideout);
        }

        /// <summary>
        /// Finds the nearest town to the specified settlement using map distance (straight-line).
        /// </summary>
        /// <param name="from">The source settlement to measure from</param>
        /// <returns>The nearest town, or null if none found or from is null</returns>
        public static Settlement FindNearestTown(Settlement from)
        {
            return FindNearestSettlementOfType(from, s => s.IsTown);
        }

        /// <summary>
        /// Finds the nearest castle to the specified settlement using map distance (straight-line).
        /// </summary>
        /// <param name="from">The source settlement to measure from</param>
        /// <returns>The nearest castle, or null if none found or from is null</returns>
        public static Settlement FindNearestCastle(Settlement from)
        {
            return FindNearestSettlementOfType(from, s => s.IsCastle);
        }

        /// <summary>
        /// Finds the nearest village to the specified settlement using map distance (straight-line).
        /// </summary>
        /// <param name="from">The source settlement to measure from</param>
        /// <returns>The nearest village, or null if none found or from is null</returns>
        public static Settlement FindNearestVillage(Settlement from)
        {
            return FindNearestSettlementOfType(from, s => s.IsVillage);
        }

        /// <summary>
        /// Finds the nearest fortification (castle or town) to the specified settlement using map distance (straight-line).
        /// </summary>
        /// <param name="from">The source settlement to measure from</param>
        /// <returns>The nearest castle or town, or null if none found or from is null</returns>
        public static Settlement FindNearestFortification(Settlement from)
        {
            return FindNearestSettlementOfType(from, s => s.IsTown || s.IsCastle);
        }

        #endregion

        #region Find by Path Distance

        /// <summary>
        /// Finds the nearest hideout to the specified settlement using path distance (terrain-aware).
        /// Slower than map distance but accounts for rivers, mountains, and other terrain obstacles.
        /// </summary>
        /// <param name="from">The source settlement to measure from</param>
        /// <param name="navigationType">Navigation capability: Default (land), Naval (sea), or All (both)</param>
        /// <returns>The nearest hideout by path, or null if none found or from is null</returns>
        public static Settlement FindNearestHideoutByPath(Settlement from, MobileParty.NavigationType navigationType = MobileParty.NavigationType.Default)
        {
            return FindNearestSettlementOfTypeByPath(from, s => s.IsHideout, navigationType);
        }

        /// <summary>
        /// Finds the nearest town to the specified settlement using path distance (terrain-aware).
        /// Slower than map distance but accounts for rivers, mountains, and other terrain obstacles.
        /// </summary>
        /// <param name="from">The source settlement to measure from</param>
        /// <param name="navigationType">Navigation capability: Default (land), Naval (sea), or All (both)</param>
        /// <returns>The nearest town by path, or null if none found or from is null</returns>
        public static Settlement FindNearestTownByPath(Settlement from, MobileParty.NavigationType navigationType = MobileParty.NavigationType.Default)
        {
            return FindNearestSettlementOfTypeByPath(from, s => s.IsTown, navigationType);
        }

        /// <summary>
        /// Finds the nearest castle to the specified settlement using path distance (terrain-aware).
        /// Slower than map distance but accounts for rivers, mountains, and other terrain obstacles.
        /// </summary>
        /// <param name="from">The source settlement to measure from</param>
        /// <param name="navigationType">Navigation capability: Default (land), Naval (sea), or All (both)</param>
        /// <returns>The nearest castle by path, or null if none found or from is null</returns>
        public static Settlement FindNearestCastleByPath(Settlement from, MobileParty.NavigationType navigationType = MobileParty.NavigationType.Default)
        {
            return FindNearestSettlementOfTypeByPath(from, s => s.IsCastle, navigationType);
        }

        /// <summary>
        /// Finds the nearest village to the specified settlement using path distance (terrain-aware).
        /// Slower than map distance but accounts for rivers, mountains, and other terrain obstacles.
        /// </summary>
        /// <param name="from">The source settlement to measure from</param>
        /// <param name="navigationType">Navigation capability: Default (land), Naval (sea), or All (both)</param>
        /// <returns>The nearest village by path, or null if none found or from is null</returns>
        public static Settlement FindNearestVillageByPath(Settlement from, MobileParty.NavigationType navigationType = MobileParty.NavigationType.Default)
        {
            return FindNearestSettlementOfTypeByPath(from, s => s.IsVillage, navigationType);
        }

        /// <summary>
        /// Finds the nearest fortification (castle or town) to the specified settlement using path distance (terrain-aware).
        /// Slower than map distance but accounts for rivers, mountains, and other terrain obstacles.
        /// </summary>
        /// <param name="from">The source settlement to measure from</param>
        /// <param name="navigationType">Navigation capability: Default (land), Naval (sea), or All (both)</param>
        /// <returns>The nearest castle or town by path, or null if none found or from is null</returns>
        public static Settlement FindNearestFortificationByPath(Settlement from, MobileParty.NavigationType navigationType = MobileParty.NavigationType.Default)
        {
            return FindNearestSettlementOfTypeByPath(from, s => s.IsTown || s.IsCastle, navigationType);
        }

        #endregion

        #region Internal Helpers

        /// <summary>
        /// Generic finder for the nearest settlement matching a predicate using map distance (straight-line).
        /// Uses DistanceSquared for performance.
        /// </summary>
        /// <param name="from">The source settlement to measure from</param>
        /// <param name="predicate">Filter predicate to match settlement type</param>
        /// <returns>The nearest matching settlement, or null if none found or from is null</returns>
        internal static Settlement FindNearestSettlementOfType(Settlement from, Func<Settlement, bool> predicate)
        {
            if (from == null || predicate == null)
                return null;

            float minDistance = float.MaxValue;
            Settlement nearest = null;
            MBReadOnlyList<Settlement> allSettlements = Settlement.All;
            int count = allSettlements.Count;
            CampaignVec2 fromPosition = from.Position;

            for (int i = 0; i < count; i++)
            {
                Settlement current = allSettlements[i];

                if (current == from)
                    continue;

                if (!predicate(current))
                    continue;

                float distance = fromPosition.DistanceSquared(current.Position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = current;
                }
            }

            return nearest;
        }

        /// <summary>
        /// Generic finder for the nearest settlement matching a predicate using path distance (terrain-aware).
        /// Slower than map distance but accounts for terrain obstacles.
        /// </summary>
        /// <param name="from">The source settlement to measure from</param>
        /// <param name="predicate">Filter predicate to match settlement type</param>
        /// <param name="navigationType">Navigation capability: Default (land), Naval (sea), or All (both)</param>
        /// <returns>The nearest matching settlement by path, or null if none found or from is null</returns>
        internal static Settlement FindNearestSettlementOfTypeByPath(Settlement from, Func<Settlement, bool> predicate, MobileParty.NavigationType navigationType)
        {
            if (from == null || predicate == null)
                return null;

            float minDistance = float.MaxValue;
            Settlement nearest = null;
            MBReadOnlyList<Settlement> allSettlements = Settlement.All;
            int count = allSettlements.Count;
            MapDistanceModel distanceModel = Campaign.Current.Models.MapDistanceModel;

            for (int i = 0; i < count; i++)
            {
                Settlement current = allSettlements[i];

                if (current == from)
                    continue;

                if (!predicate(current))
                    continue;

                float distance = distanceModel.GetDistance(from, current, false, false, navigationType);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = current;
                }
            }

            return nearest;
        }

        #endregion
    }
}
