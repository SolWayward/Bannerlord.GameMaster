using System;
using Bannerlord.GameMaster.Characters;
using Bannerlord.GameMaster.Cultures;

namespace Bannerlord.GameMaster.Console.Common.Parsing
{
	/// <summary>
	/// Parses culture and gender flag arguments from console command input.
	/// </summary>
	public static class FlagParser
	{

		/// <summary>
		/// Parse culture argument into CultureFlags
		/// Supports groups: main_cultures, bandit_cultures, all_cultures
		/// Supports individual cultures separated by comma: vlandia,battania,empire
		/// </summary>
		public static CultureFlags ParseCultureArgument(string cultureArg)
		{
			string lowerArg = cultureArg.ToLower();

			// Check for group keywords
			if (lowerArg == "main_cultures")
				return CultureFlags.AllMainCultures;
			if (lowerArg == "bandit_cultures")
				return CultureFlags.AllBanditCultures;
			if (lowerArg == "all_cultures")
				return CultureFlags.AllCultures;

			// Parse individual cultures separated by comma
			CultureFlags flags = CultureFlags.None;
			string[] cultures = cultureArg.Split(',');

			foreach (string culture in cultures)
			{
				string trimmedCulture = culture.Trim().ToLower();
				CultureFlags flag = MapCultureNameToFlag(trimmedCulture);
				if (flag != CultureFlags.None)
					flags |= flag;
			}

			return flags;
		}

		/// <summary>
		/// Map culture name or ID to CultureFlags
		/// </summary>
		public static CultureFlags MapCultureNameToFlag(string cultureName)
		{
			return cultureName switch
			{
				// Main cultures
				"vlandia" => CultureFlags.Vlandia,
				"sturgia" => CultureFlags.Sturgia,
				"empire" => CultureFlags.Empire,
				"aserai" => CultureFlags.Aserai,
				"khuzait" => CultureFlags.Khuzait,
				"battania" => CultureFlags.Battania,
				"nord" => CultureFlags.Nord,
				"neutral_culture" or "calradian" => CultureFlags.Calradian,

				// Bandit cultures
				"looters" => CultureFlags.Looters,
				"deserters" or "deserter" => CultureFlags.Deserters,
				"desert_bandits" or "desert" or "desertbandits" => CultureFlags.DesertBandits,
				"forest_bandits" => CultureFlags.ForestBandits,
				"mountain_bandits" => CultureFlags.MountainBandits,
				"steppe_bandits" => CultureFlags.SteppeBandits,
				"sea_raiders" => CultureFlags.SeaRaiders,
				"southern_pirates" or "corsairs" => CultureFlags.Corsairs,

				// Special cultures
				"darshi" => CultureFlags.DarshiSpecial,
				"vakken" => CultureFlags.VakkenSpecial,

				_ => CultureFlags.None
			};
		}

		/// <summary>
		/// Parse gender argument into GenderFlags
		/// </summary>
		public static GenderFlags ParseGenderArgument(string genderArg)
		{
			string lower = genderArg.ToLower();
			return lower switch
			{
				"both" or "b" or "either" => GenderFlags.Either,
				"female" or "f" => GenderFlags.Female,
				"male" or "m" => GenderFlags.Male,
				_ => GenderFlags.None
			};
		}
	}
}
