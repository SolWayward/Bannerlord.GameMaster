using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Library;
using TaleWorlds.CampaignSystem.Conversation;
using Bannerlord.GameMaster.Heroes;
using Bannerlord.GameMaster.Common;

namespace Bannerlord.GameMaster.Console.HeroCommands.HeroManagementCommands;

/// <summary>
/// Start a conversation with another hero
/// Usage: gm.hero.start_conversation [hero]
/// </summary>
public static class StartConversationCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("start_conversation", "gm.hero")]
    public static string StartConversation(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Message;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.hero.start_conversation", "<hero>",
                "Start a conversation with another hero.\n" +
                "Supports named arguments: hero:derther",
                "gm.hero.start_conversation derthert");

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

            // Do all these need to be check or is there one status that can be checked instead?
            //if (!hero.IsActive || hero.IsDead || hero.IsDisabled || hero.IsNotSpawned || hero.IsDisabled || hero.IsFugitive || hero.IsTraveling)
                //return CommandResult.Error(MessageFormatter.FormatErrorMessage($"{hero.Name} is not available for conversation.\nHero State: {hero.HeroState}  \nHeroID: {hero.StringId}")).Message;

            // MARK: Execute Logic
            Dictionary<string, string> resolvedValues = new()
            {
                { "hero", hero.Name.ToString() }
            };
            
            // Start Conversation
            BLGMResult result = hero.StartConversationWithPlayer();

            if (!result.IsSuccess)
                return result.Message;

            string argumentDisplay = parsed.FormatArgumentDisplay("gm.hero.start_conversation", resolvedValues);
            string fullMessage = argumentDisplay + MessageFormatter.FormatSuccessMessage(result.Message);
            return CommandResult.Success(fullMessage).Message;
        });
    }
}
