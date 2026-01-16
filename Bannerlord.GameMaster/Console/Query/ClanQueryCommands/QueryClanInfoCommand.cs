using Bannerlord.GameMaster.Clans;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Validation;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.Query.ClanQueryCommands;

/// <summary>
/// Get detailed info about a specific clan by ID
/// Usage: gm.query.clan_info [clanId]
/// Example: gm.query.clan_info clan_empire_1
/// </summary>
public static class QueryClanInfoCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("clan_info", "gm.query")]
    public static string Execute(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return error;

            if (args == null || args.Count == 0)
                return "Error: Please provide a clan ID.\nUsage: gm.query.clan_info <clanId>\n";

            // MARK: Parse Arguments
            string clanId = args[0];

            // MARK: Execute Logic
            Clan clan = ClanQueries.GetClanById(clanId);

            if (clan == null)
                return $"Error: Clan with ID '{clanId}' not found.\n";

            ClanTypes types = clan.GetClanTypes();
            string kingdomName = clan.Kingdom?.Name?.ToString() ?? "None";
            string leaderName = clan.Leader?.Name?.ToString() ?? "None";

            return $"Clan Information:\n" +
                   $"ID: {clan.StringId}\n" +
                   $"Name: {clan.Name}\n" +
                   $"Leader: {leaderName}\n" +
                   $"Kingdom: {kingdomName}\n" +
                   $"Total Heroes: {clan.Heroes.Count}\n" +
                   $"Lords: {clan.Heroes.Count}\n" +
                   $"Companions: {clan.Companions.Count()}\n" +
                   $"Gold: {clan.Gold}\n" +
                   $"Tier: {clan.Tier}\n" +
                   $"Renown: {clan.Renown:F0}\n" +
                   $"Types: {types}\n" +
                   $"Is Eliminated: {clan.IsEliminated}\n";
        });
    }
}
