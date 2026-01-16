using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Console.Query.CommandQueryHelpers;
using Bannerlord.GameMaster.Troops;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.Query.TroopQueryCommands;

/// <summary>
/// UNFILTERED query for ALL CharacterObjects (except heroes) with AND logic
/// WARNING: Returns NPCs, templates, children, etc. - not just combat troops
/// Usage: gm.query.character_objects [search terms] [type keywords] [tier] [sort parameters]
/// Example: gm.query.character_objects imperial infantry
/// Example: gm.query.character_objects militia sort:name
/// Note: Use gm.query.troop for filtered combat troops only
/// </summary>
public static class QueryCharacterObjectsCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("character_objects", "gm.query")]
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
            List<CharacterObject> matchedCharacters = TroopQueries.QueryCharacterObjects(
                queryArgs.QueryArgs.Query, 
                queryArgs.Types, 
                matchAll: true, 
                queryArgs.Tier, 
                queryArgs.QueryArgs.SortBy, 
                queryArgs.QueryArgs.SortDesc);

            string criteriaDesc = queryArgs.GetCriteriaString();

            string headerNote = "[WARNING] UNFILTERED QUERY - Shows ALL CharacterObjects (except heroes)\n" +
                                "Includes NPCs, templates, children, etc. Use gm.query.troop for combat troops only.\n\n";

            if (matchedCharacters.Count == 0)
            {
                return headerNote +
                       $"Found 0 character(s) matching {criteriaDesc}\n" +
                       "Usage: gm.query.character_objects [search] [type keywords] [tier] [sort]\n" +
                       "Type keywords: infantry, ranged, cavalry, horsearcher, shield, bow, crossbow, regular, noble, militia, mercenary, caravan, bandit, etc.\n" +
                       "Tier keywords: tier0, tier1, tier2, tier3, tier4, tier5, tier6, tier6plus\n" +
                       "Sort: sort:name, sort:tier, sort:level, sort:culture, sort:<type> (add :desc for descending)\n" +
                       "Example: gm.query.character_objects imperial infantry tier2 sort:name\n";
            }

            return headerNote +
                   $"Found {matchedCharacters.Count} character(s) matching {criteriaDesc}:\n" +
                   $"{TroopQueries.GetFormattedDetails(matchedCharacters)}";
        });
    }
}
