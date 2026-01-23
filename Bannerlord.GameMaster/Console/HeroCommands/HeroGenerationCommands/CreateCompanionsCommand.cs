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
                    return CommandResult.Error(error).Message
;

                string usageMessage = CommandValidator.CreateUsageMessage(
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

                ParsedArguments parsed = ArgumentParser.ParseArguments(args);

                parsed.SetValidArguments(
                    new ArgumentDefinition("count", true),
                    new ArgumentDefinition("heroLeader", true, null, "hero"),
                    new ArgumentDefinition("cultures", false, null, "culture"),
                    new ArgumentDefinition("gender", false),
                    new ArgumentDefinition("randomFactor", false, null, "random")
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError)).Message
;

                if (parsed.TotalCount < 2)
                    return CommandResult.Error(usageMessage).Message
;

                // MARK: Parse Arguments
                string countArg = parsed.GetArgument("count", 0);
                if (countArg == null)
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'count'.")).Message
;

                if (!CommandValidator.ValidateIntegerRange(countArg, 1, 20, out int count, out string countError))
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage(countError)).Message
;

                string heroArg = parsed.GetArgument("heroLeader", 1) ?? parsed.GetNamed("hero");
                if (heroArg == null)
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'heroLeader'.")).Message
;

                EntityFinderResult<Hero> heroResult = HeroFinder.FindSingleHero(heroArg);
                if (!heroResult.IsSuccess)
                    return CommandResult.Error(heroResult.Message).Message
;
                Hero hero = heroResult.Entity;

                if (hero.PartyBelongedTo == null)
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage($"Hero {hero.Name} is not in a party.")).Message
;

                if (hero.PartyBelongedTo.LeaderHero != hero)
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage($"Hero {hero.Name} is not the leader of their party.")).Message
;

                CultureFlags cultureFlags = CultureFlags.AllMainCultures;
                GenderFlags genderFlags = GenderFlags.Either;
                float randomFactor = 0.5f;

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
                        return CommandResult.Error(MessageFormatter.FormatErrorMessage($"Invalid culture(s): '{culturesArg}'")).Message
;
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
                        return CommandResult.Error(MessageFormatter.FormatErrorMessage($"Invalid gender: '{genderArg}'")).Message
;
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
                        return CommandResult.Error(MessageFormatter.FormatErrorMessage(randomError)).Message
;
                }

                if (!CommandValidator.ValidateHeroCreationLimit(count, out string limitError))
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage(limitError)).Message
;

                // MARK: Execute Logic
                Dictionary<string, string> resolvedValues = new()
                {
                    { "count", count.ToString() },
                    { "heroLeader", hero.Name.ToString() },
                    { "cultures", culturesArg ?? "Main Cultures" },
                    { "gender", genderFlags == GenderFlags.Either ? "Both" : (genderFlags == GenderFlags.Male ? "Male" : "Female") },
                    { "randomFactor", randomFactor.ToString("0.0") }
                };

                string argumentDisplay = parsed.FormatArgumentDisplay("gm.hero.create_companions", resolvedValues);

                List<Hero> companions = HeroGenerator.CreateCompanions(count, cultureFlags, genderFlags, randomFactor);

                if (companions == null || companions.Count == 0)
                    return CommandResult.Error(argumentDisplay + MessageFormatter.FormatErrorMessage("Failed to create companions - no templates found matching criteria")).Message
;

                hero.PartyBelongedTo.AddCompanionsToParty(companions);

                string fullMessage = argumentDisplay + MessageFormatter.FormatSuccessMessage(
                    $"Created and added {companions.Count} companion(s) to {hero.Name}'s party:\n" +
                    HeroQueries.GetFormattedDetails(companions));
                return CommandResult.Success(fullMessage).Message
;
            });
        }
    }
}
