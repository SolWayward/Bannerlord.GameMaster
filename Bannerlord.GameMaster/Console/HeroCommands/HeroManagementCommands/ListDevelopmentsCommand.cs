using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Heroes.HeroDevelopment;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.HeroCommands.HeroManagementCommands
{
    /// <summary>
    /// Console command to list all saved development files.
    /// </summary>
    public static class ListDevelopmentsCommand
    {
        /// <summary>
        /// List all saved development files.
        /// Usage: gm.hero.list_developments
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("list_developments", "gm.hero")]
        public static string ListDevelopments(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return CommandResult.Error(error);

                // MARK: Execute Logic
                string[] files = DevelopmentFileManager.Default.ListDevelopmentFiles();

                StringBuilder result = new();
                result.AppendLine("Saved Development Files:");

                if (files.Length == 0)
                {
                    result.AppendLine("  (No saved development files found)");
                    result.AppendLine($"  Directory: {DevelopmentFileManager.Default.GetDevelopmentDirectory()}");
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
