using System;
using System.Reflection;
using System.Text;
using Bannerlord.GameMaster.Common;
using Bannerlord.GameMaster.Settlements;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Settlements;

namespace Bannerlord.GameMaster.Heroes
{
    public static class HeroManager
    {
        /// <summary>
        /// tries to get a random settlement in this order: From heroes clan > from heroes kingdom > from all settlements
        /// </summary>
        public static Settlement GetBestInitialSettlement(Hero hero)
        {
            Settlement settlement;

            settlement = SettlementManager.GetRandomClanFortification(hero.Clan);
            settlement ??= SettlementManager.GetRandomKingdomFortification(hero.Clan?.Kingdom);
            settlement ??= SettlementManager.GetRandomTown();

            return settlement;
        }

        // Cached _homeSettlement field for reflection
        private static readonly FieldInfo HomeSettlementField = typeof(Hero).GetField("_homeSettlement", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// Uses reflection to try to the Heroes home settlement directly
        /// </summary>
        /// <returns>BLGM result containing bool if Setting homeSettlement succeeded and a string with details</returns>
        public static BLGMResult TrySetHomeSettlement(Hero hero, Settlement homeSettlement)
        {
            try
            {
                if (HomeSettlementField == null)
                    return new(false, "Could not find _homeSettlement field - game version incompatible");

                HomeSettlementField.SetValue(hero, homeSettlement);
                return new(true, $"Set home settlement for {hero.Name} to {homeSettlement?.Name}");
            }
            catch (Exception ex)
            {
                return new(false, $"Failed to set _homeSettlement for {hero.Name}: {ex.Message}");
            }
        }

        /// MARK: Impregnate
        /// <summary>
        /// Makes a female hero pregnant with an optional specified father.
        /// If no father is specified, resolves one automatically via ResolveFather().
        /// Uses reflection to replace the pregnancy record with the correct father after MakePregnantAction.Apply().
        /// </summary>
        /// <param name="mother">The female hero to make pregnant</param>
        /// <param name="father">Optional father hero. If null, will be auto-resolved.</param>
        /// <returns>BLGMResult with success/failure details</returns>
        public static BLGMResult Impregnate(Hero mother, Hero father = null)
        {
            // Resolve father if not explicitly provided
            father = PregnancyHelpers.ResolveFather(mother, father);

            BLGMResult validation = PregnancyHelpers.ValidatePregnancy(mother, father);
            if (!validation.IsSuccess)
                return validation;

            // Apply pregnancy (this creates a pregnancy record with mother.Spouse as father)
            TaleWorlds.CampaignSystem.Actions.MakePregnantAction.Apply(mother);

            // If the resolved father is NOT mother.Spouse, we need to replace the pregnancy record via reflection
            if (father != mother.Spouse)
            {
                BLGMResult replaceResult = PregnancyReflectionHelper.ReplacePregnancyFather(mother, father);

                if (!replaceResult.IsSuccess)
                {
                    return BLGMResult.Error(
                        $"{mother.Name} is now pregnant, but reflection failed to set {father.Name} as father. " +
                        $"The father may be incorrect (defaulted to spouse or null). Details: {replaceResult.Message}").Log();
                }
            }

            return BLGMResult.Success($"{mother.Name} is now pregnant by {father.Name}");
        }

        /// MARK: Marry
        /// <summary>
        /// Marry two heroes. Divorces both from current spouses first if needed.
        /// Tries native MarriageAction.Apply() first, only forces via Spouse setter when forceMarriage is true.
        /// </summary>
        /// <param name="hero">First hero (otherHero joins this hero's clan by default)</param>
        /// <param name="otherHero">Second hero</param>
        /// <param name="forceMarriage">If true, bypasses native validation checks on failure</param>
        /// <param name="joinClan">If true, otherHero joins hero's clan. If false, both heroes stay in original clans</param>
        /// <returns>BLGMResult with success/failure details</returns>
        public static BLGMResult Marry(Hero hero, Hero otherHero, bool forceMarriage = false, bool joinClan = true)
        {
            if (hero == null)
                return BLGMResult.Error("Marry() failed, hero cannot be null", new ArgumentNullException(nameof(hero))).Log();

            if (otherHero == null)
                return BLGMResult.Error("Marry() failed, otherHero cannot be null", new ArgumentNullException(nameof(otherHero))).Log();

            if (hero == otherHero)
                return BLGMResult.Error("Marry() failed, cannot marry a hero to themselves").Log();

            if (hero.IsDead)
                return BLGMResult.Error($"Marry() failed, {hero.Name} is dead").Log();

            if (otherHero.IsDead)
                return BLGMResult.Error($"Marry() failed, {otherHero.Name} is dead").Log();

            // Already married to each other
            if (hero.Spouse == otherHero)
                return BLGMResult.Success($"{hero.Name} and {otherHero.Name} are already married");

            // Divorce both heroes from their CURRENT spouses before attempting marriage
            if (hero.Spouse != null)
                Divorce(hero);

            if (otherHero.Spouse != null)
                Divorce(otherHero);

            // Save original clans before native action may change them
            Clan heroClan = hero.Clan;
            Clan otherHeroClan = otherHero.Clan;

            // Try native marriage first
            MarriageAction.Apply(hero, otherHero);
            bool nativeSucceeded = (hero.Spouse == otherHero);
            bool forced = false;

            // If native failed, handle based on forceMarriage flag
            if (!nativeSucceeded)
            {
                if (!forceMarriage)
                {
                    return BLGMResult.Error(
                        $"Marriage between {hero.Name} and {otherHero.Name} failed native validation. " +
                        "Use forceMarriage to bypass native checks.").Log();
                }

                // Force the marriage via Spouse setter + romantic state
                hero.Spouse = otherHero;
                ChangeRomanticStateAction.Apply(hero, otherHero, Romance.RomanceLevelEnum.Marriage);
                forced = true;
            }

            // Handle clan joining
            StringBuilder details = new();

            if (joinClan && otherHero.Clan != heroClan)
            {
                otherHero.Clan = heroClan;
                details.Append($"\n{otherHero.Name} joined clan '{heroClan?.Name}'");
            }

            else if (!joinClan && nativeSucceeded)
            {
                // Native action may have changed clans -- restore originals
                if (hero.Clan != heroClan)
                {
                    hero.Clan = heroClan;
                    details.Append($"\n{hero.Name} clan restored to '{heroClan?.Name}'");
                }

                if (otherHero.Clan != otherHeroClan)
                {
                    otherHero.Clan = otherHeroClan;
                    details.Append($"\n{otherHero.Name} clan restored to '{otherHeroClan?.Name}'");
                }
            }

            // Build result message
            string forceNote = forced ? " (forced - native validation bypassed)" : "";
            return BLGMResult.Success(
                $"{hero.Name} and {otherHero.Name} are now married{forceNote}{details}");
        }

        /// MARK: Divorce
        /// <summary>
        /// Divorce hero from their current spouse.
        /// The native Spouse setter handles both sides and exSpouses lists.
        /// </summary>
        /// <param name="hero">The hero to divorce from their current spouse</param>
        /// <returns>BLGMResult with success/failure details</returns>
        public static BLGMResult Divorce(Hero hero)
        {
            if (hero == null)
                return BLGMResult.Error("Divorce() failed, hero cannot be null", new ArgumentNullException(nameof(hero))).Log();

            if (hero.Spouse == null)
                return BLGMResult.Error($"{hero.Name} is not married");

            Hero exSpouse = hero.Spouse;

            // Null the spouse -- the native setter handles both sides:
            //   - Adds exSpouse to hero._exSpouses
            //   - Sets exSpouse.Spouse = null (which adds hero to exSpouse._exSpouses)
            hero.Spouse = null;

            // Update romantic state from Marriage to Ended
            ChangeRomanticStateAction.Apply(hero, exSpouse, Romance.RomanceLevelEnum.Ended);

            return BLGMResult.Success($"{hero.Name} divorced from {exSpouse.Name}");
        }
    }
}
