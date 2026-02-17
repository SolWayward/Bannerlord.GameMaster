using System;
using System.Collections.Generic;
using Bannerlord.GameMaster.Common;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Encyclopedia;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;

namespace Bannerlord.GameMaster.Party
{
    /// <summary>
    /// Static launcher responsible for creating PartyScreenLogic + PartyScreenLogicInitializationData,
    /// wiring them together, and pushing the PartyState to open the native party screen UI.
    /// The native GauntletPartyScreen auto-registers for PartyState via [GameStateScreen(typeof(PartyState))].
    /// <br /><br />
    /// Supports two modes: <br />
    /// - Party-to-Party: Two real parties side by side with full troop transfer <br />
    /// - Discard/All Troops: Right side is a real party, left side shows all game troops (100 each)
    /// <br /><br />
    /// Party leaders and the player character are never transferable. <br />
    /// Heroes discarded in discard mode are set to Fugitive state so they re-enter the game world.
    /// </summary>
    public static class PartyUILauncher
    {
        private const int TroopCountPerType = 100;

        /// MARK: OpenPartyToParty
        /// <summary>
        /// Opens the party screen with two real parties side by side for troop transfer.
        /// Both member and prisoner rosters are available for transfer.
        /// Blocks the player character and both party leaders from being transferred.
        /// </summary>
        /// <param name="rightParty">Party shown on the right side</param>
        /// <param name="leftParty">Party shown on the left side</param>
        /// <param name="onComplete">Optional callback when party screen closes</param>
        /// <returns>BLGMResult indicating success or failure</returns>
        public static BLGMResult OpenPartyToParty(MobileParty rightParty, MobileParty leftParty, Action onComplete = null)
        {
            if (rightParty == null)
            {
                return BLGMResult.Error("OpenPartyToParty() failed, rightParty cannot be null",
                    new ArgumentNullException(nameof(rightParty))).Log();
            }

            if (leftParty == null)
            {
                return BLGMResult.Error("OpenPartyToParty() failed, leftParty cannot be null",
                    new ArgumentNullException(nameof(leftParty))).Log();
            }

            CharacterObject rightLeaderChar = rightParty.LeaderHero?.CharacterObject;
            CharacterObject leftLeaderChar = leftParty.LeaderHero?.CharacterObject;

            PartyScreenLogic partyScreenLogic = new();
            PartyScreenLogicInitializationData initData = new()
            {
                RightOwnerParty = rightParty.Party,
                LeftOwnerParty = leftParty.Party,
                RightMemberRoster = rightParty.MemberRoster,
                RightPrisonerRoster = rightParty.PrisonRoster,
                LeftMemberRoster = leftParty.MemberRoster,
                LeftPrisonerRoster = leftParty.PrisonRoster,
                RightLeaderHero = rightParty.LeaderHero,
                LeftLeaderHero = leftParty.LeaderHero,
                RightPartyName = rightParty.Name,
                LeftPartyName = leftParty.Name,
                RightPartyMembersSizeLimit = rightParty.Party.PartySizeLimit,
                RightPartyPrisonersSizeLimit = rightParty.Party.PrisonerSizeLimit,
                LeftPartyMembersSizeLimit = leftParty.Party.PartySizeLimit,
                LeftPartyPrisonersSizeLimit = leftParty.Party.PrisonerSizeLimit,
                TroopTransferableDelegate = new IsTroopTransferableDelegate(
                    (character, type, side, leftOwner) =>
                        !character.IsPlayerCharacter &&
                        character != rightLeaderChar &&
                        character != leftLeaderChar),
                PartyPresentationDoneButtonDelegate = new PartyPresentationDoneButtonDelegate(DefaultDoneHandler),
                PartyPresentationDoneButtonConditionDelegate = null,
                PartyPresentationCancelButtonActivateDelegate = null,
                PartyPresentationCancelButtonDelegate = null,
                PartyScreenClosedDelegate = onComplete != null
                    ? new PartyScreenClosedDelegate(
                        (leftOwnerParty, leftMemberRoster, leftPrisonRoster,
                         rightOwnerParty, rightMemberRoster, rightPrisonRoster, fromCancel) =>
                        {
                            onComplete.Invoke();
                        })
                    : null,
                IsDismissMode = false,
                IsTroopUpgradesDisabled = false,
                Header = null,
                TransferHealthiesGetWoundedsFirst = false,
                ShowProgressBar = false,
                MemberTransferState = PartyScreenLogic.TransferState.Transferable,
                PrisonerTransferState = PartyScreenLogic.TransferState.Transferable,
                AccompanyingTransferState = PartyScreenLogic.TransferState.Transferable
            };

            partyScreenLogic.Initialize(initData);
            PushPartyState(partyScreenLogic, false, PartyScreenHelper.PartyScreenMode.Normal);
            return BLGMResult.Success($"Opened party editor: {rightParty.Name} (right) vs {leftParty.Name} (left)");
        }

        /// MARK: OpenWithDiscardRoster
        /// <summary>
        /// Opens the party screen with the right side showing a real party and the left side
        /// populated with all game troops (100 of each), similar to the native cheat mode pattern.
        /// The left side acts as a discard/source roster for adding troops to the party.
        /// <br /><br />
        /// Heroes removed from the right party (discarded) are set to Fugitive state via
        /// MakeHeroFugitiveAction so the game's TeleportationCampaignBehavior can re-place them
        /// at a nearby settlement. Blocks the player character and party leader from transfer.
        /// </summary>
        /// <param name="rightParty">Party shown on the right side</param>
        /// <param name="onComplete">Optional callback when party screen closes</param>
        /// <returns>BLGMResult indicating success or failure</returns>
        public static BLGMResult OpenWithDiscardRoster(MobileParty rightParty, Action onComplete = null)
        {
            if (rightParty == null)
            {
                return BLGMResult.Error("OpenWithDiscardRoster() failed, rightParty cannot be null",
                    new ArgumentNullException(nameof(rightParty))).Log();
            }

            // Snapshot heroes currently in the right party before the screen opens,
            // so we can detect which heroes were discarded when the screen closes
            HashSet<Hero> initialHeroes = SnapshotRosterHeroes(rightParty.MemberRoster);

            CharacterObject rightLeaderChar = rightParty.LeaderHero?.CharacterObject;
            TroopRoster allTroopsRoster = GetAllTroopsRoster();

            PartyScreenLogic partyScreenLogic = new();
            PartyScreenLogicInitializationData initData = new()
            {
                RightOwnerParty = rightParty.Party,
                LeftOwnerParty = null,
                RightMemberRoster = rightParty.MemberRoster,
                RightPrisonerRoster = rightParty.PrisonRoster,
                LeftMemberRoster = allTroopsRoster,
                LeftPrisonerRoster = TroopRoster.CreateDummyTroopRoster(),
                RightLeaderHero = rightParty.LeaderHero,
                LeftLeaderHero = null,
                RightPartyName = rightParty.Name,
                LeftPartyName = null,
                RightPartyMembersSizeLimit = rightParty.Party.PartySizeLimit,
                RightPartyPrisonersSizeLimit = rightParty.Party.PrisonerSizeLimit,
                LeftPartyMembersSizeLimit = 0,
                LeftPartyPrisonersSizeLimit = 0,
                TroopTransferableDelegate = new IsTroopTransferableDelegate(
                    (character, type, side, leftOwner) =>
                        !character.IsPlayerCharacter &&
                        character != rightLeaderChar),
                PartyPresentationDoneButtonDelegate = new PartyPresentationDoneButtonDelegate(DefaultDoneHandler),
                PartyPresentationDoneButtonConditionDelegate = null,
                PartyPresentationCancelButtonActivateDelegate = null,
                PartyPresentationCancelButtonDelegate = null,
                PartyScreenClosedDelegate = new PartyScreenClosedDelegate(
                    (leftOwnerParty, leftMemberRoster, leftPrisonRoster,
                     rightOwnerParty, rightMemberRoster, rightPrisonRoster, fromCancel) =>
                    {
                        if (!fromCancel)
                        {
                            ApplyFugitiveStateToDiscardedHeroes(initialHeroes, rightMemberRoster);
                        }

                        onComplete?.Invoke();
                    }),
                IsDismissMode = true,
                IsTroopUpgradesDisabled = false,
                Header = null,
                TransferHealthiesGetWoundedsFirst = false,
                ShowProgressBar = false,
                MemberTransferState = PartyScreenLogic.TransferState.Transferable,
                PrisonerTransferState = PartyScreenLogic.TransferState.Transferable,
                AccompanyingTransferState = PartyScreenLogic.TransferState.NotTransferable
            };

            partyScreenLogic.Initialize(initData);
            PushPartyState(partyScreenLogic, false, PartyScreenHelper.PartyScreenMode.Normal);
            return BLGMResult.Success($"Opened party editor for {rightParty.Name} with all troops roster");
        }

        #region Internal Helpers

        /// MARK: SnapshotRosterHeroes
        /// <summary>
        /// Creates a snapshot of all hero objects currently in a troop roster.
        /// Used to detect which heroes were removed when the party screen closes.
        /// </summary>
        private static HashSet<Hero> SnapshotRosterHeroes(TroopRoster roster)
        {
            HashSet<Hero> heroes = new();

            for (int i = 0; i < roster.Count; i++)
            {
                TroopRosterElement element = roster.GetElementCopyAtIndex(i);
                if (element.Character.IsHero && element.Character.HeroObject != null)
                {
                    heroes.Add(element.Character.HeroObject);
                }
            }

            return heroes;
        }

        /// MARK: ApplyFugitiveState
        /// <summary>
        /// Compares initial roster heroes against the final right roster after the party screen closes.
        /// Any hero that was in the initial roster but not in the final roster was discarded, 
        /// and gets MakeHeroFugitiveAction applied so the game's TeleportationCampaignBehavior
        /// will eventually re-place them at a nearby settlement.
        /// Skips the player character as a safety check.
        /// </summary>
        private static void ApplyFugitiveStateToDiscardedHeroes(HashSet<Hero> initialHeroes, TroopRoster finalRightRoster)
        {
            HashSet<Hero> remainingHeroes = SnapshotRosterHeroes(finalRightRoster);

            foreach (Hero hero in initialHeroes)
            {
                if (!remainingHeroes.Contains(hero) && hero != Hero.MainHero && hero.IsAlive)
                {
                    MakeHeroFugitiveAction.Apply(hero, true);
                }
            }
        }

        /// MARK: GetAllTroopsRoster
        /// <summary>
        /// Creates a TroopRoster containing all valid encyclopedia troops with TroopCountPerType (100) of each.
        /// Mirrors the native GetRosterWithAllGameTroops() pattern but uses a higher count.
        /// Troops are sorted alphabetically by name.
        /// </summary>
        private static TroopRoster GetAllTroopsRoster()
        {
            TroopRoster troopRoster = TroopRoster.CreateDummyTroopRoster();
            List<CharacterObject> validTroops = new();
            EncyclopediaPage pageOf = Campaign.Current.EncyclopediaManager.GetPageOf(typeof(CharacterObject));

            for (int i = 0; i < CharacterObject.All.Count; i++)
            {
                CharacterObject characterObject = CharacterObject.All[i];
                if (pageOf.IsValidEncyclopediaItem(characterObject))
                {
                    validTroops.Add(characterObject);
                }
            }

            validTroops.Sort((CharacterObject a, CharacterObject b) =>
                a.Name.ToString().CompareTo(b.Name.ToString()));

            for (int j = 0; j < validTroops.Count; j++)
            {
                CharacterObject troop = validTroops[j];
                troopRoster.AddToCounts(troop, TroopCountPerType, false, 0, 0, true, -1);
            }

            return troopRoster;
        }

        /// MARK: PushPartyState
        /// <summary>
        /// Creates a PartyState, assigns the logic and configuration, and pushes it.
        /// The native GauntletPartyScreen auto-registers for PartyState.
        /// </summary>
        private static void PushPartyState(PartyScreenLogic logic, bool isDonating,
            PartyScreenHelper.PartyScreenMode mode)
        {
            PartyState partyState = Game.Current.GameStateManager.CreateState<PartyState>();
            partyState.PartyScreenLogic = logic;
            partyState.IsDonating = isDonating;
            partyState.PartyScreenMode = mode;
            Game.Current.GameStateManager.PushState(partyState, 0);
        }

        /// MARK: DefaultDoneHandler
        /// <summary>
        /// Default handler for the Done button that simply accepts all changes.
        /// The native PartyScreenLogic applies roster changes automatically when Done returns true.
        /// </summary>
        private static bool DefaultDoneHandler(TroopRoster leftMemberRoster,
            TroopRoster leftPrisonRoster, TroopRoster rightMemberRoster,
            TroopRoster rightPrisonRoster, FlattenedTroopRoster takenPrisonerRoster,
            FlattenedTroopRoster releasedPrisonerRoster, bool isForced,
            PartyBase leftParty, PartyBase rightParty)
        {
            return true;
        }

        #endregion
    }
}
