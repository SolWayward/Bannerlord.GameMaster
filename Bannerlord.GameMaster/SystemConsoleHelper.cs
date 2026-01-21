using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster
{
    public static class SystemConsoleHelper
    {
        private static List<string> _commands = new();
        private static List<string> _allCommands = new();

        /// MARK: CollectCommands
        /// <summary>
        /// Gets all registered .NET game console commands using reflection such as campaign. gm.
        /// Native c++ engine commands will not be found such as config. commands
        /// </summary>
        private static bool TryCollectRegisteredCommands()
        {
            try
            {
                // Get the private static AllFunctions dictionary via reflection
                FieldInfo allFunctionsField = typeof(CommandLineFunctionality).GetField(
                    "AllFunctions",
                    BindingFlags.NonPublic | BindingFlags.Static
                );

                if (allFunctionsField == null)
                {
                    SystemConsoleManager.WriteLog("Could not find AllFunctions field");
                    
                    _commands = new();
                    return false;
                }

                // The field type is Dictionary<string, CommandLineFunctionality.CommandLineFunction>
                // We only need the keys (command names)
                object allFunctionsValue = allFunctionsField.GetValue(null);

                if (allFunctionsValue is System.Collections.IDictionary dictionary)
                {
                    List<string> commands = new();
                    foreach (object key in dictionary.Keys)
                    {
                        commands.Add(key.ToString());
                    }

                    _commands = commands;
                    return true;
                }
            }

            catch (Exception ex)
            {
                SystemConsoleManager.WriteLine($"Could not Access command list via reflection:\n{ex}");
            }

            _commands = new();
            return false;
        }

        /// MARK: GetCommands
        /// <summary>
        /// returns cached list of game registered commands. Collects via reflection if not yet collected
        /// </summary>
        public static List<string> GetRegisteredCommands()
        {
            // Get Command List via reflection if null or empty
            if (_commands == null || _commands.Count == 0)
                TryCollectRegisteredCommands();

            return _commands;
        }

        /// MARK: GetAllCommands
        /// <summary>
        /// returns cached list of all commands. Collects via reflection if not yet collected
        /// </summary>
        public static List<string> GetAllRegisteredCommands()
        {
            if (_allCommands == null || _allCommands.Count == 0)
            {           
                _allCommands = GetRegisteredCommands();
                _allCommands.AddRange(SystemConsoleCommands.GetCustomRegistedCommands());
                _allCommands = _allCommands.Select(c => c.ToLower())
                                            .ToList();
            }

            return _allCommands;
        }

        /// MARK: FuzzySearch
        /// <summary>
        /// Gets commands or groups matching up to next dot
        /// </summary>
        public static List<string> GetFuzzyMatches(string input)
        {
            string filterPath = input.ToLower();

            List<string> allCommands = GetAllRegisteredCommands();

            HashSet<string> uniqueResults = new();

            foreach (string cmd in allCommands)
            {
                if (!cmd.StartsWith(filterPath)) continue;

                // TRUNCATION LOGIC
                int searchStartIndex = filterPath.Length;

                // If user typed "group.", skip the dot to find next segment
                if (searchStartIndex < cmd.Length && cmd[searchStartIndex] == '.')
                {
                    searchStartIndex++;
                }

                // Find next dot
                int nextDotIndex = cmd.IndexOf('.', searchStartIndex);

                if (nextDotIndex == -1)
                {
                    // It's a full command (Leaf)
                    uniqueResults.Add(cmd);
                }
                else
                {
                    // It's a group (Node)
                    uniqueResults.Add(cmd.Substring(0, nextDotIndex));
                }
            }

            return uniqueResults.OrderBy(x => x).ToList();
        }

        /// MARK: CommonPrefix
        /// <summary>
        /// Helper to find common prefix for auto-completion
        /// </summary>
        public static string GetCommonPrefix(List<string> strings)
        {
            if (strings.Count == 0) return "";
            string prefix = strings[0];

            foreach (string s in strings)
            {
                while (!s.StartsWith(prefix))
                {
                    prefix = prefix.Substring(0, prefix.Length - 1);
                    if (string.IsNullOrEmpty(prefix)) return "";
                }
            }
            return prefix;
        }
    }
}