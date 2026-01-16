using System;
using System.Collections.Generic;
using System.Linq;

namespace Bannerlord.GameMaster.Console.Common.Parsing
{
    /// <summary>
    /// Generic parser for query command arguments that separates search terms from type keywords.
    /// </summary>
    /// <typeparam name="TEnum">The enum type for query types</typeparam>
    public static class QueryArgumentParser<TEnum> where TEnum : struct, Enum
    {
        /// <summary>
        /// Parse command arguments into search filter and enum type flags.
        /// </summary>
        /// <param name="args">Command arguments to parse</param>
        /// <param name="typeKeywords">Set of type keywords to recognize (case-insensitive)</param>
        /// <param name="parseTypes">Function to parse type terms into enum flags</param>
        /// <param name="defaultTypes">Default types if no arguments provided</param>
        /// <returns>Tuple of (query string, parsed types)</returns>
        public static (string query, TEnum types) Parse(
            List<string> args,
            HashSet<string> typeKeywords,
            Func<IEnumerable<string>, TEnum> parseTypes,
            TEnum defaultTypes)
        {
            if (args == null || args.Count == 0)
                return ("", defaultTypes);

            List<string> searchTerms = new();
            List<string> typeTerms = new();

            foreach (string arg in args)
            {
                if (typeKeywords.Contains(arg, StringComparer.OrdinalIgnoreCase))
                    typeTerms.Add(arg);
                else
                    searchTerms.Add(arg);
            }

            string query = string.Join(" ", searchTerms).Trim();
            TEnum types = parseTypes(typeTerms);

            return (query, types);
        }
    }
}
