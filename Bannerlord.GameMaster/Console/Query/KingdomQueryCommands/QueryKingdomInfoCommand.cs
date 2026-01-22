using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Kingdoms;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.Query.KingdomQueryCommands;

/// <summary>
/// Get detailed info about a specific kingdom by ID
/// Usage: gm.query.kingdom_info <kingdomId>
/// </summary>
public static class QueryKingdomInfoCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("kingdom_info", "gm.query")]
    public static string Execute(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Log().Message;

            if (args == null || args.Count == 0)
                return CommandResult.Error("Please provide a kingdom ID.\nUsage: gm.query.kingdom_info <kingdomId>\n").Log().Message;

            // MARK: Parse Arguments
            string kingdomId = args[0];
            Kingdom kingdom = KingdomQueries.GetKingdomById(kingdomId);

            if (kingdom == null)
                return CommandResult.Error($"Kingdom with ID '{kingdomId}' not found.\n").Log().Message;

            // MARK: Execute Logic
            KingdomTypes types = kingdom.GetKingdomTypes();

            // Get kingdoms at war with this kingdom
            List<Kingdom> enemies = new();
            foreach (Kingdom otherKingdom in Kingdom.All)
            {
                if (otherKingdom != kingdom && FactionManager.IsAtWarAgainstFaction(kingdom, otherKingdom))
                {
                    enemies.Add(otherKingdom);
                }
            }

            return CommandResult.Success($"Kingdom Information:\n" +
                   $"ID: {kingdom.StringId}\n" +
                   $"Name: {kingdom.Name}\n" +
                   $"Culture: {kingdom.Culture?.Name}\n" +
                   $"Ruler: {kingdom.Leader?.Name}\n" +
                   $"Ruling Clan: {kingdom.RulingClan?.Name}\n" +
                   $"Total Clans: {kingdom.Clans.Count}\n" +
                   $"Total Heroes: {kingdom.Heroes.Count()}\n" +
                   $"Total Fiefs: {kingdom.Fiefs.Count}\n" +
                   $"Total Strength: {kingdom.CurrentTotalStrength:F0}\n" +
                   $"Types: {types}\n" +
                   $"Is Eliminated: {kingdom.IsEliminated}\n" +
                   $"At War With ({enemies.Count}): {string.Join(", ", enemies.Select(k => k.Name))}\n").Log().Message;
        });
    }
}
