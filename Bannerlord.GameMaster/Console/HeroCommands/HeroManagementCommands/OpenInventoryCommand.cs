using Bannerlord.GameMaster.Common;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Items.Inventory;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.HeroCommands.HeroManagementCommands;

/// <summary>
/// Opens the native inventory UI for a hero, with an optional second hero's party on the other side.
/// Usage: gm.hero.open_inventory &lt;hero&gt; [otherHero]
/// </summary>
public static class OpenInventoryCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("open_inventory", "gm.hero")]
    public static string OpenInventory(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error);

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.hero.open_inventory", "<hero> [otherHero]",
                "Opens the native inventory screen for the specified hero.\n" +
                "- hero: required, hero ID or name. The hero shown in the middle of the inventory screen.\n" +
                "  Their party's inventory appears on the left side.\n" +
                "- otherHero: optional, hero ID or name. Their party's inventory appears on the right side.\n" +
                "  Defaults to the player hero (MainHero) if not specified.\n" +
                "Both heroes must be in a party.\n" +
                "Supports named arguments: hero:'Hero Name' otherHero:'Other Hero'",
                "gm.hero.open_inventory lord_1_1\n" +
                "gm.hero.open_inventory hero:'Hero Name'\n" +
                "gm.hero.open_inventory lord_1_1 lord_2_3\n" +
                "gm.hero.open_inventory hero:'Hero Name' otherHero:'Other Hero'");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("hero", true),
                new ArgumentDefinition("otherHero", false, "Player Hero", "other")
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError));

            if (parsed.TotalCount < 1)
                return CommandResult.Success(usageMessage);

            // MARK: Parse Arguments
            string heroArg = parsed.GetArgument("hero", 0);
            if (string.IsNullOrWhiteSpace(heroArg))
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'hero'."));

            EntityFinderResult<Hero> heroResult = HeroFinder.FindSingleHero(heroArg);
            if (!heroResult.IsSuccess)
                return CommandResult.Error(heroResult.Message);
            Hero hero = heroResult.Entity;

            if (hero.PartyBelongedTo == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage($"{hero.Name} is not in a party."));

            // Parse optional otherHero, default to MainHero
            Hero otherHero = Hero.MainHero;
            string otherHeroArg = parsed.GetArgument("otherHero", 1) ?? parsed.GetNamed("other");
            if (otherHeroArg != null)
            {
                EntityFinderResult<Hero> otherHeroResult = HeroFinder.FindSingleHero(otherHeroArg);
                if (!otherHeroResult.IsSuccess)
                    return CommandResult.Error(otherHeroResult.Message);
                otherHero = otherHeroResult.Entity;
            }

            if (otherHero.PartyBelongedTo == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage($"{otherHero.Name} is not in a party."));

            // MARK: Execute Logic
            Dictionary<string, string> resolvedValues = new()
            {
                { "hero", hero.Name.ToString() },
                { "otherHero", otherHero.Name.ToString() }
            };

            // rightHero = otherHero (right side), leftHero = hero (left side), middleHero = hero (center)
            InventoryManager inventoryManager = new(rightHero: otherHero, leftHero: hero, middleHero: hero);
            BLGMResult result = inventoryManager.OpenInventory();

            if (result == null || !result.IsSuccess)
            {
                string errorMsg = result?.Message ?? "Failed to open inventory";
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(errorMsg));
            }

            string argumentDisplay = parsed.FormatArgumentDisplay("gm.hero.open_inventory", resolvedValues);
            string fullMessage = argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"Opened inventory for {hero.Name} (ID: {hero.StringId})\n" +
                $"Left side: {hero.Name}'s party | Middle: {hero.Name} | Right side: {otherHero.Name}'s party");
            return CommandResult.Success(fullMessage);
        }).Message;
    }
}
