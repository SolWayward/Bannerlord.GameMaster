using System.Collections.Generic;
using Bannerlord.GameMaster.Characters;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Cultures;
using Bannerlord.GameMaster.Heroes;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.SettlementCommands.FortificationManagementCommands;

/// <summary>
/// Command to spawn a wanderer hero in a settlement.
/// Usage: gm.settlement.spawn_wanderer [settlement] [name] [cultures] [gender] [randomFactor]
/// </summary>
public static class SpawnWandererCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("spawn_wanderer", "gm.settlement")]
    public static string SpawnWanderer(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Message
;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.settlement.spawn_wanderer", "<settlement> [name] [cultures] [gender] [randomFactor]",
                "Spawns a wanderer hero with proper name, portrait, and stats in the specified settlement.\n" +
                "- settlement: required, settlement ID or name where the wanderer will spawn\n" +
                "- name: optional, custom name for the wanderer. Use SINGLE QUOTES for multi-word names. If not provided, generates random name\n" +
                "- cultures: optional, defines the pool of cultures allowed. Defaults to main_cultures\n" +
                "- gender: optional, use keywords both, female, or male. also allowed b, f, and m. Defaults to both\n" +
                "- randomFactor: optional, float value between 0 and 1. defaults to 0.5\n" +
                "Supports named arguments: settlement:pen name:'Wandering Bard' cultures:vlandia,battania gender:female\n",
                "gm.settlement.spawn_wanderer pen\n" +
                "gm.settlement.spawn_wanderer pen 'Wandering Bard'\n" +
                "gm.settlement.spawn_wanderer pen null vlandia female\n" +
                "gm.settlement.spawn_wanderer settlement:zeonica name:'Skilled Archer' cultures:empire,aserai gender:male\n" +
                "gm.settlement.spawn_wanderer zeonica 'Skilled Archer' empire,aserai male 0.8");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);
            parsed.SetValidArguments(
                new ArgumentDefinition("settlement", true),
                new ArgumentDefinition("name", false),
                new ArgumentDefinition("cultures", false),
                new ArgumentDefinition("gender", false),
                new ArgumentDefinition("randomFactor", false)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError)).Message
;

            if (parsed.TotalCount < 1)
                return CommandResult.Error(usageMessage).Message
;

            // MARK: Parse Arguments
            string settlementQuery = parsed.GetArgument("settlement", 0);

            EntityFinderResult<Settlement> settlementResult = SettlementFinder.FindSingleSettlement(settlementQuery);
            if (!settlementResult.IsSuccess) return CommandResult.Error(settlementResult.Message).Message
;
            Settlement settlement = settlementResult.Entity;

            // Parse optional name
            string name = null;
            int currentArgIndex = 1;
            if (args.Count > currentArgIndex)
            {
                // Check if this is NOT a culture or gender keyword
                GenderFlags testGender = FlagParser.ParseGenderArgument(args[currentArgIndex]);
                CultureFlags testCulture = FlagParser.ParseCultureArgument(args[currentArgIndex]);

                if (testGender == GenderFlags.None && testCulture == CultureFlags.None && args[currentArgIndex].ToLower() != "null")
                {
                    name = args[currentArgIndex];
                    currentArgIndex++;
                }
                else if (args[currentArgIndex].ToLower() == "null")
                {
                    currentArgIndex++;
                }
            }

            // Parse cultures (optional, defaults to AllMainCultures)
            CultureFlags cultureFlags = CultureFlags.AllMainCultures;
            if (args.Count > currentArgIndex)
            {
                GenderFlags testGender = FlagParser.ParseGenderArgument(args[currentArgIndex]);
                if (testGender != GenderFlags.None)
                {
                    // Skip cultures, this is gender
                }
                else
                {
                    cultureFlags = FlagParser.ParseCultureArgument(args[currentArgIndex]);
                    if (cultureFlags == CultureFlags.None)
                        return CommandResult.Error(MessageFormatter.FormatErrorMessage($"Invalid culture(s): '{args[currentArgIndex]})'")).Message
;
                    currentArgIndex++;
                }
            }

            // Parse gender (optional, defaults to Either)
            GenderFlags genderFlags = GenderFlags.Either;
            if (args.Count > currentArgIndex)
            {
                genderFlags = FlagParser.ParseGenderArgument(args[currentArgIndex]);
                if (genderFlags == GenderFlags.None)
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage($"Invalid gender: '{args[currentArgIndex]}'. Use 'both/b', 'female/f', or 'male/m'")).Message
;
                currentArgIndex++;
            }

            // Parse randomFactor
            float randomFactor = 0.5f;
            if (args.Count > currentArgIndex)
            {
                if (!CommandValidator.ValidateFloatRange(args[currentArgIndex], 0f, 1f, out randomFactor, out string randomError))
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage(randomError)).Message
;
            }

            // MARK: Execute Logic
            Hero wanderer;

            // Use new architecture - CreateWanderer method
            if (string.IsNullOrWhiteSpace(name))
            {
                // Generate random name from culture
                wanderer = HeroGenerator.CreateWanderers(1, cultureFlags, genderFlags, settlement, randomFactor)[0];
            }
            else
            {
                wanderer = HeroGenerator.CreateWanderer(name, cultureFlags, genderFlags, settlement, randomFactor);
            }

            if (wanderer == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Failed to spawn wanderer - no templates found matching criteria")).Message
;

            return CommandResult.Success(MessageFormatter.FormatSuccessMessage(
                $"Spawned wanderer '{wanderer.Name}' (ID: {wanderer.StringId}) in '{settlement.Name}'.\n" +
                $"Culture: {wanderer.Culture?.Name} | Age: {(int)wanderer.Age} | Gender: {(wanderer.IsFemale ? "Female" : "Male")}\n" +
                $"Wanderer can be recruited from the tavern in {settlement.Name}.")).Message
;
        });
    }
}
