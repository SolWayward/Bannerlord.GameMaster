using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster
{
    public static class SystemConsoleCommands
    {
        static Dictionary<string, Func<List<string>, string>> _customCommands = new();

        /// MARK: RegisterCommands
        /// <summary>
        /// Register custom commands
        /// </summary>
        private static void RegisterCustomCommands()
        {
            _customCommands = new Dictionary<string, Func<List<string>, string>>
            {
                { "clear", ClearCommand },
                { "close", CloseConsole },
                { "config.cheat_mode", ToggleCheatModeCommand },
                { "ls", ListCommand },
                { "quitgame", QuitGame },
            };
        }

        /// MARK: GetCustomCommands
        /// <summary>
        /// Get custom commands
        /// </summary>
        public static List<string> GetCustomRegistedCommands()
        {
            if (_customCommands == null || _customCommands.Count == 0)
                RegisterCustomCommands();

            return _customCommands.Keys.ToList();
        }

        /// MARK: Execute
        /// <summary>
        /// Executes custom command if it exists, returns command not found if it doesnt exist
        /// </summary>
        public static string ExecuteCustomSystemConsoleCommand(string command, List<string> args)
        {
            // Ensure commands are loaded
            if (_customCommands == null || _customCommands.Count == 0)
                RegisterCustomCommands();

            // Check if command exists
            if (_customCommands.TryGetValue(command, out var method))
            {
                try
                {
                    // Execute
                    return method.Invoke(args);
                }
                catch (Exception ex)
                {
                    return $"Error executing command '{command}': {ex}";
                }
            }

            return "Command not found.";
        }

        /// MARK: Clear
        /// <summary>
        /// Clears system console
        /// </summary>
        public static string ClearCommand(List<string> args)
        {
            System.Console.Clear();
            return "";
        }

        /// MARK: CloseConsole
        /// <summary>
        /// Clears system console
        /// </summary>
        public static string CloseConsole(List<string> args)
        {
            SystemConsoleManager.CloseConsole();
            return "";
        }

        /// MARK: ListCommand
        /// <summary>
        /// Lists commands
        /// </summary>
        public static string ListCommand(List<string> args)
        {
            // Normalize commands to lower case
            List<string> _commands = SystemConsoleHelper.GetRegisteredCommands()
                                        .Select(c => c.ToLower())
                                        .ToList();
            // Add custom commands                           
            _commands.AddRange(GetCustomRegistedCommands());
            
            // sort
            _commands = _commands.OrderBy(c => c).ToList();

            // Parse Arguments
            string filterPath = "";
            bool isRecursive = false;

            if (args != null && args.Count > 0)
            {
                // Handle "ListCommand -r"
                if (args[0] == "-r")
                {
                    isRecursive = true;
                }

                else
                {
                    // Handle "ListCommand filter" or "ListCommand filter -r"
                    filterPath = args[0].ToLower();
                    if (args.Count > 1 && args[1] == "-r")
                    {
                        isRecursive = true;
                    }
                }
            }

            HashSet<string> uniqueResults = new HashSet<string>();

            // Filter and Truncate
            foreach (var cmd in _commands)
            {
                // Simple StartsWith check handles both "topgroupA" and "topgroupA.pa"
                if (!cmd.StartsWith(filterPath)) continue;

                // If recursive, we return the full string immediately
                if (isRecursive)
                {
                    uniqueResults.Add(cmd);
                    continue;
                }

                // TRUNCATION LOGIC
                // Default start: end of the filter string.
                int searchStartIndex = filterPath.Length;

                // Edge Case: The user typed a full group name exactly (e.g. "group1")
                // but the command continues ("group1.sub").
                // If the character at [filterLength] is a dot, the user has hit a boundary.
                // We want to skip that dot and show the *next* segment.
                if (searchStartIndex < cmd.Length && cmd[searchStartIndex] == '.')
                {
                    searchStartIndex++;
                }

                // Find the NEXT dot after our start index
                int nextDotIndex = cmd.IndexOf('.', searchStartIndex);

                if (nextDotIndex == -1)
                {
                    // No more dots found, this is a leaf node (command)
                    uniqueResults.Add(cmd);
                }
                else
                {
                    // Found a dot, cut the string there to show the group level
                    uniqueResults.Add(cmd.Substring(0, nextDotIndex));
                }
            }

            string result = "";
            foreach (string cmd in uniqueResults.OrderBy(x => x))
                result += $"{cmd}\n";

            return result;
        }

        /// MARK: Cheat Mode
        public static string ToggleCheatModeCommand(List<string> args)
        {
            if (args.Count > 0)
            {
                if (args[0] == "1")
                    return TaleWorlds.Engine.Utilities.ExecuteCommandLineCommand("config.cheat_mode 1");

                else if (args[0] == "0")
                    return TaleWorlds.Engine.Utilities.ExecuteCommandLineCommand("config.cheat_mode 0");

                else
                    return $"invalid arg: {args[0]}. Use 1 to enable, and 0 to disable";
            }

            else
            {
                return "Error: Must specify either 1 to enable, or 0 to disable";
            }
        }

        /// MARK: QuitGame
        /// <summary>
        /// Completely exits game
        /// </summary>
        public static string QuitGame(List<string> args)
        {
            // Standard Bannerlord quit command
            TaleWorlds.Engine.Utilities.QuitGame();
            
            if (args.Count > 0 && args[0] == "-f")
            {
                Environment.Exit(0);
                return "Force Quiting game...";
            }
            
            return "Quitting game...";
        }
    }
}