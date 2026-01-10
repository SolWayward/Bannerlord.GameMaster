using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Information;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;

namespace Bannerlord.GameMaster.Console
{
	/// <summary>
	/// Used for displaying useful info
	/// </summary>
	[CommandLineFunctionality.CommandLineArgumentFunction("info", "gm")]
	public static class InfoCommands
	{
		/// <summary>
		/// List current Bannerlord game version
		/// </summary>
		[CommandLineFunctionality.CommandLineArgumentFunction("bannerlord_version", "gm.info")]
		public static string BannerlordVersionCommand(List<string> args)
		{
			return Cmd.Run(args, () =>
			{
				return CommandBase.ExecuteWithErrorHandling(() =>
				{
					return $"Bannerlord {GameEnvironment.BannerlordVersion}";
				}, "Failed to access Bannerlord version");
			});
		}

		/// <summary>
		/// List current BLGM version
		/// </summary>
		[CommandLineFunctionality.CommandLineArgumentFunction("blgm_version", "gm.info")]
		public static string BLGMVersionCommand(List<string> args)
		{
			return Cmd.Run(args, () =>
			{
				return CommandBase.ExecuteWithErrorHandling(() =>
				{
					return $"BLGM v{GameEnvironment.BLGMVersion}";
				}, "Failed to access BLGM version");			
			});
		}

		/// <summary>
		/// List count of objects created with blgm
		/// </summary>
		[CommandLineFunctionality.CommandLineArgumentFunction("blgm_object_count", "gm.info")]
		public static string BLGMObjectCount(List<string> args)
		{
			return Cmd.Run(args, () =>
			{
				return CommandBase.ExecuteWithErrorHandling(() =>
				{
					return $"{BLGMObjectManager.Instance.ObjectCount} Total objects created with BLGM\n" +
						$"Heroes: {BLGMObjectManager.BlgmHeroCount}\n" +
						$"Clans: {BLGMObjectManager.BlgmClanCount}\n" +
						$"Kingdoms: {BLGMObjectManager.BlgmKingdomCount}";
				}, "Failed to access BLGMObjectMananager Counts");							
			});
		}

		/// <summary>
		/// List current loaded mods and their load order
		/// </summary>
		[CommandLineFunctionality.CommandLineArgumentFunction("list_mods", "gm.info")]
		public static string ListLoadedMods(List<string> args)
		{
			return Cmd.Run(args, () =>
			{
				return CommandBase.ExecuteWithErrorHandling(() =>
				{
					string[] moduleNames = GameEnvironment.LoadedModules;
					StringBuilder output = new();

					output.AppendLine($"Loaded Modules ({moduleNames.Length}):");
					output.AppendLine(new string('-', 50));

					foreach (string name in moduleNames)
					{
						output.AppendLine($"- {name}");
					}

					output.AppendLine("\nUse command 'gm.log.enable' before running this command to save command output to a log file you can easily copy and paste");

					return output.ToString();
				}, "Failed to access Modlist");
			});
		}
	
		/// <summary>
		/// List current loaded mods in launch.json format for easy copy/paste
		/// </summary>
		[CommandLineFunctionality.CommandLineArgumentFunction("list_mods_launch", "gm.info")]
		public static string ListModsLaunchFormat(List<string> args)
		{
			return Cmd.Run(args, () =>
			{
				return CommandBase.ExecuteWithErrorHandling(() =>
				{
					string[] moduleNames = GameEnvironment.LoadedModules;
					StringBuilder output = new();

					// Build the launch format string
					string launchFormat = "_MODULES_*" + string.Join("*", moduleNames) + "*_MODULES_";

					output.AppendLine($"Loaded Modules in launch.json format ({moduleNames.Length} modules):");
					output.AppendLine(new string('-', 50));
					output.AppendLine(launchFormat);
					output.AppendLine();
					output.AppendLine("Copy the line above and paste it into your launch.json args section:");
					output.AppendLine("\"args\": [");
					output.AppendLine("    \"/singleplayer\",");
					output.AppendLine("    \"/continuegame\",");
					output.AppendLine($"    \"{launchFormat}\"");
					output.AppendLine("]");

					return output.ToString();
				}, "Failed to access Modlist");
			});
		}
	}
}