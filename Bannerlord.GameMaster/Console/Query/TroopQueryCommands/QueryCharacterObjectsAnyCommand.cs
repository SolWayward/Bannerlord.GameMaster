using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Console.Query.CommandQueryHelpers;
using Bannerlord.GameMaster.Troops;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.Query.TroopQueryCommands;

/// <summary>
/// UNFILTERED query for ALL CharacterObjects (except heroes) matching ANY of the specified types (OR logic)
/// WARNING: Returns NPCs, templates, children, etc. - not just combat troops
/// Usage: gm.query.character_objects_any [search terms] [type keywords] [tier] [sort parameters]
/// Example: gm.query.character_objects_any cavalry ranged (cavalry OR ranged)
/// Example: gm.query.character_objects_any bow crossbow tier4
/// Note: Use gm.query.troop_any for filtered combat troops only
/// </summary>
public static class QueryCharacterObjectsAnyCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("character_objects_any", "gm.query")]
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
                matchAll: false, 
                queryArgs.Tier, 
                queryArgs.QueryArgs.SortBy, 
                queryArgs.QueryArgs.SortDesc);

            string criteriaDesc = queryArgs.GetCriteriaString();

            string headerNote = "[WARNING] UNFILTERED QUERY - Shows ALL CharacterObjects (except heroes)\n" +
                                "Includes NPCs, templates, children, etc. Use gm.query.troop_any for combat troops only.\n\n";

            if (matchedCharacters.Count == 0)
            {
                return headerNote +
                       $"Found 0 character(s) matching ANY of {criteriaDesc}\n" +
                       "Usage: gm.query.character_objects_any [search] [type keywords] [tier] [sort]\n" +
                       "Example: gm.query.character_objects_any cavalry ranged tier3 sort:tier\n";
            }

            return headerNote +
                   $"Found {matchedCharacters.Count} character(s) matching ANY of {criteriaDesc}:\n" +
                   $"{TroopQueries.GetFormattedDetails(matchedCharacters)}";
        });
    }
}
