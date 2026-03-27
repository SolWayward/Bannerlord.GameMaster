using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Map
{
    /// <summary>
    /// Provides methods for detecting MobileParty, Settlement, or terrain
    /// entities near a given campaign map world position.
    /// Used by BLC for cursor-based entity targeting on the campaign map.
    /// </summary>
    public static class MapEntityDetector
    {
        // Default detection radii (squared to avoid sqrt in distance comparisons)
        private const float DefaultPartyDetectionRadiusSq = 1.0f;
        private const float DefaultSettlementDetectionRadiusSq = 4.0f;

        // MARK: DetectEntityAtWorldPosition
        /// <summary>
        /// Detects the nearest map entity (party or settlement) at the given world position.
        /// Parties are prioritized over settlements when both overlap, matching the game's
        /// native behavior of prioritizing mobile entities over static ones.
        /// Returns a terrain result if no entity is within detection range.
        /// </summary>
        /// <param name="worldPosition">The campaign map world position to detect entities at</param>
        /// <param name="partyRadiusSq">Squared detection radius for parties (default: 1.0)</param>
        /// <param name="settlementRadiusSq">Squared detection radius for settlements (default: 4.0)</param>
        /// <returns>Detection result containing the entity type and reference</returns>
        public static MapEntityDetectionResult DetectEntityAtWorldPosition(
            Vec2 worldPosition,
            float partyRadiusSq = DefaultPartyDetectionRadiusSq,
            float settlementRadiusSq = DefaultSettlementDetectionRadiusSq)
        {
            if (Campaign.Current == null)
                return MapEntityDetectionResult.Empty();

            // Parties are checked first - mobile entities take priority over static ones
            float partyDistSq;
            MobileParty nearestParty = FindNearestPartyInternal(worldPosition, partyRadiusSq, null, out partyDistSq);
            if (nearestParty != null)
                return MapEntityDetectionResult.ForParty(nearestParty, worldPosition, partyDistSq);

            float settlementDistSq;
            Settlement nearestSettlement = FindNearestSettlementInternal(worldPosition, settlementRadiusSq, null, out settlementDistSq);
            if (nearestSettlement != null)
                return MapEntityDetectionResult.ForSettlement(nearestSettlement, worldPosition, settlementDistSq);

            return MapEntityDetectionResult.ForTerrain(worldPosition);
        }

        // MARK: FindNearestParty
        /// <summary>
        /// Finds the nearest visible mobile party to a world position within the detection radius.
        /// Skips invisible, removed, and settlement-docked parties.
        /// Uses DistanceSquared for performance (no sqrt).
        /// </summary>
        /// <param name="worldPosition">The campaign map world position to search from</param>
        /// <param name="detectionRadiusSq">Squared maximum detection distance (default: 1.0)</param>
        /// <param name="predicate">Optional filter predicate to exclude specific parties</param>
        /// <returns>The nearest matching party, or null if none found within radius</returns>
        public static MobileParty FindNearestParty(
            Vec2 worldPosition,
            float detectionRadiusSq = DefaultPartyDetectionRadiusSq,
            Func<MobileParty, bool> predicate = null)
        {
            float distSq;
            return FindNearestPartyInternal(worldPosition, detectionRadiusSq, predicate, out distSq);
        }

        // MARK: FindNearestSettlement
        /// <summary>
        /// Finds the nearest settlement to a world position within the detection radius.
        /// Uses DistanceSquared for performance (no sqrt).
        /// </summary>
        /// <param name="worldPosition">The campaign map world position to search from</param>
        /// <param name="detectionRadiusSq">Squared maximum detection distance (default: 4.0)</param>
        /// <param name="predicate">Optional filter predicate to exclude specific settlements</param>
        /// <returns>The nearest matching settlement, or null if none found within radius</returns>
        public static Settlement FindNearestSettlement(
            Vec2 worldPosition,
            float detectionRadiusSq = DefaultSettlementDetectionRadiusSq,
            Func<Settlement, bool> predicate = null)
        {
            float distSq;
            return FindNearestSettlementInternal(worldPosition, detectionRadiusSq, predicate, out distSq);
        }

        // MARK: FindPartiesInRadius
        /// <summary>
        /// Finds all visible mobile parties within a squared radius of a world position.
        /// Skips invisible, removed, and settlement-docked parties.
        /// </summary>
        /// <param name="worldPosition">The campaign map world position to search from</param>
        /// <param name="radiusSq">Squared search radius</param>
        /// <param name="predicate">Optional filter predicate to exclude specific parties</param>
        /// <returns>List of parties within the radius (may be empty, never null)</returns>
        public static List<MobileParty> FindPartiesInRadius(
            Vec2 worldPosition,
            float radiusSq,
            Func<MobileParty, bool> predicate = null)
        {
            List<MobileParty> results = new();

            if (Campaign.Current == null)
                return results;

            MBReadOnlyList<MobileParty> allParties = MobileParty.All;
            int count = allParties.Count;

            for (int i = 0; i < count; i++)
            {
                MobileParty party = allParties[i];

                if (!IsPartyDetectable(party))
                    continue;

                if (predicate != null && !predicate(party))
                    continue;

                float distSq = worldPosition.DistanceSquared(party.GetPosition2D);
                if (distSq <= radiusSq)
                {
                    results.Add(party);
                }
            }

            return results;
        }

        // MARK: FindSettlementsInRadius
        /// <summary>
        /// Finds all settlements within a squared radius of a world position.
        /// </summary>
        /// <param name="worldPosition">The campaign map world position to search from</param>
        /// <param name="radiusSq">Squared search radius</param>
        /// <param name="predicate">Optional filter predicate to exclude specific settlements</param>
        /// <returns>List of settlements within the radius (may be empty, never null)</returns>
        public static List<Settlement> FindSettlementsInRadius(
            Vec2 worldPosition,
            float radiusSq,
            Func<Settlement, bool> predicate = null)
        {
            List<Settlement> results = new();

            if (Campaign.Current == null)
                return results;

            MBReadOnlyList<Settlement> allSettlements = Settlement.All;
            int count = allSettlements.Count;

            for (int i = 0; i < count; i++)
            {
                Settlement settlement = allSettlements[i];

                if (predicate != null && !predicate(settlement))
                    continue;

                float distSq = worldPosition.DistanceSquared(settlement.GetPosition2D);
                if (distSq <= radiusSq)
                {
                    results.Add(settlement);
                }
            }

            return results;
        }

        #region Internal Helpers

        /// <summary>
        /// Internal nearest party finder that also outputs the distance squared of the result.
        /// </summary>
        private static MobileParty FindNearestPartyInternal(
            Vec2 worldPosition,
            float detectionRadiusSq,
            Func<MobileParty, bool> predicate,
            out float resultDistSq)
        {
            resultDistSq = 0f;

            if (Campaign.Current == null)
                return null;

            MBReadOnlyList<MobileParty> allParties = MobileParty.All;
            int count = allParties.Count;
            float minDistSq = detectionRadiusSq;
            MobileParty nearest = null;

            for (int i = 0; i < count; i++)
            {
                MobileParty party = allParties[i];

                if (!IsPartyDetectable(party))
                    continue;

                if (predicate != null && !predicate(party))
                    continue;

                float distSq = worldPosition.DistanceSquared(party.GetPosition2D);
                if (distSq < minDistSq)
                {
                    minDistSq = distSq;
                    nearest = party;
                }
            }

            if (nearest != null)
            {
                resultDistSq = minDistSq;
            }

            return nearest;
        }

        /// <summary>
        /// Internal nearest settlement finder that also outputs the distance squared of the result.
        /// </summary>
        private static Settlement FindNearestSettlementInternal(
            Vec2 worldPosition,
            float detectionRadiusSq,
            Func<Settlement, bool> predicate,
            out float resultDistSq)
        {
            resultDistSq = 0f;

            if (Campaign.Current == null)
                return null;

            MBReadOnlyList<Settlement> allSettlements = Settlement.All;
            int count = allSettlements.Count;
            float minDistSq = detectionRadiusSq;
            Settlement nearest = null;

            for (int i = 0; i < count; i++)
            {
                Settlement settlement = allSettlements[i];

                if (predicate != null && !predicate(settlement))
                    continue;

                float distSq = worldPosition.DistanceSquared(settlement.GetPosition2D);
                if (distSq < minDistSq)
                {
                    minDistSq = distSq;
                    nearest = settlement;
                }
            }

            if (nearest != null)
            {
                resultDistSq = minDistSq;
            }

            return nearest;
        }

        /// <summary>
        /// Determines if a party should be considered for map entity detection.
        /// Filters out invisible, removed, and settlement-docked parties.
        /// </summary>
        private static bool IsPartyDetectable(MobileParty party)
        {
            if (!party.IsVisible)
                return false;

            if (!party.IsActive)
                return false;

            // Parties inside settlements are not visible on the map as separate entities
            if (party.CurrentSettlement != null)
                return false;

            return true;
        }

        #endregion
    }
}
