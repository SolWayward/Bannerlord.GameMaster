using System;
using Bannerlord.GameMaster.Common;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Localization;

namespace Bannerlord.GameMaster.Items.Inventory
{
    /// <summary>
    /// High-level API for opening the native inventory UI with any hero, party, or custom rosters.
    /// Supports opening inventory for heroes from ANY clan by using synthetic TroopRosters
    /// that bypass the native CanSelectHero() player clan restriction.
    /// <br /><br />
    /// The native inventory system uses:
    /// - ItemRoster for item storage on each side
    /// - TroopRoster for hero character switching arrows
    /// - InventoryLogic for transfer/trade logic
    /// - InventoryState (native GameState) + GauntletInventoryScreen (auto-registered)
    /// </summary>
    public class InventoryManager
    {
        private readonly Hero _rightHero;
        private readonly Hero _leftHero;
        private readonly Hero _middleHero;
        private readonly MobileParty _rightParty;
        private readonly MobileParty _leftParty;
        private readonly ItemRoster _leftItemRoster;
        private readonly ItemRoster _rightItemRoster;
        private readonly TroopRoster _rightMemberRoster;
        private readonly TroopRoster _leftMemberRoster;
        private readonly TextObject _leftRosterName;
        private readonly InventoryOpenMode _openMode;

        #region Constructors

        /// MARK: Hero Only
        /// <summary>
        /// Creates an inventory manager for a single hero with discard panel on the left.
        /// The hero is displayed in the middle with full equipment editing.
        /// Works for heroes from ANY clan.
        /// </summary>
        /// <param name="hero">Hero whose equipment and party inventory to display</param>
        public InventoryManager(Hero hero)
        {
            _rightHero = hero;
            _openMode = InventoryOpenMode.SingleHero;
        }

        /// MARK: Hero vs Hero
        /// <summary>
        /// Creates an inventory manager with two heroes' party inventories side by side.
        /// The middleHero is displayed in the center equipment panel.
        /// Both heroes' party items appear on their respective sides.
        /// </summary>
        /// <param name="rightHero">Hero whose party appears on the right side</param>
        /// <param name="leftHero">Hero whose party appears on the left side</param>
        /// <param name="middleHero">Hero shown in the middle equipment view (defaults to leftHero)</param>
        public InventoryManager(Hero rightHero, Hero leftHero, Hero middleHero = null)
        {
            _rightHero = rightHero;
            _leftHero = leftHero;
            _middleHero = middleHero ?? leftHero;
            _openMode = InventoryOpenMode.HeroToHero;
        }

        /// MARK: Party vs Party
        /// <summary>
        /// Creates an inventory manager with two mobile parties side by side.
        /// Party member rosters are used for hero switching arrows.
        /// The right party leader is shown in the middle initially.
        /// </summary>
        /// <param name="rightParty">Party shown on the right side</param>
        /// <param name="leftParty">Party shown on the left side</param>
        public InventoryManager(MobileParty rightParty, MobileParty leftParty)
        {
            _rightParty = rightParty;
            _leftParty = leftParty;
            _openMode = InventoryOpenMode.PartyToParty;
        }

        /// MARK: Custom Rosters
        /// <summary>
        /// Creates an inventory manager with fully custom item and hero rosters.
        /// Maximum flexibility for advanced use cases such as custom stashes,
        /// loot screens, or other non-standard inventory configurations.
        /// </summary>
        /// <param name="leftItemRoster">Items shown on the left panel</param>
        /// <param name="rightItemRoster">Items shown on the right panel</param>
        /// <param name="rightMemberRoster">Heroes available for switching on the right side</param>
        /// <param name="leftRosterName">Label for the left panel</param>
        /// <param name="leftMemberRoster">Heroes available for switching on the left side (optional)</param>
        public InventoryManager(
            ItemRoster leftItemRoster,
            ItemRoster rightItemRoster,
            TroopRoster rightMemberRoster = null,
            TextObject leftRosterName = null,
            TroopRoster leftMemberRoster = null)
        {
            _leftItemRoster = leftItemRoster;
            _rightItemRoster = rightItemRoster;
            _rightMemberRoster = rightMemberRoster;
            _leftRosterName = leftRosterName;
            _leftMemberRoster = leftMemberRoster;
            _openMode = InventoryOpenMode.CustomRosters;
        }

        #endregion

        /// MARK: OpenInventory
        /// <summary>
        /// Opens the native inventory UI based on the constructor configuration.
        /// The native GauntletInventoryScreen auto-registers for InventoryState,
        /// so no custom GameState or ScreenBase is required.
        /// </summary>
        /// <param name="onComplete">Optional callback when inventory closes</param>
        /// <returns>BLGMResult indicating success or failure</returns>
        public BLGMResult OpenInventory(Action onComplete = null)
        {
            switch (_openMode)
            {
                case InventoryOpenMode.SingleHero:
                    return InventoryUILauncher.OpenHeroInventory(_rightHero, onComplete);

                case InventoryOpenMode.HeroToHero:
                    return InventoryUILauncher.OpenHeroToHeroInventory(_rightHero, _leftHero, _middleHero, onComplete);

                case InventoryOpenMode.PartyToParty:
                    return InventoryUILauncher.OpenPartyToPartyInventory(_rightParty, _leftParty, onComplete);

                case InventoryOpenMode.CustomRosters:
                    return InventoryUILauncher.OpenWithCustomRosters(
                        _leftItemRoster,
                        _rightItemRoster,
                        _rightMemberRoster,
                        initialCharacter: null,
                        ownerParty: null,
                        _leftRosterName,
                        _leftMemberRoster,
                        isTrading: false,
                        onComplete: onComplete);

                default:
                {
                    BLGMResult.Error("OpenInventory() failed, unknown inventory open mode",
                        new InvalidOperationException($"Unhandled InventoryOpenMode: {_openMode}")).Log();
                    return null;
                }
            }
        }

        /// <summary>
        /// Defines the mode of opening the inventory, determined by which constructor was used.
        /// </summary>
        private enum InventoryOpenMode
        {
            SingleHero,
            HeroToHero,
            PartyToParty,
            CustomRosters
        }
    }
}
