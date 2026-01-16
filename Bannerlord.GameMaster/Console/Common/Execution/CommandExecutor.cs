using System;
using System.Collections.Generic;
using Bannerlord.GameMaster.Console.Common.Parsing;

namespace Bannerlord.GameMaster.Console.Common.Execution
{
    /// <summary>
    /// Provides command execution functionality with automatic logging, error handling, and argument parsing.
    /// This is the main implementation class - use <see cref="Cmd"/> for a shorter alias.
    /// </summary>
    public static class CommandExecutor
    {
        // MARK: Run Methods (String Return)

        /// <summary>
        /// Execute command with automatic logging and quote parsing.
        /// Automatically parses quoted arguments before passing to command logic.
        /// </summary>
        /// <param name="args">The raw argument list from the console</param>
        /// <param name="action">The command action to execute</param>
        /// <returns>The result string from the command or an error message</returns>
        public static string Run(List<string> args, Func<string> action)
        {
            // Parse quoted arguments BEFORE passing to action
            // This reconstructs 'multi word' arguments that were split by TaleWorlds
            List<string> parsedArgs = ArgumentParser.ParseQuotedArguments(args);

            // Replace the args list contents with parsed args
            if (args != null && parsedArgs != null)
            {
                args.Clear();
                args.AddRange(parsedArgs);
            }

            // Get command name using parsed args for cleaner logging
            string commandName = GetCallingCommandName(parsedArgs);

            try
            {
                string result = action();

                if (CommandLogger.IsEnabled)
                {
                    bool isSuccess = !result.StartsWith("Error:", StringComparison.OrdinalIgnoreCase);
                    CommandLogger.LogCommand(commandName, result, isSuccess);
                }

                return result;
            }
            catch (Exception ex)
            {
                // HandleAndLogException logs to both RGL and custom file, returns simplified message
                return CommandLogger.HandleAndLogException(commandName, ex);
            }
        }

        // MARK: Run Methods (CommandResult Return)

        /// <summary>
        /// Execute command with automatic logging and quote parsing using CommandResult.
        /// Automatically parses quoted arguments before passing to command logic.
        /// </summary>
        /// <param name="args">The raw argument list from the console</param>
        /// <param name="action">The command action to execute</param>
        /// <returns>A CommandResult containing success status and message</returns>
        public static CommandResult Run(List<string> args, Func<CommandResult> action)
        {
            // Parse quoted arguments BEFORE passing to action
            // This reconstructs 'multi word' arguments that were split by TaleWorlds
            List<string> parsedArgs = ArgumentParser.ParseQuotedArguments(args);

            // Replace the args list contents with parsed args
            if (args != null && parsedArgs != null)
            {
                args.Clear();
                args.AddRange(parsedArgs);
            }

            // Get command name using parsed args for cleaner logging
            string commandName = GetCallingCommandName(parsedArgs);

            try
            {
                CommandResult result = action();

                if (CommandLogger.IsEnabled)
                {
                    CommandLogger.LogCommand(commandName, result.Message, result.IsSuccess);
                }

                return result;
            }
            catch (Exception ex)
            {
                // HandleAndLogException logs to both RGL and custom file, returns simplified message
                string errorMessage = CommandLogger.HandleAndLogException(commandName, ex);
                return CommandResult.Error(errorMessage);
            }
        }

        // MARK: Helper Methods

        /// <summary>
        /// Automatically determine command name from calling method using reflection.
        /// Examines the call stack to find the CommandLineArgumentFunctionAttribute.
        /// </summary>
        /// <param name="args">The parsed arguments to include in the command name</param>
        /// <returns>The formatted command name with arguments</returns>
        private static string GetCallingCommandName(List<string> args)
        {
            try
            {
                System.Diagnostics.StackTrace stackTrace = new();
                // Frame 0 = GetCallingCommandName, Frame 1 = Run/ExecuteWithErrorHandling, Frame 2 = actual command
                System.Reflection.MethodBase callingMethod = stackTrace.GetFrame(2)?.GetMethod();

                if (callingMethod != null)
                {
                    // Get the CommandLineArgumentFunction attribute
                    object[] attributes = callingMethod.GetCustomAttributes(false);
                    foreach (object attr in attributes)
                    {
                        Type attrType = attr.GetType();
                        if (attrType.Name == "CommandLineArgumentFunctionAttribute")
                        {
                            // Use reflection to get attribute properties
                            System.Reflection.PropertyInfo nameProperty = attrType.GetProperty("Name");
                            System.Reflection.PropertyInfo parentProperty = attrType.GetProperty("ParentCommandName");

                            if (nameProperty != null && parentProperty != null)
                            {
                                string name = nameProperty.GetValue(attr) as string;
                                string parent = parentProperty.GetValue(attr) as string;

                                // Build command name
                                string commandName = string.IsNullOrEmpty(parent) ? name : $"{parent}.{name}";

                                // Add arguments if present
                                if (args != null && args.Count > 0)
                                {
                                    commandName += " " + string.Join(" ", args);
                                }

                                return commandName;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // If reflection fails, log the error for debugging
                TaleWorlds.Library.Debug.Print($"[BLGMCommandLogger] Failed to get calling command name: {ex.Message}");
            }

            // Fallback: Use generic name with arguments if available
            if (args != null && args.Count > 0)
            {
                return "gm.command " + string.Join(" ", args);
            }

            return "gm.command";
        }
    }

    /// <summary>
    /// Short alias for command execution with automatic logging.
    /// Delegates all functionality to <see cref="CommandExecutor"/>.
    /// Usage in any command: return Cmd.Run(args, () => { /* your logic */ });
    /// </summary>
    public static class Cmd
    {
        /// <summary>
        /// Execute command with automatic logging and quote parsing.
        /// Automatically parses quoted arguments before passing to command logic.
        /// </summary>
        /// <param name="args">The raw argument list from the console</param>
        /// <param name="action">The command action to execute</param>
        /// <returns>The result string from the command or an error message</returns>
        public static string Run(List<string> args, Func<string> action)
        {
            return CommandExecutor.Run(args, action);
        }

        /// <summary>
        /// Execute command with automatic logging and quote parsing using CommandResult.
        /// Automatically parses quoted arguments before passing to command logic.
        /// </summary>
        /// <param name="args">The raw argument list from the console</param>
        /// <param name="action">The command action to execute</param>
        /// <returns>A CommandResult containing success status and message</returns>
        public static CommandResult Run(List<string> args, Func<CommandResult> action)
        {
            return CommandExecutor.Run(args, action);
        }
    }
}
