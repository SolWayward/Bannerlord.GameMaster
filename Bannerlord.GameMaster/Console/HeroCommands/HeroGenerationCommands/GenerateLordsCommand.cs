using Bannerlord.GameMaster.Characters;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Cultures;
using Bannerlord.GameMaster.Heroes;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.HeroCommands.HeroGenerationCommands
{
    [CommandLineFunctionality.CommandLineArgumentFunction("hero", "gm")]
    public static class GenerateLordsCommand
    {
        /// <summary>
        /// Generate new lords with random templates. Lords will have parties and good equipment.
        /// Usage: gm.hero.generate_lords &lt;count&gt; [cultures] [gender] [clan] [settlement] [randomFactor]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("generate_lords", "gm.hero")]
        public static string GenerateLords(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return CommandResult.Error(error).Message;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.hero.generate_lords", "<count> [cultures] [gender] [clan] [settlement] [randomFactor] [level] [age]",
                    "Creates lords from random templates with good gear and decent stats. Names are selected from their culture\n" +
                    "- count: required, number of lords to generate (1-50)\n" +
                    "- cultures/culture: optional, defines the pool of cultures. Defaults to main_cultures. Use commas for multiple: vlandia,battania\n" +
                    "- gender: optional, use keywords both, female, or male (also b, f, m). Defaults to both\n" +
                    "- clan: optional, clanID or clanName. If not specified, each hero goes to a different random clan\n" +
                    "- settlement: optional, settlement for spawning parties or home settlement. Defaults to automatic selection\n" +
                    "- randomFactor/random: optional, float value between 0 and 1. defaults to 1\n" +
                    "- level: optional, target level (1-62). If not specified, a random level between 10-25 is assigned per hero\n" +
                    "- age: optional, hero age (minimum 18). If not specified or less than 18, a random age between 18-30 is assigned per hero\n" +
                    "Supports named arguments: count:15 cultures:vlandia,battania gender:male clan:player_faction settlement:pen_cannoc random:0.8 level:20 age:25",
                    "gm.hero.generate_lords 15\n" +
                    "gm.hero.generate_lords 15 vlandia player_faction male\n" +
                    "gm.hero.generate_lords count:12 cultures:aserai,sturgia,khuzait clan:'dey Meroc' settlement:quyaz level:18 age:28\n" +
                    "gm.hero.generate_lords 12 aserai,sturgia,khuzait,empire both 'dey Meroc' pen 0.7 20 25");

                ParsedArguments parsed = ArgumentParser.ParseArguments(args);

                parsed.SetValidArguments(
                    new ArgumentDefinition("count", true),
                    new ArgumentDefinition("cultures", false, null, "culture"),
                    new ArgumentDefinition("gender", false),
                    new ArgumentDefinition("clan", false),
                    new ArgumentDefinition("settlement", false),
                    new ArgumentDefinition("randomFactor", false, null, "random"),
                    new ArgumentDefinition("level", false),
                    new ArgumentDefinition("age", false)
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError)).Message;

                if (parsed.TotalCount < 1)
                    return CommandResult.Error(usageMessage).Message;

                // MARK: Parse Arguments
                string countArg = parsed.GetArgument("count", 0);
                if (countArg == null)
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'count'.")).Message;

                if (!CommandValidator.ValidateIntegerRange(countArg, 1, 50, out int count, out string countError))
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage(countError)).Message;

                CultureFlags cultureFlags = CultureFlags.AllMainCultures;
                GenderFlags genderFlags = GenderFlags.Either;
                Clan targetClan = null;
                Settlement settlement = null;
                float randomFactor = 1f;
                int targetLevel = -1;
                int age = -1;
                string clanArg = null;
                string settlementArg = null;

                // Try named 'cultures' or 'culture' first, then positional
                string culturesArg = parsed.GetNamed("cultures") ?? parsed.GetNamed("culture");
                if (culturesArg == null && parsed.PositionalCount > 1)
                {
                    GenderFlags testGender = FlagParser.ParseGenderArgument(parsed.GetPositional(1));
                    if (testGender == GenderFlags.None)
                    {
                        culturesArg = parsed.GetPositional(1);
                    }
                }

                if (culturesArg != null)
                {
                    cultureFlags = FlagParser.ParseCultureArgument(culturesArg);
                    if (cultureFlags == CultureFlags.None)
                        return CommandResult.Error(MessageFormatter.FormatErrorMessage($"Invalid culture(s): '{culturesArg}'. Use culture names (e.g., vlandia,battania) or groups (main_cultures, bandit_cultures, all_cultures)")).Message;
                }

                // Parse optional gender - try named first, then scan positional args
                string genderArg = parsed.GetNamed("gender");
                if (genderArg == null)
                {
                    for (int i = 1; i < parsed.PositionalCount; i++)
                    {
                        GenderFlags testGender = FlagParser.ParseGenderArgument(parsed.GetPositional(i));
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
                        return CommandResult.Error(MessageFormatter.FormatErrorMessage($"Invalid gender: '{genderArg}'. Use 'both', 'female', or 'male'.")).Message;
                }

                // Parse optional clan - try named first, then look for non-gender, non-culture, non-float positional
                clanArg = parsed.GetNamed("clan");
                if (clanArg == null)
                {
                    for (int i = 1; i < parsed.PositionalCount; i++)
                    {
                        string arg = parsed.GetPositional(i);
                        if (!float.TryParse(arg, out _) &&
                            FlagParser.ParseGenderArgument(arg) == GenderFlags.None &&
                            FlagParser.ParseCultureArgument(arg) == CultureFlags.None)
                        {
                            EntityFinderResult<Clan> testClanResult = ClanFinder.FindSingleClan(arg);
                            if (testClanResult.IsSuccess)
                            {
                                clanArg = arg;
                                targetClan = testClanResult.Entity;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    EntityFinderResult<Clan> clanResult = ClanFinder.FindSingleClan(clanArg);
                    if (!clanResult.IsSuccess)
                        return CommandResult.Error(clanResult.Message).Message;
                    targetClan = clanResult.Entity;
                }

                // Parse optional settlement - try named first, then positional
                settlementArg = parsed.GetNamed("settlement");
                if (settlementArg == null)
                {
                    for (int i = 1; i < parsed.PositionalCount; i++)
                    {
                        string arg = parsed.GetPositional(i);
                        if (!float.TryParse(arg, out _) &&
                            FlagParser.ParseGenderArgument(arg) == GenderFlags.None &&
                            FlagParser.ParseCultureArgument(arg) == CultureFlags.None &&
                            arg != clanArg)
                        {
                            EntityFinderResult<Settlement> testSettlementResult = SettlementFinder.FindSingleSettlement(arg);
                            if (testSettlementResult.IsSuccess)
                            {
                                settlementArg = arg;
                                settlement = testSettlementResult.Entity;
                                break;
                            }
                        }
                    }
                }
                else if (settlementArg.ToLower() != "null")
                {
                    EntityFinderResult<Settlement> settlementResult = SettlementFinder.FindSingleSettlement(settlementArg);
                    if (!settlementResult.IsSuccess)
                        return CommandResult.Error(settlementResult.Message).Message;
                    settlement = settlementResult.Entity;
                }

                // Parse optional randomFactor
                string randomArg = parsed.GetNamed("randomFactor") ?? parsed.GetNamed("random");
                if (randomArg == null)
                {
                    for (int i = 1; i < parsed.PositionalCount; i++)
                    {
                        if (float.TryParse(parsed.GetPositional(i), out float testFloat))
                        {
                            randomArg = parsed.GetPositional(i);
                            break;
                        }
                    }
                }

                if (randomArg != null)
                {
                    if (!CommandValidator.ValidateFloatRange(randomArg, 0f, 1f, out randomFactor, out string randomError))
                        return CommandResult.Error(MessageFormatter.FormatErrorMessage(randomError)).Message;
                }

                // Parse level - named only (positional int parsing is too ambiguous with count)
                string levelArg = parsed.GetNamed("level");
                if (levelArg != null)
                {
                    if (!CommandValidator.ValidateIntegerRange(levelArg, 1, 62, out targetLevel, out string levelError))
                        return CommandResult.Error(MessageFormatter.FormatErrorMessage(levelError)).Message;
                }
                else
                {
                    // Scan positional for integers that could be level (after count, skip floats/gender/culture/clan/settlement)
                    for (int i = 1; i < parsed.PositionalCount; i++)
                    {
                        string arg = parsed.GetPositional(i);
                        if (int.TryParse(arg, out int testInt) && testInt > 1 &&
                            FlagParser.ParseGenderArgument(arg) == GenderFlags.None &&
                            FlagParser.ParseCultureArgument(arg) == CultureFlags.None &&
                            arg != clanArg && arg != settlementArg && arg != randomArg)
                        {
                            levelArg = arg;
                            if (!CommandValidator.ValidateIntegerRange(levelArg, 1, 62, out targetLevel, out string levelError2))
                                return CommandResult.Error(MessageFormatter.FormatErrorMessage(levelError2)).Message;
                            break;
                        }
                    }
                }

                // Parse age - named only preferred, then positional scan for second integer
                string ageArg = parsed.GetNamed("age");
                if (ageArg != null)
                {
                    if (!CommandValidator.ValidateIntegerRange(ageArg, 18, 999, out age, out string ageError))
                        return CommandResult.Error(MessageFormatter.FormatErrorMessage(ageError)).Message;
                }
                else
                {
                    for (int i = 1; i < parsed.PositionalCount; i++)
                    {
                        string arg = parsed.GetPositional(i);
                        if (int.TryParse(arg, out int testInt) && testInt > 1 &&
                            FlagParser.ParseGenderArgument(arg) == GenderFlags.None &&
                            FlagParser.ParseCultureArgument(arg) == CultureFlags.None &&
                            arg != clanArg && arg != settlementArg && arg != randomArg &&
                            arg != levelArg)
                        {
                            ageArg = arg;
                            if (!CommandValidator.ValidateIntegerRange(ageArg, 18, 999, out age, out string ageError2))
                                return CommandResult.Error(MessageFormatter.FormatErrorMessage(ageError2)).Message;
                            break;
                        }
                    }
                }

                if (!CommandValidator.ValidateHeroCreationLimit(count, out string limitError))
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage(limitError)).Message;

                // MARK: Execute Logic
                Dictionary<string, string> resolvedValues = new()
                {
                    { "count", count.ToString() },
                    { "cultures", culturesArg ?? "Main Cultures" },
                    { "gender", genderFlags == GenderFlags.Either ? "Both" : (genderFlags == GenderFlags.Male ? "Male" : "Female") },
                    { "clan", targetClan != null ? targetClan.Name.ToString() : "Random" },
                    { "settlement", settlement != null ? settlement.Name.ToString() : "Auto" },
                    { "randomFactor", randomFactor.ToString("0.0") },
                    { "level", targetLevel > 0 ? targetLevel.ToString() : "Random (10-25)" },
                    { "age", age >= 18 ? age.ToString() : "Random (18-30)" }
                };

                string argumentDisplay = parsed.FormatArgumentDisplay("gm.hero.generate_lords", resolvedValues);

                List<Hero> createdHeroes;

                if (targetClan == null)
                {
                    Clan[] clans = Clan.NonBanditFactions.ToArray();
                    createdHeroes = new();

                    List<Clan> clansToUse = new();
                    for (int i = 0; i < count; i++)
                    {
                        clansToUse.Add(clans[RandomNumberGen.Instance.NextRandomInt(clans.Length)]);
                    }

                    IEnumerable<IGrouping<Clan, Clan>> groupedClans = clansToUse.GroupBy(c => c);
                    foreach (IGrouping<Clan, Clan> clanGroup in groupedClans)
                    {
                        List<Hero> clanLords = HeroGenerator.CreateLords(clanGroup.Count(), cultureFlags, genderFlags, clanGroup.Key, withParties: true, settlement, randomFactor, targetLevel, age);
                        createdHeroes.AddRange(clanLords);
                    }
                }
                else
                {
                    createdHeroes = HeroGenerator.CreateLords(count, cultureFlags, genderFlags, targetClan, withParties: true, settlement, randomFactor, targetLevel, age);
                }

                if (createdHeroes == null || createdHeroes.Count == 0)
                    return CommandResult.Error(argumentDisplay + MessageFormatter.FormatErrorMessage("Failed to create lords - no templates found matching criteria")).Message;

                string fullMessage = argumentDisplay + MessageFormatter.FormatSuccessMessage($"Created {createdHeroes.Count} lord(s):\n{HeroQueries.GetFormattedDetails(createdHeroes)}");
                return CommandResult.Success(fullMessage).Message;
            });
        }
    }
}