using System;
using System.Collections.Generic;
using System.Linq;
using Bannerlord.GameMaster.Characters;
using Bannerlord.GameMaster.Console.HeroCommands;
using Bannerlord.GameMaster.Cultures;
using Bannerlord.GameMaster.Information;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

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
		///
		/// IMPORTANT: Source characters should only be Lord or Wanderer occupation to avoid conflicts.
		/// CharacterTemplatePooler.GetAllHeroTemplatesFromFlags() ensures this by filtering out notables.
		/// </summary>
		/// <param name="sourceCharacter">Character to create hero from (Lord or Wanderer occupation only)</param>
		/// <param name="nameObj">Hero's name as TextObject</param>
		/// <param name="age">Hero's age (defaults to random between 18-30, and is forced to be atleast 18)</param>
		/// <param name="clan">Hero's clan (can be null for wanderers)</param>
		/// <returns">Created hero with occupation to be set by Initialize methods</returns>
		private static Hero CreateBasicHero(CharacterObject sourceCharacter, TextObject nameObj, int age = -1, Clan clan = null)
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

			return hero;
		}

		/// MARK: InitializeAsLord
		/// <summary>
		/// layer 2: Initializes a hero as a Lord with proper occupation, equipment, and optionally creates a party.
		/// Hero must have a clan assigned before calling this method.
		/// </summary>
		/// <param name="hero">Hero to initialize as Lord</param>
		/// <param name="homeSettlement">Settlement for hero's home (used for party spawn if creating party)</param>
		/// <param name="createParty">If true, creates a party for the lord if clan in below commander limit(default: true)</param>
		public static void InitializeAsLord(Hero hero, Settlement homeSettlement, bool createParty = true)
		{
			if (hero.Clan == null)
				throw new ArgumentException("Hero must have a clan assigned before initializing as Lord");

			hero.BornSettlement = homeSettlement ?? hero.GetHomeOrAlternativeSettlement();
			hero.SetNewOccupation(Occupation.Lord);
			hero.IsMinorFactionHero = false;
			hero.UpdateHomeSettlement();

			int initalLevel = RandomNumberGen.Instance.NextRandomInt(10, 26);
			hero.HeroDeveloper.SetInitialLevel(initalLevel);
			hero.Gold = 2000 * initalLevel;		

			// Equip lord with decent gear if under level 20, Equip lord with lord gear if level 20+
			if (initalLevel < 20)
				hero.EquipHeroBasedOnCulture();
			else 
				hero.EquipLordBasedOnCulture();

			if (createParty && hero.Clan.WarPartyComponents.Count < hero.Clan.CommanderLimit)
			{
				hero.CreateParty(homeSettlement ?? hero.GetHomeOrAlternativeSettlement());
			}

			hero.UpdateLastKnownClosestSettlement(homeSettlement ?? hero.GetHomeOrAlternativeSettlement());
			hero.UpdatePowerModifier();

			// CRITICAL: Initialize hero to set IsInitialized = true
			// Without this, when clans receive settlements and notables are transferred,
			// the uninitialized clan leader can cause notable state corruption
			hero.Initialize();

			// The native pattern shows ChangeState(Active) is the final activation step that marks the hero as ready to participate in the game world
			hero.ChangeState(Hero.CharacterStates.Active);
		}

		/// MARK: InitializeAsWanderer
		/// <summary>
		/// layer 2: Initializes a hero as a Wanderer (recruitable companion in settlement).
		/// Sets clan to null, equips basic gear, and places hero in specified settlement.
		/// </summary>
		/// <param name="hero">Hero to initialize as Wanderer</param>
		/// <param name="settlement">Settlement where wanderer will wait</param>
		public static void InitializeAsWanderer(Hero hero, Settlement settlement)
		{
			hero.BornSettlement = settlement;
			hero.Clan = null;
			hero.SetNewOccupation(Occupation.Wanderer); // Crashes if not set to wanderer when you talk to them
			hero.IsMinorFactionHero = false;
			hero.EquipHeroBasedOnCulture();

			int initalLevel = RandomNumberGen.Instance.NextRandomInt(1, 15);
			hero.HeroDeveloper.SetInitialLevel(initalLevel);
			hero.Gold = 1000 * initalLevel;

			EnterSettlementAction.ApplyForCharacterOnly(hero, settlement);
			hero.UpdateLastKnownClosestSettlement(settlement);

			// CRITICAL: Initialize hero to set IsInitialized = true
			hero.Initialize();
			
			// The native pattern shows ChangeState(Active) is the final activation step that marks the hero as ready to participate in the game world
			hero.ChangeState(Hero.CharacterStates.Active);
		}

		/// MARK: InitializeAsCompanion
		/// <summary>
		/// layer 2: Initializes a hero as a Companion ready to be added to a party.
		/// Does NOT place hero in settlement - hero is in neutral active state ready for party roster.
		/// Use MobilePartyExtensions.AddCompanionToParty() after calling this method.
		/// </summary>
		/// <param name="hero">Hero to initialize as Companion</param>
		public static void InitializeAsCompanion(Hero hero)
		{
			// Keep clan assignment (should be set by caller)
			hero.BornSettlement = hero.GetHomeOrAlternativeSettlement();
			hero.SetNewOccupation(Occupation.Lord); // Ensures character is lord (if wanderer the backstory dialog shows error text. Still functions like a wanderer)
			hero.IsMinorFactionHero = false;
			hero.EquipHeroBasedOnCulture();

			int initalLevel = RandomNumberGen.Instance.NextRandomInt(1, 15);
			hero.HeroDeveloper.SetInitialLevel(initalLevel);
			hero.Gold = 1000 * initalLevel;

			// CRITICAL: Initialize hero to set IsInitialized = true
			hero.Initialize();

			// Don't place in settlement - hero is ready for party addition
			// The native pattern shows ChangeState(Active) is the final activation step that marks the hero as ready to participate in the game world
			hero.ChangeState(Hero.CharacterStates.Active);
		}

		/// MARK: CleanupHeroState
		/// <summary>
		/// Cleans up a hero's state by removing them from parties and settlements.
		/// Useful when moving heroes between roles or clans.
		/// </summary>
		/// <param name="hero">Hero to clean up</param>
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
		/// This is a high-level convenience method that combines creation and initialization.
		/// </summary>
		/// <param name="name">Name for the lord</param>
		/// <param name="cultureFlags">Culture pool to select from</param>
		/// <param name="genderFlags">Gender selection</param>
		/// <param name="clan">Clan for the lord (required)</param>
		/// <param name="withParty">If true, creates a party for the lord if clan is below commander limit</param>
		/// <param name="randomFactor">Appearance randomization factor (0-1)</param>
		/// <returns>Created and initialized lord</returns>
		public static Hero CreateLord(string name, CultureFlags cultureFlags, GenderFlags genderFlags, Clan clan, bool withParty = true, float randomFactor = 0.5f)
		{
			if (clan == null)
				throw new ArgumentException("Clan is required for Lord creation");

			var template = SelectRandomTemplate(cultureFlags, genderFlags, randomFactor);
			TextObject nameObj = new TextObject(name);

			Hero hero = CreateBasicHero(template, nameObj, -1, clan);
			Settlement homeSettlement = hero.GetHomeOrAlternativeSettlement();
			InitializeAsLord(hero, homeSettlement, withParty);

			return hero;
		}

		/// MARK: CreateLords
		/// <summary>
		/// Creates multiple lords with random names from culture.
		/// <param name="withParty">If true, creates a party for each lord if clan is below commander limit</param>
		/// </summary>
		public static List<Hero> CreateLords(int count, CultureFlags cultureFlags, GenderFlags genderFlags, Clan clan, bool withParties = true, float randomFactor = 0.5f)
		{
			if (clan == null)
				throw new ArgumentException("Clan is required for Lord creation");

			List<Hero> lords = new List<Hero>();
			CharacterTemplatePooler templatePooler = new CharacterTemplatePooler();
			List<CharacterObject> characterPool = templatePooler.GetAllHeroTemplatesFromFlags(cultureFlags, genderFlags);

			for (int i = 0; i < count; i++)
			{
				var character = SelectRandomTemplate(characterPool, randomFactor);
				string randomName = CultureLookup.GetUniqueRandomHeroName(character.Culture, character.IsFemale);
				TextObject nameObj = new TextObject(randomName);

				Hero hero = CreateBasicHero(character, nameObj, -1, clan);
				Settlement homeSettlement = hero.GetHomeOrAlternativeSettlement();
				InitializeAsLord(hero, homeSettlement, withParties);

				lords.Add(hero);
			}

			return lords;
		}

		/// MARK: CreateWanderer
		/// <summary>
		/// Creates a wanderer (recruitable companion) at the specified settlement. Layer 3: Convenience method, automatically performing layer 1 and 2 operations.
		/// </summary>
		public static Hero CreateWanderer(string name, CultureFlags cultureFlags, GenderFlags genderFlags, Settlement settlement, float randomFactor = 0.5f)
		{
			var template = SelectRandomTemplate(cultureFlags, genderFlags, randomFactor);
			TextObject nameObj = new TextObject(name);

			Hero hero = CreateBasicHero(template, nameObj);
			InitializeAsWanderer(hero, settlement);

			return hero;
		}

		/// MARK: CreateWanderers
		/// <summary>
		/// Creates multiple wanderers with random names at the specified settlement. Layer 3: Convenience method, automatically performing layer 1 and 2 operations.
		/// </summary>
		public static List<Hero> CreateWanderers(int count, CultureFlags cultureFlags, GenderFlags genderFlags, Settlement settlement, float randomFactor = 0.5f)
		{
			List<Hero> wanderers = new List<Hero>();
			CharacterTemplatePooler templatePooler = new CharacterTemplatePooler();
			List<CharacterObject> characterPool = templatePooler.GetAllHeroTemplatesFromFlags(cultureFlags, genderFlags);

			for (int i = 0; i < count; i++)
			{
				var character = SelectRandomTemplate(characterPool, randomFactor);
				string randomName = CultureLookup.GetUniqueRandomHeroName(character.Culture, character.IsFemale);
				TextObject nameObj = new TextObject(randomName);

				Hero hero = CreateBasicHero(character, nameObj);
				InitializeAsWanderer(hero, settlement);

				wanderers.Add(hero);
			}

			return wanderers;
		}

		/// MARK: CreateCompanions
		/// <summary>
		/// Creates heroes ready to be added as party companions (no settlement state). Layer 3: Convenience method, automatically performing layer 1 and 2 operations.
		/// Use MobilePartyExtensions.AddCompanionsToParty() after calling this method.
		/// </summary>
		public static List<Hero> CreateCompanions(int count, CultureFlags cultureFlags, GenderFlags genderFlags = GenderFlags.Either, float randomFactor = 0.5f)
		{
			List<Hero> companions = new List<Hero>();
			CharacterTemplatePooler templatePooler = new CharacterTemplatePooler();
			List<CharacterObject> characterPool = templatePooler.GetAllHeroTemplatesFromFlags(cultureFlags, genderFlags);

			for (int i = 0; i < count; i++)
			{
				var character = SelectRandomTemplate(characterPool, randomFactor);
				string randomName = CultureLookup.GetUniqueRandomHeroName(character.Culture, character.IsFemale);
				TextObject nameObj = new TextObject(randomName);

				Hero hero = CreateBasicHero(character, nameObj, -1);
				InitializeAsCompanion(hero);

				companions.Add(hero);
			}

			return companions;
		}

		#endregion
		#region Helper Methods

		/// <summary>
		/// Selects and optionally randomizes a character from the given culture/gender pool.
		/// Only returns Lord and Wanderer occupation characters (no notables).
		/// </summary>
		private static CharacterObject SelectRandomTemplate(CultureFlags cultureFlags, GenderFlags genderFlags, float randomFactor)
		{
			CharacterTemplatePooler templatePooler = new CharacterTemplatePooler();
			List<CharacterObject> characterPool = templatePooler.GetAllHeroTemplatesFromFlags(cultureFlags, genderFlags);
			return SelectRandomTemplate(characterPool, randomFactor);
		}

		/// <summary>
		/// Selects and optionally randomizes a character from the given pool.
		/// Creates a copy of the character and optionally randomizes appearance.
		/// </summary>
		private static CharacterObject SelectRandomTemplate(List<CharacterObject> characterPool, float randomFactor)
		{
			int randomIndex = RandomNumberGen.Instance.NextRandomInt(characterPool.Count);
			CharacterObject character = CharacterObject.CreateFrom(characterPool[randomIndex]);

			if (randomFactor > 0)
				character = RandomizeCharacterObject(character, randomFactor);

			return character;
		}

		/// <summary>
		/// Randomizes character appearance within template constraints
		/// </summary>
		public static CharacterObject RandomizeCharacterObject(CharacterObject template, float randomFactor, bool useFaceConstraints = true, bool useBuildConstraints = true, bool useHairConstraints = true)
		{
			int _seed = RandomNumberGen.Instance.NextRandomInt();

			BodyProperties minBodyProperties = template.GetBodyPropertiesMin();
			BodyProperties maxBodyProperties = template.GetBodyPropertiesMax();

			int hairCoverType = HairCoveringType.None;
			string hairTags = HairTags.All;
			string beardTags = BeardTags.All;
			string tatooTags = TattooTags.None;

			BodyProperties randomProperties = TaleWorlds.Core.FaceGen.GetRandomBodyProperties(template.Race, template.IsFemale,
					minBodyProperties, maxBodyProperties, hairCoverType, _seed, hairTags, beardTags, tatooTags, randomFactor);

			return template;
		}

		#endregion
	}
}