using System;
using System.Collections.Generic;
using System.Reflection;
using Bannerlord.GameMaster.Common;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Armies
{
    /// <summary>
    /// High-level API for creating and configuring armies.
    /// Uses the native <see cref="Army"/> constructor directly, bypassing
    /// <c>Kingdom.CreateArmy()</c> gathering/AI logic so armies are created
    /// immediately with full control over composition.
    /// </summary>
    public static class ArmyManager
    {
        // Cached reflection for Army.LeaderParty (public get, private set auto-property)
        private static readonly PropertyInfo LeaderPartyProperty = typeof(Army).GetProperty(
            "LeaderParty",
            BindingFlags.Public | BindingFlags.Instance);

        /// MARK: SetCommander
        /// <summary>
        /// Changes the commander (leader party) of an existing army without disbanding it.<br/><br/>
        /// The new leader must already be a member of the army. This method uses reflection to set
        /// <see cref="Army.LeaderParty"/> (private setter) and then re-attaches all member parties
        /// to the new leader. The old leader becomes a regular attached member.<br/><br/>
        /// <b>Why reflection:</b> <c>Army.LeaderParty</c> has a private setter and is only assigned
        /// in the <see cref="Army"/> constructor. No public API exists to change the leader without
        /// disbanding and recreating the army, which fires <c>OnArmyDispersed</c> events and has
        /// undesirable side effects.<br/><br/>
        /// <b>Execution sequence</b> (order prevents disband cascade):<br/>
        /// 1. Detach all parties from old leader (sets <c>AttachedTo = null</c>)<br/>
        /// 2. Swap <c>LeaderParty</c> via reflection<br/>
        /// 3. Set <c>ArmyOwner</c> to new leader's hero<br/>
        /// 4. Call <c>UpdateName()</c> to regenerate army name<br/>
        /// 5. Re-attach all member parties to the new leader via <see cref="Army.AddPartyToMergedParties"/>
        /// </summary>
        /// <param name="army">The army to change the commander of</param>
        /// <param name="newLeaderParty">The party that will become the new leader (must already be in the army)</param>
        /// <returns>BLGMResult indicating success or failure with reason</returns>
        public static BLGMResult SetCommander(Army army, MobileParty newLeaderParty)
        {
            // MARK: Validation
            if (army == null)
            {
                return BLGMResult.Error("SetCommander() failed, army cannot be null",
                    new ArgumentNullException(nameof(army))).Log();
            }

            if (newLeaderParty == null)
            {
                return BLGMResult.Error("SetCommander() failed, newLeaderParty cannot be null",
                    new ArgumentNullException(nameof(newLeaderParty))).Log();
            }

            if (newLeaderParty.LeaderHero == null)
            {
                return BLGMResult.Error("SetCommander() failed, newLeaderParty must have a LeaderHero",
                    new InvalidOperationException("newLeaderParty has no LeaderHero")).Log();
            }

            if (newLeaderParty.Army != army)
            {
                return BLGMResult.Error($"SetCommander() failed, {newLeaderParty.Name} is not in this army").Log();
            }

            if (newLeaderParty == army.LeaderParty)
            {
                return BLGMResult.Error($"SetCommander() failed, {newLeaderParty.Name} is already the leader").Log();
            }

            if (LeaderPartyProperty == null)
            {
                return BLGMResult.Error("SetCommander() failed, could not find LeaderParty property via reflection",
                    new InvalidOperationException("Army.LeaderParty PropertyInfo is null")).Log();
            }

            // MARK: Capture State
            MobileParty oldLeaderParty = army.LeaderParty;
            string oldLeaderName = oldLeaderParty.LeaderHero?.Name?.ToString() ?? "Unknown";
            string newLeaderName = newLeaderParty.LeaderHero.Name?.ToString() ?? "Unknown";

            // MARK: Detach All Parties
            // Detach all parties from the old leader to prevent stale attachment references.
            // Native SetAttachedToInternal handles cleanup (removes from attached list, map event side, etc.)
            MBReadOnlyList<MobileParty> parties = army.Parties;
            for (int i = 0; i < parties.Count; i++)
            {
                MobileParty party = parties[i];
                if (party != oldLeaderParty && party.AttachedTo != null)
                {
                    party.AttachedTo = null;
                }
            }

            // MARK: Swap Leader (Reflection)
            // Set LeaderParty via reflection - we never set any party's .Army = null,
            // so OnRemovePartyInternal is never triggered and _parties list stays intact.
            LeaderPartyProperty.SetValue(army, newLeaderParty);
            army.ArmyOwner = newLeaderParty.LeaderHero;
            army.UpdateName();

            // MARK: Re-attach Members
            // Attach all member parties (including old leader) to the new leader
            for (int i = 0; i < parties.Count; i++)
            {
                MobileParty party = parties[i];
                if (party != newLeaderParty)
                {
                    army.AddPartyToMergedParties(party);
                }
            }

            return BLGMResult.Success(
                $"Changed army commander from {oldLeaderName} to {newLeaderName}");
        }

        /// MARK: CreateArmy
        /// <summary>
        /// Creates a new army led by the specified party's leader.<br/>
        /// The army is created using the native <see cref="Army(Kingdom, MobileParty, Army.ArmyTypes)"/> 
        /// constructor which registers the army with the kingdom, sets up periodic tick events,
        /// and assigns the leader party. Member parties are added via the native
        /// <see cref="MobileParty.Army"/> setter which fires <c>OnPartyJoinedArmy</c> events
        /// and handles influence costs for AI parties.<br/><br/>
        /// After all members are added, <see cref="GatherArmyAction.Apply"/> is called to
        /// fire the <c>OnArmyGathered</c> campaign event. If <paramref name="teleportMembers"/>
        /// is true, all member parties are teleported to the commander and physically attached.
        /// </summary>
        /// <param name="leaderParty">The party that will lead the army (required, must have a leader hero in a kingdom)</param>
        /// <param name="memberParties">Optional list of parties to add as army members</param>
        /// <param name="armyType">The type of army to create (default: Patrolling)</param>
        /// <param name="teleportMembers">If true, teleports all member parties to the commander and attaches them (default: true)</param>
        /// <returns>The created <see cref="Army"/>, or null if validation fails</returns>
        public static Army CreateArmy(
            MobileParty leaderParty,
            List<MobileParty> memberParties = null,
            Army.ArmyTypes armyType = Army.ArmyTypes.Patrolling,
            bool teleportMembers = true)
        {
            // MARK: Validation
            if (leaderParty == null)
            {
                BLGMResult.Error("CreateArmy() failed, leaderParty cannot be null",
                    new ArgumentNullException(nameof(leaderParty))).Log();
                return null;
            }

            Hero leaderHero = leaderParty.LeaderHero;
            if (leaderHero == null)
            {
                BLGMResult.Error("CreateArmy() failed, leaderParty must have a LeaderHero",
                    new InvalidOperationException("LeaderParty has no LeaderHero")).Log();
                return null;
            }

            Kingdom kingdom = leaderHero.Clan?.Kingdom;
            if (kingdom == null)
            {
                BLGMResult.Error($"CreateArmy() failed, {leaderHero.Name}'s clan must belong to a kingdom",
                    new InvalidOperationException("Leader's clan has no kingdom")).Log();
                return null;
            }

            if (leaderParty.Army != null)
            {
                BLGMResult.Error($"CreateArmy() failed, {leaderHero.Name}'s party is already in an army").Log();
                return null;
            }

            // MARK: Create Army
            // Native constructor: sets Kingdom (registers with kingdom), creates _parties list,
            // sets LeaderParty, sets LeaderParty.Army = this (fires OnAddPartyInternal),
            // sets ArmyOwner, updates name, sets ArmyType, registers tick events, Cohesion = 100
            Army army = new(kingdom, leaderParty, armyType);

            // MARK: Add Members
            if (memberParties != null)
            {
                for (int i = 0; i < memberParties.Count; i++)
                {
                    ArmyPartyHelpers.TryAddPartyToArmy(army, memberParties[i]);
                }
            }

            // MARK: Gather Event
            // Fire the OnArmyGathered campaign event so game systems are notified
            GatherArmyAction.Apply(leaderParty, null);

            // MARK: Teleport
            if (teleportMembers)
            {
                ArmyPartyHelpers.TeleportPartiesToCommander(army);
            }

            return army;
        }
    }
}
