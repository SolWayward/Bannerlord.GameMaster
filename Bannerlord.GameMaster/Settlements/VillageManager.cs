using Bannerlord.GameMaster.Common;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.GameMaster.Settlements
{
    /// <summary>
    /// Manages Villages for BLGM actions.<br/>
    /// Remaining Village logic should be refactored out of commands to here or similar classes
    /// </summary>
    public static class VillageManager
    {
        /// <inheritdoc cref="VillageExtensions.SetBoundSettlement"/>
        public static BLGMResult SetBoundSettlement(Village village, Settlement boundSettlement) => village.SetBoundSettlement(boundSettlement);

        /// <inheritdoc cref="VillageExtensions.SetTradeBoundSettlement"/>
        public static BLGMResult SetTradeBoundSettlement(Village village, Settlement tradeBoundSettlement) => village.SetTradeBoundSettlement(tradeBoundSettlement);
    }
}