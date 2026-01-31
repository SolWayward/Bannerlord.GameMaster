using System.Collections.Generic;
using System.Text;
using Bannerlord.GameMaster.Common;
using Bannerlord.GameMaster.Heroes;
using TaleWorlds.CampaignSystem;

namespace Bannerlord.GameMaster.Factions
{
    /// <summary>
    /// Extension methods for IFaction (shared by Clan and Kingdom).
    /// </summary>
    public static class FactionExtensions
    {
        /// MARK: EquipHeroes
        /// <summary>
        /// Equips all heroes in the faction with stat-based equipment.
        /// </summary>
        /// <param name="faction">The faction (Clan or Kingdom) whose heroes to equip.</param>
        /// <param name="tier">Equipment tier (0+). Use -1 for auto-calculation from hero level.</param>
        /// <param name="replaceCivilianEquipment">Whether to also replace civilian equipment.</param>
        /// <param name="includeNativeHeroes">If true, equips all heroes. If false, only equips BLGM-created heroes.</param>
        /// <returns>BLGMResult with details of the operation.</returns>
        public static BLGMResult EquipHeroes(
            this IFaction faction,
            int tier = -1,
            bool replaceCivilianEquipment = false,
            bool includeNativeHeroes = false)
        {
            if (faction == null)
                return BLGMResult.Error("EquipHeroes() failed: faction cannot be null");

            // Get heroes from faction
            IEnumerable<Hero> heroes = faction.Heroes;
            if (heroes == null)
                return BLGMResult.Error("EquipHeroes() failed: faction has no heroes collection");

            int equippedCount = 0;
            int skippedCount = 0;
            int failedCount = 0;
            StringBuilder failedHeroes = new();

            foreach (Hero hero in heroes)
            {
                // Skip null or dead heroes
                if (hero == null || !hero.IsAlive)
                {
                    skippedCount++;
                    continue;
                }

                // Filter BLGM heroes if includeNativeHeroes is false
                // Using StringId.StartsWith is more performant than BLGMObjectManager lookup
                if (!includeNativeHeroes && !IsBLGMHero(hero))
                {
                    skippedCount++;
                    continue;
                }

                // Equip the hero
                BLGMResult result = HeroOutfitter.EquipHeroByStats(hero, tier, replaceCivilianEquipment);

                if (result.IsSuccess)
                {
                    equippedCount++;
                }
                else
                {
                    failedCount++;
                    if (failedHeroes.Length > 0)
                        failedHeroes.Append(", ");
                    failedHeroes.Append(hero.Name);
                }
            }

            // Build result message
            string factionName = faction.Name?.ToString() ?? "Unknown";
            string tierInfo = tier >= 0 ? $"tier {tier}" : "auto-tier";
            string civilianInfo = replaceCivilianEquipment ? " (including civilian)" : "";
            string heroTypeInfo = includeNativeHeroes ? "all heroes" : "BLGM heroes only";

            if (equippedCount == 0 && failedCount == 0)
            {
                return BLGMResult.Success(
                    $"No eligible heroes found in {factionName} ({heroTypeInfo}). Skipped: {skippedCount}");
            }

            if (failedCount > 0)
            {
                return BLGMResult.Error(
                    $"Equipped {equippedCount} heroes in {factionName} with {tierInfo} equipment{civilianInfo}. " +
                    $"Failed: {failedCount} ({failedHeroes}). Skipped: {skippedCount}");
            }

            return BLGMResult.Success(
                $"Equipped {equippedCount} heroes in {factionName} with {tierInfo} equipment{civilianInfo}. " +
                $"Skipped: {skippedCount}");
        }

        /// MARK: IsBLGMHero
        /// <summary>
        /// Checks if a hero was created by BLGM.
        /// Uses StringId prefix check which is more performant than BLGMObjectManager lookup.
        /// </summary>
        /// <param name="hero">The hero to check.</param>
        /// <returns>True if the hero was created by BLGM; false otherwise.</returns>
        public static bool IsBLGMHero(Hero hero)
        {
            if (hero == null || string.IsNullOrEmpty(hero.StringId))
                return false;

            return hero.StringId.StartsWith("blgm_");
        }
    }
}
