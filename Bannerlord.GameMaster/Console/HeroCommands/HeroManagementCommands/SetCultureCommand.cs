using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace Bannerlord.GameMaster.Console.HeroCommands.HeroManagementCommands;

/// <summary>
/// Change a hero's culture
/// Usage: gm.hero.set_culture [hero] [culture]
/// </summary>
public static class SetCultureCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("set_culture", "gm.hero")]
    public static string SetCulture(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Message;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.hero.set_culture", "<hero> <culture>",
                "Changes the hero's culture. Note: This does not change the hero's equipment or appearance, only the culture property.\n" +
                "Supports named arguments: hero:lord_1_1 culture:vlandia",
                "gm.hero.set_culture lord_1_1 vlandia\n" +
                "gm.hero.set_culture companion_1 battania");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("hero", true),
                new ArgumentDefinition("culture", true)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError)).Message;

            if (parsed.TotalCount < 2)
                return CommandResult.Error(usageMessage).Message;

            // MARK: Parse Arguments
            string heroArg = parsed.GetArgument("hero", 0);
            if (heroArg == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'hero'.")).Message;

            EntityFinderResult<Hero> heroResult = HeroFinder.FindSingleHero(heroArg);
            if (!heroResult.IsSuccess) return CommandResult.Error(heroResult.Message).Message;
            Hero hero = heroResult.Entity;

            string cultureArg = parsed.GetArgument("culture", 1);
            if (cultureArg == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'culture'.")).Message;

            CultureObject newCulture = MBObjectManager.Instance.GetObject<CultureObject>(cultureArg);
            if (newCulture == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage($"Culture '{cultureArg}' not found. Valid cultures: aserai, battania, empire, khuzait, nord, sturgia, vlandia")).Message;

            // MARK: Execute Logic
            Dictionary<string, string> resolvedValues = new()
            {
                { "hero", hero.Name.ToString() },
                { "culture", newCulture.Name.ToString() }
            };

            string previousCulture = hero.Culture?.Name?.ToString() ?? "None";
            hero.Culture = newCulture;

            string argumentDisplay = parsed.FormatArgumentDisplay("gm.hero.set_culture", resolvedValues);
            string fullMessage = argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"{hero.Name}'s culture changed from '{previousCulture}' to '{hero.Culture.Name}'.");
            return CommandResult.Success(fullMessage).Message;
        });
    }
}
