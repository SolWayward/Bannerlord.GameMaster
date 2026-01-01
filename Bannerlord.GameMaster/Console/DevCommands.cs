using Bannerlord.GameMaster.Console.Common;
using System;
using System.Collections.Generic;
using System.IO;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console
{
	/// <summary>
	/// Developer utility commands for debugging and data extraction
	/// </summary>
	[CommandLineFunctionality.CommandLineArgumentFunction("gm", "dev")]
	public static class DevCommands
	{
		/// <summary>
		/// Dumps the banner color palette to a text file for reference
		/// </summary>
		[CommandLineFunctionality.CommandLineArgumentFunction("dump_banner_colors", "gm.dev")]
		public static string DumpBannerColors(List<string> args)
		{
			return Cmd.Run(args, () =>
			{
				var colorPalette = BannerManager.Instance.ReadOnlyColorPalette;
				var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
				var configDir = Path.Combine(documentsPath, "Mount and Blade II Bannerlord", "Configs", "GameMaster");
				
				// Ensure directory exists
				if (!Directory.Exists(configDir))
				{
					Directory.CreateDirectory(configDir);
				}
				
				var outputPath = Path.Combine(configDir, "ColorPalette_Dump.txt");

				using (StreamWriter writer = new StreamWriter(outputPath))
				{
					writer.WriteLine($"Banner Color Palette - Total Colors: {colorPalette.Count}");
					writer.WriteLine(new string('=', 80));
					writer.WriteLine();

					foreach (var kvp in colorPalette)
					{
						int colorId = kvp.Key;
						uint colorValue = kvp.Value.Color;
						
						// Extract ARGB from uint
						byte a = (byte)((colorValue >> 24) & 0xFF);
						byte r = (byte)((colorValue >> 16) & 0xFF);
						byte g = (byte)((colorValue >> 8) & 0xFF);
						byte b = (byte)(colorValue & 0xFF);

						writer.WriteLine($"Color ID: {colorId}");
						writer.WriteLine($"  UInt:  {colorValue}");
						writer.WriteLine($"  ARGB:  ({a}, {r}, {g}, {b})");
						writer.WriteLine($"  Hex:   #{r:X2}{g:X2}{b:X2}");
						writer.WriteLine($"  Float: R={r / 255f:F3}, G={g / 255f:F3}, B={b / 255f:F3}, A={a / 255f:F3}");
						writer.WriteLine();
					}
				}

				return $"Banner color palette dumped to: {outputPath}";
			});
		}

		[CommandLineFunctionality.CommandLineArgumentFunction("dump_hotkey_categories", "gm.dev")]
		public static string DumpHotkeyCategories(List<string> args)
		{
			return Cmd.Run(args, () =>
			{
				Dictionary<string, GameKeyContext>.ValueCollection HotKeyCategories = HotKeyManager.GetAllCategories();
				
				if (HotKeyCategories == null)
				{
					return "Failed to retrieve hotkey categories - HotKeyManager returned null";
				}

				var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
				var configDir = Path.Combine(documentsPath, "Mount and Blade II Bannerlord", "Configs", "GameMaster");
				
				// Ensure directory exists
				if (!Directory.Exists(configDir))
				{
					Directory.CreateDirectory(configDir);
				}
				
				var outputPath = Path.Combine(configDir, "HotkeyCategories_Dump.txt");

				using (StreamWriter writer = new StreamWriter(outputPath))
				{
					writer.WriteLine($"Hotkey Categories - Total Categories: {HotKeyCategories.Count}");
					writer.WriteLine(new string('=', 80));
					writer.WriteLine();

					foreach (var category in HotKeyCategories)
					{
						if (category == null)
						{
							writer.WriteLine("Category: NULL");
							writer.WriteLine();
							continue;
						}
						
						writer.WriteLine($"Category ID: {category.GameKeyCategoryId ?? "NULL"}");
						writer.WriteLine($"Category Type: {category.Type}");
						
						if (category.RegisteredGameKeys != null)
						{
							writer.WriteLine($"  Registered Game Keys: {category.RegisteredGameKeys.Count}");
							writer.WriteLine();

							foreach (var gameKey in category.RegisteredGameKeys)
							{
								if (gameKey == null)
								{
									continue;
								}
								
								writer.WriteLine($"    Key ID: {gameKey.Id}");
								writer.WriteLine($"    Key String ID: {gameKey.StringId ?? "NULL"}");
								writer.WriteLine($"    Key Group ID: {gameKey.GroupId ?? "NULL"}");
								writer.WriteLine($"    Key MainCategory ID: {gameKey.MainCategoryId ?? "NULL"}");
								writer.WriteLine($"    Key KeyboardKey: {gameKey.KeyboardKey?.ToString() ?? "NULL"}");
								writer.WriteLine($"    Key DefaultKeyboardKey: {gameKey.DefaultKeyboardKey?.ToString() ?? "NULL"}");
								writer.WriteLine();
							}
						}
						else
						{
							writer.WriteLine("  Registered Game Keys: NULL");
							writer.WriteLine();
						}

						writer.WriteLine(new string('-', 80));
						writer.WriteLine();
					}
				}

				return $"Hotkey categories dumped to: {outputPath}";
			});
		}
	}
}
