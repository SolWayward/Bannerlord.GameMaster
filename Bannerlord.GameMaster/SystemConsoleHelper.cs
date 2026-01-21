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
        /// returns list of game console commands. Collects via reflection if not yet collected
        /// </summary>
        public static List<string> GetRegisteredCommands()
        {
            // Get Command List via reflection if null or empty
            if (_commands == null || _commands.Count == 0)
                TryCollectRegisteredCommands();
            
            return _commands;
        }
    }
}