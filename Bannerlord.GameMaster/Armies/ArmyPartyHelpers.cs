using System;
using Bannerlord.GameMaster.Common;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Armies
{
    /// <summary>
    /// Utility helpers for army party management including validation, teleportation,
    /// and physical attachment of parties to army commanders.
    /// </summary>
    public static class ArmyPartyHelpers
    {
        /// MARK: TryAddPartyToArmy
        /// <summary>
        /// Validates and adds a party to an existing army using the native
        /// <see cref="MobileParty.Army"/> setter which triggers <c>OnAddPartyInternal</c>,
        /// adding the party to the army's party list and firing <c>OnPartyJoinedArmy</c>.<br/><br/>
        /// Filters out parties that are null, already in an army, disbanded, the leader party itself,
        /// or lacking a leader hero.
        /// </summary>
        /// <param name="army">The army to add the party to</param>
        /// <param name="party">The party to add</param>
        /// <returns>BLGMResult indicating success or failure with reason</returns>
        public static BLGMResult TryAddPartyToArmy(Army army, MobileParty party)
        {
            if (army == null)
            {
                return BLGMResult.Error("TryAddPartyToArmy() failed, army cannot be null",
                    new ArgumentNullException(nameof(army))).Log();
            }

            if (party == null)
            {
                return BLGMResult.Error("TryAddPartyToArmy() failed, party cannot be null",
                    new ArgumentNullException(nameof(party))).Log();
            }

            if (party == army.LeaderParty)
            {
                return BLGMResult.Error("TryAddPartyToArmy() skipped, party is the army leader").Log();
            }

            if (party.Army != null)
            {
                return BLGMResult.Error($"TryAddPartyToArmy() failed, {party.Name} is already in an army").Log();
            }

            if (!party.IsActive)
            {
                return BLGMResult.Error($"TryAddPartyToArmy() failed, {party.Name} is not active").Log();
            }

            if (party.LeaderHero == null)
            {
                return BLGMResult.Error($"TryAddPartyToArmy() failed, {party.Name} has no leader hero").Log();
            }

            if (party.IsDisbanding)
            {
                return BLGMResult.Error($"TryAddPartyToArmy() failed, {party.Name} is disbanding").Log();
            }

            // Native .Army setter: calls OnAddPartyInternal which adds to _parties,
            // fires OnPartyJoinedArmy event, and applies influence cost for AI parties
            party.Army = army;

            return BLGMResult.Success($"Added {party.Name} to {army.Name}");
        }

        /// MARK: TryRemovePartyFromArmy
        /// <summary>
        /// Validates and removes a party from an army using the native
        /// <see cref="MobileParty.Army"/> setter (set to null) which triggers <c>OnRemovePartyInternal</c>,
        /// removing the party from the army's party list, firing <c>OnPartyRemovedFromArmy</c>,
        /// and setting <c>AttachedTo = null</c>.<br/><br/>
        /// Cannot remove the leader party -- use <see cref="ArmyManager.SetCommander"/> first to swap
        /// the leader, or disband the army entirely. Removing the leader via this path would trigger
        /// <c>DisbandArmyAction.ApplyByLeaderPartyRemoved</c> which disbands the entire army.
        /// </summary>
        /// <param name="army">The army to remove the party from</param>
        /// <param name="party">The party to remove</param>
        /// <returns>BLGMResult indicating success or failure with reason</returns>
        public static BLGMResult TryRemovePartyFromArmy(Army army, MobileParty party)
        {
            if (army == null)
            {
                return BLGMResult.Error("TryRemovePartyFromArmy() failed, army cannot be null",
                    new ArgumentNullException(nameof(army))).Log();
            }

            if (party == null)
            {
                return BLGMResult.Error("TryRemovePartyFromArmy() failed, party cannot be null",
                    new ArgumentNullException(nameof(party))).Log();
            }

            if (party.Army != army)
            {
                return BLGMResult.Error($"TryRemovePartyFromArmy() failed, {party.Name} is not in this army").Log();
            }

            if (party == army.LeaderParty)
            {
                return BLGMResult.Error("TryRemovePartyFromArmy() failed, cannot remove the leader party. " +
                    "Use SetCommander() to swap the leader first, or disband the army").Log();
            }

            string partyName = party.Name?.ToString() ?? "Unknown";
            string armyName = army.Name?.ToString() ?? "Unknown";

            // Native .Army setter (null): calls OnRemovePartyInternal which removes from _parties,
            // fires OnPartyRemovedFromArmy, sets AttachedTo = null, and checks if army should disband.
            // After setter: fires OnPartyLeftArmy campaign event and updates army overlay if player army.
            party.Army = null;

            return BLGMResult.Success($"Removed {partyName} from {armyName}");
        }

        /// MARK: TeleportToCommander
        /// <summary>
        /// Teleports all army member parties to the commander's position and physically
        /// attaches them using the native <see cref="Army.AddPartyToMergedParties"/>.<br/><br/>
        /// Each party is first safely extracted from any settlement or existing attachment
        /// via <see cref="TeleportPartyToPosition"/>, then attached to the leader.
        /// Skips the leader party itself and any parties already attached.
        /// </summary>
        /// <param name="army">The army whose members to teleport</param>
        /// <returns>BLGMResult indicating success with count of teleported parties</returns>
        public static BLGMResult TeleportPartiesToCommander(Army army)
        {
            if (army == null)
            {
                return BLGMResult.Error("TeleportPartiesToCommander() failed, army cannot be null",
                    new ArgumentNullException(nameof(army))).Log();
            }

            MobileParty leaderParty = army.LeaderParty;
            if (leaderParty == null)
            {
                return BLGMResult.Error("TeleportPartiesToCommander() failed, army has no leader party",
                    new InvalidOperationException("Army.LeaderParty is null")).Log();
            }

            Vec2 leaderPosition = leaderParty.GetPosition2D;
            MBReadOnlyList<MobileParty> parties = army.Parties;
            int teleportedCount = 0;

            for (int i = 0; i < parties.Count; i++)
            {
                MobileParty party = parties[i];

                // Skip leader party - it's already where it needs to be
                if (party == leaderParty)
                    continue;

                // Skip already-attached parties
                if (party.AttachedTo == leaderParty)
                    continue;

                // Teleport to leader position
                BLGMResult teleportResult = TeleportPartyToPosition(party, leaderPosition);
                if (!teleportResult.IsSuccess)
                    continue;

                // Physically attach using native path
                army.AddPartyToMergedParties(party);
                teleportedCount++;
            }

            return BLGMResult.Success($"Teleported and attached {teleportedCount} parties to {leaderParty.Name}");
        }

        /// MARK: TeleportToPosition
        /// <summary>
        /// Safely teleports a single party to a target map position.<br/><br/>
        /// Handles the following edge cases before moving:<br/>
        /// - Leaves the current settlement via <see cref="LeaveSettlementAction.ApplyForParty"/> if inside one<br/>
        /// - Detaches from any party it is currently attached to<br/>
        /// - Sets position and puts the party in hold mode to prevent immediate AI repath
        /// </summary>
        /// <param name="party">The party to teleport</param>
        /// <param name="targetPosition">The 2D map position to teleport to</param>
        /// <returns>BLGMResult indicating success or failure</returns>
        public static BLGMResult TeleportPartyToPosition(MobileParty party, Vec2 targetPosition)
        {
            if (party == null)
            {
                return BLGMResult.Error("TeleportPartyToPosition() failed, party cannot be null",
                    new ArgumentNullException(nameof(party))).Log();
            }

            if (!party.IsActive)
            {
                return BLGMResult.Error($"TeleportPartyToPosition() failed, {party.Name} is not active").Log();
            }

            // Leave settlement if currently inside one
            if (party.CurrentSettlement != null)
            {
                LeaveSettlementAction.ApplyForParty(party);
            }

            // Detach from any party we're currently attached to
            if (party.AttachedTo != null)
            {
                party.AttachedTo = null;
            }

            // Set position and hold to prevent AI from immediately pathing away
            party.Position = new CampaignVec2(targetPosition, !party.IsCurrentlyAtSea);
            party.SetMoveModeHold();

            return BLGMResult.Success($"Teleported {party.Name} to ({targetPosition.X:F1}, {targetPosition.Y:F1})");
        }
    }
}
