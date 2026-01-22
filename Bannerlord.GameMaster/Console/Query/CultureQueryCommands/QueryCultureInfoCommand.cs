using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace Bannerlord.GameMaster.Console.Query.CultureQueryCommands;

/// <summary>
/// Get detailed info about a specific culture by ID
/// Usage: gm.query.culture_info <cultureId>
/// Example: gm.query.culture_info empire
/// Example: gm.query.culture_info vlandia
/// </summary>
public static class QueryCultureInfoCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("culture_info", "gm.query")]
    public static string Execute(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Log().Message;

            if (args == null || args.Count == 0)
                return CommandResult.Error("Please provide a culture ID.\n" +
                       "Usage: gm.query.culture_info <cultureId>\n" +
                       "Example: gm.query.culture_info empire\n").Log().Message;

            // MARK: Parse Arguments
            string cultureId = args[0];
            CultureObject culture = MBObjectManager.Instance.GetObject<CultureObject>(cultureId);

            if (culture == null)
                return CommandResult.Error($"Culture with ID '{cultureId}' not found.\n").Log().Message;

            // MARK: Execute Logic
            Dictionary<string, string> resolvedValues = new()
            {
                { "cultureId", cultureId }
            };
            string argumentDisplay = new ParsedArguments(new()).FormatArgumentDisplay("gm.query.culture_info", resolvedValues);

            string cultureInfo = $"Culture Information:\n" +
                   $"ID: {culture.StringId}\n" +
                   $"Name: {culture.Name}\n" +
                   $"Is Main Culture: {culture.IsMainCulture}\n" +
                   $"Is Bandit: {culture.IsBandit}\n" +
                   $"Color: {culture.Color}\n" +
                   $"Color2: {culture.Color2}\n" +
                   $"Male Names: {culture.MaleNameList?.Count ?? 0}\n" +
                   $"Female Names: {culture.FemaleNameList?.Count ?? 0}\n" +
                   $"Clan Names: {culture.ClanNameList?.Count ?? 0}\n";

            return CommandResult.Success(argumentDisplay + cultureInfo).Log().Message;
        });
    }
}
