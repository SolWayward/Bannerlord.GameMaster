using System;
using System.Collections.Generic;
using Bannerlord.GameMaster.Characters;
using Bannerlord.GameMaster.Common;
using Bannerlord.GameMaster.Cultures;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Bannerlord.GameMaster.Heroes
{
	/// <summary>
	/// Central system for creating heroes with flexible initialization options.
	/// Separates hero creation from role initialization to prevent hidden side effects.
	/// Now uses only Lord and Wanderer occupation characters to avoid notable occupation conflicts.
	/// Uses a continuous algorithm to generate skills for any target level (uncapped).
	/// </summary>
	public static class HeroGenerator
	{
		#region Skill Category Pools

		// Combat-oriented skills: direct fighting + physical capabilities
		private static readonly SkillObject[] CombatPrimaryPool = new SkillObject[]
		{
			DefaultSkills.OneHanded, DefaultSkills.TwoHanded, DefaultSkills.Polearm,
			DefaultSkills.Bow, DefaultSkills.Crossbow, DefaultSkills.Throwing,
			DefaultSkills.Riding, DefaultSkills.Athletics
		};

		// Noncombat-oriented skills: civilian, social, and utility focused
		private static readonly SkillObject[] NoncombatPrimaryPool = new SkillObject[]
		{
			DefaultSkills.Medicine, DefaultSkills.Charm, DefaultSkills.Trade,
			DefaultSkills.Crafting, DefaultSkills.Roguery
		};

		// Mixed/utility skills: valuable for both combat and noncombat archetypes
		private static readonly SkillObject[] MixedUtilityPool = new SkillObject[]
		{
			DefaultSkills.Steward, DefaultSkills.Engineering, DefaultSkills.Leadership,
			DefaultSkills.Tactics, DefaultSkills.Scouting
		};

		// Tier weight multipliers for XP budget distribution
		private const float PrimaryMultiplier = 2.2f;
		private const float SecondaryMultiplier = 1.1f;
		private const float TertiaryMultiplier = 0.5f;

		// Variance applied to each skill value for natural randomness (+/- this percentage)
		private const float SkillVariancePercent = 0.15f;

		// Number of skills selected as primary specializations from each pool
		private const int NumPrimarySkills = 4;

		// Native XP formula constants: TotalXp = Sum(2 * skill^XpExponent) - XpOffset
		private const float XpExponent = 2.2f;
		private const float InverseXpExponent = 1f / XpExponent; // ~0.4545
		private const int XpOffset = 2000;

		// Maximum number of correction passes to land on exact target level
		private const int MaxCorrectionPasses = 10;

		#endregion

		/// MARK: CreateBasicHero Core
		/// <summary>
		/// Creates a basic hero from a character object WITHOUT any occupation-specific initialization.
		/// This is the layer 1 foundation method - it only creates the hero object with basic properties.
		/// Use Initialize methods afterward to set up role-specific state (Lord, Wanderer, Companion).
		/// Recommended to use one of the CreateLord, CreateWanderer, etc methods unless you want more control.
		/// <br /><br />
		/// IMPORTANT: Source characters should only be Lord or Wanderer occupation to avoid conflicts.
		/// CharacterTemplatePooler.GetAllHeroTemplatesFromFlags() ensures this by filtering out notables.
		/// </summary>
		/// <param name="sourceCharacter">Character to create hero from (Lord or Wanderer occupation only).</param>
		/// <param name="nameObj">Hero's name as TextObject.</param>
		/// <param name="age">Optional. Hero's age. Minimum age is 18. If not specified (default -1), or if value is less than 18,
		/// a random age between 18 and 30 (inclusive) is assigned.</param>
		/// <param name="clan">Optional. Hero's clan. Can be null for wanderers. Defaults to null.</param>
		/// <param name="randomFactor">Optional. How much hero appearance is randomized from its base template constraints (0-1). Defaults to 0 (no randomization).</param>
		/// <returns>Created hero with occupation to be set by Initialize methods.</returns>
		public static Hero CreateBasicHero(CharacterObject sourceCharacter, TextObject nameObj, int age = -1, Clan clan = null, float randomFactor = 0)
		{
			if (age < 18) //Prevents growing up prompts having to select a attribute
				age = RandomNumberGen.Instance.NextRandomInt(18, 31);

			Hero hero = HeroCreator.CreateSpecialHero(sourceCharacter, age: age);

			// Ensure birthday is set correctly as CreateSpecialHero() doesn't seem to always respect age parameter
			if ((int)hero.Age != age) // Need cast to int otherwise age 18 may not work correctly on wanderer templates
			{
				hero.SetAge(age);
			}

			hero.PreferredUpgradeFormation = FormationClass.General;
			hero.SetRandomDeathDate();
			hero.SetName(nameObj, nameObj); //Set name before registering so stringId will contain name

			// Register hero assigns stringId
			BLGMObjectManager.RegisterHero(hero);

			hero.PreferredUpgradeFormation = FormationClass.General;
			hero.Clan = clan;
			hero.IsMinorFactionHero = false;

			// NOTE: Occupation is set by Initialize methods (InitializeAsLord, InitializeAsWanderer, etc.)
			// Source character's appearance is copied but occupation will be overridden

			// Randomize appearance using the new HeroEditor instance pattern
			if (randomFactor > 0)
			{
				HeroEditor heroEditor = new(hero);
				heroEditor.BodyEditor.BodyConstraints = BodyConstraints.GenderConstraints(hero.IsFemale);
				heroEditor.RandomizeAppearance(randomFactor);
			}

			return hero;
		}

		/// MARK: InitializeAsLord
		/// <summary>
		/// Layer 2: Initializes a hero as a Lord with proper occupation, equipment, and optionally creates a party.
		/// Hero must have a clan assigned before calling this method.
		/// </summary>
		/// <param name="hero">Hero to initialize as Lord. Must have a clan assigned.</param>
		/// <param name="homeSettlement">Settlement for hero's home (used for party spawn if creating party).</param>
		/// <param name="createParty">Optional. If true, creates a party for the lord if clan is below commander limit. Defaults to true.</param>
		/// <param name="targetLevel">Optional. Target level for the hero (uncapped). If less than 1 (default -1), a random level between 10 and 25 (inclusive) is assigned.</param>
		/// <param name="isCombatFocused">Optional. If true (default), primary skills are combat-oriented. If false, primary skills are management/utility-oriented.</param>
		public static void InitializeAsLord(Hero hero, Settlement homeSettlement, bool createParty = true, int targetLevel = -1, bool isCombatFocused = true)
		{
			if (hero.Clan == null)
				throw new ArgumentException("Hero must have a clan assigned before initializing as Lord");

			hero.SetNewOccupation(Occupation.Lord);
			hero.IsMinorFactionHero = false;

			Settlement targetSettlement = hero.InitializeHomeSettlement(homeSettlement);

			// Assign random target level if level not specified or less than 0
			if (targetLevel < 1)
				targetLevel = RandomNumberGen.Instance.NextRandomInt(10, 26);

			// Initialize skills appropriate for target level BEFORE calling InitializeHeroDeveloper
			// This ensures the level calculation uses our generated skills, not template skills
			InitializeSkillsForLevel(hero, targetLevel, isCombatFocused);

			hero.Gold = 2000 * targetLevel;

			// Equip Hero with gear appropiate for level and skills
			hero.AutoEquipHero(true);

			if (createParty && hero.Clan.WarPartyComponents.Count < hero.Clan.CommanderLimit)
			{
				hero.CreateParty(homeSettlement ?? hero.GetHomeOrAlternativeSettlement());
			}

			else
			{
				EnterSettlementAction.ApplyForCharacterOnly(hero, targetSettlement);
			}

			hero.UpdateLastKnownClosestSettlement(homeSettlement ?? hero.GetHomeOrAlternativeSettlement());
			hero.UpdatePowerModifier();

			// Without this, when clans receive settlements and notables are transferred,
			// the uninitialized clan leader can cause notable state corruption
			hero.Initialize();

			// Set active
			hero.ChangeState(Hero.CharacterStates.Active);
		}

		/// MARK: InitializeAsWanderer
		/// <summary>
		/// Layer 2: Initializes a hero as a Wanderer (recruitable companion in settlement).
		/// Sets clan to null, equips basic gear, and places hero in specified settlement.
		/// </summary>
		/// <param name="hero">Hero to initialize as Wanderer.</param>
		/// <param name="settlement">Settlement where the wanderer will reside.</param>
		/// <param name="targetLevel">Optional. Target level for the hero (uncapped). If less than 1 (default -1), a random level between 1 and 14 (inclusive) is assigned.</param>
		/// <param name="isCombatFocused">Optional. If true (default), primary skills are combat-oriented. If false, primary skills are management/utility-oriented.</param>
		public static void InitializeAsWanderer(Hero hero, Settlement settlement, int targetLevel = -1, bool isCombatFocused = true)
		{
			hero.Clan = null;
			hero.InitializeHomeSettlement(settlement);
			hero.SetNewOccupation(Occupation.Wanderer); // Crashes if not set to wanderer when you talk to them
			hero.IsMinorFactionHero = false;

			// Assign random target level if level not specified or less than 0
			if (targetLevel < 1)
				targetLevel = RandomNumberGen.Instance.NextRandomInt(1, 15);

			// Initialize skills appropriate for target level BEFORE calling InitializeHeroDeveloper
			InitializeSkillsForLevel(hero, targetLevel, isCombatFocused);

			hero.Gold = 1000 * targetLevel;

			// Equip Hero with gear appropiate for level and skills
			hero.AutoEquipHero(true);

			EnterSettlementAction.ApplyForCharacterOnly(hero, settlement);

			// CRITICAL: Initialize hero to set IsInitialized = true
			hero.Initialize();

			// Set active
			hero.ChangeState(Hero.CharacterStates.Active);
		}

		/// MARK: InitializeAsCompanion
		/// <summary>
		/// Layer 2: Initializes a hero as a Companion ready to be added to a party.
		/// Does NOT place hero in settlement - hero is in neutral active state ready for party roster.
		/// Use MobilePartyExtensions.AddCompanionToParty() after calling this method.
		/// </summary>
		/// <param name="hero">Hero to initialize as Companion.</param>
		/// <param name="targetLevel">Optional. Target level for the hero (uncapped). If less than 1 (default -1), a random level between 1 and 14 (inclusive) is assigned.</param>
		/// <param name="isCombatFocused">Optional. If true (default), primary skills are combat-oriented. If false, primary skills are management/utility-oriented.</param>
		public static void InitializeAsCompanion(Hero hero, int targetLevel = -1, bool isCombatFocused = true)
		{
			// Keep clan assignment (should be set by caller)
			hero.InitializeHomeSettlement();
			hero.SetNewOccupation(Occupation.Lord); // Ensures character is lord (if wanderer the backstory dialog shows error text. Still functions like a wanderer)
			hero.IsMinorFactionHero = false;

			// Assign random target level if level not specified or less than 0
			if (targetLevel < 1)
				targetLevel = RandomNumberGen.Instance.NextRandomInt(1, 15);

			// Initialize skills appropriate for target level BEFORE calling InitializeHeroDeveloper
			InitializeSkillsForLevel(hero, targetLevel, isCombatFocused);

			hero.Gold = 1000 * targetLevel;

			// Equip Hero with gear appropiate for level and skills
			hero.AutoEquipHero(true);

			// Initialize hero to set IsInitialized = true
			hero.Initialize();

			// Don't place in settlement - hero is ready for party addition
			hero.ChangeState(Hero.CharacterStates.Active);
		}

		/// MARK: CleanupHeroState
		/// <summary>
		/// Cleans up a hero's state by removing them from parties and settlements.
		/// Useful when moving heroes between roles or clans.
		/// </summary>
		/// <param name="hero">Hero to clean up.</param>
		public static void CleanupHeroState(Hero hero)
		{
			// Destroy existing party if hero owns it
			if (hero.PartyBelongedTo != null && hero.PartyBelongedTo.Owner == hero)
			{
				DestroyPartyAction.Apply(null, hero.PartyBelongedTo);
			}

			// Remove from settlement if present
			if (hero.CurrentSettlement != null)
			{
				LeaveSettlementAction.ApplyForCharacterOnly(hero);
			}
		}

		#region Convenience Methods

		/// MARK: CreateLord
		/// <summary>
		/// Creates a lord with the specified name and culture, optionally with a party.
		/// This is a high-level Layer 3 convenience method that combines creation (Layer 1) and initialization (Layer 2).
		/// Random level range for lords: 10-25.
		/// </summary>
		/// <param name="name">Name for the lord.</param>
		/// <param name="cultureFlags">Culture pool to select character template from.</param>
		/// <param name="genderFlags">Gender selection for character template.</param>
		/// <param name="clan">Clan for the lord (required).</param>
		/// <param name="withParty">Optional. If true, creates a party for the lord if clan is below commander limit. Defaults to true.</param>
		/// <param name="settlement">Optional. Home settlement for the lord. Defaults to null (auto-resolved).</param>
		/// <param name="randomFactor">Optional. Appearance randomization factor (0-1). Defaults to 0.5.</param>
		/// <param name="targetLevel">Optional. Target level for the hero (uncapped). If less than 1 (default -1), a random level between 10 and 25 (inclusive) is assigned.</param>
		/// <param name="age">Optional. Hero's age. Minimum age is 18. If not specified (default -1), or if value is less than 18,
		/// a random age between 18 and 30 (inclusive) is assigned.</param>
		/// <param name="isCombatFocused">Optional. If true (default), primary skills are combat-oriented. If false, primary skills are management/utility-oriented.</param>
		/// <returns>Created and initialized lord hero.</returns>
		public static Hero CreateLord(string name, CultureFlags cultureFlags, GenderFlags genderFlags, Clan clan, bool withParty = true, Settlement settlement = null, float randomFactor = 0.5f, int targetLevel = -1, int age = -1, bool isCombatFocused = true)
		{
			if (clan == null)
				throw new ArgumentException("Clan is required for Lord creation");

			CharacterObject template = SelectRandomTemplate(cultureFlags, genderFlags);
			TextObject nameObj = new(name);

			Hero hero = CreateBasicHero(template, nameObj, age, clan, randomFactor);

			InitializeAsLord(hero, settlement, withParty, targetLevel, isCombatFocused);

			return hero;
		}

		/// MARK: CreateLords
		/// <summary>
		/// Creates multiple lords with random names from culture.
		/// This is a high-level Layer 3 convenience method that combines creation (Layer 1) and initialization (Layer 2).
		/// Random level range for lords: 10-25.
		/// </summary>
		/// <param name="count">Number of lords to create.</param>
		/// <param name="cultureFlags">Culture pool to select character templates from.</param>
		/// <param name="genderFlags">Gender selection for character templates.</param>
		/// <param name="clan">Clan for the lords (required).</param>
		/// <param name="withParties">Optional. If true, creates a party for each lord if clan is below commander limit. Defaults to true.</param>
		/// <param name="settlement">Optional. Home settlement for the lords. Defaults to null (auto-resolved).</param>
		/// <param name="randomFactor">Optional. Appearance randomization factor (0-1). Defaults to 0.5.</param>
		/// <param name="targetLevel">Optional. Target level for each hero (uncapped). If less than 1 (default -1), a random level between 10 and 25 (inclusive) is assigned per hero.</param>
		/// <param name="age">Optional. Age for each hero. Minimum age is 18. If not specified (default -1), or if value is less than 18,
		/// a random age between 18 and 30 (inclusive) is assigned per hero.</param>
		/// <param name="isCombatFocused">Optional. If true (default), primary skills are combat-oriented. If false, primary skills are management/utility-oriented.</param>
		/// <returns>List of created and initialized lord heroes.</returns>
		public static List<Hero> CreateLords(int count, CultureFlags cultureFlags, GenderFlags genderFlags, Clan clan, bool withParties = true, Settlement settlement = null, float randomFactor = 0.5f, int targetLevel = -1, int age = -1, bool isCombatFocused = true)
		{
			if (clan == null)
				throw new ArgumentException("Clan is required for Lord creation");

			List<Hero> lords = new();
			CharacterTemplatePooler templatePooler = new();
			List<CharacterObject> characterPool = templatePooler.GetAllHeroTemplatesFromFlags(cultureFlags, genderFlags);

			for (int i = 0; i < count; i++)
			{
				CharacterObject character = SelectRandomTemplate(characterPool);
				string randomName = CultureLookup.GetUniqueRandomHeroName(character.Culture, character.IsFemale);
				TextObject nameObj = new(randomName);

				Hero hero = CreateBasicHero(character, nameObj, age, clan, randomFactor);
				InitializeAsLord(hero, settlement, withParties, targetLevel, isCombatFocused);

				lords.Add(hero);
			}

			return lords;
		}

		/// MARK: CreateWanderer
		/// <summary>
		/// Creates a wanderer (recruitable companion) at the specified settlement.
		/// Layer 3: Convenience method, automatically performing Layer 1 and Layer 2 operations.
		/// Random level range for wanderers: 1-14.
		/// </summary>
		/// <param name="name">Name for the wanderer.</param>
		/// <param name="cultureFlags">Culture pool to select character template from.</param>
		/// <param name="genderFlags">Gender selection for character template.</param>
		/// <param name="settlement">Settlement where the wanderer will reside.</param>
		/// <param name="randomFactor">Optional. Appearance randomization factor (0-1). Defaults to 0.5.</param>
		/// <param name="targetLevel">Optional. Target level for the hero (uncapped). If less than 1 (default -1), a random level between 1 and 14 (inclusive) is assigned.</param>
		/// <param name="age">Optional. Hero's age. Minimum age is 18. If not specified (default -1), or if value is less than 18,
		/// a random age between 18 and 30 (inclusive) is assigned.</param>
		/// <param name="isCombatFocused">Optional. If true (default), primary skills are combat-oriented. If false, primary skills are management/utility-oriented.</param>
		/// <returns>Created and initialized wanderer hero.</returns>
		public static Hero CreateWanderer(string name, CultureFlags cultureFlags, GenderFlags genderFlags, Settlement settlement, float randomFactor = 0.5f, int targetLevel = -1, int age = -1, bool isCombatFocused = true)
		{
			CharacterObject template = SelectRandomTemplate(cultureFlags, genderFlags);
			TextObject nameObj = new(name);

			Hero hero = CreateBasicHero(template, nameObj, age: age, randomFactor: randomFactor);
			InitializeAsWanderer(hero, settlement, targetLevel, isCombatFocused);

			return hero;
		}

		/// MARK: CreateWanderers
		/// <summary>
		/// Creates multiple wanderers with random names at the specified settlement.
		/// Layer 3: Convenience method, automatically performing Layer 1 and Layer 2 operations.
		/// Random level range for wanderers: 1-14.
		/// </summary>
		/// <param name="count">Number of wanderers to create.</param>
		/// <param name="cultureFlags">Culture pool to select character templates from.</param>
		/// <param name="genderFlags">Gender selection for character templates.</param>
		/// <param name="settlement">Settlement where the wanderers will reside.</param>
		/// <param name="randomFactor">Optional. Appearance randomization factor (0-1). Defaults to 0.5.</param>
		/// <param name="targetLevel">Optional. Target level for each hero (uncapped). If less than 1 (default -1), a random level between 1 and 14 (inclusive) is assigned per hero.</param>
		/// <param name="age">Optional. Age for each hero. Minimum age is 18. If not specified (default -1), or if value is less than 18,
		/// a random age between 18 and 30 (inclusive) is assigned per hero.</param>
		/// <param name="isCombatFocused">Optional. If true (default), primary skills are combat-oriented. If false, primary skills are management/utility-oriented.</param>
		/// <returns>List of created and initialized wanderer heroes.</returns>
		public static List<Hero> CreateWanderers(int count, CultureFlags cultureFlags, GenderFlags genderFlags, Settlement settlement, float randomFactor = 0.5f, int targetLevel = -1, int age = -1, bool isCombatFocused = true)
		{
			List<Hero> wanderers = new();
			CharacterTemplatePooler templatePooler = new();
			List<CharacterObject> characterPool = templatePooler.GetAllHeroTemplatesFromFlags(cultureFlags, genderFlags);

			for (int i = 0; i < count; i++)
			{
				CharacterObject character = SelectRandomTemplate(characterPool);
				string randomName = CultureLookup.GetUniqueRandomHeroName(character.Culture, character.IsFemale);
				TextObject nameObj = new(randomName);

				Hero hero = CreateBasicHero(character, nameObj, age: age, randomFactor: randomFactor);
				InitializeAsWanderer(hero, settlement, targetLevel, isCombatFocused);

				wanderers.Add(hero);
			}

			return wanderers;
		}

		/// MARK: CreateCompanions
		/// <summary>
		/// Creates heroes ready to be added as party companions (no settlement state).
		/// Layer 3: Convenience method, automatically performing Layer 1 and Layer 2 operations.
		/// Use MobilePartyExtensions.AddCompanionsToParty() after calling this method.
		/// Random level range for companions: 1-14.
		/// </summary>
		/// <param name="count">Number of companions to create.</param>
		/// <param name="cultureFlags">Culture pool to select character templates from.</param>
		/// <param name="genderFlags">Optional. Gender selection for character templates. Defaults to GenderFlags.Either.</param>
		/// <param name="randomFactor">Optional. Appearance randomization factor (0-1). Defaults to 0.5.</param>
		/// <param name="targetLevel">Optional. Target level for each hero (uncapped). If less than 1 (default -1), a random level between 1 and 14 (inclusive) is assigned per hero.</param>
		/// <param name="age">Optional. Age for each hero. Minimum age is 18. If not specified (default -1), or if value is less than 18,
		/// a random age between 18 and 30 (inclusive) is assigned per hero.</param>
		/// <param name="isCombatFocused">Optional. If true (default), primary skills are combat-oriented. If false, primary skills are management/utility-oriented.</param>
		/// <returns>List of created and initialized companion heroes ready for party assignment.</returns>
		public static List<Hero> CreateCompanions(int count, CultureFlags cultureFlags, GenderFlags genderFlags = GenderFlags.Either, float randomFactor = 0.5f, int targetLevel = -1, int age = -1, bool isCombatFocused = true)
		{
			List<Hero> companions = new();
			CharacterTemplatePooler templatePooler = new();
			List<CharacterObject> characterPool = templatePooler.GetAllHeroTemplatesFromFlags(cultureFlags, genderFlags);

			for (int i = 0; i < count; i++)
			{
				CharacterObject character = SelectRandomTemplate(characterPool);
				string randomName = CultureLookup.GetUniqueRandomHeroName(character.Culture, character.IsFemale);
				TextObject nameObj = new(randomName);

				Hero hero = CreateBasicHero(character, nameObj, age: age, randomFactor: randomFactor);
				InitializeAsCompanion(hero, targetLevel, isCombatFocused);

				companions.Add(hero);
			}

			return companions;
		}

		#endregion
		#region Backward-Compatible Overloads (v1.3.14.4)

		// These overloads preserve binary compatibility for mods compiled against v1.3.14.4 or earlier.
		// Adding optional parameters to a public method is a binary-breaking change in .NET because
		// optional parameter defaults are baked into the CALLER's compiled IL at compile time.
		// Mods compiled against the old signature will look for the exact old parameter count at runtime.

		/// <summary>Backward-compatible overload for InitializeAsLord (v1.3.14.4 signature: 3 params).</summary>
		public static void InitializeAsLord(Hero hero, Settlement homeSettlement, bool createParty)
		{
			InitializeAsLord(hero, homeSettlement, createParty, targetLevel: -1, isCombatFocused: true);
		}

		/// <summary>Backward-compatible overload for InitializeAsWanderer (v1.3.14.4 signature: 2 params).</summary>
		public static void InitializeAsWanderer(Hero hero, Settlement settlement)
		{
			InitializeAsWanderer(hero, settlement, targetLevel: -1, isCombatFocused: true);
		}

		/// <summary>Backward-compatible overload for InitializeAsCompanion (v1.3.14.4 signature: 1 param).</summary>
		public static void InitializeAsCompanion(Hero hero)
		{
			InitializeAsCompanion(hero, targetLevel: -1, isCombatFocused: true);
		}

		/// <summary>Backward-compatible overload for CreateLord (v1.3.14.4 signature: 7 params).</summary>
		public static Hero CreateLord(string name, CultureFlags cultureFlags, GenderFlags genderFlags, Clan clan, bool withParty, Settlement settlement, float randomFactor)
		{
			return CreateLord(name, cultureFlags, genderFlags, clan, withParty, settlement, randomFactor, targetLevel: -1, age: -1, isCombatFocused: true);
		}

		/// <summary>Backward-compatible overload for CreateLords (v1.3.14.4 signature: 7 params).</summary>
		public static List<Hero> CreateLords(int count, CultureFlags cultureFlags, GenderFlags genderFlags, Clan clan, bool withParties, Settlement settlement, float randomFactor)
		{
			return CreateLords(count, cultureFlags, genderFlags, clan, withParties, settlement, randomFactor, targetLevel: -1, age: -1, isCombatFocused: true);
		}

		/// <summary>Backward-compatible overload for CreateWanderer (v1.3.14.4 signature: 5 params).</summary>
		public static Hero CreateWanderer(string name, CultureFlags cultureFlags, GenderFlags genderFlags, Settlement settlement, float randomFactor)
		{
			return CreateWanderer(name, cultureFlags, genderFlags, settlement, randomFactor, targetLevel: -1, age: -1, isCombatFocused: true);
		}

		/// <summary>Backward-compatible overload for CreateWanderers (v1.3.14.4 signature: 5 params).</summary>
		public static List<Hero> CreateWanderers(int count, CultureFlags cultureFlags, GenderFlags genderFlags, Settlement settlement, float randomFactor)
		{
			return CreateWanderers(count, cultureFlags, genderFlags, settlement, randomFactor, targetLevel: -1, age: -1, isCombatFocused: true);
		}

		/// <summary>Backward-compatible overload for CreateCompanions (v1.3.14.4 signature: 4 params).</summary>
		public static List<Hero> CreateCompanions(int count, CultureFlags cultureFlags, GenderFlags genderFlags, float randomFactor)
		{
			return CreateCompanions(count, cultureFlags, genderFlags, randomFactor, targetLevel: -1, age: -1, isCombatFocused: true);
		}

		#endregion

		/// MARK: RegenerateSkillsForLevel
		/// <summary>
		/// Public entry point to regenerate a hero's skills, attributes, focus, and perks for a target level.
		/// Clears existing character development state and rebuilds from scratch using the skill distribution algorithm.
		/// Useful for reviving dead heroes whose OnDeath() cleared their skills/perks/traits.
		/// If targetLevel is less than 1, uses the hero's current Level value as the target.
		/// </summary>
		/// <param name="hero">The hero whose skills will be regenerated. HeroDeveloper must not be null.</param>
		/// <param name="targetLevel">Target level. If less than 1, defaults to hero.Level (minimum 1).</param>
		/// <param name="isCombatFocused">If true, combat skills are primary. If false, noncombat skills are primary.</param>
		public static void RegenerateSkillsForLevel(Hero hero, int targetLevel = -1, bool isCombatFocused = true)
		{
			if (hero == null)
			{
				BLGMResult.Error("RegenerateSkillsForLevel() failed, hero cannot be null",
					new ArgumentNullException(nameof(hero))).Log();
				return;
			}

			if (hero.HeroDeveloper == null)
			{
				BLGMResult.Error("RegenerateSkillsForLevel() failed, hero.HeroDeveloper is null. Reconstruct it first.",
					new InvalidOperationException("HeroDeveloper is null")).Log();
				return;
			}

			// Use hero's preserved Level if no target specified
			if (targetLevel < 1)
			{
				targetLevel = hero.Level >= 1 ? hero.Level : 1;
			}

			InitializeSkillsForLevel(hero, targetLevel, isCombatFocused);
		}

		#region Skill Distribution Algorithm

		/// MARK: InitializeSkillsForLevel
		/// <summary>
		/// Initializes hero skills and attributes to match a target level using a continuous algorithm.
		/// This clears existing skills, computes an XP budget from the native level formula, distributes
		/// skill values across combat/noncombat/utility categories, and validates the result lands on
		/// the exact target level. Supports any level (uncapped).
		/// </summary>
		/// <param name="hero">The hero whose skills will be initialized.</param>
		/// <param name="targetLevel">The desired level (minimum 1, uncapped).</param>
		/// <param name="isCombatFocused">If true, combat skills are primary. If false, noncombat skills are primary.</param>
		private static void InitializeSkillsForLevel(Hero hero, int targetLevel, bool isCombatFocused = true)
		{
			if (targetLevel < 1)
				targetLevel = 1;

			// Clear existing hero state to start fresh
			hero.HeroDeveloper.ClearHero();

			// Compute the XP thresholds for this level
			long xpForLevel = ComputeSkillsRequiredForLevel(targetLevel);
			long xpForNextLevel = ComputeSkillsRequiredForLevel(targetLevel + 1);

			// Aim for 30% into the level range to leave room for variance without overshooting
			long targetTotalXp = xpForLevel + (long)((xpForNextLevel - xpForLevel) * 0.3);
			if (targetTotalXp < 1)
				targetTotalXp = 1;

			// The raw XP sum before the -2000 offset
			long targetXpSum = targetTotalXp + XpOffset;

			// Build categorized skill lists based on focus type
			List<SkillObject> primarySkills = new();
			List<SkillObject> secondarySkills = new();
			List<SkillObject> tertiarySkills = new();
			BuildSkillCategories(isCombatFocused, primarySkills, secondarySkills, tertiarySkills);

			// Compute base skill values from the XP budget
			int primaryValue = ComputeBaseSkillValue(targetXpSum, primarySkills.Count, secondarySkills.Count, tertiarySkills.Count, PrimaryMultiplier);
			int secondaryValue = ComputeBaseSkillValue(targetXpSum, primarySkills.Count, secondarySkills.Count, tertiarySkills.Count, SecondaryMultiplier);
			int tertiaryValue = ComputeBaseSkillValue(targetXpSum, primarySkills.Count, secondarySkills.Count, tertiarySkills.Count, TertiaryMultiplier);

			// Apply skills with randomized variance
			ApplySkillsWithVariance(hero, primarySkills, primaryValue);
			ApplySkillsWithVariance(hero, secondarySkills, secondaryValue);
			ApplySkillsWithVariance(hero, tertiarySkills, tertiaryValue);

			// Validate and adjust to ensure hero lands on exact target level
			ValidateAndAdjustSkills(hero, targetLevel, xpForLevel, xpForNextLevel);

			// Initialize each skill's XP to match the skill level (prevents negative XP display)
			foreach (SkillObject skill in Skills.All)
			{
				hero.HeroDeveloper.InitializeSkillXp(skill);
			}

			// Let the native system initialize the hero developer
			// This will calculate level from skills and set up attribute/focus points
			hero.HeroDeveloper.InitializeHeroDeveloper();
		}

		/// MARK: BuildSkillCategories
		/// <summary>
		/// Builds the three-tier skill category lists based on combat/noncombat focus.
		/// Primary skills get the highest values, secondary gets medium, tertiary gets lowest.
		/// Skills within each pool are shuffled so specializations vary per hero.
		/// </summary>
		/// <param name="isCombatFocused">If true, combat pool is primary and noncombat is tertiary. If false, reversed.</param>
		/// <param name="primarySkills">Output list for primary (highest) skills.</param>
		/// <param name="secondarySkills">Output list for secondary (medium) skills.</param>
		/// <param name="tertiarySkills">Output list for tertiary (lowest) skills.</param>
		private static void BuildSkillCategories(bool isCombatFocused, List<SkillObject> primarySkills, List<SkillObject> secondarySkills, List<SkillObject> tertiarySkills)
		{
			SkillObject[] focusPool = isCombatFocused ? CombatPrimaryPool : NoncombatPrimaryPool;
			SkillObject[] offPool = isCombatFocused ? NoncombatPrimaryPool : CombatPrimaryPool;

			// Shuffle the focus pool to pick random specializations
			List<SkillObject> shuffledFocus = new(focusPool);
			ShuffleList(shuffledFocus);

			// Pick NumPrimarySkills from the focus pool as primary specializations
			int primaryCount = MBMath.ClampInt(NumPrimarySkills, 1, shuffledFocus.Count);
			for (int i = 0; i < primaryCount; i++)
			{
				primarySkills.Add(shuffledFocus[i]);
			}

			// Remaining focus pool skills become secondary
			for (int i = primaryCount; i < shuffledFocus.Count; i++)
			{
				secondarySkills.Add(shuffledFocus[i]);
			}

			// Mixed/utility pool skills are always secondary (shuffled for variety)
			List<SkillObject> shuffledMixed = new(MixedUtilityPool);
			ShuffleList(shuffledMixed);
			secondarySkills.AddRange(shuffledMixed);

			// Off-focus pool skills become tertiary (shuffled for variety)
			List<SkillObject> shuffledOff = new(offPool);
			ShuffleList(shuffledOff);
			tertiarySkills.AddRange(shuffledOff);
		}

		/// MARK: ComputeBaseSkillValue
		/// <summary>
		/// Computes the base skill value for a given tier using the inverse of the native XP formula.
		/// Distributes the total XP budget across all skills using weighted multipliers, then converts
		/// each tier's XP share back to a skill value: skillValue = (xpShare / 2)^(1/2.2).
		/// </summary>
		/// <param name="targetXpSum">The total raw XP sum (before the -2000 offset) to distribute.</param>
		/// <param name="numPrimary">Number of primary tier skills.</param>
		/// <param name="numSecondary">Number of secondary tier skills.</param>
		/// <param name="numTertiary">Number of tertiary tier skills.</param>
		/// <param name="tierMultiplier">The weight multiplier for the tier being computed.</param>
		/// <returns>The base skill value for the specified tier.</returns>
		private static int ComputeBaseSkillValue(long targetXpSum, int numPrimary, int numSecondary, int numTertiary, float tierMultiplier)
		{
			// Total weight across all skills
			float totalWeight = numPrimary * PrimaryMultiplier
							  + numSecondary * SecondaryMultiplier
							  + numTertiary * TertiaryMultiplier;

			// XP budget per weight unit
			float xpPerUnit = (float)targetXpSum / totalWeight;

			// This tier's XP contribution per skill: tierMultiplier * xpPerUnit = 2 * skill^2.2
			// Solving for skill: skill = (tierMultiplier * xpPerUnit / 2)^(1/2.2)
			float xpPerSkill = tierMultiplier * xpPerUnit;
			if (xpPerSkill <= 0f)
				return 0;

			int skillValue = (int)TaleWorlds.Library.MathF.Pow(xpPerSkill / 2f, InverseXpExponent);
			return MBMath.ClampInt(skillValue, 0, 1023);
		}

		/// MARK: ApplySkillsWithVariance
		/// <summary>
		/// Applies skill values to a hero with random variance for natural-feeling distributions.
		/// Each skill gets the base value +/- SkillVariancePercent randomized.
		/// </summary>
		/// <param name="hero">The hero to assign skills to.</param>
		/// <param name="skills">List of skills in this tier.</param>
		/// <param name="baseValue">The base skill value for this tier.</param>
		private static void ApplySkillsWithVariance(Hero hero, List<SkillObject> skills, int baseValue)
		{
			foreach (SkillObject skill in skills)
			{
				int variance = (int)(baseValue * SkillVariancePercent);
				int minValue = MBMath.ClampInt(baseValue - variance, 0, 1023);
				int maxValue = MBMath.ClampInt(baseValue + variance, 0, 1023);

				int skillValue;
				if (minValue >= maxValue)
				{
					skillValue = minValue;
				}

				else
				{
					skillValue = RandomNumberGen.Instance.NextRandomInt(minValue, maxValue + 1);
				}

				hero.HeroDeveloper.SetInitialSkillLevel(skill, skillValue);
			}
		}

		/// MARK: ValidateAndAdjustSkills
		/// <summary>
		/// Validates that the hero's current skill distribution produces the correct target level.
		/// If the total XP is too high (would overshoot to next level), scales down the highest skill.
		/// If too low (would undershoot), scales up the highest skill.
		/// Uses iterative correction with a maximum number of passes.
		/// </summary>
		/// <param name="hero">The hero whose skills to validate.</param>
		/// <param name="targetLevel">The desired level.</param>
		/// <param name="xpFloor">The minimum XP required for the target level.</param>
		/// <param name="xpCeiling">The XP threshold for the next level (must stay below this).</param>
		private static void ValidateAndAdjustSkills(Hero hero, int targetLevel, long xpFloor, long xpCeiling)
		{
			for (int pass = 0; pass < MaxCorrectionPasses; pass++)
			{
				long currentXpSum = ComputeCurrentXpSum(hero);
				long currentTotalXp = currentXpSum - XpOffset;

				// Check if we're in the valid range for the target level
				if (currentTotalXp >= xpFloor && currentTotalXp < xpCeiling)
					return; // Landed on target level

				// Find the highest and lowest skill for adjustment
				SkillObject highestSkill = null;
				SkillObject lowestSkill = null;
				int highestValue = int.MinValue;
				int lowestValue = int.MaxValue;

				foreach (SkillObject skill in Skills.All)
				{
					int value = hero.GetSkillValue(skill);
					if (value > highestValue)
					{
						highestValue = value;
						highestSkill = skill;
					}

					if (value < lowestValue)
					{
						lowestValue = value;
						lowestSkill = skill;
					}
				}

				if (currentTotalXp >= xpCeiling)
				{
					// Overshot - reduce the highest skill
					if (highestSkill != null && highestValue > 0)
					{
						// Calculate how much XP we need to shed
						long excess = currentTotalXp - (xpFloor + (xpCeiling - xpFloor) / 3);
						int newValue = SkillValueFromXpContribution(
							2f * TaleWorlds.Library.MathF.Pow((float)highestValue, XpExponent) - (float)excess);
						newValue = MBMath.ClampInt(newValue, 0, highestValue - 1);
						hero.HeroDeveloper.SetInitialSkillLevel(highestSkill, newValue);
					}
				}

				else
				{
					// Undershot - increase the lowest skill
					if (lowestSkill != null)
					{
						long deficit = xpFloor + (xpCeiling - xpFloor) / 3 - currentTotalXp;
						int newValue = SkillValueFromXpContribution(
							2f * TaleWorlds.Library.MathF.Pow((float)lowestValue, XpExponent) + (float)deficit);
						newValue = MBMath.ClampInt(newValue, lowestValue + 1, 1023);
						hero.HeroDeveloper.SetInitialSkillLevel(lowestSkill, newValue);
					}
				}
			}
		}

		/// MARK: ComputeCurrentXpSum
		/// <summary>
		/// Computes the raw XP sum from all current skill values using the native formula.
		/// Raw sum = Sum(2 * skillValue^2.2) across all 18 skills.
		/// TotalXp is then rawSum - 2000.
		/// </summary>
		/// <param name="hero">The hero to compute XP sum for.</param>
		/// <returns>The raw XP sum before the -2000 offset.</returns>
		private static long ComputeCurrentXpSum(Hero hero)
		{
			float sum = 0f;
			foreach (SkillObject skill in Skills.All)
			{
				int value = hero.GetSkillValue(skill);
				sum += 2f * TaleWorlds.Library.MathF.Pow((float)value, XpExponent);
			}

			return (long)sum;
		}

		/// MARK: SkillValueFromXpContribution
		/// <summary>
		/// Converts an XP contribution amount back to a skill value using the inverse formula.
		/// Given xpContrib = 2 * skill^2.2, solves for: skill = (xpContrib / 2)^(1/2.2).
		/// </summary>
		/// <param name="xpContribution">The XP contribution to convert (can be the contribution of a single skill).</param>
		/// <returns>The skill value that would produce the given XP contribution.</returns>
		private static int SkillValueFromXpContribution(float xpContribution)
		{
			if (xpContribution <= 0f)
				return 0;

			return MBMath.ClampInt((int)TaleWorlds.Library.MathF.Pow(xpContribution / 2f, InverseXpExponent), 0, 1023);
		}

		/// MARK: ComputeSkillsRequiredForLevel
		/// <summary>
		/// Replicates the native SkillsRequiredForLevel formula without the level 62 cap.
		/// Uses long arithmetic to avoid overflow at high levels.
		/// Native formula: starts at gap=1000, each level gap += 1000 + gap/5.
		/// </summary>
		/// <param name="level">Target level to compute XP threshold for.</param>
		/// <returns>The cumulative TotalXp required to reach the specified level.</returns>
		private static long ComputeSkillsRequiredForLevel(int level)
		{
			if (level <= 0)
				return 0;

			if (level == 1)
				return 1;

			long gap = 1000;
			long cumulative = 1;

			for (int i = 2; i <= level; i++)
			{
				cumulative += gap;
				gap += 1000 + gap / 5;
			}

			return cumulative;
		}

		#endregion
		#region Template Selection

		/// MARK: SelectRandomTemplate
		/// <summary>
		/// Selects a random character template from the given culture/gender pool.
		/// Only returns Lord and Wanderer occupation characters (no notables).
		/// Creates a new CharacterTemplatePooler internally.
		/// </summary>
		/// <param name="cultureFlags">Culture pool to select from.</param>
		/// <param name="genderFlags">Gender selection filter.</param>
		/// <returns>A copy of a randomly selected CharacterObject template.</returns>
		private static CharacterObject SelectRandomTemplate(CultureFlags cultureFlags, GenderFlags genderFlags)
		{
			CharacterTemplatePooler templatePooler = new();
			List<CharacterObject> characterPool = templatePooler.GetAllHeroTemplatesFromFlags(cultureFlags, genderFlags);
			return SelectRandomTemplate(characterPool);
		}

		/// <summary>
		/// Selects a random character from the given pool and creates a copy of it.
		/// </summary>
		/// <param name="characterPool">Pre-built pool of character templates to select from.</param>
		/// <returns>A copy of a randomly selected CharacterObject from the pool.</returns>
		private static CharacterObject SelectRandomTemplate(List<CharacterObject> characterPool)
		{
			int randomIndex = RandomNumberGen.Instance.NextRandomInt(characterPool.Count);
			CharacterObject character = CharacterObject.CreateFrom(characterPool[randomIndex]);

			return character;
		}

		/// MARK: ShuffleList
		/// <summary>
		/// Fisher-Yates shuffle for randomizing skill selection order.
		/// </summary>
		/// <typeparam name="T">Type of elements in the list.</typeparam>
		/// <param name="list">List to shuffle in-place.</param>
		private static void ShuffleList<T>(List<T> list)
		{
			int n = list.Count;
			for (int i = n - 1; i > 0; i--)
			{
				int j = RandomNumberGen.Instance.NextRandomInt(i + 1);
				T temp = list[i];
				list[i] = list[j];
				list[j] = temp;
			}
		}

		#endregion
	}
}
