using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Heroes
{
    /// <summary>
    /// Provides helpers for finding heroes based on position, proximity, and settlement location.
    /// </summary>
    public static class HeroPositionHelpers
    {
        /// MARK: GetHeroMapPosition
        /// <summary>
        /// Gets the best available map position for a hero.
        /// Checks settlement first, then party, then party's attached-to party.
        /// </summary>
        /// <param name="hero">The hero to get the position for</param>
        /// <param name="position">The resolved map position</param>
        /// <returns>True if a valid position was found, false otherwise</returns>
        public static bool TryGetHeroMapPosition(Hero hero, out Vec2 position)
        {
            if (hero.CurrentSettlement != null)
            {
                position = hero.CurrentSettlement.GetPosition2D;
                return true;
            }

            MobileParty party = hero.PartyBelongedTo;
            if (party != null)
            {
                if (party.CurrentSettlement != null)
                {
                    position = party.CurrentSettlement.GetPosition2D;
                    return true;
                }

                position = party.GetPosition2D;
                return true;
            }

            position = Vec2.Zero;
            return false;
        }

        /// MARK: GetHeroesInSettlement
        /// <summary>
        /// Collects all heroes present in a settlement, including heroes without a party
        /// and hero members of parties stationed at the settlement.
        /// </summary>
        /// <param name="settlement">The settlement to search</param>
        /// <param name="heroes">Output list of heroes found in the settlement</param>
        public static void GetHeroesInSettlement(Settlement settlement, List<Hero> heroes)
        {
            if (settlement == null)
                return;

            // Heroes without a party (notables, governors, wanderers, etc.)
            MBReadOnlyList<Hero> heroesWithoutParty = settlement.HeroesWithoutParty;
            for (int i = 0; i < heroesWithoutParty.Count; i++)
            {
                heroes.Add(heroesWithoutParty[i]);
            }

            // Heroes in parties stationed at the settlement
            List<MobileParty> parties = settlement.Parties;
            for (int i = 0; i < parties.Count; i++)
            {
                MobileParty party = parties[i];
                if (party.LeaderHero != null)
                {
                    heroes.Add(party.LeaderHero);
                }

                // Also include companion/member heroes in the party
                MBList<TroopRosterElement> memberRoster = party.MemberRoster?.GetTroopRoster();
                if (memberRoster != null)
                {
                    for (int j = 0; j < memberRoster.Count; j++)
                    {
                        TroopRosterElement element = memberRoster[j];
                        if (element.Character.IsHero && element.Character.HeroObject != null && element.Character.HeroObject != party.LeaderHero)
                        {
                            heroes.Add(element.Character.HeroObject);
                        }
                    }
                }
            }
        }

        /// MARK: FindNearestHeroes
        /// <summary>
        /// Finds the nearest heroes to a given map position from Hero.AllAliveHeroes,
        /// filtered by a predicate. Uses DistanceSquared for performance.
        /// Returns up to maxCount heroes sorted by distance (closest first).
        /// </summary>
        /// <param name="position">The map position to measure distance from</param>
        /// <param name="maxCount">Maximum number of heroes to return</param>
        /// <param name="predicate">Filter predicate (return true to include the hero)</param>
        /// <returns>List of up to maxCount nearest heroes matching the predicate</returns>
        public static List<Hero> FindNearestHeroes(Vec2 position, int maxCount, System.Func<Hero, bool> predicate)
        {
            MBReadOnlyList<Hero> allAlive = Hero.AllAliveHeroes;
            int totalCount = allAlive.Count;

            // Track the top N closest using parallel arrays (avoid allocating structs per hero)
            float[] distances = new float[maxCount];
            Hero[] heroes = new Hero[maxCount];
            int filledCount = 0;

            // Initialize distances to max
            for (int i = 0; i < maxCount; i++)
            {
                distances[i] = float.MaxValue;
            }

            for (int i = 0; i < totalCount; i++)
            {
                Hero hero = allAlive[i];

                if (!predicate(hero))
                    continue;

                if (!TryGetHeroMapPosition(hero, out Vec2 heroPosition))
                    continue;

                float distSq = position.DistanceSquared(heroPosition);

                // Check if this hero is closer than the farthest in our top N
                int worstIndex = 0;
                float worstDistance = distances[0];

                for (int j = 1; j < maxCount; j++)
                {
                    if (distances[j] > worstDistance)
                    {
                        worstDistance = distances[j];
                        worstIndex = j;
                    }
                }

                if (distSq < worstDistance)
                {
                    distances[worstIndex] = distSq;
                    heroes[worstIndex] = hero;

                    if (filledCount < maxCount)
                        filledCount++;
                }
            }

            // Collect results (only non-null entries)
            List<Hero> result = new(filledCount);
            for (int i = 0; i < maxCount; i++)
            {
                if (heroes[i] != null)
                {
                    result.Add(heroes[i]);
                }
            }

            return result;
        }
    }
}
