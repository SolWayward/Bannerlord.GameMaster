using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.HeroCommands.HeroManagementCommands;

/// <summary>
/// Imprison a hero
/// Usage: gm.hero.imprison [prisoner] [captor]
/// </summary>
public static class ImprisonHeroCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("imprison", "gm.hero")]
    public static string ImprisonHero(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return error;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.hero.imprison", "<prisoner> <captor>",
                "Imprisons a hero by another hero/party.\n" +
                "Supports named arguments: prisoner:lord_1_1 captor:lord_2_1",
                "gm.hero.imprison lord_1_1 lord_2_1");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("prisoner", true),
                new ArgumentDefinition("captor", true)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return MessageFormatter.FormatErrorMessage(validationError);

            if (parsed.TotalCount < 2)
                return usageMessage;

            // MARK: Parse Arguments
            string prisonerArg = parsed.GetArgument("prisoner", 0);
            if (prisonerArg == null)
                return MessageFormatter.FormatErrorMessage("Missing required argument 'prisoner'.");

            EntityFinderResult<Hero> prisonerResult = HeroFinder.FindSingleHero(prisonerArg);
            if (!prisonerResult.IsSuccess) return prisonerResult.Message;
            Hero prisoner = prisonerResult.Entity;

            string captorArg = parsed.GetArgument("captor", 1);
            if (captorArg == null)
                return MessageFormatter.FormatErrorMessage("Missing required argument 'captor'.");

            EntityFinderResult<Hero> captorResult = HeroFinder.FindSingleHero(captorArg);
            if (!captorResult.IsSuccess) return captorResult.Message;
            Hero captor = captorResult.Entity;

            if (prisoner.IsPrisoner)
                return MessageFormatter.FormatErrorMessage($"{prisoner.Name} is already a prisoner.");

            // MARK: Execute Logic
            Dictionary<string, string> resolvedValues = new()
            {
                { "prisoner", prisoner.Name.ToString() },
                { "captor", captor.Name.ToString() }
            };

            // Get the captor's party base
            PartyBase captorParty = captor.PartyBelongedTo?.Party
                                    ?? captor.Clan?.Kingdom?.Leader?.PartyBelongedTo?.Party
                                    ?? Settlement.FindFirst(s => s.OwnerClan == captor.Clan)?.Party;

            if (captorParty == null)
                return MessageFormatter.FormatErrorMessage($"{captor.Name} has no valid party or settlement to hold prisoners.");

            TakePrisonerAction.Apply(captorParty, prisoner);

            string argumentDisplay = parsed.FormatArgumentDisplay("imprison", resolvedValues);
            return argumentDisplay + MessageFormatter.FormatSuccessMessage($"{prisoner.Name} (ID: {prisoner.StringId}) is now imprisoned by {captor.Name}.");
        });
    }
}
