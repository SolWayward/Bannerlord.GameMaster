using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Heroes;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.HeroCommands
{
    [CommandLineFunctionality.CommandLineArgumentFunction("hero", "gm")]
    public static class HeroGenerationCommands
    {
        /// <summary>
        /// Generate new lords with random templates and good equipment
        /// Usage: gm.hero.generate_lords [count] [clan]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("generate_lords", "gm.hero")]
        public static string GenerateLords(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.hero.generate_lords", "[count=1] [clan=random]",
                    "Creates lords from random templates with good gear and decent stats. Age 30-40. If clan not specified, each lord goes to a different random clan.",
                    "gm.hero.generate_lords 3\ngm.hero.generate_lords 5 empire_south");

                // Parse count (optional, default 1)
                int count = 1;
                if (args.Count > 0)
                {
                    if (!CommandValidator.ValidateIntegerRange(args[0], 1, 20, out count, out string countError))
                        return CommandBase.FormatErrorMessage(countError);
                }

                // Parse clan (optional)
                Clan targetClan = null;
                if (args.Count > 1)
                {
                    var (clan, clanError) = CommandBase.FindSingleClan(args[1]);
                    if (clanError != null) return clanError;
                    targetClan = clan;
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
                        MinWeaponTier = TaleWorlds.Core.ItemObject.ItemTiers.Tier3
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
        /// Usage: gm.hero.create_lord [gender] [name] [clan]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("create_lord", "gm.hero")]
        public static string CreateLord(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.hero.create_lord", "<gender> <name> <clan>",
                    "Creates a fresh lord age 20-24 with minimal stats and only clothes. Gender must be 'male' or 'female'.",
                    "gm.hero.create_lord male NewLord empire_south");

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
                        AddCivilianClothes = true
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