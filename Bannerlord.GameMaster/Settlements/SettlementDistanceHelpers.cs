using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.Library;
using Helpers;

namespace Bannerlord.GameMaster.Settlements
{
    /// <summary>
    /// Provides useful methods for calculating map distance, travel distance and travel time considering pathing and terrain
    /// Allows calculations for different navigation types (Land, Naval, All)
    /// </summary>
    public static class SettlementDistanceHelpers
    {
        /// MARK: Distance Calculation
        /// <summary>
        /// Finds the nearest Town, Castle, or Village to the specified settlement
        /// </summary>
        /// <param name="settlement">The settlement to use to find the next closest settlement. Will not return the input settlement</param>
        public static Settlement FindNearestNonHideoutSettlement(Settlement settlement)
        {
            return FindNearestNonHideoutSettlement(settlement.Position, true);
        }

        /// <summary>
        /// Finds the nearest Town, Castle, or Village. By default, if the position is the position of a settlement already, that settlement will be returned
        /// Use ignoreSettlementsAtInputPosition = true to if you would rather return the next closest settlement.
        /// </summary>
        /// <param name="position">The campaign map position to find the closest settlement to</param>
        /// <param name="ignoreSettlementsAtInputPosition">If true will skip settlements that have the same position within tolerance of 0.1fas the input position
        /// The settlement position, gateposition, and port position will be checked against the tolerance if true</param>
        public static Settlement FindNearestNonHideoutSettlement(CampaignVec2 position, bool ignoreSettlementsAtInputPosition = false)
        {
            float ignoreTolerance = 0.1f;
            float minDistance = float.MaxValue;
            Settlement nearest = null;
            MBReadOnlyList<Settlement> allSettlements = Settlement.All;
            int count = allSettlements.Count;

            for (int i = 0; i < count; i++)
            {
                Settlement current = allSettlements[i];

                // Position matches settlement and ignore is true
                if (ignoreSettlementsAtInputPosition)
                {
                    if (position.DistanceSquared(current.Position) < ignoreTolerance)
                        continue;

                    if (position.DistanceSquared(current.GatePosition) < ignoreTolerance)
                        continue;

                    if (current.HasPort && position.DistanceSquared(current.PortPosition) < ignoreTolerance)
                        continue;
                }

                if (current.IsHideout || current.Culture == null || !current.Culture.IsMainCulture)
                    continue;

                float distance = position.DistanceSquared(current.Position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = current;
                }
            }

            return nearest;
        }

        /// <summary>
        /// Finds the nearest Town, Castle, or Village using actual pathing distance (considers terrain, obstacles).
        /// Slower than FindNearestNonHideoutSettlement but accounts for rivers, mountains, etc.
        /// </summary>
        /// <param name="settlement">The source settlement</param>
        /// <param name="navigationType">Navigation capability: Default (land), Naval (sea), or All (both)</param>
        /// <returns>Nearest settlement by pathing distance, or null if none found</returns>
        public static Settlement FindNearestNonHideoutSettlementByPath(Settlement settlement, MobileParty.NavigationType navigationType = MobileParty.NavigationType.Default)
        {
            if (settlement == null)
                return null;

            float minDistance = float.MaxValue;
            Settlement nearest = null;
            MBReadOnlyList<Settlement> allSettlements = Settlement.All;
            int count = allSettlements.Count;
            MapDistanceModel distanceModel = Campaign.Current.Models.MapDistanceModel;

            for (int i = 0; i < count; i++)
            {
                Settlement current = allSettlements[i];

                if (current == settlement)
                    continue;

                if (current.IsHideout || current.Culture == null || !current.Culture.IsMainCulture)
                    continue;

                float distance = distanceModel.GetDistance(settlement, current, false, false, navigationType);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = current;
                }
            }

            return nearest;
        }

        /// <summary>
        /// Gets the pathing distance between two settlements considering terrain and obstacles.
        /// </summary>
        /// <param name="from">Source settlement</param>
        /// <param name="to">Destination settlement</param>
        /// <param name="navigationType">Navigation type: Default (land), Naval (sea), or All (both)</param>
        /// <returns>Pathing distance, or float.MaxValue if no path exists</returns>
        public static float GetPathingDistance(Settlement from, Settlement to, MobileParty.NavigationType navigationType = MobileParty.NavigationType.Default)
        {
            if (from == null || to == null)
                return float.MaxValue;

            return Campaign.Current.Models.MapDistanceModel.GetDistance(from, to, false, false, navigationType);
        }

        /// <summary>
        /// Gets the pathing distance with land/sea ratio information.
        /// </summary>
        /// <param name="from">Source settlement</param>
        /// <param name="to">Destination settlement</param>
        /// <param name="navigationType">Navigation type</param>
        /// <param name="landRatio">Output: 0.0 (all sea) to 1.0 (all land)</param>
        /// <returns>Pathing distance</returns>
        public static float GetPathingDistance(Settlement from, Settlement to, MobileParty.NavigationType navigationType, out float landRatio)
        {
            landRatio = 1f;
            if (from == null || to == null)
                return float.MaxValue;

            return Campaign.Current.Models.MapDistanceModel.GetDistance(from, to, false, false, navigationType, out landRatio);
        }

        /// <summary>
        /// Gets the pathing distance considering port usage for coastal settlements.
        /// Automatically finds the shortest route (gate-to-gate, port-to-gate, gate-to-port, or port-to-port).
        /// </summary>
        /// <param name="from">Source settlement</param>
        /// <param name="to">Destination settlement</param>
        /// <param name="navigationType">Navigation type</param>
        /// <param name="usedFromPort">Output: True if route started from port</param>
        /// <param name="usedToPort">Output: True if route ended at port</param>
        /// <returns>Shortest pathing distance</returns>
        public static float GetShortestPathingDistance(Settlement from, Settlement to, MobileParty.NavigationType navigationType, out bool usedFromPort, out bool usedToPort)
        {
            usedFromPort = false;
            usedToPort = false;

            if (from == null || to == null)
                return float.MaxValue;

            float landRatio;
            return DistanceHelper.FindClosestDistanceFromSettlementToSettlement(
                from, to, navigationType, out usedFromPort, out usedToPort, out landRatio);
        }

        /// <summary>
        /// Estimates travel time in hours between two settlements.
        /// </summary>
        /// <param name="from">Source settlement</param>
        /// <param name="to">Destination settlement</param>
        /// <param name="navigationType">Navigation type</param>
        /// <param name="partySpeed">Party speed (use mobileParty.Speed or Campaign.Current.EstimatedAverageLordPartySpeed)</param>
        /// <returns>Estimated hours to travel, or float.MaxValue if unreachable</returns>
        public static float EstimateTravelTimeHours(Settlement from, Settlement to, MobileParty.NavigationType navigationType, float partySpeed)
        {
            if (from == null || to == null || partySpeed <= 0f)
                return float.MaxValue;

            float distance = GetPathingDistance(from, to, navigationType);
            if (distance >= float.MaxValue)
                return float.MaxValue;

            return distance / partySpeed;
        }

        /// <summary>
        /// Estimates travel time in days between two settlements.
        /// </summary>
        /// <param name="from">Source settlement</param>
        /// <param name="to">Destination settlement</param>
        /// <param name="navigationType">Navigation type</param>
        /// <param name="partySpeed">Party speed (use mobileParty.Speed or Campaign.Current.EstimatedAverageLordPartySpeed)</param>
        /// <returns>Estimated days to travel, or float.MaxValue if unreachable</returns>
        public static float EstimateTravelTimeDays(Settlement from, Settlement to, MobileParty.NavigationType navigationType, float partySpeed)
        {
            float hours = EstimateTravelTimeHours(from, to, navigationType, partySpeed);
            if (hours >= float.MaxValue)
                return float.MaxValue;

            return hours / (float)CampaignTime.HoursInDay;
        }

        /// <summary>
        /// Estimates travel time using average lord party speed.
        /// </summary>
        /// <param name="from">Source settlement</param>
        /// <param name="to">Destination settlement</param>
        /// <param name="navigationType">Navigation type</param>
        /// <returns>Estimated days to travel</returns>
        public static float EstimateTravelTimeDays(Settlement from, Settlement to, MobileParty.NavigationType navigationType = MobileParty.NavigationType.Default)
        {
            return EstimateTravelTimeDays(from, to, navigationType, Campaign.Current.EstimatedAverageLordPartySpeed);
        }

        /// <summary>
        /// Gets pathing distance from a mobile party to a settlement.
        /// </summary>
        /// <param name="party">Source party</param>
        /// <param name="settlement">Destination settlement</param>
        /// <param name="navigationType">Navigation type (defaults to party's capability)</param>
        /// <returns>Pathing distance</returns>
        public static float GetPathingDistance(MobileParty party, Settlement settlement, MobileParty.NavigationType? navigationType = null)
        {
            if (party == null || settlement == null)
                return float.MaxValue;

            MobileParty.NavigationType navType = navigationType ?? party.NavigationCapability;
            float landRatio;
            return Campaign.Current.Models.MapDistanceModel.GetDistance(party, settlement, false, navType, out landRatio);
        }

        /// <summary>
        /// Checks if a path exists between two settlements for the given navigation type.
        /// </summary>
        /// <param name="from">Source settlement</param>
        /// <param name="to">Destination settlement</param>
        /// <param name="navigationType">Navigation type</param>
        /// <returns>True if a valid path exists</returns>
        public static bool PathExists(Settlement from, Settlement to, MobileParty.NavigationType navigationType = MobileParty.NavigationType.Default)
        {
            if (from == null || to == null)
                return false;

            float distance = GetPathingDistance(from, to, navigationType);
            return distance < Campaign.MapDiagonal * 5f; // Native pattern for "valid path"
        }

        /// <summary>
        /// Gets settlements within a pathing distance range.
        /// </summary>
        /// <param name="from">Source settlement</param>
        /// <param name="maxPathingDistance">Maximum pathing distance</param>
        /// <param name="navigationType">Navigation type</param>
        /// <param name="filter">Optional filter predicate</param>
        /// <returns>List of settlements within range</returns>
        public static MBList<Settlement> GetSettlementsWithinPathingDistance(
            Settlement from,
            float maxPathingDistance,
            MobileParty.NavigationType navigationType = MobileParty.NavigationType.Default,
            Func<Settlement, bool> filter = null)
        {
            MBList<Settlement> result = new();
            if (from == null)
                return result;

            MBReadOnlyList<Settlement> allSettlements = Settlement.All;
            int count = allSettlements.Count;
            MapDistanceModel distanceModel = Campaign.Current.Models.MapDistanceModel;

            for (int i = 0; i < count; i++)
            {
                Settlement current = allSettlements[i];

                if (current == from)
                    continue;

                if (filter != null && !filter(current))
                    continue;

                float distance = distanceModel.GetDistance(from, current, false, false, navigationType);
                if (distance <= maxPathingDistance)
                {
                    result.Add(current);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the average distance between closest towns for the given navigation type.
        /// Useful for relative distance comparisons.
        /// </summary>
        /// <param name="navigationType">Navigation type</param>
        /// <returns>Average distance between closest towns</returns>
        public static float GetAverageDistanceBetweenTowns(MobileParty.NavigationType navigationType = MobileParty.NavigationType.Default)
        {
            return Campaign.Current.GetAverageDistanceBetweenClosestTwoTownsWithNavigationType(navigationType);
        }

        /// <summary>
        /// Checks if a settlement is within "nearby" distance (based on game's average town distance).
        /// </summary>
        /// <param name="from">Source settlement</param>
        /// <param name="to">Target settlement</param>
        /// <param name="multiplier">Multiplier for "nearby" threshold (1.0 = average town distance)</param>
        /// <param name="navigationType">Navigation type</param>
        /// <returns>True if within nearby distance</returns>
        public static bool IsSettlementNearby(Settlement from, Settlement to, float multiplier = 1.5f, MobileParty.NavigationType navigationType = MobileParty.NavigationType.Default)
        {
            if (from == null || to == null)
                return false;

            float threshold = GetAverageDistanceBetweenTowns(navigationType) * multiplier;
            float distance = GetPathingDistance(from, to, navigationType);
            return distance <= threshold;
        }
    }
}