using Bannerlord.GameMaster.Characters;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Console.Query.CommandQueryHelpers;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.Query.TroopQueryCommands;

/// <summary>
/// Query ALL CharacterObjects matching ANY of the specified types (OR logic).
/// Returns EVERYTHING by default (heroes, troops, templates, NPCs).
/// Use type keywords to filter specific categories.
/// Usage: gm.query.character_objects_any [search terms] [type keywords] [culture keywords] [tier] [sort parameters]
/// Example: gm.query.character_objects_any hero troop (returns characters that are EITHER hero OR troop)
/// Example: gm.query.character_objects_any lord wanderer (returns lords OR wanderers)
/// Example: gm.query.character_objects_any cavalry ranged tier4
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
                return CommandResult.Error(error).Message;

            // MARK: Parse Arguments
            CharacterQueryArguments queryArgs = CharacterQueryHelpers.ParseCharacterQueryArguments(args);

            // MARK: Execute Logic
            MBReadOnlyList<CharacterObject> matchedCharacters = CharacterQueries.QueryCharacterObjects(
                queryArgs.QueryArgs.Query,
                queryArgs.Types,
                matchAll: false, // OR logic - match ANY of the specified types
                queryArgs.Cultures,
                queryArgs.Tier,
                queryArgs.QueryArgs.SortBy,
                queryArgs.QueryArgs.SortDesc);

            string criteriaDesc = queryArgs.GetCriteriaString();

            if (matchedCharacters.Count == 0)
            {
                return CommandResult.Success($"Found 0 character(s) matching ANY of {criteriaDesc}\n" +
                       "Usage: gm.query.character_objects_any [search] [type keywords] [cultures] [tier] [sort]\n" +
                       "Uses OR logic - matches characters that have ANY of the specified types.\n" +
                       "Example: gm.query.character_objects_any hero troop (hero OR troop)\n" +
                       "Example: gm.query.character_objects_any lord wanderer empire (lord OR wanderer, AND empire culture)\n").Message;
            }

            List<CharacterObject> characterList = new(matchedCharacters);
            return CommandResult.Success($"Found {matchedCharacters.Count} character(s) matching ANY of {criteriaDesc}:\n" +
                   $"{CharacterQueries.GetFormattedDetails(characterList)}").Message;
        });
    }
}
