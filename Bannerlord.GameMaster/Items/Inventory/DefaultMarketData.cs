using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace Bannerlord.GameMaster.Items.Inventory
{
    /// <summary>
    /// Simple IMarketData implementation that returns base item values.
    /// Used as a fallback when no settlement market data is available.
    /// Mirrors the behavior of the native internal FakeMarketData class.
    /// </summary>
    internal class DefaultMarketData : IMarketData
    {
        /// MARK: GetPrice (ItemObject)
        public int GetPrice(ItemObject item, MobileParty tradingParty, bool isSelling, PartyBase merchantParty)
        {
            return item.Value;
        }

        /// MARK: GetPrice (EquipmentElement)
        public int GetPrice(EquipmentElement itemRosterElement, MobileParty tradingParty, bool isSelling, PartyBase merchantParty)
        {
            return itemRosterElement.ItemValue;
        }
    }
}
