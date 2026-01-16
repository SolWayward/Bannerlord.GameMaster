using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Items;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.Query.ItemModifierQueryCommands;

/// <summary>
/// Get detailed information about a specific modifier
/// Usage: gm.query.modifier_info &lt;modifier_name&gt;
/// Example: gm.query.modifier_info fine
/// </summary>
public static class QueryModifierInfoCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("modifier_info", "gm.query")]
    public static string Execute(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return error;

            if (args == null || args.Count == 0)
                return "Error: Please provide a modifier name.\n" +
                       "Usage: gm.query.modifier_info <modifier_name>\n" +
                       "Example: gm.query.modifier_info fine\n";

            // MARK: Parse Arguments
            string modifierName = string.Join(" ", args);
            
            // MARK: Execute Logic
            var (modifier, parseError) = ItemModifierHelper.ParseModifier(modifierName);

            if (parseError != null)
                return MessageFormatter.FormatErrorMessage(parseError);

            if (modifier == null)
                return MessageFormatter.FormatErrorMessage($"Modifier '{modifierName}' not found.");

            return $"Modifier Information:\n" +
                   $"StringId: {modifier.StringId}\n" +
                   $"Name: {modifier.Name}\n" +
                   $"Price Multiplier: x{modifier.PriceMultiplier:F2}\n" +
                   $"Damage Modifier: {(modifier.Damage >= 0 ? "+" : "")}{modifier.Damage}\n" +
                   $"Speed Modifier: {(modifier.Speed >= 0 ? "+" : "")}{modifier.Speed}\n" +
                   $"Missile Speed Modifier: {(modifier.MissileSpeed >= 0 ? "+" : "")}{modifier.MissileSpeed}\n" +
                   $"Armor Modifier: {(modifier.Armor >= 0 ? "+" : "")}{modifier.Armor}\n" +
                   $"Hit Points Modifier: {(modifier.HitPoints >= 0 ? "+" : "")}{modifier.HitPoints}\n" +
                   $"Stack Count Modifier: {(modifier.StackCount >= 0 ? "+" : "")}{modifier.StackCount}\n";
        });
    }
}
