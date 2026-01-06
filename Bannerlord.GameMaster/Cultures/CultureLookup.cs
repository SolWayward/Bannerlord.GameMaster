using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using Bannerlord.GameMaster.Common.Interfaces;
using TaleWorlds.ObjectSystem;
using TaleWorlds.Localization;

namespace Bannerlord.GameMaster.Cultures
{
	public static class CultureLookup
	{
		/// MARK: Culture Lookup
		public static CultureObject Aserai => MBObjectManager.Instance.GetObject<CultureObject>("aserai");
		public static CultureObject Battania => MBObjectManager.Instance.GetObject<CultureObject>("battania");
		public static CultureObject Empire => MBObjectManager.Instance.GetObject<CultureObject>("empire");
		public static CultureObject Khuzait => MBObjectManager.Instance.GetObject<CultureObject>("khuzait");
		public static CultureObject Nord => MBObjectManager.Instance.GetObject<CultureObject>("nord");
		public static CultureObject Sturgia => MBObjectManager.Instance.GetObject<CultureObject>("sturgia");
		public static CultureObject Vlandia => MBObjectManager.Instance.GetObject<CultureObject>("vlandia");

		public static CultureObject Deserters => MBObjectManager.Instance.GetObject<CultureObject>("desert_bandits");
		public static CultureObject ForestBandits => MBObjectManager.Instance.GetObject<CultureObject>("forest_bandits");
		public static CultureObject Looters => MBObjectManager.Instance.GetObject<CultureObject>("looters");
		public static CultureObject MountainBandits => MBObjectManager.Instance.GetObject<CultureObject>("mountain_bandits");
		public static CultureObject SeaRaiders => MBObjectManager.Instance.GetObject<CultureObject>("sea_raiders");
		public static CultureObject Corsairs => MBObjectManager.Instance.GetObject<CultureObject>("southern_pirates");
		public static CultureObject SteppeBandits => MBObjectManager.Instance.GetObject<CultureObject>("steppe_bandits");

		public static CultureObject DarshiSpecial => MBObjectManager.Instance.GetObject<CultureObject>("darshi");
		public static CultureObject VakkenSpecial => MBObjectManager.Instance.GetObject<CultureObject>("vakken");

		public static CultureObject CalradianNeutral => MBObjectManager.Instance.GetObject<CultureObject>("neutral_culture");

		public static List<CultureObject> AllCultures => MBObjectManager.Instance.GetObjectTypeList<CultureObject>();

		public static List<CultureObject> MainCultures
		{
			get
			{
				List<CultureObject> _mainCultures = new();
				foreach (CultureObject culture in AllCultures)
				{
					if (culture.IsMainCulture)
						_mainCultures.Add(culture);
				}

				return _mainCultures;
			}
		}

		public static List<CultureObject> BanditCultures
		{
			get
			{
				List<CultureObject> _banditCultures = new();
				foreach (CultureObject culture in AllCultures)
				{
					if (culture.IsBandit)
						_banditCultures.Add(culture);
				}

				return _banditCultures;
			}
		}

		/// MARK: Hero Name
		/// <summary>
		/// Gets a random hero name from the culture's gender-specific name list.
		/// First tries native game names, then custom names, then adds a suffix if all are exhausted.
		/// </summary>
		public static string GetUniqueRandomHeroName(CultureObject culture, bool isFemale)
		{
			// Get all existing hero names (convert to strings for comparison)
			HashSet<string> existingHeroNames = Hero.AllAliveHeroes
			 .Select(h => h.FirstName?.ToString() ?? string.Empty)
			 .Where(name => !string.IsNullOrEmpty(name))
			 .ToHashSet(StringComparer.OrdinalIgnoreCase);

			// Try native game name list first
			List<TextObject> cultureNameList = isFemale
			 ? culture.FemaleNameList.ToList()
			 : culture.MaleNameList.ToList();

			List<TextObject> availableNativeNames = cultureNameList
			 .Where(nameObj => !existingHeroNames.Contains(nameObj.ToString()))
			 .ToList();

			if (availableNativeNames.Count > 0)
			{
				int randomIndex = RandomNumberGen.Instance.NextRandomInt(availableNativeNames.Count);
				return availableNativeNames[randomIndex].ToString();
			}

			// Try custom name list if native names are exhausted
			string cultureId = culture.StringId.ToLower();
			Dictionary<string, List<string>> customNameDict = isFemale ? CustomNames.FemaleHeroNames : CustomNames.MaleHeroNames;

			if (customNameDict.TryGetValue(cultureId, out List<string> customNames))
			{
				List<string> availableCustomNames = customNames
				 .Where(name => !existingHeroNames.Contains(name))
				 .ToList();

				if (availableCustomNames.Count > 0)
				{
					int randomIndex = RandomNumberGen.Instance.NextRandomInt(availableCustomNames.Count);
					return availableCustomNames[randomIndex];
				}
			}

			// All names exhausted - add a suffix to a random name
			string[] suffixes = {
	"the Younger", "the Elder", "the Brave", "the Wise",
	"the Bold", "the Just", "the Swift", "the Strong",
	"the Fierce", "the Noble", "the Fair", "the Valiant"
   };

			// Try to use custom names first for suffix, then fall back to native
			string baseName;
			if (customNameDict.TryGetValue(cultureId, out List<string> customNamesForSuffix) && customNamesForSuffix.Count > 0)
			{
				int randomNameIndex = RandomNumberGen.Instance.NextRandomInt(customNamesForSuffix.Count);
				baseName = customNamesForSuffix[randomNameIndex];
			}
			
			else if (cultureNameList.Count > 0)
			{
				int randomNameIndex = RandomNumberGen.Instance.NextRandomInt(cultureNameList.Count);
				baseName = cultureNameList[randomNameIndex].ToString();
			}

			else
			{
				// Last resort fallback
				baseName = isFemale ? "Hero" : "Warrior";
			}

			int randomSuffixIndex = RandomNumberGen.Instance.NextRandomInt(suffixes.Length);
			return $"{baseName} {suffixes[randomSuffixIndex]}";
		}

		/// MARK: Clan Name
		/// <summary>
		/// Gets a random clan name from the culture's clan name list.
		/// First tries native game names, then custom names, then adds a suffix if all are exhausted.
		/// </summary>
		public static string GetUniqueRandomClanName(CultureObject culture)
		{
			// Get all existing clan names (convert to strings for comparison)
			HashSet<string> existingClanNames = Clan.All
				.Select(c => c.Name.ToString())
				.ToHashSet(StringComparer.OrdinalIgnoreCase);

			// Try native game clan name list first
			List<TextObject> availableNativeNames = culture.ClanNameList
				.Where(nameObj => !existingClanNames.Contains(nameObj.ToString()))
				.ToList();

			if (availableNativeNames.Count > 0)
			{
				int randomIndex = RandomNumberGen.Instance.NextRandomInt(availableNativeNames.Count);
				return availableNativeNames[randomIndex].ToString();
			}

			// Try custom clan name list if native names are exhausted
			string cultureId = culture.StringId.ToLower();

			if (CustomNames.ClanNames.TryGetValue(cultureId, out List<string> customNames))
			{
				List<string> availableCustomNames = customNames
					.Where(name => !existingClanNames.Contains(name))
					.ToList();

				if (availableCustomNames.Count > 0)
				{
					int randomIndex = RandomNumberGen.Instance.NextRandomInt(availableCustomNames.Count);
					return availableCustomNames[randomIndex];
				}
			}

			// All names exhausted - add a suffix to a random name
			string[] suffixes = {
				"of The New World", "Separatists", "Loyalists", "Conservatives",
				"of Calradia", "New Order", "Exiles", "Wanderers",
				"Reborn", "Rising", "Ascendant", "Defiant"
			};

			// Try to use custom names first for suffix, then fall back to native
			string baseName;
			if (CustomNames.ClanNames.TryGetValue(cultureId, out List<string> customNamesForSuffix) && customNamesForSuffix.Count > 0)
			{
				int randomNameIndex = RandomNumberGen.Instance.NextRandomInt(customNamesForSuffix.Count);
				baseName = customNamesForSuffix[randomNameIndex];
			}

			else if (culture.ClanNameList.Count > 0)
			{
				int randomNameIndex = RandomNumberGen.Instance.NextRandomInt(culture.ClanNameList.Count);
				baseName = culture.ClanNameList[randomNameIndex].ToString();
			}

			else
			{
				// Last resort fallback
				baseName = "Clan";
			}

			int randomSuffixIndex = RandomNumberGen.Instance.NextRandomInt(suffixes.Length);
			return $"{baseName} {suffixes[randomSuffixIndex]}";
		}

		/// MARK: Kingdom Name
		/// <summary>
		/// Gets a random kingdom name appropriate to the specified culture.
		/// First tries custom names, then adds a suffix if all are exhausted.
		/// </summary>
		public static string GetUniqueRandomKingdomName(CultureObject culture)
		{
			// Get all existing kingdom names (convert to strings for comparison)
			HashSet<string> existingKingdomNames = Kingdom.All
				.Select(k => k.Name.ToString())
				.ToHashSet(StringComparer.OrdinalIgnoreCase);

			// Try custom kingdom name list
			string cultureId = culture.StringId.ToLower();

			if (CustomNames.KingdomNames.TryGetValue(cultureId, out List<string> customNames))
			{
				List<string> availableCustomNames = customNames
					.Where(name => !existingKingdomNames.Contains(name))
					.ToList();

				if (availableCustomNames.Count > 0)
				{
					int randomIndex = RandomNumberGen.Instance.NextRandomInt(availableCustomNames.Count);
					return availableCustomNames[randomIndex];
				}
			}

			// All names exhausted - add a suffix to a random name
			string[] suffixes = {
				"Reborn", "Rising", "Ascendant", "Renewed",
				"Reformed", "Restored", "United", "Free",
				"New Order", "Resurgent", "Revived", "Triumphant"
			};

			// Try to use custom names first for suffix
			string baseName;
			if (CustomNames.KingdomNames.TryGetValue(cultureId, out List<string> customNamesForSuffix) && customNamesForSuffix.Count > 0)
			{
				int randomNameIndex = RandomNumberGen.Instance.NextRandomInt(customNamesForSuffix.Count);
				baseName = customNamesForSuffix[randomNameIndex];
			}
			else
			{
				// Last resort fallback
				baseName = $"{culture.Name} Kingdom";
			}

			int randomSuffixIndex = RandomNumberGen.Instance.NextRandomInt(suffixes.Length);
			return $"{baseName} {suffixes[randomSuffixIndex]}";
		}

		/// MARK: Culture Flags
		/// <summary>
		/// Gets the CultureFlag enum value that corresponds to the provided culture object.
		/// </summary>
		/// <param name="culture">The culture object to convert.</param>
		/// <returns>The corresponding CultureFlag value, or CultureFlags.None if not found or null.</returns>
		public static CultureFlags GetCultureFlag(CultureObject culture)
		{
			if (culture == null)
				return CultureFlags.None;

			return culture.StringId.ToLower() switch
			{
				"neutral_culture" => CultureFlags.Calradian,
				"aserai" => CultureFlags.Aserai,
				"battania" => CultureFlags.Battania,
				"empire" => CultureFlags.Empire,
				"khuzait" => CultureFlags.Khuzait,
				"nord" => CultureFlags.Nord,
				"sturgia" => CultureFlags.Sturgia,
				"vlandia" => CultureFlags.Vlandia,
				"looters" => CultureFlags.Looters,
				"desert_bandits" => CultureFlags.DesertBandits,
				"forest_bandits" => CultureFlags.ForestBandits,
				"mountain_bandits" => CultureFlags.MountainBandits,
				"steppe_bandits" => CultureFlags.SteppeBandits,
				"sea_raiders" => CultureFlags.SeaRaiders,
				"southern_pirates" => CultureFlags.Corsairs,
				"darshi" => CultureFlags.DarshiSpecial,
				"vakken" => CultureFlags.VakkenSpecial,
				_ => CultureFlags.None
			};
		}
	}
}