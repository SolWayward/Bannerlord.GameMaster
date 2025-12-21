using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Heroes;
using Bannerlord.GameMaster.Characters;
using Bannerlord.GameMaster.Party;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.HeroCommands
{
	[CommandLineFunctionality.CommandLineArgumentFunction("hero", "gm")]
	public static class HeroGenerationCommands
	{
		//MARK: generate_lords
		/// <summary>
		/// Generate new lords with random templates. Lords will have parties and good equipment.
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

				// Default clan if none specified - will create lords for different random clans
				if (targetClan == null)
				{
					var clans = Clan.NonBanditFactions.ToArray();
					List<Hero> createdHeroes = new List<Hero>();
					
					for (int i = 0; i < count; i++)
					{
						Clan randomClan = clans[RandomNumberGen.Instance.NextRandomInt(clans.Length)];
						Hero lord = HeroGenerator.CreateLord($"Lord_{i}", cultureFlags, genderFlags, randomClan, withParty: true, randomFactor);
						if (lord != null)
							createdHeroes.Add(lord);
					}
					
					if (createdHeroes.Count == 0)
						return CommandBase.FormatErrorMessage("Failed to create lords - no templates found matching criteria");

					return CommandBase.FormatSuccessMessage($"Created {createdHeroes.Count} lord(s):\n{HeroQueries.GetFormattedDetails(createdHeroes)}");
				}

				return CommandBase.ExecuteWithErrorHandling(() =>
				{
					// Use new architecture - CreateLords method for single clan
					List<Hero> createdHeroes = HeroGenerator.CreateLords(count, cultureFlags, genderFlags, targetClan, withParties: true, randomFactor);

					if (createdHeroes == null || createdHeroes.Count == 0)
						return CommandBase.FormatErrorMessage("Failed to create lords - no templates found matching criteria");

					return CommandBase.FormatSuccessMessage($"Created {createdHeroes.Count} lord(s):\n{HeroQueries.GetFormattedDetails(createdHeroes)}");
				}, "Failed to generate lords");
			});
		}

		//MARK: create_lord
		/// <summary>
		/// Create a new lord with a chosen name from random templates
		/// Usage: gm.hero.create_lord <name> [cultures] [gender] [clan] [withParty] [settlement] [randomFactor]
		/// </summary>
		[CommandLineFunctionality.CommandLineArgumentFunction("create_lord", "gm.hero")]
		public static string CreateLord(List<string> args)
		{
			return Cmd.Run(args, () =>
			{
				if (!CommandBase.ValidateCampaignMode(out string error))
					return error;

				var usageMessage = CommandValidator.CreateUsageMessage(
					"gm.hero.create_lord", "<name> [cultures] [gender] [clan] [withParty] [settlement] [randomFactor]",
					"Creates a single lord from random templates with good gear and decent stats. Age 20-30. Allows custom naming.\n" +
					"- name: required, the name for the hero. Use SINGLE QUOTES for multi-word names\n" +
					"- cultures: optional, defines the pool of cultures allowed to be chosen from. Defaults to main_cultures\n" +
					"- gender: optional, use keywords both, female, or male. also allowed b, f, and m. Defaults to both\n" +
					"- clan: optional, clanID or clanName. If not specified, hero goes to a random clan\n" +
					"- withParty: optional, true/false to create party for lord. Defaults to true\n" +
					"- settlement: optional, settlement for lord without party to reside in (only used if withParty is false)\n" +
					"- randomFactor: optional, float value between 0 and 1. defaults to 0.5\n",
					"gm.hero.create_lord 'Sir Percival'\n" +
					"gm.hero.create_lord Ragnar vlandia male player_faction\n" +
					"gm.hero.create_lord 'Lady Elara' empire female clan_x false pen\n" +
					"gm.hero.create_lord Khalid aserai male clan_y true null 0.8");

				// Minimum 1 required argument: name
				if (args.Count < 1)
					return usageMessage;

				// Parse name (required)
				string name = args[0];
				if (string.IsNullOrWhiteSpace(name))
					return CommandBase.FormatErrorMessage("Name cannot be empty.");

				// Smart parse remaining arguments
				CultureFlags cultureFlags = CultureFlags.AllMainCultures;
				GenderFlags genderFlags = GenderFlags.Either;
				Clan targetClan = null;
				bool withParty = true;
				Settlement settlement = null;
				float randomFactor = 0.5f;

				int currentArgIndex = 1;

				// Parse cultures if provided (or skip if it's a gender keyword)
				if (args.Count > currentArgIndex)
				{
					GenderFlags testGender = FlagParser.ParseGenderArgument(args[currentArgIndex]);
					if (testGender != GenderFlags.None)
					{
						genderFlags = testGender;
						currentArgIndex++;
					}
					else
					{
						cultureFlags = FlagParser.ParseCultureArgument(args[currentArgIndex]);
						if (cultureFlags == CultureFlags.None)
							return CommandBase.FormatErrorMessage($"Invalid culture(s): '{args[currentArgIndex]}'");
						currentArgIndex++;

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
					if (!bool.TryParse(args[currentArgIndex], out bool _) && !float.TryParse(args[currentArgIndex], out float _))
					{
						var (clan, clanError) = CommandBase.FindSingleClan(args[currentArgIndex]);
						if (clanError != null)
							return clanError;
						targetClan = clan;
						currentArgIndex++;
					}
				}

				// Parse withParty
				if (args.Count > currentArgIndex)
				{
					if (bool.TryParse(args[currentArgIndex], out bool parsedWithParty))
					{
						withParty = parsedWithParty;
						currentArgIndex++;
					}
				}

				// Parse settlement (if withParty is false)
				if (args.Count > currentArgIndex && !withParty)
				{
					if (!float.TryParse(args[currentArgIndex], out float _) && args[currentArgIndex].ToLower() != "null")
					{
						var (parsedSettlement, settlementError) = CommandBase.FindSingleSettlement(args[currentArgIndex]);
						if (settlementError != null)
							return settlementError;
						settlement = parsedSettlement;
						currentArgIndex++;
					}
					else if (args[currentArgIndex].ToLower() == "null")
					{
						currentArgIndex++;
					}
				}

				// Parse randomFactor
				if (args.Count > currentArgIndex)
				{
					if (!CommandValidator.ValidateFloatRange(args[currentArgIndex], 0f, 1f, out randomFactor, out string randomError))
						return CommandBase.FormatErrorMessage(randomError);
				}

				// Default clan if none specified
				if (targetClan == null)
				{
					var clans = Clan.NonBanditFactions.ToArray();
					targetClan = clans[RandomNumberGen.Instance.NextRandomInt(clans.Length)];
				}

				return CommandBase.ExecuteWithErrorHandling(() =>
				{
					// Use new architecture - CreateLord method
					Hero createdHero = HeroGenerator.CreateLord(name, cultureFlags, genderFlags, targetClan, withParty, randomFactor);

					if (createdHero == null)
						return CommandBase.FormatErrorMessage("Failed to create lord - no templates found matching criteria");

					// If no party and settlement specified, place lord there
					if (!withParty && settlement != null)
					{
						EnterSettlementAction.ApplyForCharacterOnly(createdHero, settlement);
						createdHero.UpdateLastKnownClosestSettlement(settlement);
					}

					string partyInfo = withParty ? " with party" : (settlement != null ? $" at {settlement.Name}" : " (no party)");
					return CommandBase.FormatSuccessMessage($"Created lord '{createdHero.Name}' (ID: {createdHero.StringId}){partyInfo}\n{HeroQueries.GetFormattedDetails(new List<Hero> { createdHero })}");
				}, "Failed to create lord");
			});
		}

		//MARK: create_companions
		/// <summary>
		/// Create companions ready to be added to a party
		/// Usage: gm.hero.create_companions <count> <heroLeader> [cultures] [gender] [randomFactor]
		/// </summary>
		[CommandLineFunctionality.CommandLineArgumentFunction("create_companions", "gm.hero")]
		public static string CreateCompanions(List<string> args)
		{
			return Cmd.Run(args, () =>
			{
				if (!CommandBase.ValidateCampaignMode(out string error))
					return error;

				var usageMessage = CommandValidator.CreateUsageMessage(
					"gm.hero.create_companions", "<count> <heroLeader> [cultures] [gender] [randomFactor]",
					"Creates companions and adds them directly to the specified hero's party.\n" +
					"Companions are added as party members (not wanderers in settlements).\n" +
					"- count: required, number of companions to create (1-20)\n" +
					"- heroLeader: required, hero ID or name of party leader. Use 'player' for your party\n" +
					"- cultures: optional, culture pool for template selection. Defaults to main_cultures\n" +
					"- gender: optional, use keywords both, female, or male. Defaults to both\n" +
					"- randomFactor: optional, float value between 0 and 1. defaults to 0.5\n",
					"gm.hero.create_companions 5 player\n" +
					"gm.hero.create_companions 3 player vlandia both\n" +
					"gm.hero.create_companions 2 'Lord Name' battania;sturgia female 0.8");

				if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
					return error;

				// Parse count (required)
				if (!CommandValidator.ValidateIntegerRange(args[0], 1, 20, out int count, out string countError))
					return CommandBase.FormatErrorMessage(countError);

				// Parse hero leader (required)
				var (hero, heroError) = CommandBase.FindSingleHero(args[1]);
				if (heroError != null)
					return heroError;

				if (hero.PartyBelongedTo == null)
					return CommandBase.FormatErrorMessage($"Hero {hero.Name} is not in a party.");

				if (hero.PartyBelongedTo.LeaderHero != hero)
					return CommandBase.FormatErrorMessage($"Hero {hero.Name} is not the leader of their party.");

				// Parse optional parameters
				CultureFlags cultureFlags = CultureFlags.AllMainCultures;
				GenderFlags genderFlags = GenderFlags.Either;
				float randomFactor = 0.5f;
				int currentArgIndex = 2;

				// Parse cultures if provided
				if (args.Count > currentArgIndex)
				{
					// Check if this argument is a gender keyword first
					GenderFlags testGender = FlagParser.ParseGenderArgument(args[currentArgIndex]);
					if (testGender != GenderFlags.None)
					{
						genderFlags = testGender;
						currentArgIndex++;
					}
					else
					{
						// Try to parse as culture
						cultureFlags = FlagParser.ParseCultureArgument(args[currentArgIndex]);
						if (cultureFlags == CultureFlags.None)
							return CommandBase.FormatErrorMessage($"Invalid culture(s): '{args[currentArgIndex]}'");
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

				// Parse randomFactor if provided
				if (args.Count > currentArgIndex)
				{
					if (!CommandValidator.ValidateFloatRange(args[currentArgIndex], 0f, 1f, out randomFactor, out string randomError))
						return CommandBase.FormatErrorMessage(randomError);
				}

				return CommandBase.ExecuteWithErrorHandling(() =>
				{
					// Create companions using new architecture
					List<Hero> companions = HeroGenerator.CreateCompanions(count, cultureFlags, genderFlags, hero.Clan, randomFactor);

					if (companions == null || companions.Count == 0)
						return CommandBase.FormatErrorMessage("Failed to create companions - no templates found matching criteria");

					// Add companions to party
					hero.PartyBelongedTo.AddCompanionsToParty(companions);

					return CommandBase.FormatSuccessMessage(
						$"Created and added {companions.Count} companion(s) to {hero.Name}'s party:\n" +
						HeroQueries.GetFormattedDetails(companions));
				}, "Failed to create companions");
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
