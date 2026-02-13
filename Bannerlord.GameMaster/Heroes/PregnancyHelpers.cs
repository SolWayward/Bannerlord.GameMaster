using System;
using System.Collections.Generic;
using Bannerlord.GameMaster.Common;
using Bannerlord.GameMaster.Settlements;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Heroes
{
    /// <summary>
    /// Helper methods for pregnancy father resolution and candidate searching.
    /// </summary>
    public static class PregnancyHelpers
    {
        /// MARK: ValidatePregnancy
        /// <summary>
        /// Validates if two heroes are able to conceive a child with each other.
        /// Does not resolve father -- caller must provide both heroes.
        /// Will log any errors or failed validations
        /// </summary>
        /// <returns>BLGMResult with success or specific error details</returns>
        public static BLGMResult ValidatePregnancy(Hero mother, Hero father)
        {
            if (mother == null)
            {
                return BLGMResult.Error("ValidatePregnancy() failed, mother cannot be null",
                    new ArgumentNullException(nameof(mother))).Log();
            }

            if (!mother.IsFemale)
                return BLGMResult.Error($"ValidatePregnancy() failed, {mother.Name} is not female").Log();

            if (mother.IsPregnant)
                return BLGMResult.Error($"ValidatePregnancy() failed, {mother.Name} is already pregnant").Log();

            if (father == null)
                return BLGMResult.Error($"ValidatePregnancy() failed, could not resolve a father for {mother.Name}. No valid male hero found nearby or as spouse.").Log();

            if (father.IsFemale)
                return BLGMResult.Error($"ValidatePregnancy() failed, resolved father {father.Name} is female").Log();

            return BLGMResult.Success($"Pregnancy validation passed for {mother.Name} and {father.Name}");
        }

        /// MARK: ResolveFather
        /// <summary>
        /// Resolves the father for a pregnancy in priority order: <br />
        /// 1. Explicit father if provided <br />
        /// 2. Mother's spouse if male <br />
        /// 3. Random male hero in mother's current settlement <br />
        /// 4. Random from 10 closest male heroes on the map
        /// </summary>
        /// <param name="mother">The mother hero</param>
        /// <param name="explicitFather">Explicitly specified father, or null for auto-resolution</param>
        /// <returns>The resolved father hero, or null if no valid candidate found</returns>
        public static Hero ResolveFather(Hero mother, Hero explicitFather = null)
        {
            // Priority 1: Explicit father
            if (explicitFather != null)
                return explicitFather;

            // Priority 2: Mother's spouse if male
            if (mother.Spouse != null && !mother.Spouse.IsFemale)
                return mother.Spouse;

            // Priority 3: Random male hero in mother's settlement
            Hero settlementCandidate = FindFatherInSettlement(mother);
            if (settlementCandidate != null)
                return settlementCandidate;

            // Priority 4: Random from 10 closest male heroes on the map
            Hero nearbyCandidate = FindFatherNearby(mother);
            return nearbyCandidate;
        }

        /// MARK: FindFatherInSettlement
        /// <summary>
        /// Searches the mother's current settlement for a valid male hero to be the father.
        /// Includes heroes with and without parties.
        /// </summary>
        private static Hero FindFatherInSettlement(Hero mother)
        {
            Settlement settlement = mother.CurrentSettlement;
            if (settlement == null)
            {
                // Check if mother is in a party that is at a settlement
                MobileParty party = mother.PartyBelongedTo;
                if (party?.CurrentSettlement != null)
                {
                    settlement = party.CurrentSettlement;
                }
            }

            if (settlement == null)
                return null;

            List<Hero> settlementHeroes = new();
            HeroPositionHelpers.GetHeroesInSettlement(settlement, settlementHeroes);

            // Filter to valid candidates
            List<Hero> candidates = new();
            for (int i = 0; i < settlementHeroes.Count; i++)
            {
                Hero hero = settlementHeroes[i];

                if (IsFatherCandidate(hero, mother))
                {
                    candidates.Add(hero);
                }
            }

            if (candidates.Count == 0)
                return null;

            int randomIndex = RandomNumberGen.Instance.NextRandomInt(candidates.Count);
            return candidates[randomIndex];
        }

        /// MARK: FindFatherNearby
        /// <summary>
        /// Searches for the 10 closest male heroes on the map and picks one randomly.
        /// </summary>
        private static Hero FindFatherNearby(Hero mother)
        {
            if (!HeroPositionHelpers.TryGetHeroMapPosition(mother, out Vec2 motherPosition))
                return null;

            List<Hero> nearestCandidates = HeroPositionHelpers.FindNearestHeroes(
                motherPosition,
                10,
                hero => IsFatherCandidate(hero, mother));

            if (nearestCandidates.Count == 0)
                return null;

            int randomIndex = RandomNumberGen.Instance.NextRandomInt(nearestCandidates.Count);
            return nearestCandidates[randomIndex];
        }

        /// MARK: IsFatherCandidate
        /// <summary>
        /// Checks if a hero is a valid father candidate for the given mother.
        /// Must be male, alive, not the player hero, and not in the mother's clan.
        /// </summary>
        public static bool IsFatherCandidate(Hero hero, Hero mother)
        {
            if (hero == null || hero == mother)
                return false;

            if (hero.IsFemale)
                return false;

            if (!hero.IsAlive)
                return false;

            if (hero == Hero.MainHero)
                return false;

            if (hero.Clan != null && hero.Clan == mother.Clan)
                return false;

            return true;
        }
    }
}
