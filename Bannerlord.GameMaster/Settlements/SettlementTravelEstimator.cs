using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Party;

namespace Bannerlord.GameMaster.Settlements
{
    /// <summary>
    /// Provides methods for estimating travel time to nearest settlements of specific types.
    /// Uses pathing distance calculations that account for terrain obstacles.
    /// </summary>
    public static class SettlementTravelEstimator
    {
        #region Travel Time in Hours

        /// <summary>
        /// Estimates travel time in hours to the nearest hideout from the specified settlement.
        /// </summary>
        /// <param name="from">The source settlement to travel from</param>
        /// <param name="navigationType">Navigation capability: Default (land), Naval (sea), or All (both)</param>
        /// <param name="partySpeed">Party speed (use mobileParty.Speed or Campaign.Current.EstimatedAverageLordPartySpeed)</param>
        /// <returns>Estimated hours to travel to nearest hideout, or float.MaxValue if unreachable or invalid parameters</returns>
        public static float EstimateTravelTimeHoursToNearestHideout(Settlement from, MobileParty.NavigationType navigationType, float partySpeed)
        {
            if (from == null || partySpeed <= 0f)
                return float.MaxValue;

            float distance = SettlementDistanceCalculator.GetPathingDistanceToNearestHideout(from, navigationType);
            if (distance >= float.MaxValue)
                return float.MaxValue;

            return distance / partySpeed;
        }

        /// <summary>
        /// Estimates travel time in hours to the nearest town from the specified settlement.
        /// </summary>
        /// <param name="from">The source settlement to travel from</param>
        /// <param name="navigationType">Navigation capability: Default (land), Naval (sea), or All (both)</param>
        /// <param name="partySpeed">Party speed (use mobileParty.Speed or Campaign.Current.EstimatedAverageLordPartySpeed)</param>
        /// <returns>Estimated hours to travel to nearest town, or float.MaxValue if unreachable or invalid parameters</returns>
        public static float EstimateTravelTimeHoursToNearestTown(Settlement from, MobileParty.NavigationType navigationType, float partySpeed)
        {
            if (from == null || partySpeed <= 0f)
                return float.MaxValue;

            float distance = SettlementDistanceCalculator.GetPathingDistanceToNearestTown(from, navigationType);
            if (distance >= float.MaxValue)
                return float.MaxValue;

            return distance / partySpeed;
        }

        /// <summary>
        /// Estimates travel time in hours to the nearest castle from the specified settlement.
        /// </summary>
        /// <param name="from">The source settlement to travel from</param>
        /// <param name="navigationType">Navigation capability: Default (land), Naval (sea), or All (both)</param>
        /// <param name="partySpeed">Party speed (use mobileParty.Speed or Campaign.Current.EstimatedAverageLordPartySpeed)</param>
        /// <returns>Estimated hours to travel to nearest castle, or float.MaxValue if unreachable or invalid parameters</returns>
        public static float EstimateTravelTimeHoursToNearestCastle(Settlement from, MobileParty.NavigationType navigationType, float partySpeed)
        {
            if (from == null || partySpeed <= 0f)
                return float.MaxValue;

            float distance = SettlementDistanceCalculator.GetPathingDistanceToNearestCastle(from, navigationType);
            if (distance >= float.MaxValue)
                return float.MaxValue;

            return distance / partySpeed;
        }

        /// <summary>
        /// Estimates travel time in hours to the nearest village from the specified settlement.
        /// </summary>
        /// <param name="from">The source settlement to travel from</param>
        /// <param name="navigationType">Navigation capability: Default (land), Naval (sea), or All (both)</param>
        /// <param name="partySpeed">Party speed (use mobileParty.Speed or Campaign.Current.EstimatedAverageLordPartySpeed)</param>
        /// <returns>Estimated hours to travel to nearest village, or float.MaxValue if unreachable or invalid parameters</returns>
        public static float EstimateTravelTimeHoursToNearestVillage(Settlement from, MobileParty.NavigationType navigationType, float partySpeed)
        {
            if (from == null || partySpeed <= 0f)
                return float.MaxValue;

            float distance = SettlementDistanceCalculator.GetPathingDistanceToNearestVillage(from, navigationType);
            if (distance >= float.MaxValue)
                return float.MaxValue;

            return distance / partySpeed;
        }

        /// <summary>
        /// Estimates travel time in hours to the nearest fortification (castle or town) from the specified settlement.
        /// </summary>
        /// <param name="from">The source settlement to travel from</param>
        /// <param name="navigationType">Navigation capability: Default (land), Naval (sea), or All (both)</param>
        /// <param name="partySpeed">Party speed (use mobileParty.Speed or Campaign.Current.EstimatedAverageLordPartySpeed)</param>
        /// <returns>Estimated hours to travel to nearest fortification, or float.MaxValue if unreachable or invalid parameters</returns>
        public static float EstimateTravelTimeHoursToNearestFortification(Settlement from, MobileParty.NavigationType navigationType, float partySpeed)
        {
            if (from == null || partySpeed <= 0f)
                return float.MaxValue;

            float distance = SettlementDistanceCalculator.GetPathingDistanceToNearestFortification(from, navigationType);
            if (distance >= float.MaxValue)
                return float.MaxValue;

            return distance / partySpeed;
        }

        #endregion

        #region Travel Time in Days

        /// <summary>
        /// Estimates travel time in days to the nearest hideout from the specified settlement.
        /// </summary>
        /// <param name="from">The source settlement to travel from</param>
        /// <param name="navigationType">Navigation capability: Default (land), Naval (sea), or All (both)</param>
        /// <param name="partySpeed">Party speed (use mobileParty.Speed or Campaign.Current.EstimatedAverageLordPartySpeed)</param>
        /// <returns>Estimated days to travel to nearest hideout, or float.MaxValue if unreachable or invalid parameters</returns>
        public static float EstimateTravelTimeDaysToNearestHideout(Settlement from, MobileParty.NavigationType navigationType, float partySpeed)
        {
            float hours = EstimateTravelTimeHoursToNearestHideout(from, navigationType, partySpeed);
            if (hours >= float.MaxValue)
                return float.MaxValue;

            return hours / (float)CampaignTime.HoursInDay;
        }

        /// <summary>
        /// Estimates travel time in days to the nearest town from the specified settlement.
        /// </summary>
        /// <param name="from">The source settlement to travel from</param>
        /// <param name="navigationType">Navigation capability: Default (land), Naval (sea), or All (both)</param>
        /// <param name="partySpeed">Party speed (use mobileParty.Speed or Campaign.Current.EstimatedAverageLordPartySpeed)</param>
        /// <returns>Estimated days to travel to nearest town, or float.MaxValue if unreachable or invalid parameters</returns>
        public static float EstimateTravelTimeDaysToNearestTown(Settlement from, MobileParty.NavigationType navigationType, float partySpeed)
        {
            float hours = EstimateTravelTimeHoursToNearestTown(from, navigationType, partySpeed);
            if (hours >= float.MaxValue)
                return float.MaxValue;

            return hours / (float)CampaignTime.HoursInDay;
        }

        /// <summary>
        /// Estimates travel time in days to the nearest castle from the specified settlement.
        /// </summary>
        /// <param name="from">The source settlement to travel from</param>
        /// <param name="navigationType">Navigation capability: Default (land), Naval (sea), or All (both)</param>
        /// <param name="partySpeed">Party speed (use mobileParty.Speed or Campaign.Current.EstimatedAverageLordPartySpeed)</param>
        /// <returns>Estimated days to travel to nearest castle, or float.MaxValue if unreachable or invalid parameters</returns>
        public static float EstimateTravelTimeDaysToNearestCastle(Settlement from, MobileParty.NavigationType navigationType, float partySpeed)
        {
            float hours = EstimateTravelTimeHoursToNearestCastle(from, navigationType, partySpeed);
            if (hours >= float.MaxValue)
                return float.MaxValue;

            return hours / (float)CampaignTime.HoursInDay;
        }

        /// <summary>
        /// Estimates travel time in days to the nearest village from the specified settlement.
        /// </summary>
        /// <param name="from">The source settlement to travel from</param>
        /// <param name="navigationType">Navigation capability: Default (land), Naval (sea), or All (both)</param>
        /// <param name="partySpeed">Party speed (use mobileParty.Speed or Campaign.Current.EstimatedAverageLordPartySpeed)</param>
        /// <returns>Estimated days to travel to nearest village, or float.MaxValue if unreachable or invalid parameters</returns>
        public static float EstimateTravelTimeDaysToNearestVillage(Settlement from, MobileParty.NavigationType navigationType, float partySpeed)
        {
            float hours = EstimateTravelTimeHoursToNearestVillage(from, navigationType, partySpeed);
            if (hours >= float.MaxValue)
                return float.MaxValue;

            return hours / (float)CampaignTime.HoursInDay;
        }

        /// <summary>
        /// Estimates travel time in days to the nearest fortification (castle or town) from the specified settlement.
        /// </summary>
        /// <param name="from">The source settlement to travel from</param>
        /// <param name="navigationType">Navigation capability: Default (land), Naval (sea), or All (both)</param>
        /// <param name="partySpeed">Party speed (use mobileParty.Speed or Campaign.Current.EstimatedAverageLordPartySpeed)</param>
        /// <returns>Estimated days to travel to nearest fortification, or float.MaxValue if unreachable or invalid parameters</returns>
        public static float EstimateTravelTimeDaysToNearestFortification(Settlement from, MobileParty.NavigationType navigationType, float partySpeed)
        {
            float hours = EstimateTravelTimeHoursToNearestFortification(from, navigationType, partySpeed);
            if (hours >= float.MaxValue)
                return float.MaxValue;

            return hours / (float)CampaignTime.HoursInDay;
        }

        /// <summary>
        /// Estimates travel time in days to the nearest hideout using average lord party speed.
        /// </summary>
        /// <param name="from">The source settlement to travel from</param>
        /// <param name="navigationType">Navigation capability: Default (land), Naval (sea), or All (both)</param>
        /// <returns>Estimated days to travel to nearest hideout, or float.MaxValue if unreachable</returns>
        public static float EstimateTravelTimeDaysToNearestHideout(Settlement from, MobileParty.NavigationType navigationType = MobileParty.NavigationType.Default)
        {
            return EstimateTravelTimeDaysToNearestHideout(from, navigationType, Campaign.Current.EstimatedAverageLordPartySpeed);
        }

        /// <summary>
        /// Estimates travel time in days to the nearest town using average lord party speed.
        /// </summary>
        /// <param name="from">The source settlement to travel from</param>
        /// <param name="navigationType">Navigation capability: Default (land), Naval (sea), or All (both)</param>
        /// <returns>Estimated days to travel to nearest town, or float.MaxValue if unreachable</returns>
        public static float EstimateTravelTimeDaysToNearestTown(Settlement from, MobileParty.NavigationType navigationType = MobileParty.NavigationType.Default)
        {
            return EstimateTravelTimeDaysToNearestTown(from, navigationType, Campaign.Current.EstimatedAverageLordPartySpeed);
        }

        /// <summary>
        /// Estimates travel time in days to the nearest castle using average lord party speed.
        /// </summary>
        /// <param name="from">The source settlement to travel from</param>
        /// <param name="navigationType">Navigation capability: Default (land), Naval (sea), or All (both)</param>
        /// <returns>Estimated days to travel to nearest castle, or float.MaxValue if unreachable</returns>
        public static float EstimateTravelTimeDaysToNearestCastle(Settlement from, MobileParty.NavigationType navigationType = MobileParty.NavigationType.Default)
        {
            return EstimateTravelTimeDaysToNearestCastle(from, navigationType, Campaign.Current.EstimatedAverageLordPartySpeed);
        }

        /// <summary>
        /// Estimates travel time in days to the nearest village using average lord party speed.
        /// </summary>
        /// <param name="from">The source settlement to travel from</param>
        /// <param name="navigationType">Navigation capability: Default (land), Naval (sea), or All (both)</param>
        /// <returns>Estimated days to travel to nearest village, or float.MaxValue if unreachable</returns>
        public static float EstimateTravelTimeDaysToNearestVillage(Settlement from, MobileParty.NavigationType navigationType = MobileParty.NavigationType.Default)
        {
            return EstimateTravelTimeDaysToNearestVillage(from, navigationType, Campaign.Current.EstimatedAverageLordPartySpeed);
        }

        /// <summary>
        /// Estimates travel time in days to the nearest fortification (castle or town) using average lord party speed.
        /// </summary>
        /// <param name="from">The source settlement to travel from</param>
        /// <param name="navigationType">Navigation capability: Default (land), Naval (sea), or All (both)</param>
        /// <returns>Estimated days to travel to nearest fortification, or float.MaxValue if unreachable</returns>
        public static float EstimateTravelTimeDaysToNearestFortification(Settlement from, MobileParty.NavigationType navigationType = MobileParty.NavigationType.Default)
        {
            return EstimateTravelTimeDaysToNearestFortification(from, navigationType, Campaign.Current.EstimatedAverageLordPartySpeed);
        }

        #endregion
    }
}
