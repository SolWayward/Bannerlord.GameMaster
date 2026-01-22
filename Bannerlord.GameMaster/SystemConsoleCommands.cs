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
                { "help", HelpCommand },
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

        /// MARK: CloseConsole
        /// <summary>
        /// Clears system console
        /// </summary>
        public static string HelpCommand(List<string> args)
        {
            string output = "ls: Use 'ls' to discover commands. entering 'ls' will list all top level group commands"
                            + "\n\t'ls gm' will list al BLGM command groups, 'ls gm.hero' will list all BLGM hero commands"
                            + "\n\t'ls campaign' will list all of Bannerlords native campaign commands\n"
                            + "\nTab Completion: you can press Tab for auto completion on commands or groups\n"
                            + "Command History: up/down arrows will allow you to navigate through your command history\n"
                            + "Close Console: typing 'close' will close the console window without closing the game\n"
                            + "Clear: typing 'clear' will clear the console output\n"
                            + "Quit Bannerlord: typing 'quitgame' will completely quit out of Bannerlord and close the console\n"
                            + "\nNote: While native Bannerlord commands are available, native engine commands are not. However, I have included\n"
                            + "\ta custom command for toggling cheat mode. 'config.cheat_mode 1' or 'config.cheat_mode 0'\n";

            return output;
        }

        /// MARK: ListCommand
        /// <summary>
        /// Lists commands using fuzzy search
        /// </summary>
        public static string ListCommand(List<string> args)
        {
            string filter = (args != null && args.Count > 0 && args[0] != "-r") ? args[0] : "";

            // Use the new helper
            List<string> matches = SystemConsoleHelper.GetFuzzyMatches(filter);

            string result = "";
            foreach (string cmd in matches)
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