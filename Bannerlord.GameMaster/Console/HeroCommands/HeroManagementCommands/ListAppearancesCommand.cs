using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Heroes;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.HeroCommands.HeroManagementCommands
{
    /// <summary>
    /// Console command to list all saved appearance files.
    /// </summary>
    public static class ListAppearancesCommand
    {
        /// <summary>
        /// List all saved appearance files.
        /// Usage: gm.hero.list_appearances
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("list_appearances", "gm.hero")]
        public static string ListAppearances(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return CommandResult.Error(error);

                // MARK: Execute Logic
                string[] files = AppearanceFileManager.Default.ListAppearanceFiles();

                StringBuilder result = new();
                result.AppendLine("Saved Appearance Files:");

                if (files.Length == 0)
                {
                    result.AppendLine("  (No saved appearance files found)");
                    result.AppendLine($"  Directory: {AppearanceFileManager.Default.GetAppearanceDirectory()}");
                }

                else
                {
                    result.AppendLine($"  Found {files.Length} file(s):\n");
                    for (int i = 0; i < files.Length; i++)
                    {
                        result.AppendLine($"  {i + 1}. {files[i]}");
                    }
                }

                return CommandResult.Success(result.ToString());
            }).Message;
        }
    }
}
