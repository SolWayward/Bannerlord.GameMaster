using System;
using Bannerlord.GameMaster.Common;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace Bannerlord.GameMaster.Items.Inventory
{
    /// <summary>
    /// Static launcher responsible for creating InventoryState + InventoryLogic,
    /// wiring them together, and pushing the state to open the native inventory UI.
    /// Supports opening inventory for ANY hero from ANY clan by using synthetic TroopRosters.
    /// </summary>
    public static class InventoryUILauncher
    {
        /// MARK: OpenHeroInventory
        /// <summary>
        /// Opens the inventory UI for a single hero with an empty discard panel on the left.
        /// The hero is shown in the middle with full equipment editing regardless of clan.
        /// Uses a synthetic TroopRoster so no clan restrictions apply.
        /// </summary>
        /// <param name="hero">The hero whose equipment to display in the middle</param>
        /// <param name="onComplete">Optional callback when inventory closes</param>
        /// <returns>BLGMResult indicating success or failure</returns>
        public static BLGMResult OpenHeroInventory(Hero hero, Action onComplete = null)
        {
            if (hero == null)
            {
                BLGMResult.Error("OpenHeroInventory() failed, hero cannot be null",
                    new ArgumentNullException(nameof(hero))).Log();
                return null;
            }

            MobileParty party = hero.PartyBelongedTo;
            if (party == null)
                return BLGMResult.Error($"OpenHeroInventory() failed, {hero.Name} is not in a party");

            TroopRoster syntheticRoster = CreateSyntheticHeroRoster(hero);
            InventoryLogic logic = new(party, hero.CharacterObject, null);
            logic.Initialize(
                new ItemRoster(),               // left: empty discard roster
                party.ItemRoster,               // right: hero's party items
                syntheticRoster,                // synthetic roster bypasses CanSelectHero clan check
                false,                          // isTrading
                true,                           // isSpecialActionsPermitted
                hero.CharacterObject,           // initial character shown in middle
                InventoryScreenHelper.InventoryCategoryType.None,
                GetMarketData(),
                false,                          // useBasePrices
                InventoryScreenHelper.InventoryMode.Default,
                new TextObject("{=02c5bQSM}Discard"),
                null,                           // leftMemberRoster
                null                            // capacityData
            );

            PushInventoryState(logic, InventoryScreenHelper.InventoryMode.Default, onComplete);
            return BLGMResult.Success($"Opened inventory for {hero.Name}");
        }

        /// MARK: OpenHeroToHeroInventory
        /// <summary>
        /// Opens the inventory UI with two heroes' party inventories side by side.
        /// The middleHero is displayed in the center equipment panel regardless of clan.
        /// Both parties' items are shown on their respective sides.
        /// </summary>
        /// <param name="rightHero">Hero whose party appears on the right side</param>
        /// <param name="leftHero">Hero whose party appears on the left side</param>
        /// <param name="middleHero">Hero shown in the middle equipment panel (defaults to rightHero if null)</param>
        /// <param name="onComplete">Optional callback when inventory closes</param>
        /// <returns>BLGMResult indicating success or failure</returns>
        public static BLGMResult OpenHeroToHeroInventory(Hero rightHero, Hero leftHero, Hero middleHero = null, Action onComplete = null)
        {
            if (rightHero == null)
            {
                BLGMResult.Error("OpenHeroToHeroInventory() failed, rightHero cannot be null",
                    new ArgumentNullException(nameof(rightHero))).Log();
                return null;
            }

            if (leftHero == null)
            {
                BLGMResult.Error("OpenHeroToHeroInventory() failed, leftHero cannot be null",
                    new ArgumentNullException(nameof(leftHero))).Log();
                return null;
            }

            MobileParty rightParty = rightHero.PartyBelongedTo;
            MobileParty leftParty = leftHero.PartyBelongedTo;

            if (rightParty == null)
                return BLGMResult.Error($"OpenHeroToHeroInventory() failed, {rightHero.Name} is not in a party");

            if (leftParty == null)
                return BLGMResult.Error($"OpenHeroToHeroInventory() failed, {leftHero.Name} is not in a party");

            Hero initialHero = middleHero ?? rightHero;

            // If the middle hero is NOT from the player clan, use a synthetic roster with ONLY the
            // middle hero. This prevents SPInventoryVM's dropdown from finding a player-clan hero
            // and overriding the InitialEquipmentCharacter. CanSelectHero() will fail the clan check,
            // the dropdown stays empty, no arrows appear, and the hero is locked in the middle.
            // If the middle hero IS from the player clan, use the rightHero roster so normal
            // hero-switching arrows work.
            bool isNonPlayerClanMiddle = initialHero.Clan != Clan.PlayerClan;
            TroopRoster rightRoster = CreateSyntheticHeroRoster(isNonPlayerClanMiddle ? initialHero : rightHero);
            TroopRoster leftRoster = CreateSyntheticHeroRoster(leftHero);

            InventoryLogic logic = new(rightParty, rightHero.CharacterObject, leftParty.Party);
            logic.Initialize(
                leftParty.ItemRoster,           // left: other party items
                rightParty.ItemRoster,          // right: main hero's party items
                rightRoster,                    // right side hero roster
                false,                          // isTrading
                false,                          // isSpecialActionsPermitted
                initialHero.CharacterObject,    // initial character shown in middle
                InventoryScreenHelper.InventoryCategoryType.None,
                GetMarketData(),
                false,
                InventoryScreenHelper.InventoryMode.Default,
                leftHero.Name,                  // leftRosterName
                leftRoster,                     // left side hero roster
                null
            );

            PushInventoryState(logic, InventoryScreenHelper.InventoryMode.Default, onComplete);
            return BLGMResult.Success($"Opened inventory: {rightHero.Name} (right) vs {leftHero.Name} (left)");
        }

        /// MARK: OpenPartyToPartyInventory
        /// <summary>
        /// Opens the inventory UI with two parties side by side.
        /// Uses party member rosters for hero switching arrows.
        /// The right party leader is shown in the middle initially.
        /// </summary>
        /// <param name="rightParty">Party shown on the right side</param>
        /// <param name="leftParty">Party shown on the left side</param>
        /// <param name="onComplete">Optional callback when inventory closes</param>
        /// <returns>BLGMResult indicating success or failure</returns>
        public static BLGMResult OpenPartyToPartyInventory(MobileParty rightParty, MobileParty leftParty, Action onComplete = null)
        {
            if (rightParty == null)
            {
                BLGMResult.Error("OpenPartyToPartyInventory() failed, rightParty cannot be null",
                    new ArgumentNullException(nameof(rightParty))).Log();
                return null;
            }

            if (leftParty == null)
            {
                BLGMResult.Error("OpenPartyToPartyInventory() failed, leftParty cannot be null",
                    new ArgumentNullException(nameof(leftParty))).Log();
                return null;
            }

            Hero rightLeader = rightParty.LeaderHero;
            CharacterObject initialChar = rightLeader?.CharacterObject;

            InventoryLogic logic = new(rightParty, initialChar, leftParty.Party);
            logic.Initialize(
                leftParty.ItemRoster,
                rightParty.ItemRoster,
                rightParty.MemberRoster,
                false,
                false,
                initialChar,
                InventoryScreenHelper.InventoryCategoryType.None,
                GetMarketData(),
                false,
                InventoryScreenHelper.InventoryMode.Default,
                leftParty.Name,
                leftParty.MemberRoster,
                null
            );

            PushInventoryState(logic, InventoryScreenHelper.InventoryMode.Default, onComplete);
            return BLGMResult.Success($"Opened inventory: {rightParty.Name} (right) vs {leftParty.Name} (left)");
        }

        /// MARK: OpenWithCustomRosters
        /// <summary>
        /// Opens the inventory UI with fully custom item rosters and hero rosters.
        /// Maximum flexibility for advanced use cases.
        /// </summary>
        /// <param name="leftItemRoster">Items shown on the left panel</param>
        /// <param name="rightItemRoster">Items shown on the right panel</param>
        /// <param name="rightMemberRoster">Heroes available for switching on the right side</param>
        /// <param name="initialCharacter">Hero shown in the middle equipment panel</param>
        /// <param name="ownerParty">The owner party for the right side (can be null)</param>
        /// <param name="leftRosterName">Label displayed on the left panel</param>
        /// <param name="leftMemberRoster">Heroes available for switching on the left side (optional)</param>
        /// <param name="isTrading">Whether to enable buy/sell pricing</param>
        /// <param name="mode">Inventory mode (Default, Trade, Loot, Stash, Warehouse)</param>
        /// <param name="onComplete">Optional callback when inventory closes</param>
        /// <returns>BLGMResult indicating success or failure</returns>
        public static BLGMResult OpenWithCustomRosters(
            ItemRoster leftItemRoster,
            ItemRoster rightItemRoster,
            TroopRoster rightMemberRoster,
            CharacterObject initialCharacter,
            MobileParty ownerParty = null,
            TextObject leftRosterName = null,
            TroopRoster leftMemberRoster = null,
            bool isTrading = false,
            InventoryScreenHelper.InventoryMode mode = InventoryScreenHelper.InventoryMode.Default,
            Action onComplete = null)
        {
            if (rightItemRoster == null)
            {
                BLGMResult.Error("OpenWithCustomRosters() failed, rightItemRoster cannot be null",
                    new ArgumentNullException(nameof(rightItemRoster))).Log();
                return null;
            }

            ItemRoster leftRoster = leftItemRoster ?? new ItemRoster();
            CharacterObject ownerChar = ownerParty?.LeaderHero?.CharacterObject;

            InventoryLogic logic = new(ownerParty, ownerChar, null);
            logic.Initialize(
                leftRoster,
                rightItemRoster,
                rightMemberRoster,
                isTrading,
                !isTrading,                     // special actions when not trading
                initialCharacter,
                InventoryScreenHelper.InventoryCategoryType.None,
                GetMarketData(),
                false,
                mode,
                leftRosterName,
                leftMemberRoster,
                null
            );

            PushInventoryState(logic, mode, onComplete);
            return BLGMResult.Success("Opened inventory with custom rosters");
        }

        #region Internal Helpers

        /// MARK: PushInventoryState
        /// <summary>
        /// Creates an InventoryState, assigns the logic and delegates, and pushes it.
        /// The native GauntletInventoryScreen auto-registers for InventoryState.
        /// </summary>
        private static void PushInventoryState(InventoryLogic logic, InventoryScreenHelper.InventoryMode mode, Action onComplete)
        {
            InventoryState state = Game.Current.GameStateManager.CreateState<InventoryState>();
            state.InventoryLogic = logic;
            state.InventoryMode = mode;
            state.DoneLogicExtrasDelegate = onComplete;

            // Temporarily bypass the "hero not met" check so HeroViewModel.FillFrom() populates
            // the 3D model data instead of skipping it when IsHeroInformationHidden returns true.
            // PushState is synchronous: SPInventoryVM constructor -> FillFrom() runs in-stack,
            // so the flag is safely restored before any other code can observe it.
            DefaultInformationRestrictionModel restrictionModel =
                Campaign.Current?.Models?.InformationRestrictionModel as DefaultInformationRestrictionModel;
            bool previousValue = restrictionModel?.IsDisabledByCheat ?? false;

            try
            {
                if (restrictionModel != null)
                    restrictionModel.IsDisabledByCheat = true;

                Game.Current.GameStateManager.PushState(state, 0);
            }
            finally
            {
                if (restrictionModel != null)
                    restrictionModel.IsDisabledByCheat = previousValue;
            }
        }

        /// MARK: CreateSyntheticHeroRoster
        /// <summary>
        /// Creates a synthetic TroopRoster containing only the specified hero.
        /// This bypasses the CanSelectHero() clan check in SPInventoryVM because
        /// the hero is set as InitialEquipmentCharacter BEFORE CanSelectHero runs,
        /// and CanSelectHero only affects the character switching arrows, not the
        /// initial middle character display or equipment editing capability.
        /// </summary>
        private static TroopRoster CreateSyntheticHeroRoster(Hero hero)
        {
            TroopRoster roster = TroopRoster.CreateDummyTroopRoster();
            roster.AddToCounts(hero.CharacterObject, 1, false, 0, 0, true, -1);
            return roster;
        }

        /// MARK: GetMarketData
        /// <summary>
        /// Gets market data for pricing. Uses current settlement if available,
        /// nearest town otherwise, or falls back to DefaultMarketData (base item values).
        /// Mirrors the native InventoryScreenHelper.GetCurrentMarketDataForPlayer() pattern.
        /// </summary>
        private static IMarketData GetMarketData()
        {
            if (Campaign.Current?.GameMode != CampaignGameMode.Campaign)
                return new DefaultMarketData();

            Settlement settlement = MobileParty.MainParty?.CurrentSettlement;
            if (settlement == null)
            {
                Town nearestTown = SettlementHelper.FindNearestTownToMobileParty(MobileParty.MainParty, MobileParty.NavigationType.All, null);
                settlement = nearestTown?.Settlement;
            }

            if (settlement != null)
            {
                if (settlement.IsVillage)
                    return settlement.Village.MarketData;
                if (settlement.IsTown)
                    return settlement.Town.MarketData;
            }

            return new DefaultMarketData();
        }

        #endregion
    }
}
