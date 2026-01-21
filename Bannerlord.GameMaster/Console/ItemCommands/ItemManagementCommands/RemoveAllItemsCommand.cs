using System.Collections.Generic;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.ItemCommands.ItemManagementCommands
{
    /// <summary>
    /// Remove all items from a hero's party inventory
    /// Usage: gm.item.remove_all [hero_query]
    /// </summary>
    public static class RemoveAllItemsCommand
    {
        [CommandLineFunctionality.CommandLineArgumentFunction("remove_all", "gm.item")]
        public static string Execute(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return error;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.item.remove_all", "<hero_query>",
                    "Removes all items from a hero's party inventory.\n" +
                    "- hero: required, hero ID or name whose party inventory will be cleared",
                    "gm.item.remove_all player");

                ParsedArguments parsed = ArgumentParser.ParseArguments(args);

                parsed.SetValidArguments(
                    new ArgumentDefinition("hero", true)
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError)).Log().Message;

                if (parsed.TotalCount < 1)
                    return usageMessage;

                // MARK: Parse Arguments
                string heroQuery = parsed.GetArgument("hero", 0);

                // Find hero
                EntityFinderResult<Hero> heroResult = HeroFinder.FindSingleHero(heroQuery);
                if (!heroResult.IsSuccess) return heroResult.Message;
                Hero hero = heroResult.Entity;

                // MARK: Execute Logic
                if (hero.PartyBelongedTo == null)
                    return MessageFormatter.FormatErrorMessage($"{hero.Name} does not belong to a party.");

                int itemCount = hero.PartyBelongedTo.ItemRoster.Count;
                hero.PartyBelongedTo.ItemRoster.Clear();

                Dictionary<string, string> resolvedValues = new()
                {
                    ["hero"] = hero.Name.ToString()
                };

                string display = parsed.FormatArgumentDisplay("gm.item.remove_all", resolvedValues);
                return display + MessageFormatter.FormatSuccessMessage(
                    $"Removed all items ({itemCount} types) from {hero.Name}'s party inventory.");
            });
        }
    }
}
