using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Heroes;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.HeroCommands.HeroManagementCommands;

/// <summary>
/// Create a party for a hero at their last known location or home settlement
/// Usage: gm.hero.create_party [hero]
/// </summary>
public static class CreatePartyCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("create_party", "gm.hero")]
    public static string CreateParty(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Message;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.hero.create_party", "<hero>",
                "Creates a party for the specified hero. The party will spawn at the hero's last known location if available,\n" +
                "otherwise at their home settlement or an alternative settlement.\n" +
                "The party is initialized with 10 basic troops and 20000 trade gold.\n" +
                "Supports named arguments: hero:lord_1_1",
                "gm.hero.create_party lord_1_1\n" +
                "gm.hero.create_party wanderer_1");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("hero", true)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError)).Message;

            if (parsed.TotalCount < 1)
                return CommandResult.Error(usageMessage).Message;

            // MARK: Parse Arguments
            string heroArg = parsed.GetArgument("hero", 0);
            if (heroArg == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'hero'.")).Message;

            EntityFinderResult<Hero> heroResult = HeroFinder.FindSingleHero(heroArg);
            if (!heroResult.IsSuccess) return CommandResult.Error(heroResult.Message).Message;
            Hero hero = heroResult.Entity;

            // MARK: Execute Logic
            Dictionary<string, string> resolvedValues = new()
            {
                { "hero", hero.Name.ToString() }
            };
            string argumentDisplay = parsed.FormatArgumentDisplay("gm.hero.create_party", resolvedValues);

            // Check if hero already has a party
            if (hero.PartyBelongedTo != null && hero.PartyBelongedTo.LeaderHero == hero)
                return CommandResult.Error(argumentDisplay + MessageFormatter.FormatErrorMessage($"{hero.Name} already leads a party: {hero.PartyBelongedTo.Name}")).Message;

            // Determine spawn settlement
            Settlement spawnSettlement = null;
            
            // Try to use last seen place if it's a settlement
            if (hero.LastKnownClosestSettlement != null)
            {
                spawnSettlement = hero.LastKnownClosestSettlement;
            }
            
            // Fallback to home or alternative settlement
            if (spawnSettlement == null)
            {
                spawnSettlement = hero.GetHomeOrAlternativeSettlement();
            }

            if (spawnSettlement == null)
                return CommandResult.Error(argumentDisplay + MessageFormatter.FormatErrorMessage($"Could not find a suitable settlement to spawn {hero.Name}'s party.")).Message;

            // Create the party using the extension method
            MobileParty newParty = hero.CreateParty(spawnSettlement);

            if (newParty == null)
                return CommandResult.Error(argumentDisplay + MessageFormatter.FormatErrorMessage($"Failed to create party for {hero.Name}.")).Message;

            string fullMessage = argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"Created party for {hero.Name}.\n" +
                $"Party: {newParty.Name}\n" +
                $"Location: {spawnSettlement.Name}\n" +
                $"Initial roster: {newParty.MemberRoster.TotalManCount} troops\n" +
                $"Trade gold: {newParty.PartyTradeGold}");
            return CommandResult.Success(fullMessage).Message;
        });
    }
}
