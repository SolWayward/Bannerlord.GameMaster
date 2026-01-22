using Bannerlord.GameMaster.Common;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.RemovalHelpers;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.CleanupCommands;

public static class RemoveBlgmHeroCommand
{
    /// <summary>
    /// Removes a single BLGM-generated hero
    /// Usage: gm.cleanup.remove_blgm_hero <hero>
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("remove_blgm_hero", "gm.cleanup")]
    public static string RemoveBlgmHero(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return error;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.cleanup.remove_blgm_hero", "<hero>",
                "Removes a single BLGM-generated hero.\n" +
                "- hero: Hero identifier (name or ID)",
                "gm.cleanup.remove_blgm_hero blgm_hero_123");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("hero", true)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError)).Log().Message;

            if (parsed.TotalCount < 1)
                return CommandResult.Error(usageMessage).Log().Message;

            // MARK: Parse Arguments
            string heroIdentifier = parsed.GetArgument("hero", 0);
            if (string.IsNullOrWhiteSpace(heroIdentifier))
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Hero identifier cannot be empty.")).Log().Message;

            EntityFinderResult<Hero> heroResult = HeroFinder.FindSingleHero(heroIdentifier);
            if (!heroResult.IsSuccess)
                return CommandResult.Error(heroResult.Message).Log().Message;
            Hero hero = heroResult.Entity;

            // MARK: Execute Logic
            BLGMResult result = HeroRemover.RemoveSingleHero(hero);

            Dictionary<string, string> resolvedValues = new()
            {
                { "hero", hero.Name.ToString() }
            };

            string argumentDisplay = parsed.FormatArgumentDisplay("gm.cleanup.remove_blgm_hero", resolvedValues);

            if (result.IsSuccess)
            {
                string fullMessage = argumentDisplay + MessageFormatter.FormatSuccessMessage(result.Message);
                return CommandResult.Success(fullMessage).Log().Message;
            }
            else
            {
                string fullMessage = argumentDisplay + MessageFormatter.FormatErrorMessage(result.Message);
                return CommandResult.Error(fullMessage).Log().Message;
            }
        });
    }
}
