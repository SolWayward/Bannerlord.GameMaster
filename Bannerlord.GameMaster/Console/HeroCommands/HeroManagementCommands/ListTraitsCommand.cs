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
    /// Console command to list all saved trait files.
    /// </summary>
    public static class ListTraitsCommand
    {
        /// <summary>
        /// List all saved trait files.
        /// Usage: gm.hero.list_traits
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("list_traits", "gm.hero")]
        public static string ListTraits(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return CommandResult.Error(error);

                // MARK: Execute Logic
                string[] files = TraitFileManager.Default.ListTraitFiles();

                StringBuilder result = new();
                result.AppendLine("Saved Trait Files:");

                if (files.Length == 0)
                {
                    result.AppendLine("  (No saved trait files found)");
                    result.AppendLine($"  Directory: {TraitFileManager.Default.GetTraitDirectory()}");
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
