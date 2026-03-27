using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Map
{
    /// <summary>
    /// Lightweight result struct for map entity detection at a world position.
    /// Contains the detected entity type and reference, plus the world position used.
    /// Struct to avoid GC pressure on high-frequency per-frame detection calls.
    /// </summary>
    public struct MapEntityDetectionResult
    {
        public MapEntityType EntityType;
        public MobileParty Party;
        public Settlement Settlement;
        public Vec2 WorldPosition;
        public float DistanceSquared;

        #region Factory Methods

        public static MapEntityDetectionResult ForParty(MobileParty party, Vec2 worldPos, float distSq)
        {
            return new MapEntityDetectionResult
            {
                EntityType = MapEntityType.MobileParty,
                Party = party,
                Settlement = null,
                WorldPosition = worldPos,
                DistanceSquared = distSq
            };
        }

        public static MapEntityDetectionResult ForSettlement(Settlement settlement, Vec2 worldPos, float distSq)
        {
            return new MapEntityDetectionResult
            {
                EntityType = MapEntityType.Settlement,
                Party = null,
                Settlement = settlement,
                WorldPosition = worldPos,
                DistanceSquared = distSq
            };
        }

        public static MapEntityDetectionResult ForTerrain(Vec2 worldPos)
        {
            return new MapEntityDetectionResult
            {
                EntityType = MapEntityType.Terrain,
                Party = null,
                Settlement = null,
                WorldPosition = worldPos,
                DistanceSquared = 0f
            };
        }

        public static MapEntityDetectionResult Empty()
        {
            return new MapEntityDetectionResult
            {
                EntityType = MapEntityType.None,
                Party = null,
                Settlement = null,
                WorldPosition = Vec2.Zero,
                DistanceSquared = 0f
            };
        }

        #endregion

        #region Convenience Properties

        public bool IsEmpty => EntityType == MapEntityType.None;

        public bool IsParty => EntityType == MapEntityType.MobileParty;

        public bool IsSettlement => EntityType == MapEntityType.Settlement;

        public bool IsTerrain => EntityType == MapEntityType.Terrain;

        #endregion
    }
}
