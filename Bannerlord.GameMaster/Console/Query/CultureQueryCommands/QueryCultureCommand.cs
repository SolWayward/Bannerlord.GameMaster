using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Console.Query.CommandQueryHelpers;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace Bannerlord.GameMaster.Console.Query.CultureQueryCommands;

/// <summary>
/// List all cultures with optional filtering
/// Usage: gm.query.culture [search terms] [main|bandit] [sort parameters]
/// Example: gm.query.culture
/// Example: gm.query.culture main
/// Example: gm.query.culture empire sort:name
/// Example: gm.query.culture bandit sort:name:desc
/// </summary>
public static class QueryCultureCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("culture", "gm.query")]
    public static string Execute(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return error;

            // MARK: Parse Arguments
            CultureQueryArguments queryArgs = CultureQueryHelpers.ParseCultureQueryArguments(args);

            // MARK: Execute Logic
            IEnumerable<CultureObject> cultures = MBObjectManager.Instance.GetObjectTypeList<CultureObject>();

            // Apply filters
            if (queryArgs.MainOnly)
            {
                cultures = cultures.Where(c => c.IsMainCulture);
            }
            else if (queryArgs.BanditOnly)
            {
                cultures = cultures.Where(c => c.IsBandit);
            }

            // Apply search filter
            if (!string.IsNullOrEmpty(queryArgs.QueryArgs.Query))
            {
                string lowerQuery = queryArgs.QueryArgs.Query.ToLower();
                cultures = cultures.Where(c =>
                    c.StringId.ToLower().Contains(lowerQuery) ||
                    c.Name.ToString().ToLower().Contains(lowerQuery));
            }

            // Convert to list and apply sorting
            List<CultureObject> cultureList = CultureQueryHelpers.SortCultures(
                cultures.ToList(), 
                queryArgs.QueryArgs.SortBy, 
                queryArgs.QueryArgs.SortDesc);

            string criteriaDesc = queryArgs.GetCriteriaString();

            if (cultureList.Count == 0)
            {
                return $"Found 0 culture(s) matching {criteriaDesc}\n" +
                       "Usage: gm.query.culture [search] [main|bandit] [sort]\n" +
                       "Example: gm.query.culture empire\n" +
                       "Example: gm.query.culture main sort:name\n" +
                       "Example: gm.query.culture bandit\n";
            }

            return $"Found {cultureList.Count} culture(s) matching {criteriaDesc}:\n" +
                   $"{CultureQueryHelpers.GetFormattedCultureList(cultureList)}";
        });
    }
}
