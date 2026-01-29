using System;
using System.Collections.Generic;
using System.Linq;
using Bannerlord.GameMaster.Common;
using Bannerlord.GameMaster.Items;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace Bannerlord.GameMaster.Heroes
{
    /// <summary>
    /// Provides methods for equipping heroes intelligently with appropriate gear.
    /// </summary>
    public class HeroOutfitter
    {
        /// MARK: EquipHeroByStats
        /// <summary>
        /// Equips a hero with stat-based equipment using the new equipment generation system.
        /// Analyzes hero skills to determine optimal weapon loadout.
        /// This is the recommended method for equipping heroes.
        /// </summary>
        /// <param name="hero">The hero to equip.</param>
        /// <param name="tier">Equipment tier (0-6). Use -1 for auto-calculation from hero level.</param>
        /// <param name="replaceCivilianEquipment">Whether to also replace civilian equipment.</param>
        /// <returns>BLGMResult indicating success or failure.</returns>
        public static BLGMResult EquipHeroByStats(Hero hero, int tier = -1, bool replaceCivilianEquipment = false)
        {
            if (hero == null)
                return BLGMResult.Error("EquipHeroByStats() failed: hero cannot be null");

            HeroEquipper equipper = new();
            return equipper.EquipHeroByStats(
                hero,
                tier,
                WeaponTypeFlags.None, // Derive from skills
                replaceBattleEquipment: true,
                replaceCivilianEquipment: replaceCivilianEquipment);
        }

        /// MARK: EquipHeroWithOption
        /// <summary>
        /// Equips a hero with equipment using explicit configuration options.
        /// Use this when you need full control over the equipment generation.
        /// </summary>
        /// <param name="hero">The hero to equip.</param>
        /// <param name="culture">Culture for item selection (null = use hero's culture).</param>
        /// <param name="tier">Equipment tier (0-6).</param>
        /// <param name="weaponPreferences">Explicit weapon type preferences.</param>
        /// <param name="isMounted">Whether to include horse equipment.</param>
        /// <param name="replaceBattleEquipment">Whether to replace battle equipment.</param>
        /// <param name="replaceCivilianEquipment">Whether to replace civilian equipment.</param>
        /// <returns>BLGMResult indicating success or failure.</returns>
        public static BLGMResult EquipHeroWithOptions(
            Hero hero,
            CultureObject culture,
            int tier,
            WeaponTypeFlags weaponPreferences,
            bool isMounted = false,
            bool replaceBattleEquipment = true,
            bool replaceCivilianEquipment = false)
        {
            if (hero == null)
                return BLGMResult.Error("EquipHeroWithOptions() failed: hero cannot be null");

            HeroEquipper equipper = new();
            return equipper.EquipHero(
                hero,
                culture,
                tier,
                weaponPreferences,
                isMounted,
                replaceBattleEquipment,
                replaceCivilianEquipment);
        }

        /// MARK: GenerateEquipment
        /// <summary>
        /// Generates equipment for a hero without applying it.
        /// Useful for preview or manual modification before assignment.
        /// </summary>
        /// <param name="hero">The hero to generate equipment for.</param>
        /// <param name="tier">Equipment tier (0-6). Use -1 for auto-calculation.</param>
        /// <param name="isCivilian">Whether to generate civilian equipment.</param>
        /// <returns>Generated Equipment object, or null if hero is null.</returns>
        public static Equipment GenerateEquipmentForHero(Hero hero, int tier = -1, bool isCivilian = false)
        {
            if (hero == null)
            {
                BLGMResult.Error("GenerateEquipmentForHero() failed: hero cannot be null").Log();
                return null;
            }

            HeroEquipper equipper = new();
            return equipper.GenerateEquipmentForHero(hero, tier, WeaponTypeFlags.None, isCivilian);
        }

        /// MARK: GetTierForHero
        /// <summary>
        /// Gets the appropriate equipment tier for a hero based on their level.
        /// </summary>
        /// <param name="hero">The hero to evaluate.</param>
        /// <returns>Equipment tier (0-6).</returns>
        public static int GetTierForHero(Hero hero)
        {
            if (hero == null) return 0;
            return HeroEquipper.CalculateTierFromLevel(hero.Level);
        }

        /// MARK: WeaponPreferences
        /// <summary>
        /// Derives weapon type preferences from a hero's combat skills.
        /// </summary>
        /// <param name="hero">The hero to analyze.</param>
        /// <returns>WeaponTypeFlags representing preferred weapon types.</returns>
        public static WeaponTypeFlags DeriveWeaponPreferences(Hero hero)
        {
            return HeroEquipper.DeriveWeaponPreferencesFromSkills(hero);
        }
    }
}