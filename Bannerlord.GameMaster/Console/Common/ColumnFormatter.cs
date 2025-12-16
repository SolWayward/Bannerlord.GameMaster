using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bannerlord.GameMaster.Console.Common
{
    /// <summary>
    /// Utility class for formatting query results into aligned columns.
    /// Since the game console treats tabs as fixed spaces, this uses calculated spacing
    /// to ensure proper column alignment.
    /// </summary>
    public class ColumnFormatter<T>
    {
        private readonly List<ColumnDefinition<T>> _columns = new List<ColumnDefinition<T>>();
        private const int MIN_SPACING = 2; // Minimum spaces between columns

        /// <summary>
        /// Defines a column with its extractor function
        /// </summary>
        public class ColumnDefinition<TEntity>
        {
            public Func<TEntity, string> ValueExtractor { get; set; }
            public int MaxWidth { get; set; }

            public ColumnDefinition(Func<TEntity, string> valueExtractor)
            {
                ValueExtractor = valueExtractor;
                MaxWidth = 0;
            }
        }

        /// <summary>
        /// Adds a column to the formatter
        /// </summary>
        /// <param name="valueExtractor">Function that extracts the column value from an entity</param>
        /// <returns>The formatter instance for fluent chaining</returns>
        public ColumnFormatter<T> AddColumn(Func<T, string> valueExtractor)
        {
            _columns.Add(new ColumnDefinition<T>(valueExtractor));
            return this;
        }

        /// <summary>
        /// Formats a list of entities into aligned columns
        /// </summary>
        /// <param name="entities">The entities to format</param>
        /// <returns>Formatted string with aligned columns</returns>
        public string Format(List<T> entities)
        {
            if (entities == null || entities.Count == 0)
                return "";

            // Calculate maximum width for each column
            CalculateColumnWidths(entities);

            // Build formatted output
            StringBuilder result = new StringBuilder();
            foreach (var entity in entities)
            {
                result.AppendLine(FormatRow(entity));
            }

            return result.ToString();
        }

        /// <summary>
        /// Calculates the maximum width needed for each column
        /// </summary>
        private void CalculateColumnWidths(List<T> entities)
        {
            foreach (var column in _columns)
            {
                column.MaxWidth = 0;
                foreach (var entity in entities)
                {
                    string value = column.ValueExtractor(entity) ?? "";
                    if (value.Length > column.MaxWidth)
                    {
                        column.MaxWidth = value.Length;
                    }
                }
            }
        }

        /// <summary>
        /// Formats a single row with proper column alignment
        /// </summary>
        private string FormatRow(T entity)
        {
            StringBuilder row = new StringBuilder();
            
            for (int i = 0; i < _columns.Count; i++)
            {
                var column = _columns[i];
                string value = column.ValueExtractor(entity) ?? "";
                
                // For all columns except the last, pad to column width + spacing
                if (i < _columns.Count - 1)
                {
                    int totalWidth = column.MaxWidth + MIN_SPACING;
                    row.Append(value.PadRight(totalWidth));
                }
                else
                {
                    // Last column doesn't need padding
                    row.Append(value);
                }
            }
            
            return row.ToString();
        }

        /// <summary>
        /// Static helper method for quick single-use formatting
        /// </summary>
        public static string FormatList<TEntity>(
            List<TEntity> entities,
            params Func<TEntity, string>[] columnExtractors)
        {
            var formatter = new ColumnFormatter<TEntity>();
            foreach (var extractor in columnExtractors)
            {
                formatter.AddColumn(extractor);
            }
            return formatter.Format(entities);
        }
    }
}