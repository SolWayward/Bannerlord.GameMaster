using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.DevCommands;

/// <summary>
/// Command to dump hotkey categories to a text file
/// </summary>
public static class DumpHotkeyCategoriesCommand
{
    /// <summary>
    /// Dumps hotkey categories to a text file for reference
    /// Usage: gm.dev.dump_hotkey_categories
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("dump_hotkey_categories", "gm.dev")]
    public static string DumpHotkeyCategories(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Execute Logic
            Dictionary<string, GameKeyContext>.ValueCollection hotkeyCategories = HotKeyManager.GetAllCategories();

            if (hotkeyCategories == null)
            {
                return MessageFormatter.FormatErrorMessage("Failed to retrieve hotkey categories - HotKeyManager returned null");
            }

            string outputPath = DevCommandHelpers.GetDumpFilePath("HotkeyCategories_Dump.txt");

            StringBuilder sb = new();
            sb.AppendLine($"Hotkey Categories - Total Categories: {hotkeyCategories.Count}");
            sb.AppendLine(new string('=', 80));
            sb.AppendLine();

            foreach (GameKeyContext category in hotkeyCategories)
            {
                if (category == null)
                {
                    sb.AppendLine("Category: NULL");
                    sb.AppendLine();
                    continue;
                }

                sb.AppendLine($"Category ID: {category.GameKeyCategoryId ?? "NULL"}");
                sb.AppendLine($"Category Type: {category.Type}");

                if (category.RegisteredGameKeys != null)
                {
                    sb.AppendLine($"  Registered Game Keys: {category.RegisteredGameKeys.Count}");
                    sb.AppendLine();

                    foreach (GameKey gameKey in category.RegisteredGameKeys)
                    {
                        if (gameKey == null)
                        {
                            continue;
                        }

                        sb.AppendLine($"    Key ID: {gameKey.Id}");
                        sb.AppendLine($"    Key String ID: {gameKey.StringId ?? "NULL"}");
                        sb.AppendLine($"    Key Group ID: {gameKey.GroupId ?? "NULL"}");
                        sb.AppendLine($"    Key MainCategory ID: {gameKey.MainCategoryId ?? "NULL"}");
                        sb.AppendLine($"    Key KeyboardKey: {gameKey.KeyboardKey?.ToString() ?? "NULL"}");
                        sb.AppendLine($"    Key DefaultKeyboardKey: {gameKey.DefaultKeyboardKey?.ToString() ?? "NULL"}");
                        sb.AppendLine();
                    }
                }
                else
                {
                    sb.AppendLine("  Registered Game Keys: NULL");
                    sb.AppendLine();
                }

                sb.AppendLine(new string('-', 80));
                sb.AppendLine();
            }

            File.WriteAllText(outputPath, sb.ToString());

            return MessageFormatter.FormatSuccessMessage($"Hotkey categories dumped to: {outputPath}");
        });
    }
}
