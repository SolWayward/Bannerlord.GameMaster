using Bannerlord.GameMaster.Characters;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Cultures;
using Bannerlord.GameMaster.Heroes;
using Bannerlord.GameMaster.Party;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.HeroCommands.HeroGenerationCommands
{
    public static class CreateCompanionsCommand
    {
        /// <summary>
        /// Create companions ready to be added to a party
        /// Usage: gm.hero.create_companions &lt;count&gt; &lt;heroLeader&gt; [cultures] [gender] [randomFactor]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("create_companions", "gm.hero")]
        public static string CreateCompanions(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return CommandResult.Error(error);

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.hero.create_companions", "<count> <heroLeader> [cultures] [gender] [randomFactor] [level] [age]",
                    "Creates companions and adds them directly to the specified hero's party.\n" +
                    "Companions are added as party members. Will not exceed companion limit, use create_lord instead for that.\n" +
                    "- count: required, number of companions to create (1-20)\n" +
                    "- heroLeader/hero: required, hero ID or name of party leader. Use 'player' for your party\n" +
                    "- cultures/culture: optional, culture pool for template selection. Defaults to main_cultures\n" +
                    "- gender: optional, use keywords both, female, or male. Defaults to both\n" +
                    "- randomFactor/random: optional, float value between 0 and 1. defaults to 0.5\n" +
                    "- level: optional, target level (1-62). If not specified, a random level between 1-14 is assigned per companion\n" +
                    "- age: optional, hero age (minimum 18). If not specified or less than 18, a random age between 18-30 is assigned per companion\n" +
                    "Supports named arguments: count:5 hero:player cultures:vlandia,battania gender:female level:10 age:22\n",
                    "gm.hero.create_companions 5 player\n" +
                    "gm.hero.create_companions 3 player vlandia both\n" +
                    "gm.hero.create_companions count:2 hero:'Lord Name' cultures:battania,sturgia gender:female level:8 age:25\n" +
                    "gm.hero.create_companions 2 'Lord Name' battania,sturgia female 0.8 12 24");

                ParsedArguments parsed = ArgumentParser.ParseArguments(args);

                parsed.SetValidArguments(
                    new ArgumentDefinition("count", true),
                    new ArgumentDefinition("heroLeader", true, null, "hero"),
                    new ArgumentDefinition("cultures", false, null, "culture"),
                    new ArgumentDefinition("gender", false),
                    new ArgumentDefinition("randomFactor", false, null, "random"),
                    new ArgumentDefinition("level", false),
                    new ArgumentDefinition("age", false)
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError));

                if (parsed.TotalCount < 2)
                    return CommandResult.Error(usageMessage);

                // MARK: Parse Arguments
                string countArg = parsed.GetArgument("count", 0);
                if (countArg == null)
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'count'."));

                if (!CommandValidator.ValidateIntegerRange(countArg, 1, 20, out int count, out string countError))
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage(countError));

                string heroArg = parsed.GetArgument("heroLeader", 1) ?? parsed.GetNamed("hero");
                if (heroArg == null)
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'heroLeader'."));

                EntityFinderResult<Hero> heroResult = HeroFinder.FindSingleHero(heroArg);
                if (!heroResult.IsSuccess)
                    return CommandResult.Error(heroResult.Message);
                Hero hero = heroResult.Entity;

                if (hero.PartyBelongedTo == null)
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage($"Hero {hero.Name} is not in a party."));

                if (hero.PartyBelongedTo.LeaderHero != hero)
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage($"Hero {hero.Name} is not the leader of their party."));

                CultureFlags cultureFlags = CultureFlags.AllMainCultures;
                GenderFlags genderFlags = GenderFlags.Either;
                float randomFactor = 0.5f;
                int targetLevel = -1;
                int age = -1;

                // Parse cultures - try named first, then positional
                string culturesArg = parsed.GetNamed("cultures") ?? parsed.GetNamed("culture");
                if (culturesArg == null && parsed.PositionalCount > 2)
                {
                    GenderFlags testGender = FlagParser.ParseGenderArgument(parsed.GetPositional(2));
                    if (testGender == GenderFlags.None)
                    {
                        culturesArg = parsed.GetPositional(2);
                    }
                }

                if (culturesArg != null)
                {
                    cultureFlags = FlagParser.ParseCultureArgument(culturesArg);
                    if (cultureFlags == CultureFlags.None)
                        return CommandResult.Error(MessageFormatter.FormatErrorMessage($"Invalid culture(s): '{culturesArg}'"));
                }

                // Parse gender - try named first, then scan positional
                string genderArg = parsed.GetNamed("gender");
                if (genderArg == null)
                {
                    for (int i = 2; i < parsed.PositionalCount; i++)
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
                        return CommandResult.Error(MessageFormatter.FormatErrorMessage($"Invalid gender: '{genderArg}'"));
                }

                // Parse randomFactor
                string randomArg = parsed.GetNamed("randomFactor") ?? parsed.GetNamed("random");
                if (randomArg == null)
                {
                    for (int i = 2; i < parsed.PositionalCount; i++)
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
                        return CommandResult.Error(MessageFormatter.FormatErrorMessage(randomError));
                }

                // Parse level - try named first, then scan positional for integers
                string levelArg = parsed.GetNamed("level");
                if (levelArg == null)
                {
                    for (int i = 2; i < parsed.PositionalCount; i++)
                    {
                        string arg = parsed.GetPositional(i);
                        if (int.TryParse(arg, out int testInt) && testInt > 1 &&
                            FlagParser.ParseGenderArgument(arg) == GenderFlags.None &&
                            FlagParser.ParseCultureArgument(arg) == CultureFlags.None &&
                            arg != randomArg)
                        {
                            levelArg = arg;
                            break;
                        }
                    }
                }

                if (levelArg != null)
                {
                    if (!CommandValidator.ValidateIntegerRange(levelArg, 1, 62, out targetLevel, out string levelError))
                        return CommandResult.Error(MessageFormatter.FormatErrorMessage(levelError));
                }

                // Parse age - try named first, then scan positional for second integer not already used as level
                string ageArg = parsed.GetNamed("age");
                if (ageArg == null)
                {
                    for (int i = 2; i < parsed.PositionalCount; i++)
                    {
                        string arg = parsed.GetPositional(i);
                        if (int.TryParse(arg, out int testInt) && testInt > 1 &&
                            FlagParser.ParseGenderArgument(arg) == GenderFlags.None &&
                            FlagParser.ParseCultureArgument(arg) == CultureFlags.None &&
                            arg != randomArg && arg != levelArg)
                        {
                            ageArg = arg;
                            break;
                        }
                    }
                }

                if (ageArg != null)
                {
                    if (!CommandValidator.ValidateIntegerRange(ageArg, 18, 999, out age, out string ageError))
                        return CommandResult.Error(MessageFormatter.FormatErrorMessage(ageError));
                }

                if (!CommandValidator.ValidateHeroCreationLimit(count, out string limitError))
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage(limitError));

                // MARK: Execute Logic
                Dictionary<string, string> resolvedValues = new()
                {
                    { "count", count.ToString() },
                    { "heroLeader", hero.Name.ToString() },
                    { "cultures", culturesArg ?? "Main Cultures" },
                    { "gender", genderFlags == GenderFlags.Either ? "Both" : (genderFlags == GenderFlags.Male ? "Male" : "Female") },
                    { "randomFactor", randomFactor.ToString("0.0") },
                    { "level", targetLevel > 0 ? targetLevel.ToString() : "Random (1-14)" },
                    { "age", age >= 18 ? age.ToString() : "Random (18-30)" }
                };

                string argumentDisplay = parsed.FormatArgumentDisplay("gm.hero.create_companions", resolvedValues);

                List<Hero> companions = HeroGenerator.CreateCompanions(count, cultureFlags, genderFlags, randomFactor, targetLevel, age);

                if (companions == null || companions.Count == 0)
                    return CommandResult.Error(argumentDisplay + MessageFormatter.FormatErrorMessage("Failed to create companions - no templates found matching criteria"));

                hero.PartyBelongedTo.AddCompanionsToParty(companions);

                string fullMessage = argumentDisplay + MessageFormatter.FormatSuccessMessage(
                    $"Created and added {companions.Count} companion(s) to {hero.Name}'s party:\n" +
                    HeroQueries.GetFormattedDetails(companions));
                return CommandResult.Success(fullMessage);
            }).Message;
        }
    }
}
