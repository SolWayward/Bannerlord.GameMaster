using System;
using System.Collections.Generic;
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
