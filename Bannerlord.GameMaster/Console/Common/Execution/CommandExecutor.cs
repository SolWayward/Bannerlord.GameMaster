using System;
using System.Collections.Generic;
using System.Reflection;
using Bannerlord.GameMaster.Console.Common.Parsing;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.Common.Execution
{
    /// <summary>
    /// Provides command execution functionality with automatic logging, error handling, and argument parsing.
    /// This is the main implementation class - use <see cref="Cmd"/> for a shorter alias.
    /// </summary>
    public static class CommandExecutor
    {
        // Cached Command Fields Reflection
        private static readonly FieldInfo NameField = typeof(CommandLineFunctionality.CommandLineArgumentFunction).GetField("Name");
        private static readonly FieldInfo GroupNameField = typeof(CommandLineFunctionality.CommandLineArgumentFunction).GetField("GroupName");
        

        // MARK: Run Method
        /// <summary>
        /// Execute command with automatic logging and quote parsing using CommandResult.
        /// Automatically parses quoted arguments before passing to command logic.
        /// </summary>
        /// <param name="args">The raw argument list from the console</param>
        /// <param name="action">The command action to execute</param>
        /// <returns>A CommandResult containing success status and message</returns>
        public static CommandResult Run(List<string> args, Func<CommandResult> action)
        {
            // Parse quoted arguments
            List<string> parsedArgs = ArgumentParser.ParseQuotedArguments(args);
            if (args != null && parsedArgs != null)
            {
                args.Clear();
                args.AddRange(parsedArgs);
            }

            string commandName = GetCallingCommandName(parsedArgs);

            try
            {
                CommandResult result = action();

                // Log to custom command file (if enabled)
                if (CommandLogger.IsEnabled)
                {
                    CommandLogger.LogCommand(commandName, result.Message, result.IsSuccess);
                }

                return result.Log();  // Logs to system console + RGL
            }
            catch (Exception ex)
            {
                // Already logs everything
                string errorMessage = CommandLogger.HandleAndLogException(commandName, ex);
                return CommandResult.Error(errorMessage);
            }
        }

        // MARK: Helper Methods

        private const int MAX_STACK_FRAMES_TO_SEARCH = 10;

        /// <summary>
        /// Automatically determine command name from calling method using reflection.
        /// Iterates through the call stack to find a method with CommandLineArgumentFunctionAttribute.
        /// </summary>
        /// <param name="args">The parsed arguments to include in the command name</param>
        /// <returns>The formatted command name with arguments</returns>
        private static string GetCallingCommandName(List<string> args)
        {
            try
            {
                System.Diagnostics.StackTrace stackTrace = new();
                
                // Start at frame 2 (0 = GetCallingCommandName, 1 = CommandExecutor.Run)
                // and search upward to find the method with CommandLineArgumentFunctionAttribute
                for (int frameIndex = 2; frameIndex < MAX_STACK_FRAMES_TO_SEARCH; frameIndex++)
                {
                    System.Diagnostics.StackFrame frame = stackTrace.GetFrame(frameIndex);
                    if (frame == null)
                        break;

                    System.Reflection.MethodBase callingMethod = frame.GetMethod();
                    if (callingMethod == null)
                        continue;

                    // Get the CommandLineArgumentFunction attribute
                    object[] attributes = callingMethod.GetCustomAttributes(false);
                    for (int i = 0; i < attributes.Length; i++)
                    {
                        Type attrType = attributes[i].GetType();
                        if (attrType == typeof(CommandLineFunctionality.CommandLineArgumentFunction))
                        {
                            if (NameField != null && GroupNameField != null)
                            {
                                string name = NameField.GetValue(attributes[i]) as string;
                                string parent = GroupNameField.GetValue(attributes[i]) as string;

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

    /// MARK: Run Alias
    /// <summary>
    /// Short alias for command execution with automatic logging.
    /// Delegates all functionality to <see cref="CommandExecutor"/>.
    /// Usage in any command: return Cmd.Run(args, () => { /* your logic */ });
    /// </summary>
    public static class Cmd
    {
        /// <summary>
        /// Execute string-based command by converting to CommandResult internally.
        /// </summary>
        /// <param name="args">The raw argument list from the console</param>
        /// <param name="action">The command action to execute</param>
        /// <returns>A string containing result of command</returns>
        public static string Run(List<string> args, Func<string> action)
        {
            // Convert string action to CommandResult action
            CommandResult result = CommandExecutor.Run(args, () =>
            {
                string stringResult = action();
                bool isSuccess = !stringResult.StartsWith("Error:", StringComparison.OrdinalIgnoreCase);
                return isSuccess ? CommandResult.Success(stringResult) : CommandResult.Error(stringResult);
            });

            return result.Message;
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
