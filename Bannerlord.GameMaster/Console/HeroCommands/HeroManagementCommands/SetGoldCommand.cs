using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.HeroCommands.HeroManagementCommands;

/// <summary>
/// Change hero's gold
/// Usage: gm.hero.set_gold [hero] [amount]
/// </summary>
public static class SetGoldCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("set_gold", "gm.hero")]
    public static string SetGold(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return error;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.hero.set_gold", "<hero> <amount>",
                "Sets the hero's gold amount.\n" +
                "Supports named arguments: hero:lord_1_1 amount:10000",
                "gm.hero.set_gold lord_1_1 10000");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("hero", true),
                new ArgumentDefinition("amount", true)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return MessageFormatter.FormatErrorMessage(validationError);

            if (parsed.TotalCount < 2)
                return usageMessage;

            // MARK: Parse Arguments
            string heroArg = parsed.GetArgument("hero", 0);
            if (heroArg == null)
                return MessageFormatter.FormatErrorMessage("Missing required argument 'hero'.");

            EntityFinderResult<Hero> heroResult = HeroFinder.FindSingleHero(heroArg);
            if (!heroResult.IsSuccess) return heroResult.Message;
            Hero hero = heroResult.Entity;

            string amountArg = parsed.GetArgument("amount", 1);
            if (amountArg == null)
                return MessageFormatter.FormatErrorMessage("Missing required argument 'amount'.");

            if (!CommandValidator.ValidateIntegerRange(amountArg, int.MinValue, int.MaxValue, out int amount, out string goldError))
                return MessageFormatter.FormatErrorMessage(goldError);

            // MARK: Execute Logic
            Dictionary<string, string> resolvedValues = new()
            {
                { "hero", hero.Name.ToString() },
                { "amount", amount.ToString() }
            };

            int previousGold = hero.Gold;
            hero.ChangeHeroGold(amount - previousGold);

            string argumentDisplay = parsed.FormatArgumentDisplay("set_gold", resolvedValues);
            return argumentDisplay + MessageFormatter.FormatSuccessMessage($"{hero.Name}'s gold changed from {previousGold} to {hero.Gold}.");
        });
    }
}
