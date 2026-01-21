using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.DevCommands;

/// <summary>
/// Command to dump the banner color palette to a text file
/// </summary>
public static class DumpBannerColorsCommand
{
    /// <summary>
    /// Dumps the banner color palette to a text file for reference
    /// Usage: gm.dev.dump_banner_colors
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("dump_banner_colors", "gm.dev")]
    public static string DumpBannerColors(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Execute Logic
            IEnumerable<BannerColor> colorPalette = BannerManager.Instance.ReadOnlyColorPalette.Values;
            MBReadOnlyDictionary<int, BannerColor> colorPaletteDict = BannerManager.Instance.ReadOnlyColorPalette;
            string outputPath = DevCommandHelpers.GetDumpFilePath("ColorPalette_Dump.txt");

            StringBuilder sb = new();
            sb.AppendLine($"Banner Color Palette - Total Colors: {colorPaletteDict.Count}");
            sb.AppendLine(new string('=', 80));
            sb.AppendLine();

            foreach (KeyValuePair<int, BannerColor> kvp in colorPaletteDict)
            {
                int colorId = kvp.Key;
                uint colorValue = kvp.Value.Color;

                // Extract ARGB from uint
                byte a = (byte)((colorValue >> 24) & 0xFF);
                byte r = (byte)((colorValue >> 16) & 0xFF);
                byte g = (byte)((colorValue >> 8) & 0xFF);
                byte b = (byte)(colorValue & 0xFF);

                sb.AppendLine($"Color ID: {colorId}");
                sb.AppendLine($"  UInt:  {colorValue}");
                sb.AppendLine($"  ARGB:  ({a}, {r}, {g}, {b})");
                sb.AppendLine($"  Hex:   #{r:X2}{g:X2}{b:X2}");
                sb.AppendLine($"  Float: R={r / 255f:F3}, G={g / 255f:F3}, B={b / 255f:F3}, A={a / 255f:F3}");
                sb.AppendLine();
            }

            File.WriteAllText(outputPath, sb.ToString());

            return CommandResult.Success(MessageFormatter.FormatSuccessMessage($"Banner color palette dumped to: {outputPath}")).Log().Message;
        });
    }
}
