using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Items;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.Query
{
    [CommandLineFunctionality.CommandLineArgumentFunction("query", "gm")]
    public static class ItemModifierQueryCommands
    {
        /// <summary>
        /// List all available item modifiers
        /// Usage: gm.query.modifiers [search]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("modifiers", "gm.query")]
        public static string QueryModifiers(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                var allModifiers = ItemModifierHelper.GetAllModifiers();

                // Filter by search term if provided
                if (args != null && args.Count > 0)
                {
                    string searchTerm = string.Join(" ", args).ToLower();
                    allModifiers = allModifiers
                        .Where(m => m.Name.ToString().ToLower().Contains(searchTerm) ||
                                   m.StringId.ToLower().Contains(searchTerm))
                        .ToList();
                }

                if (allModifiers.Count == 0)
                {
                    string searchMsg = args != null && args.Count > 0 
                        ? $" matching '{string.Join(" ", args)}'" 
                        : "";
                    return $"Found 0 modifiers{searchMsg}.\n" +
                           "Usage: gm.query.modifiers [search]\n" +
                           "Example: gm.query.modifiers fine\n";
                }

                // Sort by name for better readability
                allModifiers = allModifiers.OrderBy(m => m.Name.ToString()).ToList();

                string result = $"Found {allModifiers.Count} modifier(s):\n\n";
                result += "StringId\t\tName\t\t\tPrice Factor\n";
                result += "--------\t\t----\t\t\t------------\n";

                foreach (var modifier in allModifiers)
                {
                    result += $"{modifier.StringId,-20}\t{modifier.Name,-20}\tx{modifier.PriceMultiplier:F2}\n";
                }

                return result;
            });
        }

        /// <summary>
        /// Get detailed information about a specific modifier
        /// Usage: gm.query.modifier_info <modifier_name>
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("modifier_info", "gm.query")]
        public static string QueryModifierInfo(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                if (args == null || args.Count == 0)
                    return "Error: Please provide a modifier name.\n" +
                           "Usage: gm.query.modifier_info <modifier_name>\n" +
                           "Example: gm.query.modifier_info fine\n";

                string modifierName = string.Join(" ", args);
                var (modifier, parseError) = ItemModifierHelper.ParseModifier(modifierName);

                if (parseError != null)
                    return CommandBase.FormatErrorMessage(parseError);

                if (modifier == null)
                    return CommandBase.FormatErrorMessage($"Modifier '{modifierName}' not found.");

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
}