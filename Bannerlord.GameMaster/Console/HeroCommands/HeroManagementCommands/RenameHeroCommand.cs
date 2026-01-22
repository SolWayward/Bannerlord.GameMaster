using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Heroes;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.HeroCommands.HeroManagementCommands;

/// <summary>
/// Rename a hero
/// Usage: gm.hero.rename [heroQuery] [name]
/// </summary>
public static class RenameHeroCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("rename", "gm.hero")]
    public static string RenameHero(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Log().Message;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.hero.rename", "<heroQuery> <name>",
                "Renames the specified hero. Use SINGLE QUOTES for multi-word names.\n" +
                "- heroQuery/hero: hero ID or name query to find a single hero\n" +
                "- name: the new name for the hero\n" +
                "Supports named arguments: hero:lord_1_1 name:'Sir Galahad'",
                "gm.hero.rename lord_1_1 'Sir Galahad'\n" +
                "gm.hero.rename hero:'old hero name' name:NewName");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("heroQuery", true, null, "hero"),
                new ArgumentDefinition("name", true)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError)).Log().Message;

            if (parsed.TotalCount < 2)
                return CommandResult.Error(usageMessage).Log().Message;

            // MARK: Parse Arguments
            string heroQuery = parsed.GetArgument("heroQuery", 0) ?? parsed.GetNamed("hero");
            if (heroQuery == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'heroQuery'.")).Log().Message;

            EntityFinderResult<Hero> heroResult = HeroFinder.FindSingleHero(heroQuery);
            if (!heroResult.IsSuccess) return CommandResult.Error(heroResult.Message).Log().Message;
            Hero hero = heroResult.Entity;

            string newName = parsed.GetArgument("name", 1);
            if (string.IsNullOrWhiteSpace(newName))
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing or empty required argument 'name'.")).Log().Message;

            // MARK: Execute Logic
            Dictionary<string, string> resolvedValues = new()
            {
                { "heroQuery", hero.Name.ToString() },
                { "name", newName }
            };

            string previousName = hero.Name.ToString();
            hero.SetStringName(newName);

            string argumentDisplay = parsed.FormatArgumentDisplay("gm.hero.rename", resolvedValues);
            string fullMessage = argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"Hero renamed from '{previousName}' to '{hero.Name}' (ID: {hero.StringId})");
            return CommandResult.Success(fullMessage).Log().Message;
        });
    }
}
