using System;
using System.Reflection;
using System.Text;
using Bannerlord.GameMaster.Common;
using Bannerlord.GameMaster.Settlements;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace Bannerlord.GameMaster.Heroes
{
    public static class HeroManager
    {
        #region Cached Reflection

        private static readonly BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance;
        private static readonly Type HeroType = typeof(Hero);

        // Cached _homeSettlement field for reflection
        private static readonly FieldInfo HomeSettlementField = HeroType.GetField("_homeSettlement", PrivateInstance);

        // Fields nulled by Hero.OnDeath() (SaveableField = actual fields)
        private static readonly FieldInfo HeroSkillsField = HeroType.GetField("_heroSkills", PrivateInstance);
        private static readonly FieldInfo HeroPerksField = HeroType.GetField("_heroPerks", PrivateInstance);
        private static readonly FieldInfo HeroTraitsField = HeroType.GetField("_heroTraits", PrivateInstance);
        private static readonly FieldInfo CharacterAttributesField = HeroType.GetField("_characterAttributes", PrivateInstance);
        private static readonly FieldInfo HeroDeveloperField = HeroType.GetField("_heroDeveloper", PrivateInstance);

        // Equipment auto-properties nulled by Hero.OnDeath() (SaveableProperty = auto-properties with backing fields)
        private static readonly PropertyInfo BattleEquipmentProp = HeroType.GetProperty("_battleEquipment", PrivateInstance);
        private static readonly PropertyInfo CivilianEquipmentProp = HeroType.GetProperty("_civilianEquipment", PrivateInstance);
        private static readonly PropertyInfo StealthEquipmentProp = HeroType.GetProperty("_stealthEquipment", PrivateInstance);

        #endregion

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

        #region ActivateHero

        /// MARK: ActivateHero
        /// <summary>
        /// Activates a non-Active hero, placing them at a settlement.
        /// Handles all hero states: Prisoner, Fugitive, Released, Disabled, NotSpawned, Traveling, and Dead.
        /// Dead heroes are revived -- if OnDeath() was called, skills/equipment are reconstructed.
        /// Heroes already Active are returned with a success message (no-op).
        /// </summary>
        /// <param name="hero">The hero to activate</param>
        /// <param name="targetSettlement">Optional settlement to place the hero. If null, auto-resolved via GetHomeOrAlternativeSettlement()</param>
        /// <returns>BLGMResult with success/failure details</returns>
        public static BLGMResult ActivateHero(Hero hero, Settlement targetSettlement = null)
        {
            if (hero == null)
            {
                return BLGMResult.Error("ActivateHero() failed, hero cannot be null",
                    new ArgumentNullException(nameof(hero))).Log();
            }

            // Already active -- no-op
            if (hero.IsActive)
            {
                return BLGMResult.Success($"{hero.Name} is already active");
            }

            bool wasDead = hero.IsDead;
            StringBuilder details = new();

            // MARK: Revival (Dead heroes)
            if (wasDead)
            {
                BLGMResult revivalResult = ReviveHero(hero);
                if (!revivalResult.IsSuccess)
                {
                    return revivalResult;
                }

                details.Append(revivalResult.Message);
            }

            // MARK: Release from captivity
            if (hero.IsPrisoner)
            {
                EndCaptivityAction.ApplyByReleasedAfterBattle(hero);
                details.Append("\nReleased from captivity");
            }

            // MARK: Cleanup existing state
            HeroGenerator.CleanupHeroState(hero);

            // MARK: Resolve settlement
            Settlement settlement = targetSettlement ?? hero.GetHomeOrAlternativeSettlement();
            if (settlement == null)
            {
                return BLGMResult.Error("ActivateHero() failed, could not resolve a settlement to place hero",
                    new InvalidOperationException("No settlement available")).Log();
            }

            // MARK: Place hero and activate
            EnterSettlementAction.ApplyForCharacterOnly(hero, settlement);
            hero.ChangeState(Hero.CharacterStates.Active);
            hero.UpdateLastKnownClosestSettlement(settlement);

            // Heal hero to full if revived from death
            if (wasDead)
            {
                hero.HitPoints = hero.CharacterObject.MaxHitPoints();
            }

            string stateNote = wasDead ? "Revived and activated" : "Activated";
            return BLGMResult.Success(
                $"{stateNote} {hero.Name} at {settlement.Name}{details}");
        }

        /// MARK: ReviveHero
        /// <summary>
        /// Revives a dead hero by restoring their state, fixing death day, and reconstructing
        /// nulled fields if OnDeath() was called. This is a private helper for ActivateHero().
        /// </summary>
        private static BLGMResult ReviveHero(Hero hero)
        {
            StringBuilder details = new();

            // Fix DeathDay: set a random future date (3-10 years in random days)
            // so the aging system doesn't immediately kill them again
            if (hero.DeathDay.IsPast)
            {
                int minDays = 3 * CampaignTime.DaysInYear;
                int maxDays = 10 * CampaignTime.DaysInYear;
                int randomDays = RandomNumberGen.Instance.NextRandomInt(minDays, maxDays + 1);
                CampaignTime newDeathDay = CampaignTime.Now + CampaignTime.Days(randomDays);
                hero.SetDeathDay(newDeathDay);
                float yearsFromNow = randomDays / (float)CampaignTime.DaysInYear;
                details.Append($"\nDeath day was in the past, extended by ~{yearsFromNow:F1} years");
            }

            // Check if OnDeath() was called (skills/equipment nulled)
            // HeroDeveloper being null is the most reliable indicator
            bool needsReconstruction = hero.HeroDeveloper == null;

            if (needsReconstruction)
            {
                BLGMResult reconstructResult = ReconstructDeadHeroFields(hero);
                if (!reconstructResult.IsSuccess)
                {
                    return reconstructResult;
                }

                details.Append("\nReconstructed skills and equipment (OnDeath had cleared them)");
            }

            details.Insert(0, $"Revived {hero.Name} from death");
            return BLGMResult.Success(details.ToString());
        }

        /// MARK: ReconstructDeadHeroFields
        /// <summary>
        /// Reconstructs fields that were nulled by Hero.OnDeath().
        /// Uses reflection to restore private fields to default-constructed state,
        /// regenerates skills based on the hero's preserved Level, then re-equips with appropriate gear.
        /// </summary>
        private static BLGMResult ReconstructDeadHeroFields(Hero hero)
        {
            try
            {
                // Preserve the hero's level before reconstruction (Level is a direct saveable field, not nulled by OnDeath)
                int preservedLevel = hero.Level >= 1 ? hero.Level : 1;

                // Reconstruct PropertyOwner fields (skills, perks, traits, attributes)
                if (HeroSkillsField != null)
                {
                    HeroSkillsField.SetValue(hero, new PropertyOwner<SkillObject>());
                }

                if (HeroPerksField != null)
                {
                    HeroPerksField.SetValue(hero, new PropertyOwner<PerkObject>());
                }

                if (HeroTraitsField != null)
                {
                    HeroTraitsField.SetValue(hero, new PropertyOwner<TraitObject>());
                }

                if (CharacterAttributesField != null)
                {
                    CharacterAttributesField.SetValue(hero, new PropertyOwner<CharacterAttribute>());
                }

                // Reconstruct HeroDeveloper (internal constructor requires reflection)
                HeroDeveloper developer = (HeroDeveloper)Activator.CreateInstance(
                    typeof(HeroDeveloper),
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new object[] { hero },
                    null);

                if (HeroDeveloperField != null)
                {
                    HeroDeveloperField.SetValue(hero, developer);
                }

                // Reconstruct VolunteerTypes (public field, no reflection needed)
                hero.VolunteerTypes = new CharacterObject[6];

                // Reconstruct Equipment (private auto-properties)
                if (BattleEquipmentProp != null)
                {
                    BattleEquipmentProp.SetValue(hero, new Equipment(Equipment.EquipmentType.Battle));
                }

                if (CivilianEquipmentProp != null)
                {
                    CivilianEquipmentProp.SetValue(hero, new Equipment(Equipment.EquipmentType.Civilian));
                }

                if (StealthEquipmentProp != null)
                {
                    StealthEquipmentProp.SetValue(hero, new Equipment(Equipment.EquipmentType.Stealth));
                }

                // Regenerate skills based on the hero's preserved level
                HeroGenerator.RegenerateSkillsForLevel(hero, preservedLevel);

                // Re-equip with appropriate gear based on regenerated stats
                hero.AutoEquipHero(true);

                return BLGMResult.Success(
                    $"Reconstructed dead hero fields for {hero.Name} at level {preservedLevel}");
            }
            catch (Exception ex)
            {
                return BLGMResult.Error(
                    $"ReconstructDeadHeroFields() failed for {hero.Name}: {ex.Message}", ex).Log();
            }
        }

        #endregion

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
