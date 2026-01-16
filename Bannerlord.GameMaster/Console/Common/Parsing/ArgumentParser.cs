using System.Collections.Generic;
using System.Linq;

namespace Bannerlord.GameMaster.Console.Common.Parsing
{
    /// <summary>
    /// Static class providing argument parsing utilities for console commands.
    /// Handles Bannerlord's console quirk where double quotes are stripped but single quotes are preserved.
    /// This is the single entry point for all argument parsing operations.
    /// </summary>
    public static class ArgumentParser
    {
        #region Quote Parsing

        /// <summary>
        /// Parses arguments to properly handle quoted strings using SINGLE QUOTES.
        /// TaleWorlds removes double quotes but preserves single quotes.
        /// Use 'text with spaces' for multi-word arguments.
        /// </summary>
        /// <remarks>
        /// Examples:
        ///   Input:  ["'vladiv", "castle'", "'castle", "of", "stone'", "example"]
        ///   Output: ["vladiv castle", "castle of stone", "example"]
        ///
        ///   Input:  ["'Castle", "of", "Rocks'"]
        ///   Output: ["Castle of Rocks"]
        ///
        ///   Input:  ["name:'Sir", "Galahad'", "count:5"]
        ///   Output: ["name:Sir Galahad", "count:5"]
        /// </remarks>
        /// <param name="args">Raw arguments from Bannerlord console</param>
        /// <returns>Processed arguments with quoted strings combined</returns>
        public static List<string> ParseQuotedArguments(List<string> args)
        {
            if (args == null || args.Count == 0)
                return args ?? new List<string>();

            List<string> result = new();
            int i = 0;

            // MARK: Main parsing loop
            while (i < args.Count)
            {
                string arg = args[i];

                // MARK: Check for named argument with quoted value
                int colonIndex = arg.IndexOf(':');
                if (colonIndex > 0 && colonIndex < arg.Length - 1)
                {
                    string afterColon = arg.Substring(colonIndex + 1);
                    if (afterColon.StartsWith("'"))
                    {
                        // This is a named argument with a quoted value
                        string name = arg.Substring(0, colonIndex + 1); // Keep the colon
                        string firstPart = afterColon.Substring(1); // Remove leading quote

                        // Check if the quote closes in this same arg
                        if (firstPart.EndsWith("'"))
                        {
                            // Single-word quoted value
                            result.Add(name + firstPart.Substring(0, firstPart.Length - 1));
                            i++;
                            continue;
                        }

                        // Multi-word quoted value - collect remaining parts
                        List<string> quotedParts = new() { firstPart };
                        i++;

                        while (i < args.Count)
                        {
                            string part = args[i];
                            if (part.EndsWith("'"))
                            {
                                // Found closing quote
                                quotedParts.Add(part.Substring(0, part.Length - 1));
                                i++;
                                break;
                            }
                            else
                            {
                                quotedParts.Add(part);
                                i++;
                            }
                        }

                        result.Add(name + string.Join(" ", quotedParts));
                        continue;
                    }
                }

                // MARK: Check for regular quoted argument
                if (arg.StartsWith("'"))
                {
                    // Start collecting parts of the quoted string
                    List<string> quotedParts = new();

                    // Remove leading quote from first part
                    string firstPart = arg.Substring(1);

                    // Check if the quote also ends in this same arg (e.g., 'word')
                    if (firstPart.EndsWith("'"))
                    {
                        // Single-word quoted arg, remove trailing quote
                        result.Add(firstPart.Substring(0, firstPart.Length - 1));
                        i++;
                        continue;
                    }

                    // Add first part (without leading quote)
                    quotedParts.Add(firstPart);
                    i++;

                    // Continue collecting until we find the closing quote
                    while (i < args.Count)
                    {
                        string part = args[i];

                        if (part.EndsWith("'"))
                        {
                            // Found closing quote, remove it and add final part
                            quotedParts.Add(part.Substring(0, part.Length - 1));
                            i++;
                            break;
                        }
                        else
                        {
                            // Middle part of quoted string
                            quotedParts.Add(part);
                            i++;
                        }
                    }

                    // Combine all parts with spaces
                    result.Add(string.Join(" ", quotedParts));

                    // If no closing quote was found, we've consumed all remaining args
                    // which is fine - treat as one long quoted string
                }
                else
                {
                    // Regular unquoted argument
                    result.Add(arg);
                    i++;
                }
            }

            return result;
        }

        #endregion

        #region Argument Joining

        /// <summary>
        /// Joins remaining arguments starting from the specified index into a single string.
        /// Useful for commands that accept multi-word text as the last parameter.
        /// </summary>
        /// <param name="args">Argument list</param>
        /// <param name="startIndex">Index to start joining from</param>
        /// <returns>Joined string or empty string if index out of range</returns>
        /// <example>
        /// JoinRemainingArgs(args, 2) for "gm.cmd arg1 arg2 word1 word2 word3" returns "word1 word2 word3"
        /// </example>
        public static string JoinRemainingArgs(List<string> args, int startIndex)
        {
            if (args == null || startIndex >= args.Count)
                return string.Empty;

            return string.Join(" ", args.Skip(startIndex));
        }

        /// <summary>
        /// Gets an argument at the specified index, or returns all remaining arguments joined if consumeRemaining is true.
        /// This is useful for parameters that should accept multi-word input.
        /// </summary>
        /// <param name="args">Argument list</param>
        /// <param name="index">Index of the argument to get</param>
        /// <param name="consumeRemaining">If true, joins all args from index onwards</param>
        /// <returns>Argument value or empty string if index out of range</returns>
        public static string GetArgument(List<string> args, int index, bool consumeRemaining = false)
        {
            if (args == null || index >= args.Count)
                return string.Empty;

            if (consumeRemaining)
                return JoinRemainingArgs(args, index);

            return args[index];
        }

        #endregion

        #region Full Argument Parsing

        /// <summary>
        /// Parses arguments with support for both quoted strings and named arguments.
        /// First handles quoted arguments, then parses named arguments.
        /// This is the primary entry point for command argument parsing.
        /// </summary>
        /// <param name="args">Raw arguments from Bannerlord console</param>
        /// <returns>ParsedArguments instance with separated named and positional arguments</returns>
        /// <example>
        /// Input: ["count:5", "name:'Sir", "Galahad'", "vlandia"]
        /// Result: Named: {count: "5", name: "Sir Galahad"}, Positional: ["vlandia"]
        /// </example>
        public static ParsedArguments ParseArguments(List<string> args)
        {
            // First, parse quoted arguments to handle multi-word strings
            List<string> quotedParsed = ParseQuotedArguments(args);

            // Then create ParsedArguments which will identify named vs positional
            return new ParsedArguments(quotedParsed);
        }

        #endregion
    }
}
