using System.Collections.Generic;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.KingdomCommands.KingdomManagementCommands;

public static class SetRulerCommand
{
    /// <summary>
    /// Change kingdom ruler
    /// Usage: gm.kingdom.set_ruler [kingdom] [hero]
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("set_ruler", "gm.kingdom")]
    public static string SetKingdomRuler(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return error;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.kingdom.set_ruler", "<kingdom> <hero>",
                "Changes the kingdom ruler.\n" +
                "Supports named arguments: kingdom:empire hero:lord_1_1",
                "gm.kingdom.set_ruler empire lord_1_1");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("kingdom", true),
                new ArgumentDefinition("hero", true)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError)).Message
;

            if (parsed.TotalCount < 2)
                return usageMessage;

            // MARK: Parse Arguments
            string kingdomArg = parsed.GetArgument("kingdom", 0);
            if (kingdomArg == null)
                return MessageFormatter.FormatErrorMessage("Missing required argument 'kingdom'.");

            EntityFinderResult<Kingdom> kingdomResult = KingdomFinder.FindSingleKingdom(kingdomArg);
            if (!kingdomResult.IsSuccess)
                return kingdomResult.Message;
            Kingdom kingdom = kingdomResult.Entity;

            string heroArg = parsed.GetArgument("hero", 1);
            if (heroArg == null)
                return MessageFormatter.FormatErrorMessage("Missing required argument 'hero'.");

            EntityFinderResult<Hero> heroResult = HeroFinder.FindSingleHero(heroArg);
            if (!heroResult.IsSuccess)
                return heroResult.Message;
            Hero hero = heroResult.Entity;

            if (hero.MapFaction != kingdom)
                return MessageFormatter.FormatErrorMessage($"{hero.Name} is not part of {kingdom.Name}.");

            if (hero.Clan == null)
                return MessageFormatter.FormatErrorMessage($"{hero.Name} has no clan.");

            // MARK: Execute Logic
            Dictionary<string, string> resolvedValues = new()
            {
                { "kingdom", kingdom.Name.ToString() },
                { "hero", hero.Name.ToString() }
            };

            string previousRuler = kingdom.Leader?.Name?.ToString() ?? "None";

            kingdom.RulingClan = hero.Clan;
            if (hero.Clan.Leader != hero)
                hero.Clan.SetLeader(hero);

            string argumentDisplay = parsed.FormatArgumentDisplay("gm.kingdom.set_ruler", resolvedValues);
            return CommandResult.Success(argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"{kingdom.Name}'s ruler changed from {previousRuler} to {hero.Name}.\n" +
                $"Ruling clan is now {hero.Clan.Name}.")).Message
;
        });
    }
}
