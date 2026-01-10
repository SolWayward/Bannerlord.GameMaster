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
using Bannerlord.GameMaster.Cultures;

namespace Bannerlord.GameMaster.Console.HeroCommands
{
	[CommandLineFunctionality.CommandLineArgumentFunction("hero", "gm")]
	public static class HeroGenerationCommands
	{
		//MARK: generate_lords
		/// <summary>
		/// Generate new lords with random templates. Lords will have parties and good equipment.
		/// Usage: gm.hero.generate_lords &lt;count&gt; [cultures] [gender] [clan] [randomFactor]
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
					"- count: required, number of lords to generate (1-50)\n" +
					"- cultures/culture: optional, defines the pool of cultures. Defaults to main_cultures. Use commas for multiple: vlandia,battania\n" +
					"- gender: optional, use keywords both, female, or male (also b, f, m). Defaults to both\n" +
					"- clan: optional, clanID or clanName. If not specified, each hero goes to a different random clan\n" +
					"- randomFactor/random: optional, float value between 0 and 1. defaults to 1\n" +
					"Supports named arguments: count:15 cultures:vlandia,battania gender:male clan:player_faction random:0.8",
					"gm.hero.generate_lords 15\n" +
					"gm.hero.generate_lords 15 vlandia player_faction male\n" +
					"gm.hero.generate_lords count:12 cultures:aserai,sturgia,khuzait clan:'dey Meroc'\n" +
					"gm.hero.generate_lords 12 aserai,sturgia,khuzait,empire both 'dey Meroc' 0.7");

				// Parse arguments with named argument support
				var parsedArgs = CommandBase.ParseArguments(args);
				
				// Define valid arguments for validation
				parsedArgs.SetValidArguments(
					new CommandBase.ArgumentDefinition("count", true),
					new CommandBase.ArgumentDefinition("cultures", false, null, "culture"),
					new CommandBase.ArgumentDefinition("gender", false),
					new CommandBase.ArgumentDefinition("clan", false),
					new CommandBase.ArgumentDefinition("randomFactor", false, null, "random")
				);

				// Check for validation errors
				string validationError = parsedArgs.GetValidationError();
				if (validationError != null)
					return CommandBase.FormatErrorMessage(validationError);

				if (parsedArgs.TotalCount < 1)
					return usageMessage;

				// Parse count (required)
				string countArg = parsedArgs.GetArgument("count", 0);
				if (countArg == null)
					return CommandBase.FormatErrorMessage("Missing required argument 'count'.");
				
				if (!CommandValidator.ValidateIntegerRange(countArg, 1, 50, out int count, out string countError))
					return CommandBase.FormatErrorMessage(countError);

				// Parse optional cultures
				CultureFlags cultureFlags = CultureFlags.AllMainCultures;
				GenderFlags genderFlags = GenderFlags.Either;
				Clan targetClan = null;
				float randomFactor = 1f;

				// Try named 'cultures' or 'culture' first, then positional
				string culturesArg = parsedArgs.GetNamed("cultures") ?? parsedArgs.GetNamed("culture");
				if (culturesArg == null && parsedArgs.PositionalCount > 1)
				{
					// Check if positional arg 1 is a gender keyword or culture
					GenderFlags testGender = FlagParser.ParseGenderArgument(parsedArgs.GetPositional(1));
					if (testGender == GenderFlags.None)
					{
						// Not a gender, treat as culture
						culturesArg = parsedArgs.GetPositional(1);
					}
				}
				
				if (culturesArg != null)
				{
					cultureFlags = FlagParser.ParseCultureArgument(culturesArg);
					if (cultureFlags == CultureFlags.None)
						return CommandBase.FormatErrorMessage($"Invalid culture(s): '{culturesArg}'. Use culture names (e.g., vlandia,battania) or groups (main_cultures, bandit_cultures, all_cultures)");
				}

				// Parse optional gender - try named first, then scan positional args
				string genderArg = parsedArgs.GetNamed("gender");
				if (genderArg == null)
				{
					// Scan positional arguments for gender keywords
					for (int i = 1; i < parsedArgs.PositionalCount; i++)
					{
						GenderFlags testGender = FlagParser.ParseGenderArgument(parsedArgs.GetPositional(i));
						if (testGender != GenderFlags.None)
						{
							genderFlags = testGender;
							break;
						}
					}
				}
				else
				{
					genderFlags = FlagParser.ParseGenderArgument(genderArg);
					if (genderFlags == GenderFlags.None)
						return CommandBase.FormatErrorMessage($"Invalid gender: '{genderArg}'. Use 'both', 'female', or 'male'.");
				}

				// Parse optional clan - try named first, then look for non-gender, non-culture, non-float positional
				string clanArg = parsedArgs.GetNamed("clan");
				if (clanArg == null)
				{
					// Look through positional args for something that's not a number and not a gender
					for (int i = 1; i < parsedArgs.PositionalCount; i++)
					{
						string arg = parsedArgs.GetPositional(i);
						if (!float.TryParse(arg, out _) &&
						    FlagParser.ParseGenderArgument(arg) == GenderFlags.None &&
						    FlagParser.ParseCultureArgument(arg) == CultureFlags.None)
						{
							clanArg = arg;
							break;
						}
					}
				}
				
				if (clanArg != null)
				{
					var (clan, clanError) = CommandBase.FindSingleClan(clanArg);
					if (clanError != null)
						return clanError;
					targetClan = clan;
				}

				// Parse optional randomFactor
				string randomArg = parsedArgs.GetNamed("randomFactor") ?? parsedArgs.GetNamed("random");
				if (randomArg == null)
				{
					// Look for a float in positional args
					for (int i = 1; i < parsedArgs.PositionalCount; i++)
					{
						if (float.TryParse(parsedArgs.GetPositional(i), out float testFloat))
						{
							randomArg = parsedArgs.GetPositional(i);
							break;
						}
					}
				}
				
				if (randomArg != null)
				{
					if (!CommandValidator.ValidateFloatRange(randomArg, 0f, 1f, out randomFactor, out string randomError))
						return CommandBase.FormatErrorMessage(randomError);
				}

				// Validate hero creation limit
				if (!CommandValidator.ValidateHeroCreationLimit(count, out string limitError))
					return CommandBase.FormatErrorMessage(limitError);

				// Build resolved values dictionary for display
				var resolvedValues = new Dictionary<string, string>
				{
					{ "count", count.ToString() },
					{ "cultures", culturesArg ?? "Main Cultures" },
					{ "gender", genderFlags == GenderFlags.Either ? "Both" : (genderFlags == GenderFlags.Male ? "Male" : "Female") },
					{ "clan", targetClan != null ? targetClan.Name.ToString() : "Random" },
					{ "randomFactor", randomFactor.ToString("0.0") }
				};

				// Display argument header
				string argumentDisplay = parsedArgs.FormatArgumentDisplay("generate_lords", resolvedValues);

				return CommandBase.ExecuteWithErrorHandling(() =>
				{
					List<Hero> createdHeroes;

					// Default clan if none specified - will create lords for different random clans
					if (targetClan == null)
					{
						Clan[] clans = Clan.NonBanditFactions.ToArray();
						createdHeroes = new();
						
						// Group by clan to use efficient batch creation that generates proper names
						List<Clan> clansToUse = new();
						for (int i = 0; i < count; i++)
						{
							clansToUse.Add(clans[RandomNumberGen.Instance.NextRandomInt(clans.Length)]);
						}
						
						// Create lords in batches per clan using the batch method that generates culture-appropriate names
						var groupedClans = clansToUse.GroupBy(c => c);
						foreach (var clanGroup in groupedClans)
						{
							List<Hero> clanLords = HeroGenerator.CreateLords(clanGroup.Count(), cultureFlags, genderFlags, clanGroup.Key, withParties: true, randomFactor);
							createdHeroes.AddRange(clanLords);
						}
					}
					else
					{
						// Use new architecture - CreateLords method for single clan
						createdHeroes = HeroGenerator.CreateLords(count, cultureFlags, genderFlags, targetClan, withParties: true, randomFactor);
					}

					if (createdHeroes == null || createdHeroes.Count == 0)
						return argumentDisplay + CommandBase.FormatErrorMessage("Failed to create lords - no templates found matching criteria");

					return argumentDisplay + CommandBase.FormatSuccessMessage($"Created {createdHeroes.Count} lord(s):\n{HeroQueries.GetFormattedDetails(createdHeroes)}");
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
					"Creates a single lord from random templates with good gear and decent stats. Age 18-30. Allows custom naming.\n" +
					"Creates a party for the lord by default if clan is not at max allowed parties. Use create_party to exceed party limit" +
					"- name: required, the name for the hero. Use SINGLE QUOTES for multi-word names\n" +
					"- cultures/culture: optional, defines the pool of cultures allowed to be chosen from. Defaults to main_cultures\n" +
					"- gender: optional, use keywords both, female, or male. also allowed b, f, and m. Defaults to both\n" +
					"- clan: optional, clanID or clanName. If not specified, hero goes to a random clan\n" +
					"- withParty: optional, true/false to create party for lord. Defaults to true (Will only create party if clan is below party limit)\n" +
					"- settlement: optional, settlement for lord without party to reside in (only used if withParty is false)\n" +
					"- randomFactor/random: optional, float value between 0 and 1. defaults to 0.5\n" +
					"Supports named arguments: name:'Sir Percival' cultures:vlandia gender:male clan:player_faction withParty:true randomFactor:0.8",
					"gm.hero.create_lord 'Sir Percival'\n" +
					"gm.hero.create_lord Ragnar vlandia male player_faction\n" +
					"gm.hero.create_lord name:'Lady Elara' cultures:empire gender:female clan:clan_x withParty:false settlement:pen\n" +
					"gm.hero.create_lord Khalid aserai male clan_y true null 0.8");

				// Parse arguments
				var parsedArgs = CommandBase.ParseArguments(args);

				// Define valid arguments for validation
				parsedArgs.SetValidArguments(
					new CommandBase.ArgumentDefinition("name", true),
					new CommandBase.ArgumentDefinition("cultures", false, null, "culture"),
					new CommandBase.ArgumentDefinition("gender", false),
					new CommandBase.ArgumentDefinition("clan", false),
					new CommandBase.ArgumentDefinition("withParty", false),
					new CommandBase.ArgumentDefinition("settlement", false),
					new CommandBase.ArgumentDefinition("randomFactor", false, null, "random")
				);

				// Check for validation errors
				string validationError = parsedArgs.GetValidationError();
				if (validationError != null)
					return CommandBase.FormatErrorMessage(validationError);

				// Minimum 1 required argument: name
				if (parsedArgs.TotalCount < 1)
					return usageMessage;

				// Parse name (required)
				string name = parsedArgs.GetArgument("name", 0);
				if (string.IsNullOrWhiteSpace(name))
					return CommandBase.FormatErrorMessage("Name cannot be empty.");

				// Smart parse remaining arguments
				CultureFlags cultureFlags = CultureFlags.AllMainCultures;
				GenderFlags genderFlags = GenderFlags.Either;
				Clan targetClan = null;
				bool withParty = true;
				Settlement settlement = null;
				float randomFactor = 0.5f;

				// Parse cultures - try named first, then positional
				string culturesArg = parsedArgs.GetNamed("cultures") ?? parsedArgs.GetNamed("culture");
				if (culturesArg == null && parsedArgs.PositionalCount > 1)
				{
					// Check if positional arg 1 is a gender keyword or culture
					GenderFlags testGender = FlagParser.ParseGenderArgument(parsedArgs.GetPositional(1));
					if (testGender == GenderFlags.None)
					{
						// Not a gender, treat as culture
						culturesArg = parsedArgs.GetPositional(1);
					}
				}
				
				if (culturesArg != null)
				{
					cultureFlags = FlagParser.ParseCultureArgument(culturesArg);
					if (cultureFlags == CultureFlags.None)
						return CommandBase.FormatErrorMessage($"Invalid culture(s): '{culturesArg}'");
				}

				// Parse gender - try named first, then scan positional
				string genderArg = parsedArgs.GetNamed("gender");
				if (genderArg == null)
				{
					// Scan positional arguments for gender keywords
					for (int i = 1; i < parsedArgs.PositionalCount; i++)
					{
						GenderFlags testGender = FlagParser.ParseGenderArgument(parsedArgs.GetPositional(i));
						if (testGender != GenderFlags.None)
						{
							genderFlags = testGender;
							break;
						}
					}
				}
				else
				{
					genderFlags = FlagParser.ParseGenderArgument(genderArg);
					if (genderFlags == GenderFlags.None)
						return CommandBase.FormatErrorMessage($"Invalid gender: '{genderArg}'");
				}

				// Parse clan - try named first
				string clanArg = parsedArgs.GetNamed("clan");
				if (clanArg == null)
				{
					// Look through positional args for something that's not a bool, number, or gender
					for (int i = 1; i < parsedArgs.PositionalCount; i++)
					{
						string arg = parsedArgs.GetPositional(i);
						if (!bool.TryParse(arg, out _) && !float.TryParse(arg, out _) &&
						    FlagParser.ParseGenderArgument(arg) == GenderFlags.None &&
						    FlagParser.ParseCultureArgument(arg) == CultureFlags.None)
						{
							clanArg = arg;
							break;
						}
					}
				}
				
				if (clanArg != null)
				{
					var (clan, clanError) = CommandBase.FindSingleClan(clanArg);
					if (clanError != null)
						return clanError;
					targetClan = clan;
				}

				// Parse withParty - try named first
				string withPartyArg = parsedArgs.GetNamed("withParty");
				if (withPartyArg != null)
				{
					if (!bool.TryParse(withPartyArg, out withParty))
						return CommandBase.FormatErrorMessage($"Invalid withParty value: '{withPartyArg}'. Use true or false.");
				}
				else
				{
					// Look for bool in positional args
					for (int i = 1; i < parsedArgs.PositionalCount; i++)
					{
						if (bool.TryParse(parsedArgs.GetPositional(i), out bool parsedBool))
						{
							withParty = parsedBool;
							break;
						}
					}
				}

				// Parse settlement - try named first
				string settlementArg = parsedArgs.GetNamed("settlement");
				if (settlementArg != null && settlementArg.ToLower() != "null")
				{
					var (parsedSettlement, settlementError) = CommandBase.FindSingleSettlement(settlementArg);
					if (settlementError != null)
						return settlementError;
					settlement = parsedSettlement;
				}

				// Parse randomFactor
				string randomArg = parsedArgs.GetNamed("randomFactor") ?? parsedArgs.GetNamed("random");
				if (randomArg == null)
				{
					// Look for a float in positional args
					for (int i = 1; i < parsedArgs.PositionalCount; i++)
					{
						if (float.TryParse(parsedArgs.GetPositional(i), out float testFloat))
						{
							randomArg = parsedArgs.GetPositional(i);
							break;
						}
					}
				}
				
				if (randomArg != null)
				{
					if (!CommandValidator.ValidateFloatRange(randomArg, 0f, 1f, out randomFactor, out string randomError))
						return CommandBase.FormatErrorMessage(randomError);
				}

				// Validate hero creation limit (creating 1 hero)
				if (!CommandValidator.ValidateHeroCreationLimit(1, out string limitError))
					return CommandBase.FormatErrorMessage(limitError);

				// Default clan if none specified
				if (targetClan == null)
				{
					var clans = Clan.NonBanditFactions.ToArray();
					targetClan = clans[RandomNumberGen.Instance.NextRandomInt(clans.Length)];
				}

				// Build resolved values dictionary for display
				var resolvedValues = new Dictionary<string, string>
				{
					{ "name", name },
					{ "cultures", culturesArg ?? "Main Cultures" },
					{ "gender", genderFlags == GenderFlags.Either ? "Both" : (genderFlags == GenderFlags.Male ? "Male" : "Female") },
					{ "clan", targetClan.Name.ToString() },
					{ "withParty", withParty.ToString() },
					{ "settlement", settlement != null ? settlement.Name.ToString() : "None" },
					{ "randomFactor", randomFactor.ToString("0.0") }
				};

				// Display argument header
				string argumentDisplay = parsedArgs.FormatArgumentDisplay("create_lord", resolvedValues);

				return CommandBase.ExecuteWithErrorHandling(() =>
				{
					// Use new architecture - CreateLord method
					Hero createdHero = HeroGenerator.CreateLord(name, cultureFlags, genderFlags, targetClan, withParty, randomFactor);

					if (createdHero == null)
						return argumentDisplay + CommandBase.FormatErrorMessage("Failed to create lord - no templates found matching criteria");

					// If no party and settlement specified, place lord there
					if (!withParty && settlement != null)
					{
						EnterSettlementAction.ApplyForCharacterOnly(createdHero, settlement);
						createdHero.UpdateLastKnownClosestSettlement(settlement);
					}

					string partyInfo = withParty ? " with party" : (settlement != null ? $" at {settlement.Name}" : " (no party)");
					return argumentDisplay + CommandBase.FormatSuccessMessage($"Created lord '{createdHero.Name}' (ID: {createdHero.StringId}){partyInfo}\n{HeroQueries.GetFormattedDetails(new List<Hero> { createdHero })}");
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
					"Companions are added as party members. Will not exceed companion limit, use create_lord instead for that.\n" +
					"- count: required, number of companions to create (1-20)\n" +
					"- heroLeader/hero: required, hero ID or name of party leader. Use 'player' for your party\n" +
					"- cultures/culture: optional, culture pool for template selection. Defaults to main_cultures\n" +
					"- gender: optional, use keywords both, female, or male. Defaults to both\n" +
					"- randomFactor/random: optional, float value between 0 and 1. defaults to 0.5\n" +
					"Supports named arguments: count:5 hero:player cultures:vlandia,battania gender:female\n",
					"gm.hero.create_companions 5 player\n" +
					"gm.hero.create_companions 3 player vlandia both\n" +
					"gm.hero.create_companions count:2 hero:'Lord Name' cultures:battania,sturgia gender:female\n" +
					"gm.hero.create_companions 2 'Lord Name' battania,sturgia female 0.8");

				// Parse arguments
				var parsedArgs = CommandBase.ParseArguments(args);

				// Define valid arguments for validation
				parsedArgs.SetValidArguments(
					new CommandBase.ArgumentDefinition("count", true),
					new CommandBase.ArgumentDefinition("heroLeader", true, null, "hero"),
					new CommandBase.ArgumentDefinition("cultures", false, null, "culture"),
					new CommandBase.ArgumentDefinition("gender", false),
					new CommandBase.ArgumentDefinition("randomFactor", false, null, "random")
				);

				// Check for validation errors
				string validationError = parsedArgs.GetValidationError();
				if (validationError != null)
					return CommandBase.FormatErrorMessage(validationError);

				if (parsedArgs.TotalCount < 2)
					return usageMessage;

				// Parse count (required)
				string countArg = parsedArgs.GetArgument("count", 0);
				if (countArg == null)
					return CommandBase.FormatErrorMessage("Missing required argument 'count'.");
				
				if (!CommandValidator.ValidateIntegerRange(countArg, 1, 20, out int count, out string countError))
					return CommandBase.FormatErrorMessage(countError);

				// Parse hero leader (required)
				string heroArg = parsedArgs.GetArgument("heroLeader", 1) ?? parsedArgs.GetNamed("hero");
				if (heroArg == null)
					return CommandBase.FormatErrorMessage("Missing required argument 'heroLeader'.");

				var (hero, heroError) = CommandBase.FindSingleHero(heroArg);
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

				// Parse cultures - try named first, then positional
				string culturesArg = parsedArgs.GetNamed("cultures") ?? parsedArgs.GetNamed("culture");
				if (culturesArg == null && parsedArgs.PositionalCount > 2)
				{
					// Check if positional arg 2 is a gender keyword or culture
					GenderFlags testGender = FlagParser.ParseGenderArgument(parsedArgs.GetPositional(2));
					if (testGender == GenderFlags.None)
					{
						// Not a gender, treat as culture
						culturesArg = parsedArgs.GetPositional(2);
					}
				}
				
				if (culturesArg != null)
				{
					cultureFlags = FlagParser.ParseCultureArgument(culturesArg);
					if (cultureFlags == CultureFlags.None)
						return CommandBase.FormatErrorMessage($"Invalid culture(s): '{culturesArg}'");
				}

				// Parse gender - try named first, then scan positional
				string genderArg = parsedArgs.GetNamed("gender");
				if (genderArg == null)
				{
					// Scan positional arguments for gender keywords
					for (int i = 2; i < parsedArgs.PositionalCount; i++)
					{
						GenderFlags testGender = FlagParser.ParseGenderArgument(parsedArgs.GetPositional(i));
						if (testGender != GenderFlags.None)
						{
							genderFlags = testGender;
							break;
						}
					}
				}
				else
				{
					genderFlags = FlagParser.ParseGenderArgument(genderArg);
					if (genderFlags == GenderFlags.None)
						return CommandBase.FormatErrorMessage($"Invalid gender: '{genderArg}'");
				}

				// Parse randomFactor
				string randomArg = parsedArgs.GetNamed("randomFactor") ?? parsedArgs.GetNamed("random");
				if (randomArg == null)
				{
					// Look for a float in positional args
					for (int i = 2; i < parsedArgs.PositionalCount; i++)
					{
						if (float.TryParse(parsedArgs.GetPositional(i), out float testFloat))
						{
							randomArg = parsedArgs.GetPositional(i);
							break;
						}
					}
				}
				
				if (randomArg != null)
				{
					if (!CommandValidator.ValidateFloatRange(randomArg, 0f, 1f, out randomFactor, out string randomError))
						return CommandBase.FormatErrorMessage(randomError);
				}

				// Validate hero creation limit
				if (!CommandValidator.ValidateHeroCreationLimit(count, out string limitError))
					return CommandBase.FormatErrorMessage(limitError);

				// Build resolved values dictionary for display
				var resolvedValues = new Dictionary<string, string>
				{
					{ "count", count.ToString() },
					{ "heroLeader", hero.Name.ToString() },
					{ "cultures", culturesArg ?? "Main Cultures" },
					{ "gender", genderFlags == GenderFlags.Either ? "Both" : (genderFlags == GenderFlags.Male ? "Male" : "Female") },
					{ "randomFactor", randomFactor.ToString("0.0") }
				};

				// Display argument header
				string argumentDisplay = parsedArgs.FormatArgumentDisplay("create_companions", resolvedValues);

				return CommandBase.ExecuteWithErrorHandling(() =>
				{
					// Create companions using new architecture
					List<Hero> companions = HeroGenerator.CreateCompanions(count, cultureFlags, genderFlags, randomFactor);

					if (companions == null || companions.Count == 0)
						return argumentDisplay + CommandBase.FormatErrorMessage("Failed to create companions - no templates found matching criteria");

					// Add companions to party
					hero.PartyBelongedTo.AddCompanionsToParty(companions);

					return argumentDisplay + CommandBase.FormatSuccessMessage(
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
					"- heroQuery/hero: hero ID or name query to find a single hero\n" +
					"- name: the new name for the hero\n" +
					"Supports named arguments: hero:lord_1_1 name:'Sir Galahad'",
					"gm.hero.rename lord_1_1 'Sir Galahad'\n" +
					"gm.hero.rename hero:'old hero name' name:NewName");

				// Parse arguments
				var parsedArgs = CommandBase.ParseArguments(args);

				// Define valid arguments for validation
				parsedArgs.SetValidArguments(
					new CommandBase.ArgumentDefinition("heroQuery", true, null, "hero"),
					new CommandBase.ArgumentDefinition("name", true)
				);

				// Check for validation errors
				string validationError = parsedArgs.GetValidationError();
				if (validationError != null)
					return CommandBase.FormatErrorMessage(validationError);

				// Minimum 2 required arguments: heroQuery and name
				if (parsedArgs.TotalCount < 2)
					return usageMessage;

				// Parse hero query (required)
				string heroQuery = parsedArgs.GetArgument("heroQuery", 0) ?? parsedArgs.GetNamed("hero");
				if (heroQuery == null)
					return CommandBase.FormatErrorMessage("Missing required argument 'heroQuery'.");

				var (hero, heroError) = CommandBase.FindSingleHero(heroQuery);
				if (heroError != null)
					return heroError;

				// Parse name (required)
				string newName = parsedArgs.GetArgument("name", 1);
				if (string.IsNullOrWhiteSpace(newName))
					return CommandBase.FormatErrorMessage("Missing or empty required argument 'name'.");

				// Build resolved values dictionary for display
				var resolvedValues = new Dictionary<string, string>
				{
					{ "heroQuery", hero.Name.ToString() },
					{ "name", newName }
				};

				// Display argument header
				string argumentDisplay = parsedArgs.FormatArgumentDisplay("rename", resolvedValues);

				return CommandBase.ExecuteWithErrorHandling(() =>
				{
					string previousName = hero.Name.ToString();
					hero.SetStringName(newName);

					return argumentDisplay + CommandBase.FormatSuccessMessage(
						$"Hero renamed from '{previousName}' to '{hero.Name}' (ID: {hero.StringId})");
				}, "Failed to rename hero");
			});
		}
	}
}
