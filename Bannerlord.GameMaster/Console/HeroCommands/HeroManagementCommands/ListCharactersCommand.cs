using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Heroes;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.HeroCommands.HeroManagementCommands
{
    /// <summary>
    /// Console command to list all saved character set files.
    /// </summary>
    public static class ListCharactersCommand
    {
        /// <summary>
        /// List all saved character set files.
        /// Usage: gm.hero.list_characters
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("list_characters", "gm.hero")]
        public static string ListCharacters(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return CommandResult.Error(error);

                // MARK: Execute Logic
                string[] files = CharacterSetFileManager.Default.ListCharacterSetFiles();

                StringBuilder result = new();
                result.AppendLine("Saved Character Set Files:");

                if (files.Length == 0)
                {
                    result.AppendLine("  (No saved character set files found)");
                    result.AppendLine($"  Directory: {CharacterSetFileManager.Default.GetCharacterSetDirectory()}");
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
