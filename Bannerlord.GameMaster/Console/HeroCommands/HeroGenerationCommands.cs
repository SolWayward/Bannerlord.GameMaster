using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Heroes;
using Bannerlord.GameMaster.Characters;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.HeroCommands
{
	[CommandLineFunctionality.CommandLineArgumentFunction("hero", "gm")]
	public static class HeroGenerationCommands
	{
		//MARK: generate_lords
		/// <summary>
		/// Generate new heroes with random templates
		/// Usage: gm.hero.generate_lords <count> [cultures] [gender] [clan] [randomFactor]
		/// </summary>
		[CommandLineFunctionality.CommandLineArgumentFunction("generate_lords", "gm.hero")]
		public static string GenerateLords(List<string> args)
		{
			return Cmd.Run(args, () =>
			{
				if (!CommandBase.ValidateCampaignMode(out string error))
					return error;

				var usageMessage = CommandValidator.CreateUsageMessage(
					"gm.hero.generate_lords", "<count> [cultures] [gender] [clan] [randomFactor]",
					"Creates lords from random templates with good gear and decent stats. Age 20-30. Names are selected from their culture\n" +
					"- cultures: optional, defines the pool of cultures allowed to be chosen from. Defaults to main_cultures. Run command gm.query.culture to see available cultures\n" +
						"\tuse ; (semi-colon) with no spaces to specify multiple cultures\n" +
						"\texample: gm.hero.generate_lords 10 vlandia;battania;sturgia both\n" +
						"\texample: gm.hero.generate_lords 7 main_cultures female\n" +
					"- gender: optional, use keywords both, female, or male. also allowed b, f, and m. Defaults to both\n" +
					"- clan: optional, clanID or clanName. If clan not specified, each hero goes to a different random clan.\n" +
					"- randomFactor: optional, float value between 0 and 1. defaults to 1 (1 is recommended)\n" +
						"\tControls how much the template is randomized within its constraints\n" +
					"gm.hero.generate_lords 15\n" +
					"gm.hero.generate_lords 15 vlandia player_faction male\n" +
					"gm.hero.generate_lords 5 main_cultures player_faction b 0.8\n" +
					"gm.hero.generate_lords 2 bandit_cultures f 0.9\n" +
					"gm.hero.generate_lords 30 all_cultures m\n" +
					"gm.hero.generate_lords 12 aserai;sturgia;khuzait;empire both 'dey Meroc' 0.7");

				// Minimum 1 required argument: count
				if (args.Count < 1)
					return usageMessage;

				// Parse count (required)
				if (!CommandValidator.ValidateIntegerRange(args[0], 1, 50, out int count, out string countError))
					return CommandBase.FormatErrorMessage(countError);

				// Smart parse remaining arguments - detect gender flags vs cultures
				CultureFlags cultureFlags = CultureFlags.AllMainCultures;
				GenderFlags genderFlags = GenderFlags.Either;
				Clan targetClan = null;
				float randomFactor = 1f;

				int currentArgIndex = 1;

				// Parse cultures if provided (args[1] or skip if it's a gender keyword)
				if (args.Count > currentArgIndex)
				{
					// Check if this argument is a gender keyword first
					GenderFlags testGender = FlagParser.ParseGenderArgument(args[currentArgIndex]);
					if (testGender != GenderFlags.None)
					{
						// It's a gender keyword, so no culture was provided
						genderFlags = testGender;
						currentArgIndex++;
					}
					else
					{
						// Try to parse as culture
						cultureFlags = FlagParser.ParseCultureArgument(args[currentArgIndex]);
						if (cultureFlags == CultureFlags.None)
							return CommandBase.FormatErrorMessage($"Invalid culture(s): '{args[currentArgIndex]}'. Use culture names (e.g., vlandia;battania) or groups (main_cultures, bandit_cultures, all_cultures)");
						currentArgIndex++;

						// Now check for gender in the next position
						if (args.Count > currentArgIndex)
						{
							testGender = FlagParser.ParseGenderArgument(args[currentArgIndex]);
							if (testGender != GenderFlags.None)
							{
								genderFlags = testGender;
								currentArgIndex++;
							}
						}
					}
				}

				// Parse optional clan
				if (args.Count > currentArgIndex)
				{
					// Check if this might be a float (randomFactor) instead
					if (float.TryParse(args[currentArgIndex], out float testFloat))
					{
						// It's a number, treat as randomFactor
						if (!CommandValidator.ValidateFloatRange(args[currentArgIndex], 0f, 1f, out randomFactor, out string randomError))
							return CommandBase.FormatErrorMessage(randomError);
					}
					else
					{
						// Try to parse as clan
						var (clan, clanError) = CommandBase.FindSingleClan(args[currentArgIndex]);
						if (clanError != null)
							return clanError;
						targetClan = clan;
						currentArgIndex++;

						// Check for randomFactor after clan
						if (args.Count > currentArgIndex)
						{
							if (!CommandValidator.ValidateFloatRange(args[currentArgIndex], 0f, 1f, out randomFactor, out string randomError))
								return CommandBase.FormatErrorMessage(randomError);
						}
					}
				}

				return CommandBase.ExecuteWithErrorHandling(() =>
				{
					// Generate heroes using HeroGenerator
					List<Hero> createdHeroes = HeroGenerator.CreateHeroesFromRandomTemplates(count, cultureFlags, genderFlags, randomFactor, targetClan);

					if (createdHeroes == null || createdHeroes.Count == 0)
						return CommandBase.FormatErrorMessage("Failed to create lords - no templates found matching criteria");

					return CommandBase.FormatSuccessMessage($"Created {createdHeroes.Count} lord(s):\n{HeroQueries.GetFormattedDetails(createdHeroes)}");
				}, "Failed to generate lords");
			});
		}

		//MARK: create_lord
		/// <summary>
		/// Create a new hero with a chosen name from random templates
		/// Usage: gm.hero.create_lord <name> [cultures] [gender] [clan] [randomFactor]
		/// </summary>
		[CommandLineFunctionality.CommandLineArgumentFunction("create_lord", "gm.hero")]
		public static string CreateLord(List<string> args)
		{
			return Cmd.Run(args, () =>
			{
				if (!CommandBase.ValidateCampaignMode(out string error))
					return error;

				var usageMessage = CommandValidator.CreateUsageMessage(
					"gm.hero.create_lord", "<name> [cultures] [gender] [clan] [randomFactor]",
					"Creates a single lord from random templates with good gear and decent stats. Age 20-30. Allows custom naming.\n" +
					"- name: required, the name for the hero. Use SINGLE QUOTES for multi-word names\n" +
					"- cultures: optional, defines the pool of cultures allowed to be chosen from. Defaults to main_cultures. Run command gm.query.culture to see available cultures\n" +
						"\tuse ; (semi-colon) with no spaces to specify multiple cultures\n" +
					"- gender: optional, use keywords both, female, or male. also allowed b, f, and m. Defaults to both\n" +
					"- clan: optional, clanID or clanName. If not specified, hero goes to a random clan.\n" +
					"- randomFactor: optional, float value between 0 and 1. defaults to 0.5 (1 = more variety and still good looking, 0 = less variety, better looking )\n" +
						"\tControls how much the template is randomized within its constraints\n" +
					"gm.hero.create_lord 'Sir Percival'\n" +
					"gm.hero.create_lord Ragnar vlandia male\n" +
					"gm.hero.create_lord 'Lady Elara' empire female player_faction\n" +
					"gm.hero.create_lord Khalid aserai male 'dey Meroc' 0.8");

				// Minimum 1 required argument: name
				if (args.Count < 1)
					return usageMessage;

				// Parse name (required)
				string name = args[0];
				if (string.IsNullOrWhiteSpace(name))
					return CommandBase.FormatErrorMessage("Name cannot be empty.");

				// Smart parse remaining arguments - detect gender flags vs cultures
				CultureFlags cultureFlags = CultureFlags.AllMainCultures;
				GenderFlags genderFlags = GenderFlags.Either;
				Clan targetClan = null;
				float randomFactor = 1f;

				int currentArgIndex = 1;

				// Parse cultures if provided (args[1] or skip if it's a gender keyword)
				if (args.Count > currentArgIndex)
				{
					// Check if this argument is a gender keyword first
					GenderFlags testGender = FlagParser.ParseGenderArgument(args[currentArgIndex]);
					if (testGender != GenderFlags.None)
					{
						// It's a gender keyword, so no culture was provided
						genderFlags = testGender;
						currentArgIndex++;
					}
					else
					{
						// Try to parse as culture
						cultureFlags = FlagParser.ParseCultureArgument(args[currentArgIndex]);
						if (cultureFlags == CultureFlags.None)
							return CommandBase.FormatErrorMessage($"Invalid culture(s): '{args[currentArgIndex]}'. Use culture names (e.g., vlandia;battania) or groups (main_cultures, bandit_cultures, all_cultures)");
						currentArgIndex++;

						// Now check for gender in the next position
						if (args.Count > currentArgIndex)
						{
							testGender = FlagParser.ParseGenderArgument(args[currentArgIndex]);
							if (testGender != GenderFlags.None)
							{
								genderFlags = testGender;
								currentArgIndex++;
							}
						}
					}
				}

				// Parse optional clan
				if (args.Count > currentArgIndex)
				{
					// Check if this might be a float (randomFactor) instead
					if (float.TryParse(args[currentArgIndex], out float testFloat))
					{
						// It's a number, treat as randomFactor
						if (!CommandValidator.ValidateFloatRange(args[currentArgIndex], 0f, 1f, out randomFactor, out string randomError))
							return CommandBase.FormatErrorMessage(randomError);
					}
					else
					{
						// Try to parse as clan
						var (clan, clanError) = CommandBase.FindSingleClan(args[currentArgIndex]);
						if (clanError != null)
							return clanError;
						targetClan = clan;
						currentArgIndex++;

						// Check for randomFactor after clan
						if (args.Count > currentArgIndex)
						{
							if (!CommandValidator.ValidateFloatRange(args[currentArgIndex], 0f, 1f, out randomFactor, out string randomError))
								return CommandBase.FormatErrorMessage(randomError);
						}
					}
				}

				return CommandBase.ExecuteWithErrorHandling(() =>
				{
					// Generate hero using HeroGenerator
					Hero createdHero = HeroGenerator.CreateSingleHeroFromRandomTemplates(name, cultureFlags, genderFlags, randomFactor, targetClan);

					if (createdHero == null)
						return CommandBase.FormatErrorMessage("Failed to create lord - no templates found matching criteria");

					return CommandBase.FormatSuccessMessage($"Created lord '{createdHero.Name}' (ID: {createdHero.StringId})\n{HeroQueries.GetFormattedDetails(new List<Hero> { createdHero })}");
				}, "Failed to create lord");
			});
		}

		//MARK: rename
		/// <summary>
		/// Rename a hero
		/// Usage: gm.hero.rename <heroQuery> <name>
		/// </summary>
		[CommandLineFunctionality.CommandLineArgumentFunction("rename", "gm.hero")]
		public static string RenameHero(List<string> args)
		{
			return Cmd.Run(args, () =>
			{
				if (!CommandBase.ValidateCampaignMode(out string error))
					return error;

				var usageMessage = CommandValidator.CreateUsageMessage(
					"gm.hero.rename", "<heroQuery> <name>",
					"Renames the specified hero. Use SINGLE QUOTES for multi-word names.\n" +
					"- heroQuery: hero ID or name query to find a single hero\n" +
					"- name: the new name for the hero\n",
					"gm.hero.rename lord_1_1 'Sir Galahad'\n" +
					"gm.hero.rename 'old hero name' NewName");

				// Minimum 2 required arguments: heroQuery and name
				if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
					return error;

				// Parse hero query (required)
				var (hero, heroError) = CommandBase.FindSingleHero(args[0]);
				if (heroError != null)
					return heroError;

				// Parse name (required)
				string newName = args[1];
				if (string.IsNullOrWhiteSpace(newName))
					return CommandBase.FormatErrorMessage("New name cannot be empty.");

				return CommandBase.ExecuteWithErrorHandling(() =>
				{
					string previousName = hero.Name.ToString();
					hero.SetStringName(newName);

					return CommandBase.FormatSuccessMessage(
						$"Hero renamed from '{previousName}' to '{hero.Name}' (ID: {hero.StringId})");
				}, "Failed to rename hero");
			});
		}
	}
}