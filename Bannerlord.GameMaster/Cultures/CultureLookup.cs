using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
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

		public static CultureObject Deserters => MBObjectManager.Instance.GetObject<CultureObject>("deserters");
		public static CultureObject DesertBandits => MBObjectManager.Instance.GetObject<CultureObject>("desert_bandits");
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

		/// <summary>
		/// Returns a random bandit culture appropriate for hideouts (excludes looters and optionally deserters).
		/// </summary>
		/// <param name="includeDeserters">If true, includes deserters in the selection pool.</param>
		/// <returns>A random hideout-appropriate bandit culture.</returns>
		public static CultureObject RandomHideoutBanditCulture(bool includeDeserters = false)
		{
			List<CultureObject> banditCultures = BanditCultures;
			banditCultures.Remove(Looters);  // Looters don't have hideouts
			
			if (!includeDeserters)
				banditCultures.Remove(Deserters);  // Deserters are special
			
			return banditCultures[RandomNumberGen.Instance.NextRandomInt(banditCultures.Count)];
		}

		/// MARK: Hero Name
		/// <summary>
		/// Gets a unique random hero name for the specified culture and gender.
		/// Fallback chain: BLGM custom names -> Native Bannerlord names -> BLGM name + suffix -> Last resort.
		/// </summary>
		public static string GetUniqueRandomHeroName(CultureObject culture, bool isFemale)
		{
			HashSet<string> existingHeroNames = Hero.AllAliveHeroes
				.Select(h => h.FirstName?.ToString() ?? string.Empty)
				.Where(name => !string.IsNullOrEmpty(name))
				.ToHashSet(StringComparer.OrdinalIgnoreCase);

			string cultureId = culture.StringId.ToLower();
			string[] customNames = GetHeroNames(cultureId, isFemale);

			// Step 1: Try BLGM custom names for the culture+gender
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

			// Step 2: Try native Bannerlord names from culture name lists
			MBReadOnlyList<TextObject> nativeNameList = isFemale ? culture.FemaleNameList : culture.MaleNameList;
			if (nativeNameList != null && nativeNameList.Count > 0)
			{
				List<string> availableNativeNames = nativeNameList
					.Select(textObj => textObj.ToString())
					.Where(name => !string.IsNullOrEmpty(name) && !existingHeroNames.Contains(name))
					.ToList();

				if (availableNativeNames.Count > 0)
				{
					int randomIndex = RandomNumberGen.Instance.NextRandomInt(availableNativeNames.Count);
					return availableNativeNames[randomIndex];
				}
			}

			// Step 3: BLGM name + random culture suffix
			string[] suffixes = GetHeroSuffixes(cultureId);
			if (customNames != null && customNames.Length > 0 && suffixes != null && suffixes.Length > 0)
			{
				int maxAttempts = 100;
				for (int attempt = 0; attempt < maxAttempts; attempt++)
				{
					int nameIndex = RandomNumberGen.Instance.NextRandomInt(customNames.Length);
					int suffixIndex = RandomNumberGen.Instance.NextRandomInt(suffixes.Length);
					string candidate = $"{customNames[nameIndex]} {suffixes[suffixIndex]}";

					if (!existingHeroNames.Contains(candidate))
						return candidate;
				}
			}

			// Step 4: Last resort fallback (should never happen)
			string fallbackBase = isFemale ? "Hero" : "Warrior";
			if (suffixes != null && suffixes.Length > 0)
			{
				int suffixIndex = RandomNumberGen.Instance.NextRandomInt(suffixes.Length);
				return $"{fallbackBase} {suffixes[suffixIndex]}";
			}

			return fallbackBase;
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

		/// <summary>
		/// Gets hero name suffixes for a specific culture
		/// </summary>
		private static string[] GetHeroSuffixes(string cultureId)
		{
			return cultureId switch
			{
				"aserai" => AseraiHeroNames.HeroSuffixes,
				"battania" => BattaniaHeroNames.HeroSuffixes,
				"empire" => EmpireHeroNames.HeroSuffixes,
				"khuzait" => KhuzaitHeroNames.HeroSuffixes,
				"nord" => NordHeroNames.HeroSuffixes,
				"sturgia" => SturgiaHeroNames.HeroSuffixes,
				"vlandia" => VlandiaHeroNames.HeroSuffixes,
				_ => null
			};
		}

		/// MARK: Clan Name
		/// <summary>
		/// Gets a unique random clan name for the specified culture.
		/// Fallback chain: BLGM custom names -> Native Bannerlord names -> Prefix + BLGM name -> Last resort.
		/// </summary>
		public static string GetUniqueRandomClanName(CultureObject culture)
		{
			HashSet<string> existingClanNames = Clan.All
				.Select(c => c.Name.ToString())
				.ToHashSet(StringComparer.OrdinalIgnoreCase);

			string cultureId = culture.StringId.ToLower();
			string[] customNames = GetClanNames(cultureId);

			// Step 1: Try BLGM custom clan names for the culture
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

			// Step 2: Try native Bannerlord names from culture clan name list
			MBReadOnlyList<TextObject> nativeClanNames = culture.ClanNameList;
			if (nativeClanNames != null && nativeClanNames.Count > 0)
			{
				List<string> availableNativeNames = nativeClanNames
					.Select(textObj => textObj.ToString())
					.Where(name => !string.IsNullOrEmpty(name) && !existingClanNames.Contains(name))
					.ToList();

				if (availableNativeNames.Count > 0)
				{
					int randomIndex = RandomNumberGen.Instance.NextRandomInt(availableNativeNames.Count);
					return availableNativeNames[randomIndex];
				}
			}

			// Step 3: Random prefix + BLGM clan name
			string[] prefixes = GetFactionPrefixes(cultureId);
			if (customNames != null && customNames.Length > 0 && prefixes != null && prefixes.Length > 0)
			{
				int maxAttempts = 100;
				for (int attempt = 0; attempt < maxAttempts; attempt++)
				{
					int prefixIndex = RandomNumberGen.Instance.NextRandomInt(prefixes.Length);
					int nameIndex = RandomNumberGen.Instance.NextRandomInt(customNames.Length);
					string candidate = $"{prefixes[prefixIndex]} {customNames[nameIndex]}";

					if (!existingClanNames.Contains(candidate))
						return candidate;
				}
			}

			// Step 4: Last resort fallback
			if (prefixes != null && prefixes.Length > 0)
			{
				int prefixIndex = RandomNumberGen.Instance.NextRandomInt(prefixes.Length);
				return $"{prefixes[prefixIndex]} Clan";
			}

			return "Clan";
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

		/// <summary>
		/// Gets faction name prefixes for a specific culture
		/// </summary>
		private static string[] GetFactionPrefixes(string cultureId)
		{
			return cultureId switch
			{
				"aserai" => AseraiFactionNames.FactionPrefixes,
				"battania" => BattaniaFactionNames.FactionPrefixes,
				"empire" => EmpireFactionNames.FactionPrefixes,
				"khuzait" => KhuzaitFactionNames.FactionPrefixes,
				"nord" => NordFactionNames.FactionPrefixes,
				"sturgia" => SturgiaFactionNames.FactionPrefixes,
				"vlandia" => VlandiaFactionNames.FactionPrefixes,
				_ => null
			};
		}

		/// MARK: Kingdom Name
		/// <summary>
		/// Gets a unique random kingdom name for the specified culture.
		/// Fallback chain: BLGM custom names -> Prefix + BLGM name -> Last resort.
		/// Note: No native culture.KingdomNameList exists in Bannerlord, so that step is skipped.
		/// </summary>
		public static string GetUniqueRandomKingdomName(CultureObject culture)
		{
			HashSet<string> existingKingdomNames = Kingdom.All
				.Select(k => k.Name.ToString())
				.ToHashSet(StringComparer.OrdinalIgnoreCase);

			string cultureId = culture.StringId.ToLower();
			string[] customNames = GetKingdomNames(cultureId);

			// Step 1: Try BLGM custom kingdom names for the culture
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

			// Step 2: Random prefix + BLGM kingdom name
			string[] prefixes = GetFactionPrefixes(cultureId);
			if (customNames != null && customNames.Length > 0 && prefixes != null && prefixes.Length > 0)
			{
				int maxAttempts = 100;
				for (int attempt = 0; attempt < maxAttempts; attempt++)
				{
					int prefixIndex = RandomNumberGen.Instance.NextRandomInt(prefixes.Length);
					int nameIndex = RandomNumberGen.Instance.NextRandomInt(customNames.Length);
					string candidate = $"{prefixes[prefixIndex]} {customNames[nameIndex]}";

					if (!existingKingdomNames.Contains(candidate))
						return candidate;
				}
			}

			// Step 3: Last resort fallback
			string fallbackBase = $"{culture.Name} Kingdom";
			if (prefixes != null && prefixes.Length > 0)
			{
				int prefixIndex = RandomNumberGen.Instance.NextRandomInt(prefixes.Length);
				return $"{prefixes[prefixIndex]} {fallbackBase}";
			}

			return fallbackBase;
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
				"deserters" => CultureFlags.Deserters,
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
