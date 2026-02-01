using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using Bannerlord.GameMaster.Common.Interfaces;
using NavalDLC;
using Bannerlord.GameMaster.Cultures;

namespace Bannerlord.GameMaster.Characters
{
	public class CharacterTemplatePooler
	{
		List<CharacterObject> _allTemplates;
		List<CharacterObject> _femaleTemplates;
		List<CharacterObject> _maleTemplates;

		List<CharacterObject> _mainFactionTemplates;
		List<CharacterObject> _mainFactionMaleTemplates;
		List<CharacterObject> _mainFactionFemaleTemplates;

		List<CharacterObject> _banditTemplates;
		List<CharacterObject> _banditFemaleTemplates;
		List<CharacterObject> _banditMaleTemplates;

		public string Debug_CountTemplates()
		{
			int all = 0;
			int female = 0;
			int heroAll = 0;
			int heroFemale = 0;

			foreach (CharacterObject character in AllTemplates)
			{
				if (character.Occupation == Occupation.Lord)
				{
					all++;
					if (character.IsFemale)
						female++;
				}
				else if (character.IsHero)
				{
					heroAll++;
					if (character.IsFemale)
						heroFemale++;
				}
			}

			return $"Occupation Lord: {all}, Female: {female}\nIsHero: {heroAll}, Female: {heroFemale}";
		}

		//If backing field is null, get list (Allows list to be cached and reused without relooping eveytime) or not take any memory if that specific list is never used
		public List<CharacterObject> AllTemplates => _allTemplates ??= GetAllTemplates();
		public List<CharacterObject> FemaleTemplates => _femaleTemplates ??= FilterByGender(AllTemplates, true);
		public List<CharacterObject> MaleTemplates => _maleTemplates ??= FilterByGender(AllTemplates, false);

		/// <summary>
		/// Main Map factions
		/// </summary>
		public List<CharacterObject> MainFactionTemplates => _mainFactionTemplates ??= GetMainFactionTemplates();
		public List<CharacterObject> MainFactionFemaleTemplates => _mainFactionFemaleTemplates ??= FilterByGender(MainFactionTemplates, true);
		public List<CharacterObject> MainFactionMaleTemplates => _mainFactionMaleTemplates ??= FilterByGender(MainFactionTemplates, false);

		/// <summary>
		/// Loots, bandits, and pirates
		/// </summary>
		public List<CharacterObject> BanditTemplates => _banditTemplates ??= GetBanditTemplates();
		public List<CharacterObject> BanditFemaleTemplates => _banditFemaleTemplates ??= FilterByGender(BanditTemplates, true);
		public List<CharacterObject> BanditMaleTemplates => _banditMaleTemplates ??= FilterByGender(BanditTemplates, false);

		/// <summary>
		/// Gets all templates templates
		/// </summary>
		List<CharacterObject> GetAllTemplates()
		{
			List<CharacterObject> templates = new();

			foreach (CharacterObject characterObject in CharacterObject.All)
			{
				if (characterObject.IsTemplate)
					templates.Add(characterObject);
			}

			return templates;
		}

		/// <summary>
		/// Gets templates of all the main factions
		/// </summary>
		List<CharacterObject> GetMainFactionTemplates()
		{
			List<CharacterObject> templates = new();

			foreach (CharacterObject characterObject in AllTemplates)
			{
				if (characterObject.Culture.IsMainCulture)
					templates.Add(characterObject);
			}

			return templates;
		}

		/// <summary>
		/// Gets templates of all the bandit factions
		/// </summary>
		List<CharacterObject> GetBanditTemplates()
		{
			List<CharacterObject> templates = new();

			foreach (CharacterObject characterObject in AllTemplates)
			{
				if (characterObject.Culture.IsBandit || characterObject.IsPirate())
					templates.Add(characterObject);
			}

			return templates;
		}

		/// <summary>
		/// Filters the by the specified gender using the supplied list
		/// </summary>
		List<CharacterObject> FilterByGender(List<CharacterObject> listToFilter, bool isFemale)
		{
			List<CharacterObject> genderTemplates = new();

			if (listToFilter == null)
				return genderTemplates; // Should I return null instead?

			foreach (CharacterObject template in listToFilter)
			{
				if (template.IsFemale == isFemale)
					genderTemplates.Add(template);
			}

			return genderTemplates;
		}

		/// <summary>
		/// Filters character objects to ONLY include Lord and Wanderer occupations.
		/// Includes both templates and actual characters for maximum variety.
		/// Excludes notables (RuralNotable, Headman, Artisan, Merchant, etc.) to prevent occupation conflicts.
		/// Excludes dead heroes to avoid using deceased character appearances.
		/// </summary>
		List<CharacterObject> FilterToLordAndWandererCharacters(List<CharacterObject> listToFilter)
		{
			List<CharacterObject> heroCharacters = new();
	
			if (listToFilter == null)
				return heroCharacters;
	
			foreach (CharacterObject character in listToFilter)
			{
				// Check if it's a dead hero - skip if dead
				if (character.IsHero && character.HeroObject != null && !character.HeroObject.IsAlive)
				{
					continue;
				}
	
				// ONLY include Lord and Wanderer occupations
				// This excludes ALL notable types (RuralNotable, Headman, Artisan, Merchant, GangLeader, Preacher)
				// and also excludes Soldiers
				if (character.Occupation == Occupation.Lord || character.Occupation == Occupation.Wanderer)
				{
					heroCharacters.Add(character);
				}
			}
	
			return heroCharacters;
		}

		/// <summary>
		/// Gets the character templates of a single specified culture </br>
		/// Use CultureLookup class to easily retrieve CultureObjects to use.
		/// </summary>
		public List<CharacterObject> GetCulturalTemplates(CultureObject culture)
		{
			List<CharacterObject> templates = new();

			foreach (CharacterObject template in AllTemplates)
			{
				if (template.Culture == culture)
					templates.Add(template);
			}

			return templates;
		}

		/// <summary>
		/// Gets ONLY Lord and Wanderer character objects (both templates and actual characters) for a single specified culture.
		/// Excludes notables and soldiers to prevent occupation conflicts.
		/// Safe for use in all hero creation (lords, wanderers, companions).
		/// </summary>
		public List<CharacterObject> GetLordAndWandererCharacters(CultureObject culture)
		{
			return FilterToLordAndWandererCharacters(GetCulturalTemplates(culture));
		}

		/// <summary>
		/// Gets the character templates of the specified gender for a single culture </br>
		/// Use CultureLookup class to easily retrieve CultureObjects to use. </br>
		/// if false is provided as an argument, males will be returned, if true is provided females are returned
		/// </summary>
		public List<CharacterObject> GetGenderCulturalTemplates(CultureObject culture, bool isFemale)
		{
			List<CharacterObject> templates = new();

			if (isFemale)
			{
				foreach (CharacterObject femaleTemplate in FemaleTemplates)
				{
					if (femaleTemplate.Culture == culture)
						templates.Add(femaleTemplate);
				}
			}

			else
			{
				foreach (CharacterObject template in MaleTemplates)
				{
					if (template.Culture == culture)
						templates.Add(template);
				}
			}

			return templates;
		}

		/// <summary>
		/// Gets character objects suitable for ALL hero creation (lords, wanderers, companions).
		/// Returns ONLY Lord and Wanderer occupations (both templates and actual characters).
		/// Excludes ALL notable occupations (Headman, RuralNotable, Artisan, Merchant, etc.) to prevent crashes.
		/// Excludes dead heroes to avoid using deceased character appearances.
		/// Provides maximum character variety, especially for female characters.
		/// </summary>
		public List<CharacterObject> GetAllHeroTemplatesFromFlags(CultureFlags cultureFlags, GenderFlags genderFlags)
		{
			List<CharacterObject> characters = new();
	
			// For group flags, we need to get all characters and filter
			if (cultureFlags == CultureFlags.AllCultures)
			{
				// Get all characters (templates and non-templates)
				var allChars = CharacterObject.All.ToList();
				characters = FilterToLordAndWandererCharacters(allChars);
			}
			else if (cultureFlags == CultureFlags.AllMainCultures)
			{
				// Get all main culture characters
				var mainCultureChars = CharacterObject.All
					.Where(c => c.Culture != null && c.Culture.IsMainCulture)
					.ToList();
				characters = FilterToLordAndWandererCharacters(mainCultureChars);
			}
			else if (cultureFlags == CultureFlags.AllBanditCultures)
			{
				// Get all bandit culture characters
				var banditChars = CharacterObject.All
					.Where(c => c.Culture != null && (c.Culture.IsBandit || c.IsPirate()))
					.ToList();
				characters = FilterToLordAndWandererCharacters(banditChars);
			}
			else
			{
				// Individual culture flags - accumulate characters from each specified culture
				if (cultureFlags.HasFlag(CultureFlags.Calradian))
				{
					var cultureChars = CharacterObject.All.Where(c => c.Culture == CultureLookup.CalradianNeutral).ToList();
					characters.AddRange(FilterToLordAndWandererCharacters(cultureChars));
				}
	
				if (cultureFlags.HasFlag(CultureFlags.Aserai))
				{
					var cultureChars = CharacterObject.All.Where(c => c.Culture == CultureLookup.Aserai).ToList();
					characters.AddRange(FilterToLordAndWandererCharacters(cultureChars));
				}
	
				if (cultureFlags.HasFlag(CultureFlags.Battania))
				{
					var cultureChars = CharacterObject.All.Where(c => c.Culture == CultureLookup.Battania).ToList();
					characters.AddRange(FilterToLordAndWandererCharacters(cultureChars));
				}
	
				if (cultureFlags.HasFlag(CultureFlags.Empire))
				{
					var cultureChars = CharacterObject.All.Where(c => c.Culture == CultureLookup.Empire).ToList();
					characters.AddRange(FilterToLordAndWandererCharacters(cultureChars));
				}
	
				if (cultureFlags.HasFlag(CultureFlags.Khuzait))
				{
					var cultureChars = CharacterObject.All.Where(c => c.Culture == CultureLookup.Khuzait).ToList();
					characters.AddRange(FilterToLordAndWandererCharacters(cultureChars));
				}
	
				if (cultureFlags.HasFlag(CultureFlags.Nord))
				{
					var cultureChars = CharacterObject.All.Where(c => c.Culture == CultureLookup.Nord).ToList();
					characters.AddRange(FilterToLordAndWandererCharacters(cultureChars));
				}
	
				if (cultureFlags.HasFlag(CultureFlags.Sturgia))
				{
					var cultureChars = CharacterObject.All.Where(c => c.Culture == CultureLookup.Sturgia).ToList();
					characters.AddRange(FilterToLordAndWandererCharacters(cultureChars));
				}
	
				if (cultureFlags.HasFlag(CultureFlags.Vlandia))
				{
					var cultureChars = CharacterObject.All.Where(c => c.Culture == CultureLookup.Vlandia).ToList();
					characters.AddRange(FilterToLordAndWandererCharacters(cultureChars));
				}
	
				if (cultureFlags.HasFlag(CultureFlags.Corsairs))
				{
					var cultureChars = CharacterObject.All.Where(c => c.Culture == CultureLookup.Corsairs).ToList();
					characters.AddRange(FilterToLordAndWandererCharacters(cultureChars));
				}
	
				if (cultureFlags.HasFlag(CultureFlags.Deserters))
				{
					List<CharacterObject> cultureChars = CharacterObject.All.Where(c => c.Culture == CultureLookup.Deserters).ToList();
					characters.AddRange(FilterToLordAndWandererCharacters(cultureChars));
				}

				if (cultureFlags.HasFlag(CultureFlags.DesertBandits))
				{
					List<CharacterObject> cultureChars = CharacterObject.All.Where(c => c.Culture == CultureLookup.DesertBandits).ToList();
					characters.AddRange(FilterToLordAndWandererCharacters(cultureChars));
				}
	
				if (cultureFlags.HasFlag(CultureFlags.ForestBandits))
				{
					var cultureChars = CharacterObject.All.Where(c => c.Culture == CultureLookup.ForestBandits).ToList();
					characters.AddRange(FilterToLordAndWandererCharacters(cultureChars));
				}
	
				if (cultureFlags.HasFlag(CultureFlags.MountainBandits))
				{
					var cultureChars = CharacterObject.All.Where(c => c.Culture == CultureLookup.MountainBandits).ToList();
					characters.AddRange(FilterToLordAndWandererCharacters(cultureChars));
				}
	
				if (cultureFlags.HasFlag(CultureFlags.SeaRaiders))
				{
					var cultureChars = CharacterObject.All.Where(c => c.Culture == CultureLookup.SeaRaiders).ToList();
					characters.AddRange(FilterToLordAndWandererCharacters(cultureChars));
				}
	
				if (cultureFlags.HasFlag(CultureFlags.SteppeBandits))
				{
					var cultureChars = CharacterObject.All.Where(c => c.Culture == CultureLookup.SteppeBandits).ToList();
					characters.AddRange(FilterToLordAndWandererCharacters(cultureChars));
				}
	
				if (cultureFlags.HasFlag(CultureFlags.DarshiSpecial))
				{
					var cultureChars = CharacterObject.All.Where(c => c.Culture == CultureLookup.DarshiSpecial).ToList();
					characters.AddRange(FilterToLordAndWandererCharacters(cultureChars));
				}
	
				if (cultureFlags.HasFlag(CultureFlags.VakkenSpecial))
				{
					var cultureChars = CharacterObject.All.Where(c => c.Culture == CultureLookup.VakkenSpecial).ToList();
					characters.AddRange(FilterToLordAndWandererCharacters(cultureChars));
				}
			}
	
			// Apply gender filtering to accumulated characters
			return genderFlags switch
			{
				GenderFlags.Female => FilterByGender(characters, true),
				GenderFlags.Male => FilterByGender(characters, false),
				_ => characters
			};
		}

	}
}
