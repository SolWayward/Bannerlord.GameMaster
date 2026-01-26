using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using Bannerlord.GameMaster.Common.Interfaces;
using TaleWorlds.ObjectSystem;
using TaleWorlds.Localization;
using Bannerlord.GameMaster.Cultures.HeroNames;
using Bannerlord.GameMaster.Cultures.FactionNames;

namespace Bannerlord.GameMaster.Cultures
{
	public static class CultureLookup
	{
		/// MARK: Culture Lookup
		public static CultureObject Aserai => MBObjectManager.Instance.GetObject<CultureObject>("aserai");
		public static CultureObject Battania => MBObjectManager.Instance.GetObject<CultureObject>("battania");
		public static CultureObject Empire => MBObjectManager.Instance.GetObject<CultureObject>("empire");
		public static CultureObject Khuzait => MBObjectManager.Instance.GetObject<CultureObject>("khuzait");
		public static CultureObject Sturgia => MBObjectManager.Instance.GetObject<CultureObject>("sturgia");
		public static CultureObject Vlandia => MBObjectManager.Instance.GetObject<CultureObject>("vlandia");

		/// <summary>
		/// Returns Nord if War Sails is active, other wise returns Sturgia
		/// </summary>
		public static CultureObject Nord
		{
			get
			{ // Return Sturgia if War Sails is not active
				if (Information.GameEnvironment.IsWarsailsDlcLoaded)
					return MBObjectManager.Instance.GetObject<CultureObject>("nord");
				else
					return Sturgia;
			}
		}

		public static CultureObject Deserters => MBObjectManager.Instance.GetObject<CultureObject>("desert_bandits");
		public static CultureObject ForestBandits => MBObjectManager.Instance.GetObject<CultureObject>("forest_bandits");
		public static CultureObject Looters => MBObjectManager.Instance.GetObject<CultureObject>("looters");
		public static CultureObject MountainBandits => MBObjectManager.Instance.GetObject<CultureObject>("mountain_bandits");
		public static CultureObject SeaRaiders => MBObjectManager.Instance.GetObject<CultureObject>("sea_raiders");
		public static CultureObject SteppeBandits => MBObjectManager.Instance.GetObject<CultureObject>("steppe_bandits");
		
		/// <summary>
		/// Returns Corsairs (southern_pirates) if War Sails is active, other wise returns Sea Raiders (sea_raiders)
		/// </summary>
		public static CultureObject Corsairs
		{
			get
			{
				if (Information.GameEnvironment.IsWarsailsDlcLoaded)
					return MBObjectManager.Instance.GetObject<CultureObject>("southern_pirates");
				else
					return SeaRaiders;
			}
		}

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

		/// <summary>
		/// Get a random main culture
		/// </summary>
		public static CultureObject RandomMainCulture()
		{
			List<CultureObject> mainCultures = MainCultures;
			return mainCultures[RandomNumberGen.Instance.NextRandomInt(mainCultures.Count)];
		}

		/// <summary>
		/// Get a random bandit culture
		/// </summary>
		public static CultureObject RandomBanditCulture(bool includeLooters)
		{
			List<CultureObject> banditCultures = BanditCultures;
			if (!includeLooters)
				banditCultures.Remove(Looters);

			return banditCultures[RandomNumberGen.Instance.NextRandomInt(banditCultures.Count)];
		}

		/// MARK: Hero Name
		/// <summary>
		/// Gets a random hero name from the culture's gender-specific name list.
		/// Uses custom names only, then adds a suffix if all are exhausted.
		/// </summary>
		public static string GetUniqueRandomHeroName(CultureObject culture, bool isFemale)
		{
			// Get all existing hero names (convert to strings for comparison)
			HashSet<string> existingHeroNames = Hero.AllAliveHeroes
			 .Select(h => h.FirstName?.ToString() ?? string.Empty)
			 .Where(name => !string.IsNullOrEmpty(name))
			 .ToHashSet(StringComparer.OrdinalIgnoreCase);
	
			// Get custom name array for the culture
			string cultureId = culture.StringId.ToLower();
			string[] customNames = GetHeroNames(cultureId, isFemale);
	
			if (customNames != null && customNames.Length > 0)
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
	
			// All custom names exhausted - add a suffix to a random custom name
			string[] suffixes = {
		"the Younger", "the Elder", "the Brave", "the Wise",
		"the Bold", "the Just", "the Swift", "the Strong",
		"the Fierce", "the Noble", "the Fair", "the Valiant"
		  };
	
			// Use custom names for suffix base, or fallback
			string baseName;
			if (customNames != null && customNames.Length > 0)
			{
				int randomNameIndex = RandomNumberGen.Instance.NextRandomInt(customNames.Length);
				baseName = customNames[randomNameIndex];
			}
			else
			{
				// Last resort fallback
				baseName = isFemale ? "Hero" : "Warrior";
			}
	
			int randomSuffixIndex = RandomNumberGen.Instance.NextRandomInt(suffixes.Length);
			return $"{baseName} {suffixes[randomSuffixIndex]}";
		}
	
		/// <summary>
		/// Gets hero names for a specific culture and gender
		/// </summary>
		private static string[] GetHeroNames(string cultureId, bool isFemale)
		{
			return cultureId switch
			{
				"aserai" => isFemale ? AseraiHeroNames.FemaleNames : AseraiHeroNames.MaleNames,
				"battania" => isFemale ? BattaniaHeroNames.FemaleNames : BattaniaHeroNames.MaleNames,
				"empire" => isFemale ? EmpireHeroNames.FemaleNames : EmpireHeroNames.MaleNames,
				"khuzait" => isFemale ? KhuzaitHeroNames.FemaleNames : KhuzaitHeroNames.MaleNames,
				"nord" => isFemale ? NordHeroNames.FemaleNames : NordHeroNames.MaleNames,
				"sturgia" => isFemale ? SturgiaHeroNames.FemaleNames : SturgiaHeroNames.MaleNames,
				"vlandia" => isFemale ? VlandiaHeroNames.FemaleNames : VlandiaHeroNames.MaleNames,
				_ => null
			};
		}

		/// MARK: Clan Name
		/// <summary>
		/// Gets a random clan name from the culture's clan name list.
		/// Uses custom names only, then adds a suffix if all are exhausted.
		/// </summary>
		public static string GetUniqueRandomClanName(CultureObject culture)
		{
			// Get all existing clan names (convert to strings for comparison)
			HashSet<string> existingClanNames = Clan.All
				.Select(c => c.Name.ToString())
				.ToHashSet(StringComparer.OrdinalIgnoreCase);
	
			// Get custom clan names for the culture
			string cultureId = culture.StringId.ToLower();
			string[] customNames = GetClanNames(cultureId);
	
			if (customNames != null && customNames.Length > 0)
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
	
			// All custom names exhausted - add a suffix to a random custom name
			string[] suffixes = {
				"of The New World", "Separatists", "Loyalists", "Conservatives",
				"of Calradia", "New Order", "Exiles", "Wanderers",
				"Reborn", "Rising", "Ascendant", "Defiant"
			};
	
			// Use custom names for suffix base, or fallback
			string baseName;
			if (customNames != null && customNames.Length > 0)
			{
				int randomNameIndex = RandomNumberGen.Instance.NextRandomInt(customNames.Length);
				baseName = customNames[randomNameIndex];
			}
			else
			{
				// Last resort fallback
				baseName = "Clan";
			}
	
			int randomSuffixIndex = RandomNumberGen.Instance.NextRandomInt(suffixes.Length);
			return $"{baseName} {suffixes[randomSuffixIndex]}";
		}
	
		/// <summary>
		/// Gets clan names for a specific culture
		/// </summary>
		private static string[] GetClanNames(string cultureId)
		{
			return cultureId switch
			{
				"aserai" => AseraiFactionNames.ClanNames,
				"battania" => BattaniaFactionNames.ClanNames,
				"empire" => EmpireFactionNames.ClanNames,
				"khuzait" => KhuzaitFactionNames.ClanNames,
				"nord" => NordFactionNames.ClanNames,
				"sturgia" => SturgiaFactionNames.ClanNames,
				"vlandia" => VlandiaFactionNames.ClanNames,
				_ => null
			};
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
	
			// Get custom kingdom names for the culture
			string cultureId = culture.StringId.ToLower();
			string[] customNames = GetKingdomNames(cultureId);
	
			if (customNames != null && customNames.Length > 0)
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
			if (customNames != null && customNames.Length > 0)
			{
				int randomNameIndex = RandomNumberGen.Instance.NextRandomInt(customNames.Length);
				baseName = customNames[randomNameIndex];
			}
			else
			{
				// Last resort fallback
				baseName = $"{culture.Name} Kingdom";
			}
	
			int randomSuffixIndex = RandomNumberGen.Instance.NextRandomInt(suffixes.Length);
			return $"{baseName} {suffixes[randomSuffixIndex]}";
		}
	
		/// <summary>
		/// Gets kingdom names for a specific culture
		/// </summary>
		private static string[] GetKingdomNames(string cultureId)
		{
			return cultureId switch
			{
				"aserai" => AseraiFactionNames.KingdomNames,
				"battania" => BattaniaFactionNames.KingdomNames,
				"empire" => EmpireFactionNames.KingdomNames,
				"khuzait" => KhuzaitFactionNames.KingdomNames,
				"nord" => NordFactionNames.KingdomNames,
				"sturgia" => SturgiaFactionNames.KingdomNames,
				"vlandia" => VlandiaFactionNames.KingdomNames,
				_ => null
			};
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