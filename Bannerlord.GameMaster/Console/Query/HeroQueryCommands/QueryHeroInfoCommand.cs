using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Heroes;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.Query.HeroQueryCommands;

/// <summary>
/// Get detailed info about a specific hero by ID
/// Usage: gm.query.hero_info [heroId]
/// </summary>
public static class QueryHeroInfoCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("hero_info", "gm.query")]
    public static string Execute(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Message;

            if (args == null || args.Count == 0)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Please provide a hero ID.\nUsage: gm.query.hero_info <heroId>")).Message;

            // MARK: Parse Arguments
            string heroId = args[0];

            // MARK: Execute Logic
            Hero hero = HeroQueries.GetHeroById(heroId);

            if (hero == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage($"Hero with ID '{heroId}' not found.")).Message;

            HeroTypes types = hero.GetHeroTypes();
            string clanName = hero.Clan?.Name?.ToString() ?? "None";
            string kingdomName = hero.Clan?.Kingdom?.Name?.ToString() ?? "None";

            return CommandResult.Success($"Hero Information:\n" +
                   $"ID: {hero.StringId}\n" +
                   $"Name: {hero.Name}\n" +
                   $"Clan: {clanName}\n" +
                   $"Kingdom: {kingdomName}\n" +
                   $"Age: {hero.Age:F0}\n" +
                   $"Types: {types}\n" +
                   $"Is Alive: {hero.IsAlive}\n" +
                   $"Is Prisoner: {hero.IsPrisoner}\n").Message;
        });
    }
}
