using Bannerlord.GameMaster.Common;
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
/// Opens the native face generator UI to edit a hero's appearance.
/// Usage: gm.hero.edit_appearance [heroQuery]
/// </summary>
public static class EditAppearanceCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("edit_appearance", "gm.hero")]
    public static string EditAppearance(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error);

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.hero.edit_appearance", "<heroQuery>",
                "Opens the native face generator UI to edit the specified hero's appearance.\n" +
                "- heroQuery/hero: hero ID or name query to find a single hero\n" +
                "Supports named arguments: hero:lord_1_1",
                "gm.hero.edit_appearance lord_1_1\n" +
                "gm.hero.edit_appearance hero:'Hero Name'");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("heroQuery", true, null, "hero")
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError));

            if (parsed.TotalCount < 1)
                return CommandResult.Success(usageMessage);

            // MARK: Parse Arguments
            string heroQuery = parsed.GetArgument("heroQuery", 0) ?? parsed.GetNamed("hero");
            if (heroQuery == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'heroQuery'."));

            EntityFinderResult<Hero> heroResult = HeroFinder.FindSingleHero(heroQuery);
            if (!heroResult.IsSuccess) return CommandResult.Error(heroResult.Message);
            Hero hero = heroResult.Entity;

            // MARK: Execute Logic
            Dictionary<string, string> resolvedValues = new()
            {
                { "heroQuery", hero.Name.ToString() }
            };

            HeroEditor editor = new(hero);
            BLGMResult result = editor.HeroAppearanceEditorUI.OpenFullEditor();

            if (!result.IsSuccess)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(result.Message));

            string argumentDisplay = parsed.FormatArgumentDisplay("gm.hero.edit_appearance", resolvedValues);
            string fullMessage = argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"Opened appearance editor for '{hero.Name}' (ID: {hero.StringId})");
            return CommandResult.Success(fullMessage);
        }).Message;
    }
}
