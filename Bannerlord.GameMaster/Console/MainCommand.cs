using Bannerlord.GameMaster.Console.Common;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;

namespace Bannerlord.GameMaster.Console
{
	/// <summary>
	/// Used for displaying useful info
	/// </summary>
	[CommandLineFunctionality.CommandLineArgumentFunction("info", "gm")]
	public static class MainCommand
	{

		/// <summary>
		/// List current BLGM version
		/// </summary>
		[CommandLineFunctionality.CommandLineArgumentFunction("version", "gm.info")]
		public static string VersionCommand(List<string> args)
		{
			return Cmd.Run(args, () =>
			{
				return $"BLGM {SubModule.Version}";
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
				return $"{BLGMObjectManager.Instance.ObjectCount} BLGM created objects";
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
				string[] moduleNames = TaleWorlds.Engine.Utilities.GetModulesNames();
				StringBuilder output = new();
	
				output.AppendLine($"Loaded Modules ({moduleNames.Length}):");
				output.AppendLine(new string('-', 50));
	
				foreach (string name in moduleNames)
				{
					output.AppendLine($"- {name}");
				}
	
				output.AppendLine("\nUse command 'gm.log.enable' before running this command to save command output to a log file you can easily copy and paste");
	
				return output.ToString();
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
				string[] moduleNames = TaleWorlds.Engine.Utilities.GetModulesNames();
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
			});
		}
	}
}