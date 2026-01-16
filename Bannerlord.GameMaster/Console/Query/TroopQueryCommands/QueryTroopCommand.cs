using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Console.Query.CommandQueryHelpers;
using Bannerlord.GameMaster.Troops;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.Query.TroopQueryCommands;

/// <summary>
/// Unified troop listing command with AND logic
/// Usage: gm.query.troop [search terms] [type keywords] [tier] [sort parameters]
/// Example: gm.query.troop imperial infantry
/// Example: gm.query.troop aserai cavalry tier3
/// Example: gm.query.troop shield infantry sort:tier:desc
/// Example: gm.query.troop battania ranged bow
/// Example: gm.query.troop noble empire tier5
/// </summary>
public static class QueryTroopCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("troop", "gm.query")]
    public static string Execute(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return error;

            // MARK: Parse Arguments
            TroopQueryArguments queryArgs = TroopQueryHelpers.ParseTroopQueryArguments(args);

            // MARK: Execute Logic
            List<CharacterObject> matchedTroops = TroopQueries.QueryTroops(
                queryArgs.QueryArgs.Query, 
                queryArgs.Types, 
                matchAll: true, 
                queryArgs.Tier, 
                queryArgs.QueryArgs.SortBy, 
                queryArgs.QueryArgs.SortDesc);

            string criteriaDesc = queryArgs.GetCriteriaString();

            if (matchedTroops.Count == 0)
            {
                return $"Found 0 troop(s) matching {criteriaDesc}\n" +
                       "Usage: gm.query.troop [search] [type keywords] [tier] [sort]\n" +
                       "Type keywords: infantry, ranged, cavalry, horsearcher, shield, bow, crossbow, regular, noble, militia, mercenary, caravan, bandit, female, male, etc.\n" +
                       "Tier keywords: tier0, tier1, tier2, tier3, tier4, tier5, tier6, tier6plus\n" +
                       "Sort: sort:name, sort:tier, sort:level, sort:culture, sort:<type> (add :desc for descending)\n" +
                       "Example: gm.query.troop imperial infantry tier2 sort:name\n" +
                       "Example: gm.query.troop female cavalry (find female cavalry troops)\n" +
                       "Note: Non-troops (heroes, NPCs, children, templates, etc.) are automatically excluded.\n";
            }

            return $"Found {matchedTroops.Count} troop(s) matching {criteriaDesc}:\n" +
                   $"{TroopQueries.GetFormattedDetails(matchedTroops)}";
        });
    }
}
