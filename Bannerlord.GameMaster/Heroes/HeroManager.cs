using System;
using System.Reflection;
using Bannerlord.GameMaster.Common;
using Bannerlord.GameMaster.Settlements;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Heroes
{
    public static class HeroManager
    {
        /// <summary>
        /// tries to get a random settlement in this order: From heroes clan > from heroes kingdom > from all settlements
        /// </summary>
        public static Settlement GetBestInitialSettlement(Hero hero)
        {
            Settlement settlement;

            settlement = SettlementManager.GetRandomClanFortification(hero.Clan);
            settlement ??= SettlementManager.GetRandomKingdomFortification(hero.Clan?.Kingdom);
            settlement ??= SettlementManager.GetRandomTown();

            return settlement;
        }

        /// <summary>
        /// Uses reflection to try to the Heroes home settlement directly
        /// </summary>
        /// <returns>BLGM result containing bool if Setting homeSettlement succeeded and a string with details</returns>
        public static BLGMResult TrySetHomeSettlement(Hero hero, Settlement homeSettlement)
        {
            try
            {
                if (HomeSettlementField == null)
                    return new(false, "Could not find _homeSettlement field - game version incompatible");

                HomeSettlementField.SetValue(hero, homeSettlement);
                return new(true, $"Set home settlement for {hero.Name} to {homeSettlement?.Name}");
            }
            catch (Exception ex)
            {
                return new(false, $"Failed to set _homeSettlement for {hero.Name}: {ex.Message}");
            }
        }

        // Cached _homeSettlement field for reflection
        private static readonly FieldInfo HomeSettlementField = typeof(Hero).GetField("_homeSettlement", BindingFlags.NonPublic | BindingFlags.Instance);
    }
}