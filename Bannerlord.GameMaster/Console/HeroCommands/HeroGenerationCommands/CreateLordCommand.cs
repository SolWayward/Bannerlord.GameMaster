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
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.HeroCommands.HeroGenerationCommands
{
    public static class CreateLordCommand
    {
        /// <summary>
        /// Create a new lord with a chosen name from random templates
        /// Usage: gm.hero.create_lord &lt;name&gt; [cultures] [gender] [clan] [withParty] [settlement] [randomFactor]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("create_lord", "gm.hero")]
        public static string CreateLord(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return CommandResult.Error(error).Message;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.hero.create_lord", "<name> [cultures] [gender] [clan] [withParty] [settlement] [randomFactor] [level] [age]",
                    "Creates a single lord from random templates with good gear and decent stats. Allows custom naming.\n" +
                    "Creates a party for the lord by default if clan is not at max allowed parties. Use create_party to exceed party limit\n" +
                    "- name: required, the name for the hero. Use SINGLE QUOTES for multi-word names\n" +
                    "- cultures/culture: optional, defines the pool of cultures allowed to be chosen from. Defaults to main_cultures\n" +
                    "- gender: optional, use keywords both, female, or male. also allowed b, f, and m. Defaults to both\n" +
                    "- clan: optional, clanID or clanName. If not specified, hero goes to a random clan\n" +
                    "- withParty: optional, true/false to create party for lord. Defaults to true (Will only create party if clan is below party limit)\n" +
                    "- settlement: optional, settlement for lord without party to reside in (only used if withParty is false)\n" +
                    "- randomFactor/random: optional, float value between 0 and 1. defaults to 0.5\n" +
                    "- level: optional, target level (1-62). If not specified, a random level between 10-25 is assigned\n" +
                    "- age: optional, hero age (minimum 18). If not specified or less than 18, a random age between 18-30 is assigned\n" +
                    "Supports named arguments: name:'Sir Percival' cultures:vlandia gender:male clan:player_faction withParty:true randomFactor:0.8 level:20 age:25",
                    "gm.hero.create_lord 'Sir Percival'\n" +
                    "gm.hero.create_lord Ragnar vlandia male player_faction\n" +
                    "gm.hero.create_lord name:'Lady Elara' cultures:empire gender:female clan:clan_x withParty:false settlement:pen level:15 age:22\n" +
                    "gm.hero.create_lord Khalid aserai male clan_y true null 0.8 20 25");

                ParsedArguments parsed = ArgumentParser.ParseArguments(args);

                parsed.SetValidArguments(
                    new ArgumentDefinition("name", true),
                    new ArgumentDefinition("cultures", false, null, "culture"),
                    new ArgumentDefinition("gender", false),
                    new ArgumentDefinition("clan", false),
                    new ArgumentDefinition("withParty", false),
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
                string name = parsed.GetArgument("name", 0);
                if (string.IsNullOrWhiteSpace(name))
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage("Name cannot be empty.")).Message;

                CultureFlags cultureFlags = CultureFlags.AllMainCultures;
                GenderFlags genderFlags = GenderFlags.Either;
                Clan targetClan = null;
                bool withParty = true;
                Settlement settlement = null;
                float randomFactor = 0.5f;
                int targetLevel = -1;
                int age = -1;
                string clanArg = null;
                string settlementArg = null;

                // Parse cultures - try named first, then positional
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
                        return CommandResult.Error(MessageFormatter.FormatErrorMessage($"Invalid culture(s): '{culturesArg}'")).Message;
                }

                // Parse gender - try named first, then scan positional
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
                        return CommandResult.Error(MessageFormatter.FormatErrorMessage($"Invalid gender: '{genderArg}'")).Message;
                }

                // Parse clan - try named first
                clanArg = parsed.GetNamed("clan");
                if (clanArg == null)
                {
                    for (int i = 1; i < parsed.PositionalCount; i++)
                    {
                        string arg = parsed.GetPositional(i);
                        if (!bool.TryParse(arg, out _) && !float.TryParse(arg, out _) &&
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

                // Parse withParty - try named first
                string withPartyArg = parsed.GetNamed("withParty");
                if (withPartyArg != null)
                {
                    if (!bool.TryParse(withPartyArg, out withParty))
                        return CommandResult.Error(MessageFormatter.FormatErrorMessage($"Invalid withParty value: '{withPartyArg}'. Use true or false.")).Message;
                }
                else
                {
                    for (int i = 1; i < parsed.PositionalCount; i++)
                    {
                        if (bool.TryParse(parsed.GetPositional(i), out bool parsedBool))
                        {
                            withParty = parsedBool;
                            break;
                        }
                    }
                }

                // Parse settlement - try named first, then positional
                settlementArg = parsed.GetNamed("settlement");
                if (settlementArg == null)
                {
                    for (int i = 1; i < parsed.PositionalCount; i++)
                    {
                        string arg = parsed.GetPositional(i);
                        if (!bool.TryParse(arg, out _) && !float.TryParse(arg, out _) &&
                            FlagParser.ParseGenderArgument(arg) == GenderFlags.None &&
                            FlagParser.ParseCultureArgument(arg) == CultureFlags.None &&
                            arg != clanArg)
                        {
                            EntityFinderResult<Settlement> testResult = SettlementFinder.FindSingleSettlement(arg);
                            if (testResult.IsSuccess)
                            {
                                settlementArg = arg;
                                settlement = testResult.Entity;
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

                // Parse randomFactor
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

                // Parse level - try named first, then scan positional for integers > 1
                string levelArg = parsed.GetNamed("level");
                if (levelArg == null)
                {
                    for (int i = 1; i < parsed.PositionalCount; i++)
                    {
                        string arg = parsed.GetPositional(i);
                        if (int.TryParse(arg, out int testInt) && testInt > 1 &&
                            FlagParser.ParseGenderArgument(arg) == GenderFlags.None &&
                            FlagParser.ParseCultureArgument(arg) == CultureFlags.None &&
                            !bool.TryParse(arg, out _) &&
                            arg != randomArg && arg != clanArg && arg != settlementArg)
                        {
                            levelArg = arg;
                            break;
                        }
                    }
                }

                if (levelArg != null)
                {
                    if (!CommandValidator.ValidateIntegerRange(levelArg, 1, 62, out targetLevel, out string levelError))
                        return CommandResult.Error(MessageFormatter.FormatErrorMessage(levelError)).Message;
                }

                // Parse age - try named first, then scan positional for integers > 1 not already used as level
                string ageArg = parsed.GetNamed("age");
                if (ageArg == null)
                {
                    for (int i = 1; i < parsed.PositionalCount; i++)
                    {
                        string arg = parsed.GetPositional(i);
                        if (int.TryParse(arg, out int testInt) && testInt > 1 &&
                            FlagParser.ParseGenderArgument(arg) == GenderFlags.None &&
                            FlagParser.ParseCultureArgument(arg) == CultureFlags.None &&
                            !bool.TryParse(arg, out _) &&
                            arg != randomArg && arg != clanArg && arg != settlementArg &&
                            arg != levelArg)
                        {
                            ageArg = arg;
                            break;
                        }
                    }
                }

                if (ageArg != null)
                {
                    if (!CommandValidator.ValidateIntegerRange(ageArg, 18, 999, out age, out string ageError))
                        return CommandResult.Error(MessageFormatter.FormatErrorMessage(ageError)).Message;
                }

                if (!CommandValidator.ValidateHeroCreationLimit(1, out string limitError))
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage(limitError)).Message;

                // Default clan if none specified
                if (targetClan == null)
                {
                    Clan[] clans = Clan.NonBanditFactions.ToArray();
                    targetClan = clans[RandomNumberGen.Instance.NextRandomInt(clans.Length)];
                }

                // MARK: Execute Logic
                Dictionary<string, string> resolvedValues = new()
                {
                    { "name", name },
                    { "cultures", culturesArg ?? "Main Cultures" },
                    { "gender", genderFlags == GenderFlags.Either ? "Both" : (genderFlags == GenderFlags.Male ? "Male" : "Female") },
                    { "clan", targetClan.Name.ToString() },
                    { "withParty", withParty.ToString() },
                    { "settlement", settlement != null ? settlement.Name.ToString() : "None" },
                    { "randomFactor", randomFactor.ToString("0.0") },
                    { "level", targetLevel > 0 ? targetLevel.ToString() : "Random (10-25)" },
                    { "age", age >= 18 ? age.ToString() : "Random (18-30)" }
                };

                string argumentDisplay = parsed.FormatArgumentDisplay("gm.hero.create_lord", resolvedValues);

                Hero createdHero = HeroGenerator.CreateLord(name, cultureFlags, genderFlags, targetClan, withParty, settlement, randomFactor, targetLevel, age);

                if (createdHero == null)
                    return CommandResult.Error(argumentDisplay + MessageFormatter.FormatErrorMessage("Failed to create lord - no templates found matching criteria")).Message;

                // If no party and settlement specified, place lord there
                if (!withParty && settlement != null)
                {
                    EnterSettlementAction.ApplyForCharacterOnly(createdHero, settlement);
                    createdHero.UpdateLastKnownClosestSettlement(settlement);
                }

                string partyInfo = withParty ? " with party" : (settlement != null ? $" at {settlement.Name}" : " (no party)");
                string fullMessage = argumentDisplay + MessageFormatter.FormatSuccessMessage($"Created lord '{createdHero.Name}' (ID: {createdHero.StringId}){partyInfo}\n{HeroQueries.GetFormattedDetails(new List<Hero> { createdHero })}");
                return CommandResult.Success(fullMessage).Message;
            });
        }
    }
}
