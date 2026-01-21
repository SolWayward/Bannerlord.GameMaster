using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.TroopCommands.TroopPartyManagementCommands;

[CommandLineFunctionality.CommandLineArgumentFunction("troops", "gm")]
public static class GiveHeroTroopsCommand
{
    /// <summary>
    /// Give troops to a hero's party
    /// Usage: gm.troops.give_hero_troops [heroQuery] [troopQuery] [count]
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("give_hero_troops", "gm.troops")]
    public static string GiveHeroTroops(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Log().Message;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.troops.give_hero_troops", "<hero_query> <character_query> <count>",
                "Gives characters/troops to a hero's party. Accepts any CharacterObject including dancers, refugees, etc.\n" +
                "Supports named arguments: hero:player character:imperial_recruit count:10",
                "gm.troops.give_hero_troops player imperial_recruit 10");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("hero", true),
                new ArgumentDefinition("character", true, null, "troop"),
                new ArgumentDefinition("count", true)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError)).Log().Message;

            if (parsed.TotalCount < 3)
                return CommandResult.Error(usageMessage).Log().Message;

            // MARK: Parse Arguments
            string heroArg = parsed.GetArgument("hero", 0);
            if (heroArg == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'hero'.")).Log().Message;

            EntityFinderResult<Hero> heroResult = HeroFinder.FindSingleHero(heroArg);
            if (!heroResult.IsSuccess)
                return CommandResult.Error(heroResult.Message).Log().Message;
            Hero hero = heroResult.Entity;

            string characterArg = parsed.GetArgument("character", 1) ?? parsed.GetNamed("troop");
            if (characterArg == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'character'.")).Log().Message;

            EntityFinderResult<CharacterObject> characterResult = CharacterObjectFinder.FindSingleCharacterObject(characterArg);
            if (!characterResult.IsSuccess)
                return CommandResult.Error(characterResult.Message).Log().Message;
            CharacterObject character = characterResult.Entity;

            // Validate character is not a hero
            if (character.IsHero)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage($"{character.Name} is a hero and cannot be added as a troop.")).Log().Message;

            string countArg = parsed.GetArgument("count", 2);
            if (countArg == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'count'.")).Log().Message;

            if (!CommandValidator.ValidateIntegerRange(countArg, 1, 10000, out int count, out string countError))
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(countError)).Log().Message;

            // MARK: Execute Logic
            if (hero.PartyBelongedTo == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage($"{hero.Name} does not belong to a party.")).Log().Message;

            bool isPartyLeader = hero.PartyBelongedTo.LeaderHero == hero;

            hero.PartyBelongedTo.MemberRoster.AddToCounts(character, count);

            Dictionary<string, string> resolvedValues = new()
            {
                { "hero", hero.Name.ToString() },
                { "character", character.Name.ToString() },
                { "count", count.ToString() }
            };

            StringBuilder result = new();
            result.AppendLine($"Added {count}x {character.Name} to {hero.Name}'s party.");

            if (!isPartyLeader)
            {
                result.AppendLine($"Party: {hero.PartyBelongedTo.Name} (Leader: {hero.PartyBelongedTo.LeaderHero.Name})");
            }
            else
            {
                result.AppendLine($"Party: {hero.PartyBelongedTo.Name}");
            }

            string argumentDisplay = parsed.FormatArgumentDisplay("give_hero_troops", resolvedValues);
            string fullMessage = argumentDisplay + MessageFormatter.FormatSuccessMessage(result.ToString());
            return CommandResult.Success(fullMessage).Log().Message;
        });
    }
}
