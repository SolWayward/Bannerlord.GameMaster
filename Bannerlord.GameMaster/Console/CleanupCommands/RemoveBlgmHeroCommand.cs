using Bannerlord.GameMaster.Common;
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
                return MessageFormatter.FormatErrorMessage(validationError);

            if (parsed.TotalCount < 1)
                return usageMessage;

            // MARK: Parse Arguments
            string heroIdentifier = parsed.GetArgument("hero", 0);
            if (string.IsNullOrWhiteSpace(heroIdentifier))
                return MessageFormatter.FormatErrorMessage("Hero identifier cannot be empty.");

            EntityFinderResult<Hero> heroResult = HeroFinder.FindSingleHero(heroIdentifier);
            if (!heroResult.IsSuccess)
                return heroResult.Message;
            Hero hero = heroResult.Entity;

            // MARK: Execute Logic
            BLGMResult result = HeroRemover.RemoveSingleHero(hero);

            Dictionary<string, string> resolvedValues = new()
            {
                { "hero", hero.Name.ToString() }
            };

            string argumentDisplay = parsed.FormatArgumentDisplay("remove_blgm_hero", resolvedValues);

            if (result.wasSuccessful)
            {
                return argumentDisplay + MessageFormatter.FormatSuccessMessage(result.message);
            }
            else
            {
                return argumentDisplay + MessageFormatter.FormatErrorMessage(result.message);
            }
        });
    }
}
