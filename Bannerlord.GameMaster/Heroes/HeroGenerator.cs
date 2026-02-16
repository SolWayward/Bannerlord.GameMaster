using System;
using System.Collections.Generic;
using Bannerlord.GameMaster.Characters;
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
	/// </summary>
	public static class HeroGenerator
	{
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
			if (hero.Age < age)
			{
				// Force correct age
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
		/// <param name="targetLevel">Optional. Target level for the hero. If less than 1 (default -1), a random level between 10 and 25 (inclusive) is assigned.</param>
		public static void InitializeAsLord(Hero hero, Settlement homeSettlement, bool createParty = true, int targetLevel = -1)
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
			InitializeSkillsForLevel(hero, targetLevel);

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
		/// <param name="targetLevel">Optional. Target level for the hero. If less than 1 (default -1), a random level between 1 and 14 (inclusive) is assigned.</param>
		public static void InitializeAsWanderer(Hero hero, Settlement settlement, int targetLevel = -1)
		{
			hero.Clan = null;
			hero.InitializeHomeSettlement(settlement);
			hero.SetNewOccupation(Occupation.Wanderer); // Crashes if not set to wanderer when you talk to them
			hero.IsMinorFactionHero = false;

			// Assign random target level if level not specified or less than 0
			if (targetLevel < 1)
				targetLevel = RandomNumberGen.Instance.NextRandomInt(1, 15);

			// Initialize skills appropriate for target level BEFORE calling InitializeHeroDeveloper
			InitializeSkillsForLevel(hero, targetLevel);

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
		/// <param name="targetLevel">Optional. Target level for the hero. If less than 1 (default -1), a random level between 1 and 14 (inclusive) is assigned.</param>
		public static void InitializeAsCompanion(Hero hero, int targetLevel = -1)
		{
			// Keep clan assignment (should be set by caller)
			hero.InitializeHomeSettlement();
			hero.SetNewOccupation(Occupation.Lord); // Ensures character is lord (if wanderer the backstory dialog shows error text. Still functions like a wanderer)
			hero.IsMinorFactionHero = false;

			// Assign random target level if level not specified or less than 0
			if (targetLevel < 1)
				targetLevel = RandomNumberGen.Instance.NextRandomInt(1, 15);

			// Initialize skills appropriate for target level BEFORE calling InitializeHeroDeveloper
			InitializeSkillsForLevel(hero, targetLevel);

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
		/// <param name="targetLevel">Optional. Target level for the hero. If less than 1 (default -1), a random level between 10 and 25 (inclusive) is assigned.</param>
		/// <param name="age">Optional. Hero's age. Minimum age is 18. If not specified (default -1), or if value is less than 18,
		/// a random age between 18 and 30 (inclusive) is assigned.</param>
		/// <returns>Created and initialized lord hero.</returns>
		public static Hero CreateLord(string name, CultureFlags cultureFlags, GenderFlags genderFlags, Clan clan, bool withParty = true, Settlement settlement = null, float randomFactor = 0.5f, int targetLevel = -1, int age = -1)
		{
			if (clan == null)
				throw new ArgumentException("Clan is required for Lord creation");

			CharacterObject template = SelectRandomTemplate(cultureFlags, genderFlags);
			TextObject nameObj = new(name);

			Hero hero = CreateBasicHero(template, nameObj, age, clan, randomFactor);

			InitializeAsLord(hero, settlement, withParty, targetLevel);

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
		/// <param name="targetLevel">Optional. Target level for each hero. If less than 1 (default -1), a random level between 10 and 25 (inclusive) is assigned per hero.</param>
		/// <param name="age">Optional. Age for each hero. Minimum age is 18. If not specified (default -1), or if value is less than 18,
		/// a random age between 18 and 30 (inclusive) is assigned per hero.</param>
		/// <returns>List of created and initialized lord heroes.</returns>
		public static List<Hero> CreateLords(int count, CultureFlags cultureFlags, GenderFlags genderFlags, Clan clan, bool withParties = true, Settlement settlement = null, float randomFactor = 0.5f, int targetLevel = -1, int age = -1)
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
				InitializeAsLord(hero, settlement, withParties, targetLevel);

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
		/// <param name="targetLevel">Optional. Target level for the hero. If less than 1 (default -1), a random level between 1 and 14 (inclusive) is assigned.</param>
		/// <param name="age">Optional. Hero's age. Minimum age is 18. If not specified (default -1), or if value is less than 18,
		/// a random age between 18 and 30 (inclusive) is assigned.</param>
		/// <returns>Created and initialized wanderer hero.</returns>
		public static Hero CreateWanderer(string name, CultureFlags cultureFlags, GenderFlags genderFlags, Settlement settlement, float randomFactor = 0.5f, int targetLevel = -1, int age = -1)
		{
			CharacterObject template = SelectRandomTemplate(cultureFlags, genderFlags);
			TextObject nameObj = new(name);

			Hero hero = CreateBasicHero(template, nameObj, age: age, randomFactor: randomFactor);
			InitializeAsWanderer(hero, settlement, targetLevel);

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
		/// <param name="targetLevel">Optional. Target level for each hero. If less than 1 (default -1), a random level between 1 and 14 (inclusive) is assigned per hero.</param>
		/// <param name="age">Optional. Age for each hero. Minimum age is 18. If not specified (default -1), or if value is less than 18,
		/// a random age between 18 and 30 (inclusive) is assigned per hero.</param>
		/// <returns>List of created and initialized wanderer heroes.</returns>
		public static List<Hero> CreateWanderers(int count, CultureFlags cultureFlags, GenderFlags genderFlags, Settlement settlement, float randomFactor = 0.5f, int targetLevel = -1, int age = -1)
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
				InitializeAsWanderer(hero, settlement, targetLevel);

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
		/// <param name="targetLevel">Optional. Target level for each hero. If less than 1 (default -1), a random level between 1 and 14 (inclusive) is assigned per hero.</param>
		/// <param name="age">Optional. Age for each hero. Minimum age is 18. If not specified (default -1), or if value is less than 18,
		/// a random age between 18 and 30 (inclusive) is assigned per hero.</param>
		/// <returns>List of created and initialized companion heroes ready for party assignment.</returns>
		public static List<Hero> CreateCompanions(int count, CultureFlags cultureFlags, GenderFlags genderFlags = GenderFlags.Either, float randomFactor = 0.5f, int targetLevel = -1, int age = -1)
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
				InitializeAsCompanion(hero, targetLevel);

				companions.Add(hero);
			}

			return companions;
		}

		#endregion
		#region Helper Methods

		/// MARK: InitializeSkillsForLevel
		/// <summary>
		/// Initializes hero skills and attributes to match a target level.
		/// This clears existing skills and generates appropriate values so that
		/// InitializeHeroDeveloper() will calculate the correct level from skills.
		/// Also syncs XP for each skill and properly distributes attribute/focus points.
		/// </summary>
		/// <param name="hero">The hero whose skills will be initialized.</param>
		/// <param name="targetLevel">The desired level (clamped to 1-62).</param>
		private static void InitializeSkillsForLevel(Hero hero, int targetLevel)
		{
			targetLevel = MBMath.ClampInt(targetLevel, 1, 62);

			// Clear existing hero state to start fresh
			hero.HeroDeveloper.ClearHero();

			// Get skill distribution configuration for this level
			SkillDistributionConfig config = GetSkillDistributionForLevel(targetLevel);

			// Generate skills that will result in the target level
			// The native formula is: TotalXp = Sum(2 * skillValue^2.2) - 2000
			// We distribute skill points across skills to reach the required total
			GenerateSkillsForConfig(hero, config);

			// Initialize each skill's XP to match the skill level (prevents negative XP display)
			foreach (SkillObject skill in Skills.All)
			{
				hero.HeroDeveloper.InitializeSkillXp(skill);
			}

			// Now let the native system initialize the hero developer
			// This will calculate level from skills and set up attribute/focus points
			hero.HeroDeveloper.InitializeHeroDeveloper();
		}

		/// <summary>
		/// Configuration for skill distribution at a given level range.
		/// </summary>
		private struct SkillDistributionConfig
		{
			public int PrimarySkillMin;      // Primary combat skills (3-4 skills)
			public int PrimarySkillMax;
			public int SecondarySkillMin;    // Secondary skills (3-4 skills)
			public int SecondarySkillMax;
			public int TertiarySkillMin;     // Remaining skills
			public int TertiarySkillMax;
			public int NumPrimarySkills;
			public int NumSecondarySkills;
		}

		/// <summary>
		/// Gets skill distribution configuration appropriate for the target level.
		/// Skill distributions are calibrated to produce correct levels using the
		/// native formula: TotalXp = Sum(2 * skill^2.2) - 2000.
		/// </summary>
		/// <param name="level">Target level to get distribution config for.</param>
		/// <returns>A SkillDistributionConfig with min/max ranges for primary, secondary, and tertiary skills.</returns>
		private static SkillDistributionConfig GetSkillDistributionForLevel(int level)
		{
			// Skill distributions calibrated to produce correct levels
			// Native formula: TotalXp = Sum(2 * skill^2.2) - 2000
			if (level <= 5)
			{
				return new SkillDistributionConfig
				{
					PrimarySkillMin = 30,
					PrimarySkillMax = 50,
					SecondarySkillMin = 15,
					SecondarySkillMax = 30,
					TertiarySkillMin = 0,
					TertiarySkillMax = 15,
					NumPrimarySkills = 3,
					NumSecondarySkills = 3
				};
			}
			else if (level <= 10)
			{
				return new SkillDistributionConfig
				{
					PrimarySkillMin = 50,
					PrimarySkillMax = 80,
					SecondarySkillMin = 25,
					SecondarySkillMax = 50,
					TertiarySkillMin = 5,
					TertiarySkillMax = 25,
					NumPrimarySkills = 3,
					NumSecondarySkills = 4
				};
			}
			else if (level <= 15)
			{
				return new SkillDistributionConfig
				{
					PrimarySkillMin = 80,
					PrimarySkillMax = 120,
					SecondarySkillMin = 40,
					SecondarySkillMax = 70,
					TertiarySkillMin = 10,
					TertiarySkillMax = 35,
					NumPrimarySkills = 4,
					NumSecondarySkills = 4
				};
			}
			else if (level <= 20)
			{
				return new SkillDistributionConfig
				{
					PrimarySkillMin = 120,
					PrimarySkillMax = 160,
					SecondarySkillMin = 60,
					SecondarySkillMax = 100,
					TertiarySkillMin = 20,
					TertiarySkillMax = 50,
					NumPrimarySkills = 4,
					NumSecondarySkills = 4
				};
			}
			else if (level <= 25)
			{
				return new SkillDistributionConfig
				{
					PrimarySkillMin = 160,
					PrimarySkillMax = 200,
					SecondarySkillMin = 80,
					SecondarySkillMax = 130,
					TertiarySkillMin = 30,
					TertiarySkillMax = 60,
					NumPrimarySkills = 4,
					NumSecondarySkills = 4
				};
			}
			else // level > 25
			{
				return new SkillDistributionConfig
				{
					PrimarySkillMin = 200,
					PrimarySkillMax = 250,
					SecondarySkillMin = 100,
					SecondarySkillMax = 160,
					TertiarySkillMin = 40,
					TertiarySkillMax = 80,
					NumPrimarySkills = 4,
					NumSecondarySkills = 5
				};
			}
		}

		/// <summary>
		/// Generates and assigns skills based on the distribution config.
		/// Skills are randomly shuffled and then assigned to primary, secondary, and tertiary categories
		/// with random values within each category's min/max range.
		/// </summary>
		/// <param name="hero">The hero to assign skills to.</param>
		/// <param name="config">Skill distribution configuration defining ranges and category counts.</param>
		private static void GenerateSkillsForConfig(Hero hero, SkillDistributionConfig config)
		{
			// Create shuffled list of all skills for random selection
			List<SkillObject> shuffledSkills = new(Skills.All);
			ShuffleList(shuffledSkills);

			int skillIndex = 0;

			// Assign primary skills (highest values)
			for (int i = 0; i < config.NumPrimarySkills && skillIndex < shuffledSkills.Count; i++)
			{
				int skillValue = RandomNumberGen.Instance.NextRandomInt(config.PrimarySkillMin, config.PrimarySkillMax + 1);
				hero.HeroDeveloper.SetInitialSkillLevel(shuffledSkills[skillIndex], skillValue);
				skillIndex++;
			}

			// Assign secondary skills (medium values)
			for (int i = 0; i < config.NumSecondarySkills && skillIndex < shuffledSkills.Count; i++)
			{
				int skillValue = RandomNumberGen.Instance.NextRandomInt(config.SecondarySkillMin, config.SecondarySkillMax + 1);
				hero.HeroDeveloper.SetInitialSkillLevel(shuffledSkills[skillIndex], skillValue);
				skillIndex++;
			}

			// Assign tertiary skills (lowest values) to remaining skills
			while (skillIndex < shuffledSkills.Count)
			{
				int skillValue = RandomNumberGen.Instance.NextRandomInt(config.TertiarySkillMin, config.TertiarySkillMax + 1);
				hero.HeroDeveloper.SetInitialSkillLevel(shuffledSkills[skillIndex], skillValue);
				skillIndex++;
			}
		}

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

		#endregion
	}
}