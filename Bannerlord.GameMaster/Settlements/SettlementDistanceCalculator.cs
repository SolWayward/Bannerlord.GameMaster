using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.ComponentInterfaces;

namespace Bannerlord.GameMaster.Settlements
{
    /// <summary>
    /// Provides methods for calculating distances between settlements and to nearest settlements of specific types.
    /// Supports both map distance (straight-line, fast) and path distance (terrain-aware, slower) calculations.
    /// </summary>
    public static class SettlementDistanceCalculator
    {
        #region Base Distance Calculations

        /// <summary>
        /// Gets the straight-line map distance between two settlements.
        /// This is the fastest distance calculation but does not account for terrain.
        /// </summary>
        /// <param name="from">The source settlement</param>
        /// <param name="to">The destination settlement</param>
        /// <returns>The straight-line distance, or float.MaxValue if either parameter is null</returns>
        public static float GetMapDistance(Settlement from, Settlement to)
        {
            if (from == null || to == null)
                return float.MaxValue;

            return from.Position.Distance(to.Position);
        }

        #endregion

        #region Map Distance to Nearest Type

        /// <summary>
        /// Gets the straight-line map distance to the nearest hideout from the specified settlement.
        /// </summary>
        /// <param name="from">The source settlement to measure from</param>
        /// <returns>Distance to the nearest hideout, or float.MaxValue if none found or from is null</returns>
        public static float GetMapDistanceToNearestHideout(Settlement from)
        {
            if (from == null)
                return float.MaxValue;

            Settlement nearest = SettlementNearestFinder.FindNearestHideout(from);
            if (nearest == null)
                return float.MaxValue;

            return GetMapDistance(from, nearest);
        }

        /// <summary>
        /// Gets the straight-line map distance to the nearest town from the specified settlement.
        /// </summary>
        /// <param name="from">The source settlement to measure from</param>
        /// <returns>Distance to the nearest town, or float.MaxValue if none found or from is null</returns>
        public static float GetMapDistanceToNearestTown(Settlement from)
        {
            if (from == null)
                return float.MaxValue;

            Settlement nearest = SettlementNearestFinder.FindNearestTown(from);
            if (nearest == null)
                return float.MaxValue;

            return GetMapDistance(from, nearest);
        }

        /// <summary>
        /// Gets the straight-line map distance to the nearest castle from the specified settlement.
        /// </summary>
        /// <param name="from">The source settlement to measure from</param>
        /// <returns>Distance to the nearest castle, or float.MaxValue if none found or from is null</returns>
        public static float GetMapDistanceToNearestCastle(Settlement from)
        {
            if (from == null)
                return float.MaxValue;

            Settlement nearest = SettlementNearestFinder.FindNearestCastle(from);
            if (nearest == null)
                return float.MaxValue;

            return GetMapDistance(from, nearest);
        }

        /// <summary>
        /// Gets the straight-line map distance to the nearest village from the specified settlement.
        /// </summary>
        /// <param name="from">The source settlement to measure from</param>
        /// <returns>Distance to the nearest village, or float.MaxValue if none found or from is null</returns>
        public static float GetMapDistanceToNearestVillage(Settlement from)
        {
            if (from == null)
                return float.MaxValue;

            Settlement nearest = SettlementNearestFinder.FindNearestVillage(from);
            if (nearest == null)
                return float.MaxValue;

            return GetMapDistance(from, nearest);
        }

        /// <summary>
        /// Gets the straight-line map distance to the nearest fortification (castle or town) from the specified settlement.
        /// </summary>
        /// <param name="from">The source settlement to measure from</param>
        /// <returns>Distance to the nearest castle or town, or float.MaxValue if none found or from is null</returns>
        public static float GetMapDistanceToNearestFortification(Settlement from)
        {
            if (from == null)
                return float.MaxValue;

            Settlement nearest = SettlementNearestFinder.FindNearestFortification(from);
            if (nearest == null)
                return float.MaxValue;

            return GetMapDistance(from, nearest);
        }

        #endregion

        #region Path Distance to Nearest Type

        /// <summary>
        /// Gets the pathing distance to the nearest hideout from the specified settlement.
        /// Slower than map distance but accounts for terrain obstacles like rivers and mountains.
        /// </summary>
        /// <param name="from">The source settlement to measure from</param>
        /// <param name="navigationType">Navigation capability: Default (land), Naval (sea), or All (both)</param>
        /// <returns>Pathing distance to the nearest hideout, or float.MaxValue if none found or from is null</returns>
        public static float GetPathingDistanceToNearestHideout(Settlement from, MobileParty.NavigationType navigationType = MobileParty.NavigationType.Default)
        {
            if (from == null)
                return float.MaxValue;

            Settlement nearest = SettlementNearestFinder.FindNearestHideoutByPath(from, navigationType);
            if (nearest == null)
                return float.MaxValue;

            MapDistanceModel distanceModel = Campaign.Current.Models.MapDistanceModel;
            return distanceModel.GetDistance(from, nearest, false, false, navigationType);
        }

        /// <summary>
        /// Gets the pathing distance to the nearest town from the specified settlement.
        /// Slower than map distance but accounts for terrain obstacles like rivers and mountains.
        /// </summary>
        /// <param name="from">The source settlement to measure from</param>
        /// <param name="navigationType">Navigation capability: Default (land), Naval (sea), or All (both)</param>
        /// <returns>Pathing distance to the nearest town, or float.MaxValue if none found or from is null</returns>
        public static float GetPathingDistanceToNearestTown(Settlement from, MobileParty.NavigationType navigationType = MobileParty.NavigationType.Default)
        {
            if (from == null)
                return float.MaxValue;

            Settlement nearest = SettlementNearestFinder.FindNearestTownByPath(from, navigationType);
            if (nearest == null)
                return float.MaxValue;

            MapDistanceModel distanceModel = Campaign.Current.Models.MapDistanceModel;
            return distanceModel.GetDistance(from, nearest, false, false, navigationType);
        }

        /// <summary>
        /// Gets the pathing distance to the nearest castle from the specified settlement.
        /// Slower than map distance but accounts for terrain obstacles like rivers and mountains.
        /// </summary>
        /// <param name="from">The source settlement to measure from</param>
        /// <param name="navigationType">Navigation capability: Default (land), Naval (sea), or All (both)</param>
        /// <returns>Pathing distance to the nearest castle, or float.MaxValue if none found or from is null</returns>
        public static float GetPathingDistanceToNearestCastle(Settlement from, MobileParty.NavigationType navigationType = MobileParty.NavigationType.Default)
        {
            if (from == null)
                return float.MaxValue;

            Settlement nearest = SettlementNearestFinder.FindNearestCastleByPath(from, navigationType);
            if (nearest == null)
                return float.MaxValue;

            MapDistanceModel distanceModel = Campaign.Current.Models.MapDistanceModel;
            return distanceModel.GetDistance(from, nearest, false, false, navigationType);
        }

        /// <summary>
        /// Gets the pathing distance to the nearest village from the specified settlement.
        /// Slower than map distance but accounts for terrain obstacles like rivers and mountains.
        /// </summary>
        /// <param name="from">The source settlement to measure from</param>
        /// <param name="navigationType">Navigation capability: Default (land), Naval (sea), or All (both)</param>
        /// <returns>Pathing distance to the nearest village, or float.MaxValue if none found or from is null</returns>
        public static float GetPathingDistanceToNearestVillage(Settlement from, MobileParty.NavigationType navigationType = MobileParty.NavigationType.Default)
        {
            if (from == null)
                return float.MaxValue;

            Settlement nearest = SettlementNearestFinder.FindNearestVillageByPath(from, navigationType);
            if (nearest == null)
                return float.MaxValue;

            MapDistanceModel distanceModel = Campaign.Current.Models.MapDistanceModel;
            return distanceModel.GetDistance(from, nearest, false, false, navigationType);
        }

        /// <summary>
        /// Gets the pathing distance to the nearest fortification (castle or town) from the specified settlement.
        /// Slower than map distance but accounts for terrain obstacles like rivers and mountains.
        /// </summary>
        /// <param name="from">The source settlement to measure from</param>
        /// <param name="navigationType">Navigation capability: Default (land), Naval (sea), or All (both)</param>
        /// <returns>Pathing distance to the nearest castle or town, or float.MaxValue if none found or from is null</returns>
        public static float GetPathingDistanceToNearestFortification(Settlement from, MobileParty.NavigationType navigationType = MobileParty.NavigationType.Default)
        {
            if (from == null)
                return float.MaxValue;

            Settlement nearest = SettlementNearestFinder.FindNearestFortificationByPath(from, navigationType);
            if (nearest == null)
                return float.MaxValue;

            MapDistanceModel distanceModel = Campaign.Current.Models.MapDistanceModel;
            return distanceModel.GetDistance(from, nearest, false, false, navigationType);
        }

        #endregion
    }
}
