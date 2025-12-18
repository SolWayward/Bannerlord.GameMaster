using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Heroes;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.HeroCommands
{
	[CommandLineFunctionality.CommandLineArgumentFunction("hero", "gm")]
	public static class HeroGenerationCommands
	{
		/// <summary>
		/// Generate new lords with random templates and good equipment
		/// Usage: gm.hero.generate_lords <count> [clan] [random|template] [culture]
		/// </summary>
		[CommandLineFunctionality.CommandLineArgumentFunction("test_hero", "gm.hero")]
		public static string CreateHeroTest(List<string> args)
		{
				HeroGenerator2_DontUse gen2 = new();
				gen2.CreateHero("test hero", Clan.PlayerClan);
				
				return "hero created";		
		}

		/// <summary>
		/// Generate new lords with random templates and good equipment
		/// Usage: gm.hero.generate_lords <count> [clan] [random|template] [culture]
		/// </summary>
		[CommandLineFunctionality.CommandLineArgumentFunction("generate_lords", "gm.hero")]
		public static string GenerateLords(List<string> args)
		{
			return Cmd.Run(args, () =>
			{
				if (!CommandBase.ValidateCampaignMode(out string error))
					return error;

				var usageMessage = CommandValidator.CreateUsageMessage(
					"gm.hero.generate_lords", "<count> [clan=random] [random|template] [culture]",
					"Creates lords from random templates with good gear and decent stats. Age 30-40.\n" +
					"- If clan not specified, each lord goes to a different random clan.\n" +
					"- Default: Uses template appearance without extra randomization.\n" +
					"- 'random': Fully randomizes appearance beyond template constraints.\n" +
					"- 'template': Uses random templates (same as default).\n" +
					"- 'culture': Optional culture name to filter templates.",
					"gm.hero.generate_lords 3\ngm.hero.generate_lords 5 empire_south random\ngm.hero.generate_lords 2 empire culture_empire");

				// Count is now required
				if (args.Count == 0)
					return usageMessage;

				// Parse count (required)
				if (!CommandValidator.ValidateIntegerRange(args[0], 1, 20, out int count, out string countError))
					return CommandBase.FormatErrorMessage(countError);

				// Parse remaining arguments
				Clan targetClan = null;
				bool fullRandomization = false;
				TaleWorlds.Core.BasicCultureObject culture = null;

				for (int i = 1; i < args.Count; i++)
				{
					string arg = args[i].ToLower();

					if (arg == "random")
					{
						fullRandomization = true;
					}
					else if (arg == "template")
					{
						fullRandomization = false;
					}
					else if (arg.StartsWith("culture"))
					{
						// Try to find culture
						string cultureName = arg.Contains("_") ? arg : (i + 1 < args.Count ? args[++i] : "");
						var foundCulture = TaleWorlds.ObjectSystem.MBObjectManager.Instance
							.GetObjectTypeList<TaleWorlds.Core.BasicCultureObject>()
							.FirstOrDefault(c => c.StringId.ToLower().Contains(cultureName.ToLower()) ||
												 c.Name.ToString().ToLower().Contains(cultureName.ToLower()));
						if (foundCulture != null)
							culture = foundCulture;
					}
					else if (targetClan == null)
					{
						// Try to parse as clan
						var (clan, clanError) = CommandBase.FindSingleClan(args[i]);
						if (clan != null)
							targetClan = clan;
					}
				}

				return CommandBase.ExecuteWithErrorHandling(() =>
				{
					// Create configuration for lord generation
					var config = new HeroGenerator.HeroGenerationConfig
					{
						Count = count,
						TargetClan = targetClan,
						MinAge = 30,
						MaxAge = 40,
						MinLevel = 15,
						MaxLevel = 25,
						MinArmorTier = TaleWorlds.Core.ItemObject.ItemTiers.Tier4,
						MinWeaponTier = TaleWorlds.Core.ItemObject.ItemTiers.Tier3,
						FullRandomization = fullRandomization,
						Culture = culture
					};

					// Generate lords using HeroGenerator
					var generator = new HeroGenerator();
					var result = generator.GenerateHeroes(config);

					if (!result.Success)
						return CommandBase.FormatErrorMessage(result.ErrorMessage);

					// Format success message
					var output = new System.Text.StringBuilder();
					output.AppendLine($"Successfully created {result.CreatedLords.Count} lord(s):");
					foreach (var (lord, clan) in result.CreatedLords)
					{
						output.AppendLine($"  - {lord.Name} (ID: {lord.StringId}, Age: {(int)lord.Age}, Clan: {clan.Name})");
					}

					return CommandBase.FormatSuccessMessage(output.ToString());
				}, "Failed to generate lords");
			});
		}

		/// <summary>
		/// Create a fresh lord with minimal stats and equipment
		/// Usage: gm.hero.create_lord <gender> <name> <clan> [random|template|template:ID] [culture]
		/// </summary>
		[CommandLineFunctionality.CommandLineArgumentFunction("create_lord", "gm.hero")]
		public static string CreateLord(List<string> args)
		{
			return Cmd.Run(args, () =>
			{
				if (!CommandBase.ValidateCampaignMode(out string error))
					return error;

				var usageMessage = CommandValidator.CreateUsageMessage(
					"gm.hero.create_lord", "<gender> <name> <clan> [random|template|template:ID] [culture]",
					"Creates a fresh lord age 20-24 with minimal stats and only clothes.\n" +
					"- Gender must be 'male' or 'female'.\n" +
					"- Name: Use SINGLE QUOTES for multi-word names (double quotes don't work).\n" +
					"- Default: Uses template appearance without extra randomization.\n" +
					"- 'random': Fully randomizes appearance beyond template constraints.\n" +
					"- 'template': Uses random template (same as default).\n" +
					"- 'template:ID': Uses specific CharacterObject template ID.\n" +
					"- 'culture': Optional culture name to filter templates.",
					"gm.hero.create_lord male NewLord empire_south\ngm.hero.create_lord female 'Jane Doe' empire random\ngm.hero.create_lord male 'Lord One' empire template:lord_1_1");

				if (!CommandBase.ValidateArgumentCount(args, 3, usageMessage, out error))
					return error;

				// Parse gender
				bool isFemale;
				string genderArg = args[0].ToLower();
				if (genderArg == "male" || genderArg == "m")
					isFemale = false;
				else if (genderArg == "female" || genderArg == "f")
					isFemale = true;
				else
					return CommandBase.FormatErrorMessage("Gender must be 'male' or 'female'.");

				string name = args[1];
				if (string.IsNullOrWhiteSpace(name))
					return CommandBase.FormatErrorMessage("Name cannot be empty.");

				var (clan, clanError) = CommandBase.FindSingleClan(args[2]);
				if (clanError != null) return clanError;

				// Parse optional arguments
				bool fullRandomization = false;
				CharacterObject specificTemplate = null;
				TaleWorlds.Core.BasicCultureObject culture = null;

				for (int i = 3; i < args.Count; i++)
				{
					string arg = args[i].ToLower();

					if (arg == "random")
					{
						fullRandomization = true;
					}
					else if (arg.StartsWith("template:"))
					{
						// Extract template ID
						string templateId = arg.Substring(9);
						specificTemplate = CharacterObject.All.FirstOrDefault(c => c.StringId.ToLower() == templateId.ToLower());
						if (specificTemplate == null)
							return CommandBase.FormatErrorMessage($"Template '{templateId}' not found.");
					}
					else if (arg == "template")
					{
						fullRandomization = false;
					}
					else if (arg.StartsWith("culture"))
					{
						// Try to find culture
						string cultureName = arg.Contains("_") ? arg : (i + 1 < args.Count ? args[++i] : "");
						var cultures = TaleWorlds.ObjectSystem.MBObjectManager.Instance.GetObjectTypeList<TaleWorlds.Core.BasicCultureObject>();
						foreach (var c in cultures)
						{
							if (c.StringId.ToLower().Contains(cultureName.ToLower()) || c.Name.ToString().ToLower().Contains(cultureName.ToLower()))
							{
								culture = c;
								break;
							}
						}
					}
				}

				return CommandBase.ExecuteWithErrorHandling(() =>
				{
					// Create configuration for lord creation
					var config = new HeroGenerator.HeroCreationConfig
					{
						IsFemale = isFemale,
						Name = name,
						TargetClan = clan,
						MinAge = 20,
						MaxAge = 24,
						RandomizeAppearance = true,
						AddCivilianClothes = true,
						FullRandomization = fullRandomization,
						SpecificTemplate = specificTemplate,
						Culture = culture
					};

					// Create lord using HeroGenerator
					var generator = new HeroGenerator();
					var result = generator.CreateFreshHero(config);

					if (!result.Success)
						return CommandBase.FormatErrorMessage(result.ErrorMessage);

					var (newLord, lordClan) = result.CreatedLords[0];

					return CommandBase.FormatSuccessMessage(
						$"Created fresh lord '{newLord.Name}' (ID: {newLord.StringId}):\n" +
						$"Age: {(int)newLord.Age} | Gender: {(isFemale ? "Female" : "Male")} | Clan: {lordClan.Name}\n" +
						$"Level: 1 | Equipment: Minimal");
				}, "Failed to create lord");
			});
		}
	}
}