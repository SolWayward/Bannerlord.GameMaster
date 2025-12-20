using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using Bannerlord.GameMaster.Common.Interfaces;
using NavalDLC;

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
		/// Gets templates all templates
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
		/// Gets templates of all the main factions
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

		public List<CharacterObject> GetTemplatesFromFlags(CultureFlags cultureFlags, GenderFlags genderFlags)
		{
			// Check group flags first and return if just groups set
			if (cultureFlags == CultureFlags.AllCultures)
			{
				return genderFlags switch
				{
					GenderFlags.Female => FemaleTemplates,
					GenderFlags.Male => MaleTemplates,
					_ => AllTemplates
				};
			}

			if (cultureFlags == CultureFlags.AllMainCultures)
			{
				return genderFlags switch
				{
					GenderFlags.Female => MainFactionFemaleTemplates,
					GenderFlags.Male => MainFactionMaleTemplates,
					_ => MainFactionTemplates
				};
			}

			if (cultureFlags == CultureFlags.AllBanditCultures)
			{
				return genderFlags switch
				{
					GenderFlags.Female => BanditFemaleTemplates,
					GenderFlags.Male => BanditMaleTemplates,
					_ => BanditTemplates
				};
			}

			// No group set, or more than just group set, check individual cultures and acumulate.
			List<CharacterObject> templates = new();

			// Check individual flags - these accumulate
			if (cultureFlags.HasFlag(CultureFlags.Calradian))
				templates.AddRange(GetCulturalTemplates(CultureLookup.CalradianNeutral));

			if (cultureFlags.HasFlag(CultureFlags.Aserai))
				templates.AddRange(GetCulturalTemplates(CultureLookup.Aserai));

			if (cultureFlags.HasFlag(CultureFlags.Battania))
				templates.AddRange(GetCulturalTemplates(CultureLookup.Battania));

			if (cultureFlags.HasFlag(CultureFlags.Empire))
				templates.AddRange(GetCulturalTemplates(CultureLookup.Empire));

			if (cultureFlags.HasFlag(CultureFlags.Khuzait))
				templates.AddRange(GetCulturalTemplates(CultureLookup.Khuzait));

			if (cultureFlags.HasFlag(CultureFlags.Nord))
				templates.AddRange(GetCulturalTemplates(CultureLookup.Nord));

			if (cultureFlags.HasFlag(CultureFlags.Sturgia))
				templates.AddRange(GetCulturalTemplates(CultureLookup.Sturgia));

			if (cultureFlags.HasFlag(CultureFlags.Vlandia))
				templates.AddRange(GetCulturalTemplates(CultureLookup.Vlandia));

			if (cultureFlags.HasFlag(CultureFlags.Corsairs))
				templates.AddRange(GetCulturalTemplates(CultureLookup.Corsairs));

			if (cultureFlags.HasFlag(CultureFlags.DesertBandits))
				templates.AddRange(GetCulturalTemplates(CultureLookup.DesertBandits));

			if (cultureFlags.HasFlag(CultureFlags.ForestBandits))
				templates.AddRange(GetCulturalTemplates(CultureLookup.ForestBandits));

			if (cultureFlags.HasFlag(CultureFlags.MountainBandits))
				templates.AddRange(GetCulturalTemplates(CultureLookup.MountainBandits));

			if (cultureFlags.HasFlag(CultureFlags.SeaRaiders))
				templates.AddRange(GetCulturalTemplates(CultureLookup.SeaRaiders));

			if (cultureFlags.HasFlag(CultureFlags.SteppeBandits))
				templates.AddRange(GetCulturalTemplates(CultureLookup.SteppeBandits));	

			if (cultureFlags.HasFlag(CultureFlags.DarshiSpecial))
				templates.AddRange(GetCulturalTemplates(CultureLookup.DarshiSpecial));

			if (cultureFlags.HasFlag(CultureFlags.VakkenSpecial))
				templates.AddRange(GetCulturalTemplates(CultureLookup.VakkenSpecial));

			return templates;
		}

	}
}
