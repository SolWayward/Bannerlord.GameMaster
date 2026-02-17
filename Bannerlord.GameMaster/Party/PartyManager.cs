using System;
using Bannerlord.GameMaster.Common;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;

namespace Bannerlord.GameMaster.Party
{
    /// <summary>
    /// High-level API for opening the native party screen UI with any hero or party.
    /// Provides convenience methods that resolve heroes to their parties and delegate
    /// to PartyUILauncher for the actual native screen wiring.
    /// <br /><br />
    /// The native party system uses: <br />
    /// - TroopRoster for member and prisoner rosters on each side <br />
    /// - PartyScreenLogic for troop transfer, upgrade, and recruitment logic <br />
    /// - PartyState (native GameState) + GauntletPartyScreen (auto-registered)
    /// <br /><br />
    /// Two modes are supported: <br />
    /// - Discard mode: Right side is a real party, left side has all game troops (100 each) <br />
    /// - Party-to-Party mode: Two real parties side by side for troop exchange
    /// </summary>
    public static class PartyManager
    {
        /// MARK: Hero + Hero
        /// <summary>
        /// Opens the party editor with two heroes' parties side by side.
        /// If leftSideHero is null, same party as rightSideHero, or has no party,
        /// falls back to discard mode with all game troops on the left.
        /// </summary>
        /// <param name="rightSideHero">Hero whose party appears on the right side (required)</param>
        /// <param name="leftSideHero">Hero whose party appears on the left side (null for discard mode)</param>
        /// <param name="onComplete">Optional callback when party screen closes</param>
        /// <returns>BLGMResult indicating success or failure</returns>
        public static BLGMResult OpenPartyEditor(Hero rightSideHero, Hero leftSideHero, Action onComplete = null)
        {
            if (rightSideHero == null)
            {
                return BLGMResult.Error("OpenPartyEditor() failed, rightSideHero cannot be null",
                    new ArgumentNullException(nameof(rightSideHero))).Log();
            }

            MobileParty rightParty = rightSideHero.PartyBelongedTo;
            if (rightParty == null)
            {
                return BLGMResult.Error(
                    $"OpenPartyEditor() failed, {rightSideHero.Name} is not in a party").Log();
            }

            // Null left hero -> discard mode
            if (leftSideHero == null)
            {
                return PartyUILauncher.OpenWithDiscardRoster(rightParty, onComplete);
            }

            MobileParty leftParty = leftSideHero.PartyBelongedTo;

            // Left hero has no party -> fall back to discard mode
            if (leftParty == null)
            {
                return PartyUILauncher.OpenWithDiscardRoster(rightParty, onComplete);
            }

            // Same party -> fall back to discard mode to avoid roster corruption
            if (rightParty == leftParty)
            {
                return PartyUILauncher.OpenWithDiscardRoster(rightParty, onComplete);
            }

            return PartyUILauncher.OpenPartyToParty(rightParty, leftParty, onComplete);
        }

        /// MARK: Hero Only
        /// <summary>
        /// Opens the party editor for a single hero's party with all game troops on the left (discard mode).
        /// </summary>
        /// <param name="rightSideHero">Hero whose party to edit (required)</param>
        /// <param name="onComplete">Optional callback when party screen closes</param>
        /// <returns>BLGMResult indicating success or failure</returns>
        public static BLGMResult OpenPartyEditor(Hero rightSideHero, Action onComplete = null)
        {
            return OpenPartyEditor(rightSideHero, null, onComplete);
        }

        /// MARK: Party + Party
        /// <summary>
        /// Opens the party editor with two mobile parties side by side for troop transfer.
        /// If leftSideParty is null or the same as rightSideParty, falls back to discard mode.
        /// </summary>
        /// <param name="rightSideParty">Party shown on the right side (required)</param>
        /// <param name="leftSideParty">Party shown on the left side (null for discard mode)</param>
        /// <param name="onComplete">Optional callback when party screen closes</param>
        /// <returns>BLGMResult indicating success or failure</returns>
        public static BLGMResult OpenPartyEditor(MobileParty rightSideParty, MobileParty leftSideParty, Action onComplete = null)
        {
            if (rightSideParty == null)
            {
                return BLGMResult.Error("OpenPartyEditor() failed, rightSideParty cannot be null",
                    new ArgumentNullException(nameof(rightSideParty))).Log();
            }

            // Null left party -> discard mode
            if (leftSideParty == null)
            {
                return PartyUILauncher.OpenWithDiscardRoster(rightSideParty, onComplete);
            }

            // Same party -> fall back to discard mode to avoid roster corruption
            if (rightSideParty == leftSideParty)
            {
                return PartyUILauncher.OpenWithDiscardRoster(rightSideParty, onComplete);
            }

            return PartyUILauncher.OpenPartyToParty(rightSideParty, leftSideParty, onComplete);
        }

        /// MARK: Party Only
        /// <summary>
        /// Opens the party editor for a single party with all game troops on the left (discard mode).
        /// </summary>
        /// <param name="rightSideParty">Party to edit (required)</param>
        /// <param name="onComplete">Optional callback when party screen closes</param>
        /// <returns>BLGMResult indicating success or failure</returns>
        public static BLGMResult OpenPartyEditor(MobileParty rightSideParty, Action onComplete = null)
        {
            return OpenPartyEditor(rightSideParty, null, onComplete);
        }
    }
}
